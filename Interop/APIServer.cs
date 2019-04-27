using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using SierraLib.Net.Web;

namespace CoreMonitor.Interop
{
    class APIServer
    {
        public event EventHandler<MenuDetails> MenuRegistrationRequest = null;

        public event EventHandler<MenuDetails> MenuValueUpdated = null;

        public event EventHandler<MenuDetails> MenuRemoved = null;

        public event EventHandler<Notification> NotificationDisplayRequest = null;
        
        public WebHostAsync WebServer;

        public List<APIClient> Clients { get; private set; }

        public List<MenuDetails> Menus { get; private set; }

        bool exit = false;

        public APIServer()
        {
            Clients = new List<APIClient>();
            Menus = new List<MenuDetails>();

            WebServer = new WebHostAsync(IPAddress.Loopback, 56302);

            WebServer.MalformedRequest += WebServer_MalformedRequest;
            WebServer.ClientConnected += WebServer_ClientConnected;
            WebServer.ClientDisconnected += WebServer_ClientDisconnected;

            WebServer.DefaultPage = new WebPage("/", new RequestProcessor((request) => {
                return new SierraLib.Net.Web.WebResponse(CoreMonitor.Properties.Resources.HomePage.Replace("<!--CoreMonitorVersion-->",SierraLib.AssemblyInformation.GetAssemblyVersion().ToString()),"text/html",request);
            }));
            WebServer.Pages.Add(WebServer.DefaultPage);
            
            WebServer.Pages.Add(new WebPage("/notifications", CoreMonitor.Properties.Resources.Notifications.Replace("<!--CoreMonitorVersion-->",SierraLib.AssemblyInformation.GetAssemblyVersion().ToString())));
            WebServer.Pages.Add(new WebPage("/information", CoreMonitor.Properties.Resources.Information.Replace("<!--CoreMonitorVersion-->", SierraLib.AssemblyInformation.GetAssemblyVersion().ToString())));


            WebServer.Pages.Add(new WebPage("/notifications/show", new RequestProcessor((request) =>
                {
                    //Handle the request
                    if (request.Arguments.Count == 0)
                        return new SierraLib.Net.Web.WebResponse("failed", "text/plain", SierraLib.Net.Web.HTTPHeaderCode.BadRequest, request);
                    else if (!request.Arguments.ContainsKey("title") && !request.Arguments.ContainsKey("text"))
                        return new SierraLib.Net.Web.WebResponse("failed", "text/plain", SierraLib.Net.Web.HTTPHeaderCode.BadRequest, request);
                    else
                    {
                        //Parse the details
                        Notification notification = null;

                        if (!request.Arguments.ContainsKey("displayperiod"))
                        {
                            notification = new Notification(request.Arguments["title"], request.Arguments["text"]);
                        }
                        else
                        {
                            notification = new Notification(request.Arguments["title"], request.Arguments["text"], Convert.ToInt32(request.Arguments["displayperiod"]));
                        }

                        if (NotificationDisplayRequest != null && notification != null)
                            NotificationDisplayRequest(this, notification);
                    }

                    //Return "success" if it worked or "failed" if it didn't
                    return new SierraLib.Net.Web.WebResponse("success", "text/plain", SierraLib.Net.Web.HTTPHeaderCode.OK, request);
                })));

            WebServer.Pages.Add(new WebPage("/system/version",new RequestProcessor((request) => {
                return new SierraLib.Net.Web.WebResponse(SierraLib.AssemblyInformation.GetAssemblyVersion().ToString(), "text/plain", request);
            })));

            WebServer.Pages.Add(new WebPage("/system/version/show", new RequestProcessor((request) =>
            {
                if (NotificationDisplayRequest != null)
                    NotificationDisplayRequest(this, new Notification("CoreMonitor","Version: v" + SierraLib.AssemblyInformation.GetAssemblyVersion().ToString()));

                return new SierraLib.Net.Web.WebResponse("success", "text/plain", SierraLib.Net.Web.HTTPHeaderCode.OK, request);
                
            })));

            MemoryStream favicon = new MemoryStream();
            CoreMonitor.Properties.Resources.CoreMonitorIcon.ToBitmap().Save(favicon,System.Drawing.Imaging.ImageFormat.Png);
            WebServer.Pages.Add(new WebContent("/favicon.ico","", favicon, "image/png", false));

            WebServer.Pages.Add(new WebRedirect("/logo.png", "/favicon.ico"));

            WebServer.Pages.Add(new WebContent("/Style.css","", new MemoryStream(Encoding.UTF8.GetBytes(CoreMonitor.Properties.Resources.Style)),"text/css",false));
        }

        void WebServer_ClientDisconnected(SierraLib.Net.Web.WebClient client)
        {
            if (clientsContains(client))
            {
                var clientObj = getClient(client);
                foreach (MenuDetails menu in clientObj.Menus)
                    if (MenuRemoved != null)
                        MenuRemoved(this, menu);
                Clients.Remove(clientObj);
            }
        }

        void WebServer_ClientConnected(SierraLib.Net.Web.WebClient client)
        {
            if (!clientsContains(client))
                Clients.Add(new APIClient(client,Menus));
        }

        bool clientsContains(SierraLib.Net.Web.WebClient webClient)
        {
            foreach (APIClient client in Clients)
                if ((client.Client) == webClient)
                    return true;

            return false;
        }

        APIClient getClient(SierraLib.Net.Web.WebClient webClient)
        {
            foreach (APIClient client in Clients)
                if ((client.Client) == webClient)
                    return client;

            return null;
        }

        void WebServer_MalformedRequest(string request, SierraLib.Net.Web.WebClient client)
        {
            if (IsXMLRequest(request))
            {
                ProcessXML(request, getClient(client));
            }
        }

        public void StartServer()
        {
            exit = false;
            Thread serverLoopThread = new Thread(serverLoop);
            WebServer.Start();
            serverLoopThread.Start();
        }

        public void StopServer()
        {
            WebServer.Stop();
            exit = true;
        }

        void serverLoop()
        {
            while (!exit)
            {
                try
                {
                    //Check if any of the clients is making a request
                    for (int i = 0; i < Clients.Count; i++)
                    {

                        if (!Clients[i].Client.ClientSocket.Connected)
                        {
                            foreach (MenuDetails menu in Menus)
                                if (menu.Client == Clients[i] && MenuRemoved != null)
                                    MenuRemoved(this, menu);

                            Clients.Remove(Clients[i]);
                            break;
                        }


                        if (DateTime.Now.Subtract(Clients[i].LastPoll).TotalSeconds > 5)
                        {
                            //Client has disconnected
                            Clients[i].Client.ClientSocket.Close();
                            foreach (MenuDetails menu in Menus)
                                if (menu.Client == Clients[i] && MenuRemoved != null)
                                    MenuRemoved(this, menu);

                            Clients.Remove(Clients[i]);
                            break;
                        }
                    }
                }
                catch
                { }



                Thread.CurrentThread.Join(100);
            }
            WebServer.Stop();
        }
        
        private bool IsXMLRequest(string request)
        {
            if (request.StartsWith("<?xml"))
                return true;
            else
                return false;
        }
        
        private void ProcessXML(string xmlmessage, APIClient client)
        {
            string[] xmlMessages = xmlmessage.Split(new string[] { "<!--EOM-->" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string xml in xmlMessages)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    XmlNode rootNode = doc.SelectSingleNode("Message");

                    if (rootNode.SelectSingleNode("type").InnerText == "MenuRegistration")
                    {
                        ProcessMenuRegistration(doc, client);
                    }
                    else if (rootNode.SelectSingleNode("type").InnerText == "MenuUpdate")
                    {
                        ProcessMenuUpdate(doc, client);
                    }
                    else if (rootNode.SelectSingleNode("type").InnerText == "RemoveMenu") //may be used to remove a menu without disconnecting
                    {
                        ProcessMenuRemoval(doc, client);
                    }
                    else if (rootNode.SelectSingleNode("type").InnerText == "Notification")
                    {
                        ProcessNotification(doc);
                    }
                    else if (rootNode.SelectSingleNode("type").InnerText == "MenuCheck")
                    {
                        ProcessMenuCheck(doc, client);
                    }
                    else if (rootNode.SelectSingleNode("type").InnerText == "Poll")
                    {
                        client.LastPoll = DateTime.Now;
                    }
                }
                catch(Exception ex)
                {
                    ex.ToString();
                }
            }
        }

        string[] getInnerText(XmlNodeList nodes)
        {
            string[] values = new string[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
                values[i] = nodes[i].InnerText;

            return values;
        }

        void ProcessMenuRegistration(XmlDocument doc, APIClient client)
        {
            foreach (MenuDetails menu2 in Menus)
                if (menu2.Identifier == doc.SelectSingleNode("//menuid").InnerText)
                {
                    ProcessMenuUpdate(doc, client);
                    return;
                }

            MenuDetails menu;
            
            if (doc.SelectSingleNode("//value") != null && doc.SelectSingleNode("//values") != null)
            {
                //Combo Options
                menu = new MenuDetails(client,doc.SelectSingleNode("//menuid").InnerText,doc.SelectSingleNode("//text").InnerText, doc.SelectSingleNode("//value").InnerText, getInnerText(doc.SelectSingleNode("//values").SelectNodes("value")), doc.SelectSingleNode("//onchangemessage").InnerText);
            }
            else
            {
                //Button
                menu = new MenuDetails(client, doc.SelectSingleNode("//menuid").InnerText, doc.SelectSingleNode("//text").InnerText, doc.SelectSingleNode("//onclickmessage").InnerText);
            }

            if (MenuRegistrationRequest != null)
                MenuRegistrationRequest(this, menu);

            Menus.Add(menu);

            menu.ItemClicked += (o, e) =>
                {
                    client.OnMenuClicked(menu);
                };

            menu.ItemValueChanged += (o, e) =>
                {
                    client.OnMenuValueChanged(menu);
                };
        }

        void ProcessMenuUpdate(XmlDocument doc, APIClient client)
        {
            string menuID = doc.SelectSingleNode("//menuid").InnerText;
            
            foreach(MenuDetails menu in Menus)
                if (menu.Identifier == menuID)
                {
                    if (doc.SelectSingleNode("//text") != null)
                        menu.Text = doc.SelectSingleNode("//text").InnerText;
                    if (doc.SelectSingleNode("//value") != null)
                        menu.Value = doc.SelectSingleNode("//value").InnerText;
                    if (doc.SelectSingleNode("//values") != null)
                        menu.ValidValues  = getInnerText(doc.SelectSingleNode("//values").SelectNodes("value"));
                    if (MenuValueUpdated != null)
                        MenuValueUpdated(this, menu);
                    break;
                }
        }

        void ProcessMenuRemoval(XmlDocument doc, APIClient client)
        {
            foreach(MenuDetails menu in Menus)
                if (menu.Identifier == doc.SelectSingleNode("//menuid").InnerText)
                {
                    if (MenuRemoved != null)
                        MenuRemoved(this, menu);
                    Menus.Remove(menu);
                    break;
                }


        }

        void ProcessNotification(XmlDocument doc)
        {
            Notification notification = null;

            if (doc.SelectSingleNode("//image") == null && doc.SelectSingleNode("//displayperiod") == null)
            {
                notification = new Notification(doc.SelectSingleNode("//title").InnerText, doc.SelectSingleNode("//text").InnerText);
            }
            else if (doc.SelectSingleNode("//displayperiod") == null)
            {
                notification = new Notification(doc.SelectSingleNode("//title").InnerText, doc.SelectSingleNode("//text").InnerText, doc.SelectSingleNode("//image").InnerText);
            }
            else if(doc.SelectSingleNode("//image") == null)
            {
                notification = new Notification(doc.SelectSingleNode("//title").InnerText, doc.SelectSingleNode("//text").InnerText, Convert.ToInt32(doc.SelectSingleNode("//displayperiod").InnerText));
            }
            else
            {
                notification = new Notification(doc.SelectSingleNode("//title").InnerText, doc.SelectSingleNode("//text").InnerText, doc.SelectSingleNode("//image").InnerText, Convert.ToInt32(doc.SelectSingleNode("//displayperiod").InnerText));
            }

            if (NotificationDisplayRequest != null && notification != null)
                NotificationDisplayRequest(this, notification);
        }

        void ProcessMenuCheck(XmlDocument doc, APIClient client)
        {
            if (client == null)
                return;
            foreach (MenuDetails menu in Menus)
                if (menu.Identifier == doc.SelectSingleNode("//menuid").InnerText)
                {
                    client.Client.ClientSocket.Client.Send(Encoding.UTF8.GetBytes("<Message><menuid>" + menu.Identifier + "</menuid><type>MenuCheck</type><value>1</value></Message>"));
                    return;
                }
            client.Client.ClientSocket.Client.Send(Encoding.UTF8.GetBytes("<Message><menuid>" + doc.SelectSingleNode("//menuid").InnerText + "</menuid><type>MenuCheck</type><value>0</value></Message>"));
        }        
    }
}
