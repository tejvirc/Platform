namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System;
    using Kernel;

    /// <summary>Disabled reasons enumerations.</summary>
    [Flags]
    public enum DisabledReasons
    {
        /// <summary>Indicates disabled by service.</summary>
        [LabelResourceKey("Service")]
        Service = 1,

        /// <summary>Indicates disabled by configuration.</summary>
        [LabelResourceKey("Configuration")]
        Configuration = Service * 2,

        /// <summary>Indicates disabled by system.</summary>
        [LabelResourceKey("System")]
        System = Configuration * 2,

        /// <summary>Indicates disabled by operator.</summary>
        [LabelResourceKey("OperatorRoleName")]
        Operator = System * 2,

        /// <summary>Indicates disabled by error.</summary>
        [LabelResourceKey("Error")]
        Error = Operator * 2,

        /// <summary>Indicates disabled by firmware update.</summary>
        [LabelResourceKey("FirmwareUpdateText")]
        FirmwareUpdate = Error * 2,

        /// <summary>Indicates disabled by backend.</summary>
        [LabelResourceKey("Backend")]
        Backend = FirmwareUpdate * 2,

        /// <summary>Indicates disabled by device (GDS).</summary>
        [LabelResourceKey("HardwareDeviceCaption")]
        Device = Backend * 2,

        /// <summary>Indicates disabled during game play.</summary>
        [LabelResourceKey("Gameplay")]
        GamePlay = Device * 2
    }

    /// <summary>Enabled reason enumerations.</summary>
    [Flags]
    public enum EnabledReasons
    {
        /// <summary>Indicates enabled by service.</summary>
        [LabelResourceKey("Service")]
        Service = 1,

        /// <summary>Indicates enabled by configuration.</summary>
        [LabelResourceKey("Configuration")]
        Configuration = Service * 2,

        /// <summary>Indicates enabled by system.</summary>
        [LabelResourceKey("System")]
        System = Configuration * 2,

        /// <summary>Indicates enabled by operator.</summary>
        [LabelResourceKey("OperatorRoleName")]
        Operator = System * 2,

        /// <summary>Indicates enabled by reset (power up/error cleared).</summary>
        [LabelResourceKey("Reset")]
        Reset = Operator * 2,

        /// <summary>Indicates enabled by backend.</summary>
        [LabelResourceKey("Backend")]
        Backend = Reset * 2,

        /// <summary>Indicates enabled by device (GDS).</summary>
        [LabelResourceKey("HardwareDeviceCaption")]
        Device = Backend * 2,

        /// <summary>Indicates enabled after game play.</summary>
        [LabelResourceKey("Gameplay")]
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