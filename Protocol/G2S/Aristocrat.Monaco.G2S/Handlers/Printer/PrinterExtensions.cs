namespace Aristocrat.Monaco.G2S.Handlers.Printer
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;

    /// <summary>
    ///     Extension methods for printer-related commands
    /// </summary>
    public static class PrinterExtensions
    {
        private const int VoucherTemplateIndex = 102;

        /// <summary>
        ///     Converts a <see cref="PrintLog" /> instance to a <see cref="printLog" />
        /// </summary>
        /// <param name="this">The <see cref="PrintLog" /> instance to convert.</param>
        /// <returns>A <see cref="printLog" /> instance.</returns>
        public static printLog ToPrintLog(this PrintLog @this)
        {
            return new printLog
            {
                logSequence = @this.Id,
                deviceId = @this.PrinterId,
                transactionId = @this.TransactionId,
                printDateTime = @this.PrintDateTime,
                transactionClass = TemplateIndexToDeviceClass(@this.TemplateIndex),
                transactionDevice = TemplateIndexToDeviceId(@this.TemplateIndex),
                templateIndex = @this.TemplateIndex,
                transferState = @this.State,
                printComplete = @this.Complete,
                regionComplete = new regionComplete[0]
            };
        }

        private static string TemplateIndexToDeviceClass(int index)
        {
            return index == VoucherTemplateIndex ? @"G2S_voucher" : Constants.None;
        }

        private static int TemplateIndexToDeviceId(int index)
        {
            return index == VoucherTemplateIndex ? 1 : 0;
        }
    }
}