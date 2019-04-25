using System;
using System.Collections.Generic;
using System.Text;
using CoreMonitor.Interop;
using SierraLib.LCD.Logitech;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CoreMonitor.Controls
{
    class NotificationDisplay
    {
        public APIServer NotificationProvider
        {
            get;
            set;
        }

        public List<LcdGdiObject> DisplayObjects
        {
            get;
            set;
        }

        public LcdDevice LCDDevice { get; private set; }

        public int LastNotificationDisplayPeriod { get; set; }

        private bool visible = true;
        public bool Visible
        {
            get { return visible; }
            private set
            {
                if (value != visible)
                {
                    foreach (var child in DisplayObjects)
                        child.IsVisible = value;
                    visible = value;
                }
            }
        }

        Notification lastNotification = null;

        public NotificationDisplay(APIServer notificationServer, LcdDevice device)
        {
            NotificationProvider = notificationServer;
            LCDDevice = device;

            DisplayObjects = new List<LcdGdiObject>();

            LinearGradientBrush headerTextBrush = new LinearGradientBrush(new Rectangle(35, 60, 250, 120), Color.White, Color.FromArgb(190, 190, 190), LinearGradientMode.Vertical);
            
            Font headerTextFontMedium = new Font("Arial", 11.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font headerTextFontLarge = new Font("Arial", 22.0f, FontStyle.Bold, GraphicsUnit.Pixel);


            //0 - Dim Screen
            DisplayObjects.Add(new LcdGdiRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), new RectangleF(0, 0, 322, 240)));

            //1 - Media Outline
            DisplayObjects.Add(new LcdGdiRoundedRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), new RectangleF(35, 60, 250, 120),new LcdGdiRoundedRectangle.RoundedCorners(6,6,10,10)));

            //2 - Title
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontLarge,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(40, 65, 40, 60),
                Text = "Alert"
            });

            //3/0 - Title
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Text = "",
                Margin = new MarginF(40, 100, 40, 45)
            });

            //4 - Image
            DisplayObjects.Add(new LcdGdiImage()
            {
                AlignOnPixels = false,
                Margin = new MarginF(45,65,166,88),
                Size = new SizeF(64,64),
                Image = null,
                IsVisible = false
            });

            NotificationProvider.NotificationDisplayRequest += (o, e) =>
                {
                    lastNotification = e;
                };

            LCDDevice.SoftButtonsChanged += (o, e) =>
                {
                    if (Visible)
                    {
                        if ((e.SoftButtons == LcdSoftButtons.Ok || e.SoftButtons == LcdSoftButtons.Cancel) && LastNotificationDisplayPeriod == 0)
                            LastNotificationDisplayPeriod = 1;
                    }
                };

            Visible = false;

        }

        DateTime displayTime;

        public void Update()
        {
            if (lastNotification != null)
            {
                Visible = true;

                LastNotificationDisplayPeriod = lastNotification.DisplayPeriod;

                if (lastNotification.HasImage)
                {
                    ((LcdGdiText)DisplayObjects[2]).Text = lastNotification.Title ?? "Alert";
                    ((LcdGdiText)DisplayObjects[3]).Text = lastNotification.Text;
                    ((LcdGdiImage)DisplayObjects[4]).Image = lastNotification.Image;
                    //DisplayObjects[2].Margin = new MarginF(109, 60, 40, 60);
                    //DisplayObjects[3].Margin = new MarginF(109, 85, 45, 45);
                }
                else
                {
                    ((LcdGdiText)DisplayObjects[2]).Text = lastNotification.Title ?? "Alert";
                    ((LcdGdiText)DisplayObjects[3]).Text = lastNotification.Text;
                    //DisplayObjects[2].Margin = new MarginF(40, 60, 40, 60);
                    //DisplayObjects[3].Margin = new MarginF(45, 85, 45, 45);
                    ((LcdGdiImage)DisplayObjects[4]).IsVisible = false;
                }

                displayTime = DateTime.Now;
                lastNotification = null;
            }

            else if (LastNotificationDisplayPeriod != 0 && DateTime.Now.Subtract(displayTime).TotalMilliseconds > LastNotificationDisplayPeriod)
            {
                Visible = false;
            }
        }
    }
}
