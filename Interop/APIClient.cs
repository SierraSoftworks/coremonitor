using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using CoreMonitor.Controls;
using SierraLib.Net.Web;

namespace CoreMonitor.Interop
{
    class APIClient
    {
        public WebClient Client { get; set; }

        public List<MenuDetails> Menus { get; set; }

        public DateTime LastPoll { get; set; }

        public void OnMenuClicked(MenuItem menu)
        {
            foreach (MenuDetails menuDet in Menus)
                if (menuDet.DisplayMenu == menu)
                {
                    OnMenuClicked(menuDet);
                    return;
                }
        }

        public void OnMenuClicked(MenuDetails menu)
        {
            //Send a message to this client notifying them that the menu was clicked
            if (Client.ClientSocket.Connected)
            {
                Client.ClientSocket.Client.Send(Encoding.UTF8.GetBytes("<Message><type>MenuClicked</type><text>" + menu.Text + "</text><message>" + menu.OnClickEventMessage + "</message></Message>"));
            }
        }

        public void OnMenuValueChanged(MenuItem menu)
        {
            foreach (MenuDetails menuDet in Menus)
                if (menuDet.DisplayMenu == menu)
                {
                    OnMenuValueChanged(menuDet);
                    return;
                }
        }

        public void OnMenuValueChanged(MenuDetails menu)
        {
            //Send a message to this client notifying them that the menu was modified
            if (Client.ClientSocket.Connected)
            {
                Client.ClientSocket.Client.Send(Encoding.UTF8.GetBytes("<Message><type>MenuValueChanged</type><text>" + menu.Text + "</text><message>" + menu.OnValueChangedEventMessage + "</message><value>" + menu.Value + "</value></Message>"));
            }
        }

        public APIClient(WebClient clientSocket, List<MenuDetails> menus)
        {
            Menus = menus;
            Client = clientSocket;
        }
    }
}
