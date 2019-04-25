using System;
using System.Collections.Generic;
using System.Text;
using SierraLib.XML;

namespace CoreMonitor
{
    public class Settings : ISerializable
    {
        public Settings()
            : base(typeof(Settings))
        {
            UpdateInterval = 1000;
            VolumeDisplayInterval = 2000;
        }

        public int UpdateInterval
        { get; set; }

        public int VolumeDisplayInterval
        { get; set; }        
    }
}
