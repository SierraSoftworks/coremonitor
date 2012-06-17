using System;
using System.Collections.Generic;
using System.Text;
using SierraLib.LCD.Logitech;
using System.Drawing;
using System.Drawing.Drawing2D;
using SierraLib.IO.Fonts;

namespace CoreMonitor.LCDControls
{
    class Core
    {
        public List<LcdGdiObject> DisplayObjects
        {
            get;
            private set;
        }

        public SystemInfo.SystemInformationProvider SystemInformationProvider
        {
            get;
            set;
        }

        public SystemInfo.Core CoreInformation
        {
            get;
            set;
        }

        public Rectangle CoreDisplayArea
        {
            get;
            private set;
        }

        LinearGradientBrush headerTextBrush;
        LinearGradientBrush offlineBrush;
        Rectangle headerArea;
        Rectangle graphArea;

        public Core(Rectangle area, SystemInfo.SystemInformationProvider systemInfo, SystemInfo.Core core)
        {
            CoreDisplayArea = area;

            DisplayObjects = new List<LcdGdiObject>();
            SystemInformationProvider = systemInfo;
            CoreInformation = core;

            CoreInformation.UsageHistoryCount = ((area.Width - 2) / 4);

            //Create the display objects
            Font headerTextFontSmall = new Font(FontLoader.Fonts.Families[0], 6.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font headerTextFontMedium = new Font("11px2bus", 11.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font headerTextFontLarge = new Font("Arial", 22.0f, FontStyle.Regular, GraphicsUnit.Pixel);

            headerArea = new Rectangle(area.Left, area.Top, area.Width, 14);
            graphArea = new Rectangle(area.Left, area.Top + 14, area.Width, area.Height - 14);

            LinearGradientBrush headerBrush = new LinearGradientBrush(headerArea, Color.FromArgb(40, 40, 40), Color.FromArgb(10, 10, 10), LinearGradientMode.Vertical);
            headerTextBrush = new LinearGradientBrush(headerArea, Color.White, Color.FromArgb(190, 190, 190), LinearGradientMode.Vertical);
            LinearGradientBrush graphBrush = new LinearGradientBrush(graphArea, Color.FromArgb(100, 100, 100), Color.FromArgb(60, 60, 60), LinearGradientMode.Vertical);
            offlineBrush = new LinearGradientBrush(area, Color.FromArgb(180, 0, 0), Color.FromArgb(255, 0, 0), LinearGradientMode.Vertical);
            Pen borderPen = Pens.Black;

            //0 - Border
            DisplayObjects.Add(new LcdGdiRectangle(borderPen, area));

            //1 - Header
            DisplayObjects.Add(new LcdGdiRectangle(headerBrush, headerArea));

            //2 - Core Name
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontSmall,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Text = "CORE " + (CoreInformation.CoreIndex + 1),
                Margin = new MarginF(headerArea.Left + 1, headerArea.Top + 4,320 - headerArea.Right - 1, 240 - headerArea.Bottom - 1)
            });

            //3 - Core Status
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = (!CoreInformation.IsIdle) ? headerTextBrush : offlineBrush,
                Font = headerTextFontSmall,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Text = (!CoreInformation.IsIdle) ? Math.Round(CoreInformation.CurrentUsage,1) + "%" : "SLEEP",
                Margin = new MarginF(headerArea.Left + 1, headerArea.Top + 4, 320 - headerArea.Right + 1, 240 - headerArea.Bottom - 1)
            });

            for (int i = CoreInformation.UsageHistoryCount; i > 0; i--)
            {
                DisplayObjects.Add(new LcdGdiRectangle(
                    graphBrush,new RectangleF(graphArea.Right - (4 * (CoreInformation.UsageHistory.Count - i)) - 2,
                        graphArea.Top + graphArea.Height,
                        4,
                        0)));
            }
        }

        public void UpdateCoreDisplay()
        {

            ((LcdGdiText)DisplayObjects[3]).Brush = (!CoreInformation.IsIdle) ? headerTextBrush : offlineBrush;
            ((LcdGdiText)DisplayObjects[3]).Text = (!CoreInformation.IsIdle) ? Math.Round(CoreInformation.CurrentUsage, 1) + "%" : "SLEEP";

            for (int i = CoreInformation.UsageHistory.Count - 1; i >= 0; i--)
            {
                int displayI = DisplayObjects.Count - i - 1;

                if (displayI < 0)
                    continue;

                DisplayObjects[displayI].Margin = new MarginF(graphArea.Right - (4 * (CoreInformation.UsageHistory.Count - i)) - 2,
                        graphArea.Top + (graphArea.Height * (1 - (CoreInformation.UsageHistory[i] / 100))));
                DisplayObjects[displayI].Size = new SizeF(4, graphArea.Height * (CoreInformation.UsageHistory[i] / 100));

            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            headerTextBrush.Dispose();
            offlineBrush.Dispose();
        }
    }
}
