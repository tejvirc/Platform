namespace Aristocrat.Monaco.Hardware.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Controls audio using the Windows CoreAudio API
    ///     from: http://stackoverflow.com/questions/14306048/controling-volume-mixer
    ///     and: http://netcoreaudio.codeplex.com/
    /// </summary>
    internal static class AudioManager
    {
        private const string DeviceEnumeratorClsid = "BCDE0395-E52F-467C-8E3D-C4579291692E";

        /// <summary>
        ///     Gets the device enumerator
        /// </summary>
        /// <returns>a <see cref="IMMDeviceEnumerator" /></returns>
        internal static IMMDeviceEnumerator CreateDeviceEnumerator()
        {
            var deviceEnumeratorType = Type.GetTypeFromCLSID(new Guid(DeviceEnumeratorClsid));
            return (IMMDeviceEnumerator)Activator.CreateInstance(deviceEnumeratorType);
        }

        /// <summary>
        ///     Gets the current master volume in scalar values (percentage)
        /// </summary>
        /// <returns>-1 in case of an error, if successful the value will be between 0 and 100</returns>
        internal static float GetMasterVolume()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                {
                    return -1;
                }

                masterVol.GetMasterVolumeLevelScalar(out var volumeLevel);

                return volumeLevel * 100;
            }
            finally
            {
                if (masterVol != null)
                {
                    Marshal.ReleaseComObject(masterVol);
                }
            }
        }

        /// <summary>
        ///     Gets the mute state of the master volume.
        ///     While the volume can be muted the <see cref="GetMasterVolume" /> will still return the pre-muted volume value.
        /// </summary>
        /// <returns>false if not muted, true if volume is muted</returns>
        internal static bool GetMasterVolumeMute()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                {
                    return false;
                }

                masterVol.GetMute(out var isMuted);

                return isMuted;
            }
            finally
            {
                if (masterVol != null)
                {
                    Marshal.ReleaseComObject(masterVol);
                }
            }
        }

        /// <summary>
        ///     Sets the master volume to a specific level
        /// </summary>
        /// <param name="newLevel">Value between 0 and 100 indicating the desired scalar value of the volume</param>
        internal static void SetMasterVolume(float newLevel)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                {
                    return;
                }

                masterVol.SetMasterVolumeLevelScalar(newLevel / 100, Guid.Empty);
            }
            finally
            {
                if (masterVol != null)
                {
                    Marshal.ReleaseComObject(masterVol);
                }
            }
        }

        /// <summary>
        ///     Increments or decrements the current volume level by the
        ///     <see>
        ///         <cref>stepAmount</cref>
        ///     </see>
        ///     .
        /// </summary>
        /// <param name="stepAmount">
        ///     Value between -100 and 100 indicating the desired step amount. Use negative numbers to decrease
        ///     the volume and positive numbers to increase it.
        /// </param>
        /// <returns>the new volume level assigned</returns>
        internal static float StepMasterVolume(float stepAmount)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                {
                    return -1;
                }

                var stepAmountScaled = stepAmount / 100;

                // Get the level
                masterVol.GetMasterVolumeLevelScalar(out var volumeLevel);

                // Calculate the new level
                var newLevel = volumeLevel + stepAmountScaled;
                newLevel = Math.Min(1, newLevel);
                newLevel = Math.Max(0, newLevel);

                masterVol.SetMasterVolumeLevelScalar(newLevel, Guid.Empty);

                // Return the new volume level that was set
                return newLevel * 100;
            }
            finally
            {
                if (masterVol != null)
                {
                    Marshal.ReleaseComObject(masterVol);
                }
            }
        }

        /// <summary>
        ///     Mute or unmute the master volume
        /// </summary>
        /// <param name="isMuted">true to mute the master volume, false to unmute</param>
        internal static void SetMasterVolumeMute(bool isMuted)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                {
                    return;
                }

                masterVol.SetMute(isMuted, Guid.Empty);
            }
            finally
            {
                if (masterVol != null)
                {
                    Marshal.ReleaseComObject(masterVol);
                }
            }
        }

        /// <summary>
        ///     Switches between the master volume mute states depending on the current state
        /// </summary>
        /// <returns>the current mute state, true if the volume was muted, false if unmuted</returns>
        internal static bool ToggleMasterVolumeMute()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                {
                    return false;
                }

                masterVol.GetMute(out var isMuted);
                masterVol.SetMute(!isMuted, Guid.Empty);

                return !isMuted;
            }
            finally
            {
                if (masterVol != null)
                {
                    Marshal.ReleaseComObject(masterVol);
                }
            }
        }

        /// <summary>
        ///     Get Application Volume
        /// </summary>
        /// <param name="pid">a</param>
        /// <returns>b</returns>
        internal static float? GetApplicationVolume(int pid)
        {
            var volume = GetVolumeObject(pid);
            if (volume == null)
            {
                return null;
            }

            volume.GetMasterVolume(out var level);
            Marshal.ReleaseComObject(volume);
            return level * 100;
        }

        /// <summary>
        ///     Get Application Mute
        /// </summary>
        /// <param name="pid">a</param>
        /// <returns>b</returns>
        internal static bool? GetApplicationMute(int pid)
        {
            var volume = GetVolumeObject(pid);
            if (volume == null)
            {
                return null;
            }

            volume.GetMute(out var mute);
            Marshal.ReleaseComObject(volume);
            return mute;
        }

        /// <summary>
        ///     Set Application Volume
        /// </summary>
        /// <param name="pid">a</param>
        /// <param name="level">b</param>
        internal static void SetApplicationVolume(int pid, float level)
        {
            var volume = GetVolumeObject(pid);
            if (volume == null)
            {
                return;
            }

            var guid = Guid.Empty;
            volume.SetMasterVolume(level / 100, ref guid);
            Marshal.ReleaseComObject(volume);
        }

        /// <summary>
        ///     Set Application Mute
        /// </summary>
        /// <param name="pid">a</param>
        /// <param name="mute">b</param>
        internal static void SetApplicationMute(int pid, bool mute)
        {
            var volume = GetVolumeObject(pid);
            if (volume == null)
            {
                return;
            }

            var guid = Guid.Empty;
            volume.SetMute(mute, ref guid);
            Marshal.ReleaseComObject(volume);
        }

        /// <summary>
        ///     Check if a speaker device is available
        /// </summary>
        internal static bool IsSpeakerDeviceAvailable()
        {
            return GetPrimarySpeakerDevice()?.State == DeviceState.Active;
        }

        /// <summary>
        ///     Check if device id corresponds to a speaker or not
        /// </summary>
        /// <param name="id">Device id to search for</param>
        /// <returns></returns>
        internal static bool IsSpeakerDevice(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(nameof(id));
            }

            return GetSpeakerDevices().Any(device => id.Equals(device.Id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Registers a client's notification callback interface.
        /// </summary>
        /// <param name="notificationClient">
        ///     Implementation of the <see cref="IMMNotificationClient" /> which is should receive the
        ///     notifications.
        /// </param>
        internal static void RegisterEndpointNotificationCallback(IMMNotificationClient notificationClient)
        {
            var enumerator = CreateDeviceEnumerator();

            enumerator?.RegisterEndpointNotificationCallback(notificationClient);
        }

        /// <summary>
        ///     Deletes the registration of a notification interface that the client registered in a previous call to the
        ///     <see cref="RegisterEndpointNotificationCallback" /> method.
        /// </summary>
        /// <param name="notificationClient">
        ///     Implementation of the <see cref="IMMNotificationClient" /> which should be
        ///     unregistered from any notifications.
        /// </param>
        internal static void UnregisterEndpointNotificationCallback(IMMNotificationClient notificationClient)
        {
            var enumerator = CreateDeviceEnumerator();

            enumerator?.UnregisterEndpointNotificationCallback(notificationClient);
        }

        private static IAudioEndpointVolume GetMasterVolumeObject()
        {
            IMMDevice speakers = null;
            try
            {
                speakers = GetPrimarySpeakerDeviceCom();
                if (speakers == null)
                {
                    return null;
                }

                var iidIAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
                speakers.Activate(ref iidIAudioEndpointVolume, 0, IntPtr.Zero, out var o);
                var masterVol = (IAudioEndpointVolume)o;

                return masterVol;
            }
            finally
            {
                if (speakers != null)
                {
                    Marshal.ReleaseComObject(speakers);
                }
            }
        }

        private static ISimpleAudioVolume GetVolumeObject(int pid)
        {
            IAudioSessionEnumerator sessionEnumerator = null;
            IAudioSessionManager2 mgr = null;
            IMMDevice speakers = null;
            try
            {
                speakers = GetPrimarySpeakerDeviceCom();
                if (speakers == null)
                {
                    return null;
                }

                // activate the session manager. we need the enumerator
                var iidIAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;

                speakers.Activate(ref iidIAudioSessionManager2, 0, IntPtr.Zero, out var o);
                mgr = (IAudioSessionManager2)o;

                // enumerate sessions for on this device
                mgr.GetSessionEnumerator(out sessionEnumerator);
                sessionEnumerator.GetCount(out var count);

                var cpid = 0;

                // search for an audio session with the required process-id
                for (var i = 0; i < count; ++i)
                {
                    IAudioSessionControl2 ctl = null;
                    try
                    {
                        sessionEnumerator.GetSession(i, out ctl);

                        // NOTE: we could also use the app name from ctl.GetDisplayName()
                        ctl.GetProcessId(out cpid);

                        if (cpid == pid)
                        {
                            // ReSharper disable once SuspiciousTypeConversion.Global
                            return ctl as ISimpleAudioVolume;
                        }
                    }
                    finally
                    {
                        if (ctl != null && cpid != pid)
                        {
                            Marshal.ReleaseComObject(ctl);
                        }
                    }
                }

                return null;
            }
            finally
            {
                if (sessionEnumerator != null)
                {
                    Marshal.ReleaseComObject(sessionEnumerator);
                }

                if (mgr != null)
                {
                    Marshal.ReleaseComObject(mgr);
                }

                if (speakers != null)
                {
                    Marshal.ReleaseComObject(speakers);
                }
            }
        }

        private static IEnumerable<MultimediaDevice> GetAudioDevices()
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            try
            {
                deviceEnumerator = CreateDeviceEnumerator();
                deviceEnumerator.EnumAudioEndpoints(EDataFlow.ERender, (uint)DeviceState.All, out var collection);

                collection.GetCount(out var size);
                for (uint i = 0; i < size; i++)
                {
                    IMMDevice device = null;
                    try
                    {
                        collection?.Item(i, out device);
                        if (device != null)
                        {
                            yield return new MultimediaDevice(device);
                        }
                    }
                    finally
                    {
                        if (device != null)
                        {
                            Marshal.ReleaseComObject(device);
                        }
                    }
                }
            }
            finally
            {
                if (deviceEnumerator != null)
                {
                    Marshal.ReleaseComObject(deviceEnumerator);
                }
            }
        }

        private static MultimediaDevice GetPrimarySpeakerDevice()
        {
            var speakerDevices = GetSpeakerDevices();
            var device = speakerDevices.FirstOrDefault(speaker => speaker.State == DeviceState.Active);
            if (device != null)
            {
                return device;
            }

            return speakerDevices.FirstOrDefault();
        }

        private static IMMDevice GetPrimarySpeakerDeviceCom()
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            try
            {
                deviceEnumerator = CreateDeviceEnumerator();
                deviceEnumerator.EnumAudioEndpoints(EDataFlow.ERender, (uint)DeviceState.All, out var collection);

                collection.GetCount(out var size);
                for (uint i = 0; i < size; i++)
                {
                    IMMDevice device = null;
                    IPropertyStore properties = null;
                    PropVariant property = default(PropVariant);
                    try
                    {
                        collection?.Item(i, out device);
                        if (device != null)
                        {
                            device.GetState(out var state);
                            device.OpenPropertyStore(0, out properties);

                            var propKey = PropKey.PKeyDeviceDeviceDesc();
                            properties.GetValue(ref propKey, out property);
                            if (state == DeviceState.Active && property.Value.ToString().Contains(MultimediaDevice.SpeakerDescriptor))
                            {
                                return device;
                            }
                        }
                    }
                    finally
                    {
                        if (property._ptrVal != IntPtr.Zero)
                        {
                            property.Clear();
                        }

                        if (properties != null)
                        {
                            Marshal.ReleaseComObject(properties);
                        }
                    }
                }
            }
            finally
            {
                if (deviceEnumerator != null)
                {
                    Marshal.ReleaseComObject(deviceEnumerator);
                }
            }

            return null;
        }

        private static IEnumerable<MultimediaDevice> GetSpeakerDevices()
        {
            return GetAudioDevices().Where(device => device.IsSpeakerDevice);
        }
    }
}