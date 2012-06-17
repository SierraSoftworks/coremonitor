using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace CoreMonitor.SystemInfo
{
    class Core
    {
        public float CurrentUsage { get; private set; }
        public bool IsIdle { get; private set; }
        public int CoreIndex { get; private set; }

        private List<float> usageHistory = new List<float>();
        private int usageHistoryCount = 18;

        public int UsageHistoryCount { get { return usageHistoryCount; } set { usageHistoryCount = value; } }

        public SystemInformationProvider Parent { get; set; }

        private PerformanceCounter parkedChecker;
        private PerformanceCounter usageChecker;

        public event EventHandler Updated = null;

        public DateTime LastUpdate
        {
            get;
            private set;
        }

        public List<float> UsageHistory
        {
            get { return usageHistory; }
        }

        bool exit = false;

        public Core(SystemInformationProvider parent,int coreIndex)
        {
            Parent = parent;
            CoreIndex = coreIndex;
            parkedChecker = new PerformanceCounter("Processor Information", "Parking Status", "0," + coreIndex);
            usageChecker = new PerformanceCounter("Processor", "% Processor Time", coreIndex.ToString());

            Thread updateThread = new Thread(UpdateLoop);
            updateThread.Name = "Core " + coreIndex + " update Thread";
            updateThread.Start();
        }

        public void Stop()
        {
            exit = true;
        }

        private void UpdateLoop()
        {
            while (!exit)
            {
                while (DateTime.Now.Subtract(LastUpdate).TotalMilliseconds < Parent.UpdateInterval && !exit)
                    Thread.CurrentThread.Join(100);

                lock (this)
                    Update();

                LastUpdate = DateTime.Now;
            }
        }

        public void Update()
        {
            CurrentUsage = usageChecker.NextValue();
            IsIdle = parkedChecker.NextValue() == 1;

            usageHistory.Add(CurrentUsage);
            if (usageHistory.Count > usageHistoryCount)
                usageHistory.RemoveAt(0);

            if (Updated != null)
                Updated(this, new EventArgs());
        }
    }
}
