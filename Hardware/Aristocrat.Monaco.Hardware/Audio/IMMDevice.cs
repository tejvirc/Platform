namespace Aristocrat.Monaco.Hardware.Audio
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     An audio service provider.
    /// </summary>
    internal sealed class MultimediaDevice
    {
        public const string SpeakerDescriptor = "Speaker";
        
        public MultimediaDevice(IMMDevice comDevice)
        {
            IPropertyStore properties = null;
            PropVariant property = default(PropVariant);

            try
            {
                int result = comDevice.GetId(out var id);
                if (result != 0)
                {
                    IsValid = false;
                    return;
                }
                Id = id;

                result = comDevice.GetState(out var state);
                if (result != 0)
                {
                    IsValid = false;
                    return;
                }
                State = state;

                comDevice.OpenPropertyStore(0, out properties);
                var propKey = PropKey.PKeyDeviceDeviceDesc();

                properties.GetValue(ref propKey, out property);
                Descriptor = property.Value.ToString();
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

        public string Descriptor { get; private set; }

        public string Id { get; private set; }

        public bool IsSpeakerDevice => Descriptor?.Contains(SpeakerDescriptor) ?? false;

        public bool IsValid { get; private set; } = true;

        public DeviceState State { get; private set; }
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(
            ref Guid iid,
            int dwClsCtx,
            IntPtr pActivationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        [PreserveSig]
        int OpenPropertyStore(
            uint stgmAccess,
            out IPropertyStore ppProperties);

        [PreserveSig]
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

        [PreserveSig]
        int GetState(out DeviceState pdwState);
    }
}