namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    using System;

    /// <summary>The Display states.</summary>
    [Flags]
    public enum DisplayDeviceStateFlags
    {
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,

        /// <summary>if child device The device is active.</summary>
        DeviceActive = AttachedToDesktop,

        /// <summary>No idea.</summary>
        MultiDriver = 0x2,

        /// <summary>if child device The device is active.</summary>
        DeviceAttached = MultiDriver,

        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,

        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,

        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x10,

        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,

        /// <summary>The device of type Acc.</summary>
        AccDriver = 0x00000040,

        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,

        /// <summary>Remote session.</summary>
        Remote = 0x4000000,

        /// <summary>Display is disconnected.</summary>
        Disconnect = 0x2000000,

        /// <summary>Display is TS Compatible.</summary>
        TSCompatible = 0x00200000,

        /// <summary>Display is TS Compatible.</summary>
        UnsafeMode = 0x00080000
    }
}