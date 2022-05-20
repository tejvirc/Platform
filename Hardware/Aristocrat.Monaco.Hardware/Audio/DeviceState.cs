namespace Aristocrat.Monaco.Hardware.Audio
{
    using System;

    /// <summary>
    ///     Defines the device states as they relate to DEVICE_STATE_XXX Constants
    /// </summary>
    /// <remarks>
    ///     See https://docs.microsoft.com/en-us/windows/desktop/CoreAudio/device-state-xxx-constants
    /// </remarks>
    [Flags]
    [CLSCompliant(false)]
    public enum DeviceState : uint
    {
        Active = 0x00000001,
        Disabled = 0x00000002,
        NotPresent = 0x00000004,
        Unplugged = 0x00000008,
        All = 0x0000000F
    }
}