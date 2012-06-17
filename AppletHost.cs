using System;
using System.Collections.Generic;
using System.Text;
using SierraLib.LCD.Logitech;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace CoreMonitor
{
    sealed class AppletHost
    {
        LcdApplet LCDApplet { get; set; }
        Version AppletVersion { get; set; }

        bool qvgaArrived = false;
        bool monoArrived = false;

        bool exit = false;
        bool pause = false;

        LcdDeviceQvga qvgaDevice = null;

        AutoResetEvent waitARE = new AutoResetEvent(false);

        Displays.QVGA QVGADisplay = null;
        SystemInfo.SystemInformationProvider SystemInformation;
        Interop.APIServer IPCServer;


        public AppletHost()
        {

            IPCServer = new Interop.APIServer();
            SystemInformation = new SystemInfo.SystemInformationProvider(1000);

            IPCServer.StartServer();

            Fonts.FontLoader.LoadFonts();


            AppletVersion = SierraLib.AssemblyInformation.GetAssemblyVersion();

            LCDApplet = new LcdApplet();
            LCDApplet.FriendlyName = "CoreMonitor";
            LCDApplet.Capabilities = LcdAppletCapabilities.Qvga;
#if !DEBUG
            LCDApplet.IsAutoStartable = true;
#else
            LCDApplet.IsAutoStartable = false;
#endif

            LCDApplet.DeviceArrival += (o, e) =>
                {
                    //Add a new device to the device list or
                    //reconnect an existing device.
                    if (e.DeviceType == LcdDeviceType.Qvga)
                        qvgaArrived = true;
                    else
                        monoArrived = true;

                    waitARE.Set();
                };

            LCDApplet.DeviceRemoval += (o, e) =>
                {
                    //Remove the relevant device from the update list
                };

            LCDApplet.ConnectionDisrupted += (o, e) =>
                {
                    //Disable/disconnect all displays and close their threads
                    exit = true;
                    
                };

            LCDApplet.IsEnabledChanged += (o, e) =>
                {
                    //Stop or restart the draw and update loops.
                    pause = !LCDApplet.IsEnabled;
                };


        }
        

        
        public bool restarting = false;

        //This loop will execute as long as the application is
        //running. When the application closes this loop will exit.
        
        //Run this function from the Program.cs file to keep it running.
        [MTAThread]
        public void Start()
        {
            //Keeps trying to connect until it succeeds or fails 3 times
            int tries = 0;
            while (!LCDApplet.Connect() && tries < 3) 
                tries++;

            waitARE.WaitOne();
            

            while (!exit)
            {

                if (qvgaArrived)
                {
                    if (qvgaDevice == null)
                    {
                        qvgaDevice = (LcdDeviceQvga)LCDApplet.OpenDeviceByType(LcdDeviceType.Qvga);
                        QVGADisplay = new Displays.QVGA(qvgaDevice, SystemInformation,IPCServer);

                        QVGADisplay.Exiting += (o, e) =>
                            {
                                exit = true;
                                Application.Exit();
                            };

                        qvgaDevice.CurrentPage = QVGADisplay.LCDPage;
                        qvgaArrived = false;

                    }
                    else
                    {
                        qvgaDevice.ReOpen();
                        qvgaArrived = false;
                    }
                }

                

                if (!pause)
                {
                    if (QVGADisplay != null && LCDApplet.IsEnabled)
                        QVGADisplay.Update();
                }

                if (qvgaDevice != null && !qvgaDevice.IsDisposed)
                    qvgaDevice.DoUpdateAndDraw();

                Thread.Sleep(10);
            }

            IPCServer.StopServer();
            LCDApplet.Disconnect();
            SystemInformation.StopUpdating();
        }


        public void Stop()
        {
            exit = true;
            IPCServer.StopServer();
            SystemInformation.StopUpdating();
        }
    }
}
