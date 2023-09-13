namespace Aristocrat.Monaco.Hardware.Audio
{
    using System;

    internal struct PropKey
    {
        /// <summary>
        ///     Returns a PropKey that corresponds to the PKEY_Device_DeviceDesc constant in Core Audio API.
        /// </summary>
        /// <returns></returns>
        public static PropKey PKeyDeviceDeviceDesc() => new PropKey(
            new Guid(
                0xa45c254e,
                0xdf1c,
                0x4efd,
                0x80,
                0x20,
                0x67,
                0xd1,
                0x46,
                0xa8,
                0x50,
                0xe0),
            2);

        private Guid _formatId;
        private int _propertyId;

        /// <summary>
        ///     Encapsulates a property key that can be used to query information in the form of PropVariants.
        /// </summary>
        /// <param name="formatId"></param>
        /// <param name="propertyId"></param>
        public PropKey(Guid formatId, int propertyId)
        {
            _formatId = formatId;
            _propertyId = propertyId;
        }
    }
}
