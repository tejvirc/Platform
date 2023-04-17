namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System.ComponentModel;

    /// <summary>
    ///     Enumeration for different event log types.
    /// </summary>
    public enum EventLogType : byte
    {
        /// <summary>All</summary>
        [Description("All")] All = 0,
        /// <summary>BillIn</summary>
        [Description("BillIn")] BillIn = 1,
        /// <summary>Handpay</summary>
        [Description("Handpay")] Handpay = 2,
        /// <summary>VoucherIn</summary>
        [Description("VoucherIn")] VoucherIn = 3,
        /// <summary>VoucherOut</summary>
        [Description("VoucherOut")] VoucherOut = 4,
        /// <summary>BonusAward</summary>
        [Description("BonusAward")] BonusAward = 5,
        /// <summary>TransferIn</summary>
        [Description("TransferIn")] TransferIn = 6,
        /// <summary>TransferOut</summary>
        [Description("TransferOut")] TransferOut = 7,
        /// <summary>Gameplay</summary>
        [Description("Gameplay")] GamePlay = 8,
        /// <summary>Comms</summary>
        [Description("Comms")] Comms = 9,
        /// <summary>Error</summary>
        [Description("Error")] Error = 10,
        /// <summary>General</summary>
        [Description("General")] General = 11,
        /// <summary>Power</summary>
        [Description("Power")] Power = 12,
        /// <summary>GameConfigurationChange</summary>
        [Description("GameConfigurationChange")] GameConfigurationChange = 13,
        /// <summary>GPU</summary>
        [Description("GPU")] Gpu = 14,
        /// <summary>SoftwareChange</summary>
        [Description("SoftwareChange")] SoftwareChange = 15,
        /// <summary>Progressive</summary>
        [Description("Progressive")] Progressive = 16,
        /// <summary>KeyedCredit</summary>
        [Description("KeyedCredit")] KeyedCredit = 17,
        /// <summary>HardMeterOut</summary>
        [Description("HardMeterOut")] HardMeterOut = 18,
    }
}