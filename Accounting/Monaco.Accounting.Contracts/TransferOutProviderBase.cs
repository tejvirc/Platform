namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Hardware.Contracts.Printer;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Base class for TransferOutProviders
    /// </summary>
    public abstract class TransferOutProviderBase
    {
        private const int RetryDelay = 500; // It's in milliseconds
        private const int RetryCount = 60; // This gives us a timeout of 30 seconds

        /// <summary> MaxSequence can be configured. It defines the max sequence number for generating VoucherSequence </summary>
        protected static long MaxSequence = 9999;

        /// <summary>
        ///     Logger
        /// </summary>
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Checks if the printer is available.
        /// </summary>
        protected static bool IsPrinterAvailable => ServiceManager.GetInstance().IsServiceAvailable<IPrinter>();

        /// <summary>
        ///     Gets the Printer
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected static async Task<IPrinter> GetPrinter(CancellationToken cancellationToken)
        {
            if (!IsPrinterAvailable)
            {
                Logger.Info("Printer has not been configured");
                return null;
            }

            var printer = ServiceManager.GetInstance().GetService<IPrinter>();
            if (IsReady(printer))
            {
                return printer;
            }

            Logger.Debug($"Printer is not ready - current state {printer.LogicalState}");

            for (var currentRetry = 0; currentRetry < RetryCount; currentRetry++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(RetryDelay), cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (IsReady(printer))
                {
                    return printer;
                }
            }

            return null;
        }

        /// <summary>
        ///     Converts the input sequence number to a number within the acceptable range.
        ///     Acceptable range is any decimal number up to 4 digits.
        /// </summary>
        /// <param name="ticketSequence">The unbound sequence number</param>
        /// <returns>A four digit number related to the sequence</returns>
        protected static int GetTicketSequence(long ticketSequence)
        {
            // Ticket sequence (1-9999)
            if (ticketSequence == 0)
            {
                return 0;
            }

            var result = ticketSequence % MaxSequence;
            return (int)(result == 0 ? MaxSequence : result);
        }

        private static bool IsReady(IPrinter printer)
        {
            return printer.CanPrint;
        }
    }
}