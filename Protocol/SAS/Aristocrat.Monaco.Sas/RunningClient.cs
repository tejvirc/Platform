namespace Aristocrat.Monaco.Sas
{
    using System;
    using Aristocrat.Sas.Client;
    using Exceptions;
    using HandPay;

    /// <summary>
    ///     The running client configuration
    /// </summary>
    public class RunningClient : IDisposable
    {
        private bool _disposed;

        /// <summary>
        ///     Creates the running client configuration
        /// </summary>
        /// <param name="client">The sas client to use</param>
        /// <param name="exceptionQueue">The exception queue to use</param>
        /// <param name="messageQueue">The message queue to use</param>
        /// <param name="handpayQueue">The handpay queue</param>
        public RunningClient(
            SasClient client,
            SasPriorityExceptionQueue exceptionQueue,
            ISasMessageQueue messageQueue,
            HandpayQueue handpayQueue)
        {
            Client = client;
            ExceptionQueue = exceptionQueue;
            MessageQueue = messageQueue;
            HandpayQueue = handpayQueue;
        }

        /// <summary>
        ///     Gets the sas client
        /// </summary>
        public SasClient Client { get; }

        /// <summary>
        ///     Gets the exception queue
        /// </summary>
        public SasPriorityExceptionQueue ExceptionQueue { get; }

        /// <summary>
        ///     Gets the message queue
        /// </summary>
        public ISasMessageQueue MessageQueue { get; }

        /// <summary>
        ///     Gets the handpay queue
        /// </summary>
        public HandpayQueue HandpayQueue { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose of resources
        /// </summary>
        /// <param name="disposing">True if disposing the first time</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Client?.Stop();
                Client?.Dispose();
                ExceptionQueue?.Dispose();
                HandpayQueue?.Dispose();
            }

            _disposed = true;
        }
    }
}