using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Drawing;
using System.IO;
using System.Threading;

namespace CoreMonitorLib
{

    /// <summary>
    /// Provides functions for displaying notifications on an
    /// active CoreMonitor display.
    /// </summary>
    /// <remarks>
    /// It is inadvisable to send notifications more than 10
    /// times per second (i.e. with intervals less than 100ms)
    /// since this will result in a noticable lag between when 
    /// the notification is sent and when it is displayed since
    /// CoreMonitor will display all notifications in sequence.
    /// 
    /// Additionally you may send multiple notifications in
    /// a very short space of time to create the idea of a
    /// real time updated dialog box. Please note that this
    /// will make it impossible for the user to access other
    /// controls and is inadvisable for any length of time.
    /// </remarks>
    public static class Notifications
    {

        static string escapeXML(string text)
        {
            return text
                .Replace("<", "&#60;")
                .Replace(">", "&#62;");
        }

        /// <summary>
        /// Displays a notification on the active CoreMonitor
        /// display with the given information.
        /// </summary>
        /// <param name="title">
        /// The title or heading of the notification. A good idea to
        /// use the name of the application which is sending the
        /// notification to allow the user to understand where
        /// the notification is coming from.
        /// </param>
        /// <param name="text">
        /// The text that should be displayed as the body for the
        /// notification.
        /// </param>
        public static void ShowNotification(string title, string text)
        {
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(IPAddress.Loopback, 56302);
            }
            catch
            {
                //We can't send notifications if CoreMonitor is not running
                return;
            }

            NetworkStream ns = client.GetStream();

            byte[] data = Encoding.UTF8.GetBytes(
                "<Message><type>Notification</type><title>" + escapeXML(title) + "</title><text>" + escapeXML(text) + "</text></Message><!--EOM-->");

            ns.Write(data, 0, data.Length);

            ns.Close();

            client.Close();

        }

        /// <summary>
        /// Displays a notification on the active CoreMonitor
        /// display with the given information.
        /// </summary>
        /// <param name="title">
        /// The title or heading of the notification. A good idea to
        /// use the name of the application which is sending the
        /// notification to allow the user to understand where
        /// the notification is coming from.
        /// </param>
        /// <param name="text">
        /// The text that should be displayed as the body for the
        /// notification.
        /// </param>
        /// <param name="displayPeriod">
        /// The amount of time in milliseconds that the notification
        /// should be displayed for or 0 for a notification that
        /// will remain on screen until the user hides it.
        /// </param>
        public static void ShowNotification(string title, string text, int displayPeriod)
        {
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(IPAddress.Loopback, 56302);
            }
            catch
            {
                //We can't send notifications if CoreMonitor is not running
                return;
            }

            NetworkStream ns = client.GetStream();

            byte[] data = Encoding.UTF8.GetBytes(
                "<Message><type>Notification</type><title>" + escapeXML(title) + "</title><text>" + escapeXML(text) + "</text><displayperiod>" + displayPeriod + "</displayperiod></Message><!--EOM-->");

            ns.Write(data, 0, data.Length);

            ns.Close();

            client.Close();

        }

        /// <summary>
        /// Displays a notification on the active CoreMonitor
        /// display with the given information.
        /// </summary>
        /// <param name="title">
        /// The title or heading of the notification. A good idea to
        /// use the name of the application which is sending the
        /// notification to allow the user to understand where
        /// the notification is coming from.
        /// </param>
        /// <param name="text">
        /// The text that should be displayed as the body for the
        /// notification.
        /// </param>
        /// <param name="image">
        /// A <see cref="Bitmap"/> image that will be scaled to
        /// 64x64 pixels and placed in the top left corner of the
        /// notification.
        /// </param>
        public static void ShowNotification(string title, string text, Bitmap image)
        {
            ShowNotification(title, text, image, 2000);
        }

        /// <summary>
        /// Displays a notification on the active CoreMonitor
        /// display with the given information.
        /// </summary>
        /// <param name="title">
        /// The title or heading of the notification. A good idea to
        /// use the name of the application which is sending the
        /// notification to allow the user to understand where
        /// the notification is coming from.
        /// </param>
        /// <param name="text">
        /// The text that should be displayed as the body for the
        /// notification.
        /// </param>
        /// <param name="image">
        /// A <see cref="Bitmap"/> image that will be scaled to
        /// 64x64 pixels and placed in the top left corner of the
        /// notification.
        /// </param>
        /// <param name="displayPeriod">
        /// The amount of time in milliseconds that the notification
        /// should be displayed for or 0 for a notification that
        /// will remain on screen until the user hides it.
        /// </param>
        public static void ShowNotification(string title, string text, Bitmap image, int displayPeriod)
        {
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(IPAddress.Loopback, 56302);
            }
            catch
            {
                //We can't send notifications if CoreMonitor is not running
                return;
            }

            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            string imageb64 = Convert.ToBase64String(ms.ToArray());
            ms.Close();

            

            NetworkStream ns = client.GetStream();

            byte[] data = Encoding.UTF8.GetBytes(
                "<Message><type>Notification</type><title>" + escapeXML(title) + "</title><text>" + escapeXML(text) + "</text><image>" + imageb64 + "</image><displayperiod>" + displayPeriod + "</displayperiod></Message><!--EOM-->");

            ns.Write(data, 0, data.Length);

            ns.Close();

            client.Close();
        }

        /// <summary>
        /// Asynchronously displays a notification on the active CoreMonitor
        /// display with the given information.
        /// </summary>
        /// <param name="title">
        /// The title or heading of the notification. A good idea to
        /// use the name of the application which is sending the
        /// notification to allow the user to understand where
        /// the notification is coming from.
        /// </param>
        /// <param name="text">
        /// The text that should be displayed as the body for the
        /// notification.
        /// </param>
        public static void ShowNotificationAsync(string title, string text)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
            {
                ShowNotification(title, text);
            }));
        }

        /// <summary>
        /// Asynchronously displays a notification on the active CoreMonitor
        /// display with the given information.
        /// </summary>
        /// <param name="title">
        /// The title or heading of the notification. A good idea to
        /// use the name of the application which is sending the
        /// notification to allow the user to understand where
        /// the notification is coming from.
        /// </param>
        /// <param name="text">
        /// The text that should be displayed as the body for the
        /// notification.
        /// </param>
        /// <param name="displayPeriod">
        /// The amount of time in milliseconds that the notification
        /// should be displayed for or 0 for a notification that
        /// will remain on screen until the user hides it.
        /// </param>
        public static void ShowNotificationAsync(string title, string text, int displayPeriod)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
            {
                ShowNotification(title, text, displayPeriod);
            }));
        }

        /// <summary>
        /// Asynchronously displays a notification on the active CoreMonitor
        /// display with the given information.
        /// </summary>
        /// <param name="title">
        /// The title or heading of the notification. A good idea to
        /// use the name of the application which is sending the
        /// notification to allow the user to understand where
        /// the notification is coming from.
        /// </param>
        /// <param name="text">
        /// The text that should be displayed as the body for the
        /// notification.
        /// </param>
        /// <param name="image">
        /// A <see cref="Bitmap"/> image that will be scaled to
        /// 64x64 pixels and placed in the top left corner of the
        /// notification.
        /// </param>
        public static void ShowNotificationAsync(string title, string text, Bitmap image)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
            {
                ShowNotification(title, text, image);
            }));
        }

        /// <summary>
        /// Asychronously displays a notification on the active CoreMonitor
        /// display with the given information.
        /// </summary>
        /// <param name="title">
        /// The title or heading of the notification. A good idea to
        /// use the name of the application which is sending the
        /// notification to allow the user to understand where
        /// the notification is coming from.
        /// </param>
        /// <param name="text">
        /// The text that should be displayed as the body for the
        /// notification.
        /// </param>
        /// <param name="image">
        /// A <see cref="Bitmap"/> image that will be scaled to
        /// 64x64 pixels and placed in the top left corner of the
        /// notification.
        /// </param>
        /// <param name="displayPeriod">
        /// The amount of time in milliseconds that the notification
        /// should be displayed for or 0 for a notification that
        /// will remain on screen until the user hides it.
        /// </param>
        public static void ShowNotificationAsync(string title, string text, Bitmap image, int displayPeriod)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
            {
                ShowNotification(title, text, image, displayPeriod);
            }));
        }
    }
}
