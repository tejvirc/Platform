namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>Definition of the CheckRebootWhilePrintingEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the VoucherOutHandler in the Accounting layer and is consumed by the PrinterMonitor in the
    ///     Application layer.
    ///     If the VoucherOutHandler fails to recover an unfinished transaction and a voucher out is triggered by another
    ///     recovery, this event will be posted
    ///     so that the voucher out that was triggered will force a reboot while printing error to be displayed by the
    ///     PrinterMonitor.
    /// </remarks>
    [Serializable]
    public class CheckRebootWhilePrintingEvent : BaseEvent
    {
    }
}