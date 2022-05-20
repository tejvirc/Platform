namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System;
    using Kernel;

    /// <summary>Disabled reasons enumerations.</summary>
    [Flags]
    public enum DisabledReasons
    {
        /// <summary>Indicates disabled by service.</summary>
        Service = 1,

        /// <summary>Indicates disabled by configuration.</summary>
        Configuration = Service * 2,

        /// <summary>Indicates disabled by system.</summary>
        System = Configuration * 2,

        /// <summary>Indicates disabled by operator.</summary>
        Operator = System * 2,

        /// <summary>Indicates disabled by error.</summary>
        Error = Operator * 2,

        /// <summary>Indicates disabled by firmware update.</summary>
        FirmwareUpdate = Error * 2,

        /// <summary>Indicates disabled by backend.</summary>
        Backend = FirmwareUpdate * 2,

        /// <summary>Indicates disabled by device (GDS).</summary>
        Device = Backend * 2,

        /// <summary>Indicates disabled during game play.</summary>
        GamePlay = Device * 2
    }

    /// <summary>Enabled reason enumerations.</summary>
    [Flags]
    public enum EnabledReasons
    {
        /// <summary>Indicates enabled by service.</summary>
        Service = 1,

        /// <summary>Indicates enabled by configuration.</summary>
        Configuration = Service * 2,

        /// <summary>Indicates enabled by system.</summary>
        System = Configuration * 2,

        /// <summary>Indicates enabled by operator.</summary>
        Operator = System * 2,

        /// <summary>Indicates enabled by reset (power up/error cleared).</summary>
        Reset = Operator * 2,

        /// <summary>Indicates enabled by backend.</summary>
        Backend = Reset * 2,

        /// <summary>Indicates enabled by device (GDS).</summary>
        Device = Backend * 2,

        /// <summary>Indicates enabled after game play.</summary>
        GamePlay = Device * 2
    }

    /// <summary>Definition of the IDeviceService interface.</summary>
    public interface IDeviceService : IService
    {
        /// <summary>Gets a value indicating whether the service is enabled.</summary>
        bool Enabled { get; }

        /// <summary>Gets a value indicating whether the service is initialized.</summary>
        bool Initialized { get; }

        /// <summary>Gets a value indicating the last error the service posted.</summary>
        string LastError { get; }

        /// <summary>Gets a value indicating the reason the service is disabled.</summary>
        DisabledReasons ReasonDisabled { get; }

        /// <summary>Gets or sets the service protocol.</summary>
        string ServiceProtocol { get; set; }

        /// <summary>Disables the service.</summary>
        /// <param name="reason">Reason for disabling.</param>
        void Disable(DisabledReasons reason);

        /// <summary>Enables the service.</summary>
        /// <param name="reason">Reason for enabling.</param>
        /// <returns>True if enabling the service succeeded.</returns>
        bool Enable(EnabledReasons reason);
    }
}