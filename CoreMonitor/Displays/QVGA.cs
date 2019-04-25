using System;
using System.Collections.Generic;
using System.Text;
using SierraLib.LCD.Logitech;
using CoreMonitor.Interop;
using CoreMonitor.SystemInfo;
using System.Drawing.Drawing2D;
using System.Drawing;
using CoreMonitor.Controls;
using System.IO;
using SierraLib.Drawing;

namespace CoreMonitor.Displays
{
    class QVGA
    {        
        public LcdGdiPage LCDPage
        {
            get;
            set;
        }

        public LcdDeviceQvga LCDDevice
        {
            get;
            set;
        }

        public List<Controls.Core> CoreDisplays
        {
            get;
            set;
        }

        public Controls.VolumeDisplay VolumeDisplay
        {
            get;
            set;
        }

        public Controls.NotificationDisplay MediaDisplay
        {
            get;
            set;
        }

        public APIServer IPCServer
        {
            get;set;
        }

        public SystemInformationProvider SystemInformation
        {
            get;
            set;
        }

        public Controls.UpdateDisplay UpdateDisplay
        { get; set; }

        public Menu Menu
        { get; set; }

        private int RAMDisplayOffset = 0;
        
        public event EventHandler Exiting = null;

        public Settings MySettings
        { get; set; }

        public QVGA(LcdDevice device, SystemInformationProvider systemInfoProvider, APIServer ipcserver)
        {
            MySettings = new Settings();
            if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Sierra Softworks\\CoreMonitor\\settings.xml")))
            {                
                try
                {
                    MySettings = SierraLib.XML.Serialization.Deserialize<Settings>(Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Sierra Softworks\\CoreMonitor\\settings.xml"));
                }
                catch
                {
                    MySettings = new Settings();
                }
            }
                
            if(!Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Sierra Softworks\\CoreMonitor")))
                Directory.CreateDirectory(Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Sierra Softworks\\CoreMonitor"));

            SierraLib.XML.Serialization.Serialize<Settings>(MySettings,Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Sierra Softworks\\CoreMonitor\\settings.xml"));

            UpdateInterval = MySettings.UpdateInterval;

            LCDDevice = (LcdDeviceQvga)device;
            SystemInformation = systemInfoProvider;
            IPCServer = ipcserver;

            CoreDisplays = new List<Controls.Core>();
            LCDPage = new LcdGdiPage(LCDDevice);
            
            LCDPage.DesiredFramerate = 30;
            UpdateInterval = 1000;

            #region Drawing Objects

            Gradient backgroundGradient = new Gradient(Gradient.Positions.Top, Gradient.Positions.Bottom,

                //CPU Header
                new GradientStop(Color.FromArgb(255,80,80,80),GradientStop.PositionType.Absolute,0),
                new GradientStop(Color.FromArgb(255,20,20,20), GradientStop.PositionType.Absolute,26),
                new GradientStop(Color.White,GradientStop.PositionType.Absolute,29),

                //RAM Header
                new GradientStop(Color.White,GradientStop.PositionType.Absolute,155.9999f),
                new GradientStop(Color.FromArgb(255,80,80,80),GradientStop.PositionType.Absolute,156f),
                new GradientStop(Color.FromArgb(255,20,20,20),GradientStop.PositionType.Absolute,196),
                
                //RAM Progress Bar Background                
                new GradientStop(Color.FromArgb(220,220,220), GradientStop.PositionType.Absolute, 199),
                new GradientStop(Color.White,GradientStop.PositionType.Absolute,204),
                new GradientStop(Color.White, GradientStop.PositionType.Absolute, 213),

                
                //OS Display
                new GradientStop(Color.FromArgb(255,80,80,80),GradientStop.PositionType.Absolute,216),
                new GradientStop(Color.FromArgb(20,20,20),GradientStop.PositionType.Absolute,240)
                );

            Gradient backgroundOverlayGradient = new Gradient(Gradient.Positions.Left,Gradient.Positions.Right,
                new GradientStop(Color.FromArgb(80,0,0,0),GradientStop.PositionType.Absolute,0),
                new GradientStop(Color.FromArgb(0,0,0,0),GradientStop.PositionType.Absolute,4),
                new GradientStop(Color.FromArgb(0,0,0,0),GradientStop.PositionType.Absolute,-4),
                new GradientStop(Color.FromArgb(80,0,0,0),GradientStop.PositionType.Percentage,100));

            Gradient progressBarGradient = new Gradient(Gradient.Positions.Top, Gradient.Positions.Bottom,
                new GradientStop(Color.FromArgb(40,40,40),GradientStop.PositionType.Absolute,0),
                new GradientStop(Color.FromArgb(100,100,240),GradientStop.PositionType.Absolute,2),
                new GradientStop(Color.FromArgb(0,0,140),GradientStop.PositionType.Absolute,-3),
                new GradientStop(Color.FromArgb(60,60,60),GradientStop.PositionType.Percentage,100));

            //LinearGradientBrush headerBrush = new LinearGradientBrush(new Rectangle(0, 0, 320, 28), Color.FromArgb(40, 40, 40), Color.FromArgb(10, 10, 10), LinearGradientMode.Vertical);
            Brush headerBrush = Brushes.Transparent;
            LinearGradientBrush headerTextBrush = new LinearGradientBrush(new Rectangle(0, 0, 320, 28), Color.White, Color.FromArgb(190, 190, 190), LinearGradientMode.Vertical);
            

            Font headerTextFontSmall = new Font(Fonts.FontLoader.Fonts.Families[0], 6.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font headerTextFontMedium = new Font("Arial", 11.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font headerTextFontLarge = new Font("Arial", 22.0f, FontStyle.Bold, GraphicsUnit.Pixel);
            
            #endregion

            //Generate display

            #region Background
            //0 - Background
            LCDPage.Children.Add(new LcdGdiImage(backgroundGradient.RenderGradient(new Rectangle(0, 0, 320, 240))));
            #endregion

            #region CPU Header

            //0 - Header
            //LCDPage.Children.Add(new LcdGdiRectangle(headerBrush, new RectangleF(0, 0, 322, 28)));

            //1 - CPU Text
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontLarge,
                Text = "CPU",
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                Margin = new MarginF(0, 0, 0, 214)
            });

            //2 - Processor Name
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.ProcessorName,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                Margin = new MarginF(0, 0, 0, 214)
            });

            //3 - Processor Speed
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.ProcessorClockFrequency + "MHz [" + SystemInformation.ProcessorBaseClock + "x" + SystemInformation.ProcessorMultiplier + "]",
                VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                Margin = new MarginF(0, 0, 0, 214)
            });

            //4 - Total Usage
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = Math.Round(SystemInformation.TotalProcessorUsage, 1).ToString() + "%",
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                Margin = new MarginF(0, 0, 0, 214)
            });

            //5 - Core Count
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.ProcessorCount + " Cores",
                VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                Margin = new MarginF(0, 0, 2, 214)
            });

            #endregion

            #region CPU Blocks
            if (SystemInformation.ProcessorCount == 1)
            {
                Controls.Core core1 = new Controls.Core(new Rectangle(2, 30, 316, 124), SystemInformation, SystemInformation.CoreStatus[0]);

                CoreDisplays.Add(core1);

                LCDPage.Children.AddRange(core1.DisplayObjects);
            }
            else if (SystemInformation.ProcessorCount == 2)
            {
                Controls.Core core1 = new Controls.Core(new Rectangle(2, 30, 157, 124), SystemInformation, SystemInformation.CoreStatus[0]);
                Controls.Core core2 = new Controls.Core(new Rectangle(161, 30, 157, 124), SystemInformation, SystemInformation.CoreStatus[1]);

                CoreDisplays.Add(core1);
                CoreDisplays.Add(core2);

                LCDPage.Children.AddRange(core1.DisplayObjects);
                LCDPage.Children.AddRange(core2.DisplayObjects);
            }
            else if (SystemInformation.ProcessorCount == 3)
            {
                Controls.Core core1 = new Controls.Core(new Rectangle(2, 30, 104, 124), SystemInformation, SystemInformation.CoreStatus[0]);
                Controls.Core core2 = new Controls.Core(new Rectangle(108, 30, 104, 124), SystemInformation, SystemInformation.CoreStatus[1]);
                Controls.Core core3 = new Controls.Core(new Rectangle(214, 30, 104, 124), SystemInformation, SystemInformation.CoreStatus[2]);

                CoreDisplays.Add(core1);
                CoreDisplays.Add(core2);
                CoreDisplays.Add(core3);

                LCDPage.Children.AddRange(core1.DisplayObjects);
                LCDPage.Children.AddRange(core2.DisplayObjects);
                LCDPage.Children.AddRange(core3.DisplayObjects);
            }
            else if (SystemInformation.ProcessorCount == 4)
            {
                Controls.Core core1 = new Controls.Core(new Rectangle(2, 30, 157, 60), SystemInformation, SystemInformation.CoreStatus[0]);
                Controls.Core core2 = new Controls.Core(new Rectangle(161, 30, 157, 60), SystemInformation, SystemInformation.CoreStatus[1]);

                Controls.Core core3 = new Controls.Core(new Rectangle(2, 94, 157, 60), SystemInformation, SystemInformation.CoreStatus[2]);
                Controls.Core core4 = new Controls.Core(new Rectangle(161, 94, 157, 60), SystemInformation, SystemInformation.CoreStatus[3]);

                CoreDisplays.Add(core1);
                CoreDisplays.Add(core2);
                CoreDisplays.Add(core3);
                CoreDisplays.Add(core4);

                LCDPage.Children.AddRange(core1.DisplayObjects);
                LCDPage.Children.AddRange(core2.DisplayObjects);
                LCDPage.Children.AddRange(core3.DisplayObjects);
                LCDPage.Children.AddRange(core4.DisplayObjects);
            }
            else if (SystemInformation.ProcessorCount == 6)
            {
                Controls.Core core1 = new Controls.Core(new Rectangle(2, 30, 104, 60), SystemInformation, SystemInformation.CoreStatus[0]);
                Controls.Core core2 = new Controls.Core(new Rectangle(108, 30, 104, 60), SystemInformation, SystemInformation.CoreStatus[1]);
                Controls.Core core3 = new Controls.Core(new Rectangle(214, 30, 104, 60), SystemInformation, SystemInformation.CoreStatus[2]);

                Controls.Core core4 = new Controls.Core(new Rectangle(2, 94, 104, 60), SystemInformation, SystemInformation.CoreStatus[3]);
                Controls.Core core5 = new Controls.Core(new Rectangle(108, 94, 104, 60), SystemInformation, SystemInformation.CoreStatus[4]);
                Controls.Core core6 = new Controls.Core(new Rectangle(214, 94, 104, 60), SystemInformation, SystemInformation.CoreStatus[5]);

                CoreDisplays.Add(core1);
                CoreDisplays.Add(core2);
                CoreDisplays.Add(core3);
                CoreDisplays.Add(core4);
                CoreDisplays.Add(core5);
                CoreDisplays.Add(core6);

                LCDPage.Children.AddRange(core1.DisplayObjects);
                LCDPage.Children.AddRange(core2.DisplayObjects);
                LCDPage.Children.AddRange(core3.DisplayObjects);
                LCDPage.Children.AddRange(core4.DisplayObjects);
                LCDPage.Children.AddRange(core5.DisplayObjects);
                LCDPage.Children.AddRange(core6.DisplayObjects);
            }
            else if (SystemInformation.ProcessorCount == 8)
            {
                Controls.Core core1 = new Controls.Core(new Rectangle(2, 30, 76, 60), SystemInformation, SystemInformation.CoreStatus[0]);
                Controls.Core core2 = new Controls.Core(new Rectangle(82, 30, 76, 60), SystemInformation, SystemInformation.CoreStatus[1]);
                Controls.Core core3 = new Controls.Core(new Rectangle(162, 30, 76, 60), SystemInformation, SystemInformation.CoreStatus[2]);
                Controls.Core core4 = new Controls.Core(new Rectangle(242, 30, 76, 60), SystemInformation, SystemInformation.CoreStatus[3]);

                Controls.Core core5 = new Controls.Core(new Rectangle(2, 94, 76, 60), SystemInformation, SystemInformation.CoreStatus[4]);
                Controls.Core core6 = new Controls.Core(new Rectangle(82, 94, 76, 60), SystemInformation, SystemInformation.CoreStatus[5]);
                Controls.Core core7 = new Controls.Core(new Rectangle(162, 94, 76, 60), SystemInformation, SystemInformation.CoreStatus[6]);
                Controls.Core core8 = new Controls.Core(new Rectangle(242, 94, 76, 60), SystemInformation, SystemInformation.CoreStatus[7]);

                CoreDisplays.Add(core1);
                CoreDisplays.Add(core2);
                CoreDisplays.Add(core3);
                CoreDisplays.Add(core4);
                CoreDisplays.Add(core5);
                CoreDisplays.Add(core6);
                CoreDisplays.Add(core7);
                CoreDisplays.Add(core8);

                LCDPage.Children.AddRange(core1.DisplayObjects);
                LCDPage.Children.AddRange(core2.DisplayObjects);
                LCDPage.Children.AddRange(core3.DisplayObjects);
                LCDPage.Children.AddRange(core4.DisplayObjects);
                LCDPage.Children.AddRange(core5.DisplayObjects);
                LCDPage.Children.AddRange(core6.DisplayObjects);
                LCDPage.Children.AddRange(core7.DisplayObjects);
                LCDPage.Children.AddRange(core8.DisplayObjects);
            }
            RAMDisplayOffset = LCDPage.Children.Count - 1;

            #endregion

            #region RAM Header

            //headerBrush = new LinearGradientBrush(new Rectangle(0, 158, 320, 40), Color.FromArgb(40, 40, 40), Color.FromArgb(10, 10, 10), LinearGradientMode.Vertical);
            headerTextBrush = new LinearGradientBrush(new Rectangle(0, 158, 320, 40), Color.White, Color.FromArgb(190, 190, 190), LinearGradientMode.Vertical);


            //+1 - Header
            LCDPage.Children.Add(new LcdGdiRectangle(headerBrush, new RectangleF(0, 158, 322, 40)));

            //+2 - RAM Text
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontLarge,
                Text = "RAM",
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Center,
                Margin = new MarginF(0, 164, 0, 55)
            });

            //+3 - Total Memory
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.TotalMemoryFormatted + " Total",
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                Margin = new MarginF(0, 158, 0, 55)
            });

            //+4 - Used Memory
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.UsedMemoryFormatted + " Used",
                VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                Margin = new MarginF(0, 158, 0, 43)
            });

            //+5 - Installed Memory
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.InstalledMemoryFormatted + " Installed",
                VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                Margin = new MarginF(0, 158, 0, 55)
            });

            //+6 - RAM Used Percent
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = Math.Round(SystemInformation.PercentageMemoryUsage, 1) + "%",
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                Margin = new MarginF(0, 158, 0, 55)
            });

            //+7 - Free Memory
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.AvailableMemory + " Free",
                VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                Margin = new MarginF(0, 158, 2, 43)
            });

            //+8 - System Reserved Memory
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.SystemReservedMemoryFormatted + " Reserved",
                VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                Margin = new MarginF(0, 158, 2, 55)
            });

            #endregion

            #region RAM Display

            //headerBrush = new LinearGradientBrush(new Rectangle(0, 200, 320, 20), Color.FromArgb(100, 100, 240), Color.FromArgb(0, 0, 140), LinearGradientMode.Vertical);

            //+9 - RAM Border
            //LCDPage.Children.Add(
            //    new LcdGdiRectangle(Pens.Transparent, new LinearGradientBrush(new Rectangle(2, 200, 320, 20),
            //        Color.FromArgb(100, 100, 100), Color.FromArgb(60, 60, 60), LinearGradientMode.Vertical),
            //        new Rectangle(2, 200, 316, 20)));
            LCDPage.Children.Add(new LcdGdiRectangle());

            //+10 - RAM Usage
            //LCDPage.Children.Add(new LcdGdiRectangle(Pens.Black,headerBrush, new RectangleF(2, 200, (float)(316 * (((double)(SystemInformation.UsedMemory) / SystemInformation.TotalMemory))), 20)));
            LCDPage.Children.Add(new LcdGdiImage(progressBarGradient.RenderGradient(new Rectangle(0, 196, 320, 20))) { Margin = new MarginF(2,200,2,24), HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch });

            #endregion
            
            #region OS Display
            //headerBrush = new LinearGradientBrush(new Rectangle(0, 222, 320, 18), Color.FromArgb(40, 40, 40), Color.FromArgb(10, 10, 10), LinearGradientMode.Vertical);
            headerTextBrush = new LinearGradientBrush(new Rectangle(0, 216, 320, 24), Color.White, Color.FromArgb(190, 190, 190), LinearGradientMode.Vertical);

            //+11
            LCDPage.Children.Add(new LcdGdiRectangle(headerBrush, new RectangleF(0, 223, 322, 17)));

            //+12
            LCDPage.Children.Add(new LcdGdiText()
            {
                Brush = headerTextBrush,
                Font = headerTextFontMedium,
                Text = SystemInformation.OperatingSystem,
                VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                Margin = new MarginF(4, 216, 4, 4)
            });

            //+13
            LCDPage.Children.Add(new LcdGdiText()
                {
                    Brush = headerTextBrush,
                    Font = headerTextFontLarge,
                    Text = DateTime.Now.ToString("HH:mm:ss"),
                    VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
                    HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                    Margin = new MarginF(4, 218, 4,0)
                });

            #endregion

            #region Volume Display

            VolumeDisplay = new Controls.VolumeDisplay(SystemInformation);
            VolumeDisplay.VolumeDisplayInterval = MySettings.VolumeDisplayInterval;
            LCDPage.Children.AddRange(VolumeDisplay.DisplayObjects);
            
            #endregion

            #region Media Display

            MediaDisplay = new Controls.NotificationDisplay(ipcserver,LCDDevice);
            LCDPage.Children.AddRange(MediaDisplay.DisplayObjects);

            #endregion

            #region Update Display

            UpdateDisplay = new Controls.UpdateDisplay(this,LCDDevice);
            LCDPage.Children.AddRange(UpdateDisplay.DisplayObjects);

            #endregion

            #region Menu

            Menu = new Menu(LCDDevice);

            MenuItem disableStandbyMenu = new MenuItem("System Standby", "Enabled", new string[] { "Disabled", "Enabled" });
            disableStandbyMenu.ValueChanged += (o, e) =>
                {
                    if (disableStandbyMenu.Value == "Disabled")
                        SierraLib.Windows.SystemPowerState.Reset();
                    else if (disableStandbyMenu.Value == "Enabled")
                        SierraLib.Windows.SystemPowerState.PreventStandby();
                };

            Menu.Items.Add(disableStandbyMenu);

            MenuItem updateIntervalMenu = new MenuItem("Update Interval", MySettings.UpdateInterval + "ms", new string[] { "250ms", "500ms", "1000ms", "2000ms", "5000ms", "10000ms" });
            updateIntervalMenu.ValueChanged += (o, e) =>
                {
                    string value = updateIntervalMenu.Value.Substring(0, updateIntervalMenu.Value.Length - 2);
                    UpdateInterval = Convert.ToInt32(value);
                    SystemInformation.UpdateInterval = UpdateInterval;

                    MySettings.UpdateInterval = UpdateInterval;
                    MySettings.Serialize(Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Sierra Softworks\\CoreMonitor\\settings.xml"));
                };

            Menu.Items.Add(updateIntervalMenu);

            MenuItem volumeTimeMenu = new MenuItem("Volume Notification Time", MySettings.VolumeDisplayInterval + "ms", new string[] { "250ms", "500ms", "1000ms", "2000ms", "5000ms","10000ms" });
            volumeTimeMenu.ValueChanged += (o, e) =>
                {
                    string value = volumeTimeMenu.Value.Substring(0, volumeTimeMenu.Value.Length - 2);
                    VolumeDisplay.VolumeDisplayInterval = Convert.ToInt32(value);

                    MySettings.VolumeDisplayInterval = VolumeDisplay.VolumeDisplayInterval;
                    MySettings.Serialize(Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Sierra Softworks\\CoreMonitor\\settings.xml"));
                };
            Menu.Items.Add(volumeTimeMenu);

            /*
            MenuItem shutdownMenu = new MenuItem("Shutdown After", "Disabled", new string[] { "Disabled", "1 min", "5 min", "10 min", "30 min", "1 hour" });
            shutdownMenu.ValueChanged += (o, e) =>
                {
                    if (shutdownMenu.Value == "Disabled")
                        SierraLib.Windows.SystemStatus.AbortShutdown("localhost");

                    else if (shutdownMenu.Value == "1 min")
                        SierraLib.Windows.SystemStatus.Shutdown("localhost", "Shutting Down", 60);

                    else if (shutdownMenu.Value == "5 min")
                        SierraLib.Windows.SystemStatus.Shutdown("localhost", "Shutting Down", 300);
                    
                    else if (shutdownMenu.Value == "10 min")
                        SierraLib.Windows.SystemStatus.Shutdown("localhost", "Shutting Down", 600);

                    else if (shutdownMenu.Value == "30 min")
                        SierraLib.Windows.SystemStatus.Shutdown("localhost", "Shutting Down", 1800);
                        
                    else if (shutdownMenu.Value == "1 hour")
                        SierraLib.Windows.SystemStatus.Shutdown("localhost", "Shutting Down", 3600);
                };

            Menu.Items.Add(shutdownMenu);
             */

            MenuItem aboutMenu = new MenuItem("About");
            aboutMenu.ItemClicked += (o, e) =>
                {
                    System.Diagnostics.Process.Start("https://sierrasoftworks.com/coremonitor");
                };

            Menu.Items.Add(aboutMenu);

            MenuItem exitMenu = new MenuItem("Exit");
            exitMenu.ItemClicked += (o, e) =>
                {
                    if (Exiting != null)
                        Exiting(this, new EventArgs());
                };

            Menu.Items.Add(exitMenu);

            LCDPage.Children.AddRange(Menu.DisplayObjects);

            #endregion

            #region APIMenu
            int menuInsertIndex = 0;

            IPCServer.MenuRegistrationRequest += (o, e) =>
                {
                    MenuItem menuItem;
                    if (e.IsButton)
                        menuItem = new MenuItem(e.Text);
                    else
                        menuItem = new MenuItem(e.Text, e.Value, e.ValidValues);

                    e.DisplayMenu = menuItem;
                    Menu.Items.Insert(menuInsertIndex, menuItem);
                    menuInsertIndex++;
                };

            IPCServer.MenuRemoved += (o, e) =>
                {
                    Menu.Items.Remove(e.DisplayMenu);
                    menuInsertIndex--;
                    if (menuInsertIndex < 0)
                        menuInsertIndex = 0;
                };

            IPCServer.MenuValueUpdated += (o, e) =>
                {
                    //Shouldn't need to do anything here since this is handled by the API
                };
            #endregion

            LCDPage.Children.Add(new LcdGdiImage(backgroundOverlayGradient.RenderGradient(new Rectangle(0, 0, 320, 240))));
        }

        DateTime LastUpdate;
        public int UpdateInterval { get; set; }

        public void Update()
        {
            if (DateTime.Now.Subtract(LastUpdate).TotalMilliseconds > UpdateInterval)
            {

                ((LcdGdiText)LCDPage.Children[3]).Text = SystemInformation.ProcessorClockFrequency + "MHz [" + SystemInformation.ProcessorBaseClock + "x" + SystemInformation.ProcessorMultiplier + "]";

                ((LcdGdiText)LCDPage.Children[4]).Text = Math.Round(SystemInformation.TotalProcessorUsage, 1).ToString() + "%";


                foreach (Controls.Core core in CoreDisplays)
                    core.UpdateCoreDisplay();

                ((LcdGdiText)LCDPage.Children[RAMDisplayOffset + 4]).Text = SystemInformation.UsedMemoryFormatted + " Used";
                ((LcdGdiText)LCDPage.Children[RAMDisplayOffset + 6]).Text = Math.Round(SystemInformation.PercentageMemoryUsage, 1) + "%";
                ((LcdGdiText)LCDPage.Children[RAMDisplayOffset + 7]).Text = SystemInformation.AvailableMemoryFormatted + " Free";
                ((LcdGdiText)LCDPage.Children[RAMDisplayOffset + 8]).Text = SystemInformation.SystemReservedMemoryFormatted + " Reserved";

                LCDPage.Children[RAMDisplayOffset + 10].Margin = new MarginF(0, 196, LCDPage.Bitmap.Width - (float)(320 * (((double)(SystemInformation.UsedMemory) / SystemInformation.TotalMemory))), 24);

                ((LcdGdiText)LCDPage.Children[RAMDisplayOffset + 13]).Text = DateTime.Now.ToString("HH:mm:ss");

                LastUpdate = DateTime.Now;

            }

            VolumeDisplay.Update();
            MediaDisplay.Update();
            UpdateDisplay.Update();

            try
            {
                LCDDevice.SetAsForegroundApplet = VolumeDisplay.Visible || MediaDisplay.Visible;
            }
            catch { }

        }

        public void Exit()
        {
            if (this.Exiting != null)
                Exiting(this, new EventArgs());
        }
    }
}
