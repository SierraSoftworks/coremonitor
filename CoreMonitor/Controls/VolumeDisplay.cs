using System;
using System.Collections.Generic;
using System.Text;
using SierraLib.LCD.Logitech;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CoreMonitor.Controls
{
    class VolumeDisplay
    {
        public SystemInfo.SystemInformationProvider SystemInformation
        {
            get;
            set;
        }

        public void Show()
        {
            volumeChanged = true;
        }

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

        public int VolumeDisplayInterval
        { get; set; }

        DateTime lastVolumeDisplay;
        bool volumeChanged = false;

        public List<LcdGdiObject> DisplayObjects { get; set; }

        public VolumeDisplay(SystemInfo.SystemInformationProvider systemInfo)
        {
            SystemInformation = systemInfo;

            VolumeDisplayInterval = 1000;

            DisplayObjects = new List<LcdGdiObject>();

            LinearGradientBrush headerBrush = new LinearGradientBrush(new Rectangle(45, 100, 230, 11), Color.FromArgb(40, 40, 240), Color.FromArgb(10, 10, 180), LinearGradientMode.Vertical);
            LinearGradientBrush headerTextBrush = new LinearGradientBrush(new Rectangle(40, 75, 240, 50), Color.White, Color.FromArgb(190, 190, 190), LinearGradientMode.Vertical);
            
            Font headerTextFontMedium = new Font("Arial", 11.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font headerTextFontLarge = new Font("Arial", 22.0f, FontStyle.Bold, GraphicsUnit.Pixel);


            //0 - Dim Screen
            DisplayObjects.Add(new LcdGdiRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), new RectangleF(0, 0, 322, 240)));

            //1 - Volume Outline
            DisplayObjects.Add(new LcdGdiRoundedRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), 
                new RectangleF(35, 75, 250, 50),
                new LcdGdiRoundedRectangle.RoundedCorners(2)));

            //2 - Volume Title
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontLarge,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(40,75,40,100),
                Text = "Volume"
            });

            //3 - Volume Progress Background
            DisplayObjects.Add(new LcdGdiRectangle(headerTextBrush, new RectangleF(45, 100, 230, 11)));

            //4 - Volume Progress Bar
            DisplayObjects.Add(new LcdGdiRectangle(headerBrush, new RectangleF(45, 100, 0, 11)));

            //5 - Volume Progress Text
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.Black,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(45, 98, 45, 105),
                Text = "Mute"
            });

            //6 - Audio Device Name Text
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(45, 110, 45, 95),
                Text = SystemInformation.DefaultAudioDeviceName
            });

            Visible = false;

            
            SystemInformation.DefaultAudioDevice.AudioEndpointVolume.OnVolumeNotification += (e) =>
                {
                    volumeChanged = true;
                };
        }

        //This method should be called each time the display is updated to
        //provide new information if necessary
        public void Update()
        {
            if (volumeChanged)
            {
                volumeChanged = false;

                DisplayObjects[4].Size = new SizeF(230 * SystemInformation.DefaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar, 11);
                ((LcdGdiText)DisplayObjects[5]).Text = (SystemInformation.DefaultAudioDevice.AudioEndpointVolume.Mute) ? "Mute" : Math.Round(SystemInformation.DefaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100) + "%";
                
                Visible = true;

                lastVolumeDisplay = DateTime.Now;
            }
            else if (DateTime.Now.Subtract(lastVolumeDisplay).TotalMilliseconds > VolumeDisplayInterval)
            {
                Visible = false;
            }
        }
    }
}
