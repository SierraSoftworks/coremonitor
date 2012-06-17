using System;
using System.Collections.Generic;
using System.Text;
using SierraLib.LCD.Logitech;
using System.Drawing;
using System.Drawing.Drawing2D;
using CoreMonitor.Displays;

namespace CoreMonitor.Controls
{
    class UpdateDisplay
    {
        public List<LcdGdiObject> DisplayObjects
        { get; set; }

        public LcdDevice LCDDevice { get; private set; }
        public QVGA Applet { get; private set; }

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

        bool updateChanged = false;
        DateTime lastUpdate;

        public UpdateDisplay(QVGA display, LcdDevice device)
        {
            Applet = display;
            LCDDevice = device;

            Font headerTextFontMedium = new Font("Arial", 11.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font headerTextFontLarge = new Font("Arial", 22.0f, FontStyle.Bold, GraphicsUnit.Pixel);

            DisplayObjects = new List<LcdGdiObject>();

            //0 - Dim Screen
            DisplayObjects.Add(new LcdGdiRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), new RectangleF(0, 0, 322, 240)));

            //1 - Outline
            DisplayObjects.Add(new LcdGdiRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), new RectangleF(60, 60, 200, 40)));

            //2 - Title
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.White,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(60, 60, 60, 100),
                Text = "Update available"
            });

            //3 - Details
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.Gray,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(60, 80, 60, 100),
                Text = ""
            });

            Visible = false;

            SierraLib.Updates.Advanced.Updates.UpdateListAvailable += (o, e) =>
                {
                    if (e.LatestVersion.ApplicationVersion > SierraLib.AssemblyInformation.GetAssemblyVersion())
                    {
                        ((LcdGdiText)DisplayObjects[2]).Text = "An update is available [" + e.LatestVersion.ApplicationVersion.ToString(2) + "]";
                        ((LcdGdiText)DisplayObjects[3]).Text = "Press 'OK' to download";
                        updateChanged = true;
                    }
                    else
                    {
                        ((LcdGdiText)DisplayObjects[2]).Text = "No update is available";
                        ((LcdGdiText)DisplayObjects[3]).Text = "Press 'OK' to close";
                    }
                };

            LCDDevice.SoftButtonsChanged += (o, e) =>
                {
                    if (Visible)
                    {
                        if (e.SoftButtons == LcdSoftButtons.Cancel)
                        {
                            Visible = false;
                        }
                        else if (e.SoftButtons == LcdSoftButtons.Ok)
                        {
                            //SierraLib.Updates.Automatic.AutomaticUpdates.InitiateStadgeOne(SierraLib.Updates.Advanced.Updates.ProcessXML(SierraLib.Updates.Advanced.Updates.LastUpdateXML), System.Windows.Forms.Application.ExecutablePath);
                            Visible = false;
                            Applet.Exit();
                        }

                    }
                };
        }

        public void Update()
        {
            if (updateChanged)
            {         
                Visible = true;

                updateChanged = false;
            }
        }
    }
}
