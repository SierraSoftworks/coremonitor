using System;
using System.Collections.Generic;
using System.Text;
using CoreAudioApi;

namespace CoreMonitor.SystemInfo
{
    class VolumeMonitor
    {
        public VolumeMonitor(MMDevice device)
        {
            Name = device.FriendlyName;
            Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            Muted = device.AudioEndpointVolume.Mute;

            device.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
        }

        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            Volume = data.MasterVolume;
            Muted = data.Muted;

            if (VolumeChanged != null)
                VolumeChanged(this, data);
        }

        public string Name
        { get; private set; }

        public float Volume
        { get; private set; }

        public bool Muted
        { get; private set; }

        public event EventHandler<AudioVolumeNotificationData> VolumeChanged = null;
    }
}
