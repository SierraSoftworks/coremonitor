using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using SierraLib.Windows.Win7API.ApplicationServices;

namespace CoreMonitor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += Application_ThreadException;
                        
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
                {
                    StoreException(e.ExceptionObject as Exception);
                };

            if (Environment.OSVersion.Version.Major >= 6)
                ApplicationRestartRecoveryManager.RegisterForApplicationRestart(new RestartSettings("", RestartRestrictions.NotOnReboot));
                        
            try
            {
                using (AppletHost applet = new AppletHost())
                {
                    bool createdNew = false;
                    using (Mutex mutex = new Mutex(true, "Sierra Softworks - CoreMonitor", out createdNew))
                    {
                        if (!createdNew)
                            return;
                        applet.Initialize();
                        applet.Start();
                    }
                }
            }
            catch(Exception e)
            {
                StoreException(e);
            }
        }

        internal static void StoreException(Exception ex)
        {
            if (!Directory.Exists(Environment.ExpandEnvironmentVariables("%AppData%\\Sierra Softworks\\CoreMonitor\\Crash Logs")))
                Directory.CreateDirectory(Environment.ExpandEnvironmentVariables("%AppData%\\Sierra Softworks\\CoreMonitor\\Crash Logs"));

            

            StreamWriter sw = new StreamWriter(Environment.ExpandEnvironmentVariables("%AppData%\\Sierra Softworks\\CoreMonitor\\Crash Logs\\Crash - " + DateTime.Now.ToString("dd.MM.yyyy HH.mm") + ".txt"));
            sw.WriteLine("Unhandled Exception:\n" + ex.Message);
            sw.WriteLine();
            if (ex.TargetSite != null)
            {
                sw.WriteLine("Target Site:\n" + ex.TargetSite.DeclaringType.FullName + ex.TargetSite.Name);
                sw.WriteLine();
            }
            sw.WriteLine("Source:\n" + ex.Source);
            sw.WriteLine();
            sw.WriteLine("Stack Trace:\n" + ex.StackTrace);
            sw.Close();
#if !DEBUG
            //GoogleAnalytics.AccountNumber = "UA-9682191-4";

            //if(Environment.OSVersion.Version.Major >= 6 && SierraLib.Windows.Win7API.Net.NetworkListManager.IsConnectedToInternet)
            //    GoogleAnalytics.FireTrackingEventAsync("http://apptracking.sierrasoftworks.com/CoreMonitor", "Application Crashes", ex.StackTrace, "CoreMonitor", -10);
            //else if (Environment.OSVersion.Version.Major < 6)
            //    GoogleAnalytics.FireTrackingEventAsync("http://apptracking.sierrasoftworks.com/CoreMonitor", "Application Crashes", ex.StackTrace, "CoreMonitor", -10);
                                   
#endif
        }


        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            StoreException(e.Exception);
        }
    }
}
