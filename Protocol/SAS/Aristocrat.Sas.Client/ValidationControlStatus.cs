namespace Aristocrat.Sas.Client
{
    using System;

    /// <summary>
    ///     The validation control statuses
    /// </summary>
    [Flags]
    public enum ValidationControlStatus
    {
        None = 0,
        UsePrinterAsCashoutDevice = 0x0001,
        UsePrinterAsHandPayDevice = 0x0002,
        ValidateHandPays = 0x0004,
        PrintRestrictedTickets = 0x0008,
        PrintForeignRestrictedTickets = 0x0010,
        TicketRedemption = 0x0020,
        SecureEnhancedConfiguration = 0x8000
    }
}