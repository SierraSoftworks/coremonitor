using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using SierraLib.Windows.Win7API.ApplicationServices;
using SierraLib.Net.CrashReporting;

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

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                        
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
                {
                    StoreException(e.ExceptionObject as Exception);
                };

            if (Environment.OSVersion.Version.Major >= 6)
                ApplicationRestartRecoveryManager.RegisterForApplicationRestart(new RestartSettings("", RestartRestrictions.NotOnReboot));


            SierraLib.Updates.Automatic.AutomaticUpdates automatedUpdates = new SierraLib.Updates.Automatic.AutomaticUpdates("Sierra Softworks - CoreMonitor");

            bool exitNow = false;

            SierraLib.Updates.Automatic.AutomaticUpdates.ExitApplication += (o, e) =>
                {
                    exitNow = true;
                };

            automatedUpdates.UpdateCompleted += (o, e) =>
                {
                    
                };

            automatedUpdates.MultipleInstancesDetected += (o, e) =>
                {
                    exitNow = true;
                    Environment.Exit(0);
                };

            automatedUpdates.ProcessCommandLine(Environment.CommandLine);

            if (exitNow)
                return;

            AppletHost applet = new AppletHost();
            try
            {
                bool createdNew = false;
                using (Mutex mutex = new Mutex(true, "Sierra Softworks - CoreMonitor", out createdNew))
                {
                    if (!createdNew)
                        return;

                    applet.Start();
                }
            }
            catch(Exception e)
            {
                
            }
        }

        static void StoreException(Exception ex)
        {
            if (!Directory.Exists(Environment.ExpandEnvironmentVariables("%AppData%\\Sierra Softworks\\CoreMonitor\\Crash Logs")))
                Directory.CreateDirectory(Environment.ExpandEnvironmentVariables("%AppData%\\Sierra Softworks\\CoreMonitor\\Crash Logs"));

            

            StreamWriter sw = new StreamWriter(Environment.ExpandEnvironmentVariables("%AppData%\\Sierra Softworks\\CoreMonitor\\Crash Logs\\Crash - " + DateTime.Now.ToString("dd.MM.yyyy HH.mm") + ".txt"));
            sw.WriteLine("Unhandled Exception:\n" + ex.Message);
            sw.WriteLine();
            sw.WriteLine("Target Site:\n" + ex.TargetSite.DeclaringType.FullName + ex.TargetSite.Name);
            sw.WriteLine();
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

            if (Environment.OSVersion.Version.Major >= 6 && SierraLib.Windows.Win7API.Net.NetworkListManager.IsConnectedToInternet)
            {
                try
                {
                    WebCrashReport.SendCrashReport("http://sierrasoftworks.com/CrashReporting/ReportCrash.php",
                    new SierraLib.Net.Web.MD5Credentials("apptracking", "password"),
                    "http://apptracking.sierrasoftworks.com",
                    (int)WebCrashReport.Applications.CoreMonitor, ex);
                }
                catch
                {

                }
            }
            else if (Environment.OSVersion.Version.Major < 6)
            {
                try
                {
                    WebCrashReport.SendCrashReport("http://sierrasoftworks.com/ReportCrash.php",
                    new SierraLib.Net.Web.MD5Credentials("apptracking", "password"),
                    "http://apptracking.sierrasoftworks.com",
                    (int)WebCrashReport.Applications.CoreMonitor, ex);
                }
                catch
                {

                }
            }

        }


        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            StoreException(e.Exception);
        }
    }
}
