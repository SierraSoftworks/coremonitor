using System;
using System.Collections.Generic;
using System.Text;
using CoreAudioApi;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace CoreMonitor.SystemInfo
{
    class SystemInformationProvider
    {        
        private Microsoft.VisualBasic.Devices.ComputerInfo systemInfo;
        PerformanceCounter totalCPUcounter;
        private List<Core> cores = new List<Core>();
        ManagementObject processorInfo;

        public int UpdateInterval
        {
            get { return updateInterval; }
            set { updateInterval = value; }
        }

        public float TotalProcessorUsage
        {
            get;
            private set;
        }

        public Core[] CoreStatus
        {
            get { return cores.ToArray(); }
        }

        public ulong TotalMemory
        {
            get { return systemInfo.TotalPhysicalMemory; }
        }
        public string TotalMemoryFormatted
        {
            get
            {
                //if (TotalMemory / 1024 / 1024 < 1024)
                    return (TotalMemory / 1024 / 1024) + "MB";
                //else
                //    return Math.Round((double)TotalMemory / 1024 / 1024 / 1024,1) + "GB";
            }
        }

        public ulong AvailableMemory
        {
            get { return systemInfo.AvailablePhysicalMemory; }
        }
        public string AvailableMemoryFormatted
        {
            get
            {
                //if (AvailableMemory / 1024 / 1024 < 1024)
                    return (AvailableMemory / 1024 / 1024) + "MB";
                //else
                //    return Math.Round((double)AvailableMemory / 1024 / 1024 / 1024, 1) + "GB";
            }
        }

        public ulong UsedMemory
        {
            get { return systemInfo.TotalPhysicalMemory - systemInfo.AvailablePhysicalMemory; }
        }
        public string UsedMemoryFormatted
        {
            get
            {
                //if (UsedMemory / 1024 / 1024 < 1024)
                    return (UsedMemory / 1024 / 1024) + "MB";
                //else
                //    return Math.Round((double)UsedMemory / 1024 / 1024 / 1024, 1) + "GB";
            }
        }

        public ulong InstalledMemory
        {
            get
            {
                return TotalMemory + ((512 * 1024 * 1024) - TotalMemory % (512 * 1024 * 1024));
            }
        }
        public string InstalledMemoryFormatted
        {
            get
            {
                //if (InstalledMemory / 1024 / 1024 < 1024)
                    return (InstalledMemory / 1024 / 1024) + "MB";
                //else
                //    return Math.Round((double)InstalledMemory / 1024 / 1024 / 1024, 1) + "GB";
            }
        }

        public ulong SystemReservedMemory
        {
            get
            {
                return InstalledMemory - TotalMemory;
            }
        }
        public string SystemReservedMemoryFormatted
        {
            get
            {
                //if (SystemReservedMemory / 1024 / 1024 < 1024)
                    return (SystemReservedMemory / 1024 / 1024) + "MB";
                //else
                //    return Math.Round((double)SystemReservedMemory / 1024 / 1024 / 1024, 1) + "GB";
            }
        }

        public double PercentageMemoryUsage
        {
            get { return (double)UsedMemory * 100 / TotalMemory; }
        }

        public string OperatingSystem
        {
            get
            {
                return systemInfo.OSFullName;
            }
        }

        private int ForcedProcessorCount
        {
            get;
            set;
        }

        public int ProcessorCount
        {
            get 
            { 
                if(ForcedProcessorCount == 0)
                    return Environment.ProcessorCount;

                return ForcedProcessorCount;
            }
        }

        //No Leak
        public string ProcessorName
        {
            get
            {
                return GetProcessorName(processorInfo["Name"].ToString(), true);
            }
        }

        //No Leak
        public int ProcessorClockFrequency
        {
            get
            {
                return Convert.ToInt32(processorInfo["CurrentClockSpeed"]);
            }
        }

        //No Leak
        public int ProcessorBaseClock
        {
            get
            {
                return Convert.ToInt32(processorInfo["ExtClock"]);
            }
        }

        //No Leak
        public float ProcessorMultiplier
        {
            get
            {
                return ProcessorClockFrequency / ProcessorBaseClock;
            }
        }

        public List<VolumeMonitor> AudioDevices
        { get; private set; }

        string GetProcessorName(string processorName,bool showManufacturer = false)
        {
            string tmp = processorName;
            if (tmp.StartsWith("Intel(R)"))
            {
                string manufacturer = "Intel";
                string model = tmp.Replace("Intel(R) ", "");
                model = model.Remove(model.IndexOf("CPU"));
                model = model.Replace("(TM)", "");
                model += tmp.Substring(tmp.IndexOf("         "), tmp.IndexOf("@") - tmp.IndexOf("         ")).Trim();
                model = model.Trim();

                if (showManufacturer)
                    return manufacturer + " " + model;
                return model;
            }
            return tmp;
        }

        public SystemInformationProvider() :
            this(1000) { }
        
        public SystemInformationProvider(int _updateInterval)
            : this(_updateInterval, 0)
        { }

        public SystemInformationProvider(int _updateInterval, int forcedCoreCount)
        {
            systemInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            totalCPUcounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            MMDeviceEnumerator audioDeviceEnum = new MMDeviceEnumerator();
            AudioDevices = new List<VolumeMonitor>();
            var devices = audioDeviceEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATEMASK_ALL);
            for (int i = 0; i < devices.Count; i++ )
            {
                AudioDevices.Add(new VolumeMonitor(devices[i]));
            }
            


            //No Leak
            processorInfo = new ManagementObject("Win32_Processor.DeviceID=\"CPU0\"");
            processorInfo.Get();

            ForcedProcessorCount = forcedCoreCount;

            if (forcedCoreCount == 0)
            {
                for (int i = 0; i < ProcessorCount; i++)
                {
                    cores.Add(new Core(this, i));
                }
            }
            else
            {
                for (int i = 0; i < ProcessorCount; i++)
                {
                    cores.Add(new Core(this, i % forcedCoreCount));
                }
            }

            updateInterval = _updateInterval;

            //No Leak
            Thread updateLoop = new Thread(UpdateLoop);
            updateLoop.Start();
        }

        static bool stopUpdating = false;
        static DateTime lastUpdateTime = DateTime.Now;
        int updateInterval = 1000;

        [MTAThread]
        private void UpdateLoop()
        {
            Thread.CurrentThread.Name = "CoreMonitor SystemInformationProvider update loop";
            while (!stopUpdating)
            {
                if (DateTime.Now.Subtract(lastUpdateTime).TotalMilliseconds >= updateInterval)
                {                    
                    TotalProcessorUsage = totalCPUcounter.NextValue();
                    lastUpdateTime = DateTime.Now;
                    processorInfo.Get();

                    if (UpdateCompleted != null)
                        UpdateCompleted(this, new EventArgs());
                }
                Thread.Sleep(100);
            }

            foreach (Core core in cores)
                core.Stop();

            Thread.CurrentThread.Abort();
            stopUpdating = false;
        }
        
        public void RestartUpdating()
        {
            stopUpdating = false;

            Thread updateLoop = new Thread(UpdateLoop);
            updateLoop.Start();
        }

        public void StopUpdating()
        {
            stopUpdating = true;
        }

        public event EventHandler UpdateCompleted = null;
    }
}
