namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Base;
    using Storage.Models;
    using Storage.Repository;

    /// <summary>
    ///     The host cost cashout provider
    /// </summary>
    public class AftHostCashOutProvider : IAftHostCashOutProvider, IDisposable
    {
        private const double ExceptionTimeOut = 800.0;
        private const int HostCashoutTimeout = 8000;

        private readonly IStorageDataProvider<AftTransferOptions> _transferDataProvider;
        private readonly IBank _bank;
        private readonly IHardCashOutLock _hardCashOutLock;
        private readonly AutoResetEvent _hostCashOutResetEvent;
        private readonly SasExceptionTimer _sasExceptionTimer;

        private GeneralExceptionCode? _watCashoutException;
        private bool _transferAccepted;
        private bool _disposed;

        /// <summary>
        ///     Creates the AftHostCashOutProvider
        /// </summary>
        /// <param name="exceptionHandler">The exception handler</param>
        /// <param name="transferDataProvider"></param>
        /// <param name="bank">The bank</param>
        /// <param name="hardCashOutLock">The hard cashout lock</param>
        public AftHostCashOutProvider(
            ISasExceptionHandler exceptionHandler,
            IStorageDataProvider<AftTransferOptions> transferDataProvider,
            IBank bank,
            IHardCashOutLock hardCashOutLock)
        {
            if (exceptionHandler == null)
            {
                throw new ArgumentNullException(nameof(exceptionHandler));
            }

            _transferDataProvider = transferDataProvider ?? throw new ArgumentNullException(nameof(transferDataProvider));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _hardCashOutLock = hardCashOutLock ?? throw new ArgumentNullException(nameof(hardCashOutLock));
            _hostCashOutResetEvent = new AutoResetEvent(false);
            _sasExceptionTimer = new SasExceptionTimer(
                exceptionHandler,
                () => _watCashoutException,
                () => _watCashoutException.HasValue,
                ExceptionTimeOut);
        }

        /// <inheritdoc />
        public bool CanCashOut
        {
            get
            {
                var hostCashOutMode = CashOutMode;
                return hostCashOutMode != HostCashOutMode.None && _hardCashOutLock.CanCashOut;
            }
        }

        /// <inheritdoc />
        public bool CashOutWinPending => _watCashoutException == GeneralExceptionCode.AftRequestForHostToCashOutWin;

        /// <inheritdoc />
        public bool HostCashOutPending => _watCashoutException.HasValue;

        /// <inheritdoc />
        public HostCashOutMode CashOutMode
        {
            get
            {
                var transferFlags = _transferDataProvider.GetData().CurrentTransferFlags;
                if ((transferFlags & AftTransferFlags.HostCashOutEnableControl) != 0 &&
                    (transferFlags & AftTransferFlags.HostCashOutEnable) != 0)
                {
                    return (transferFlags & AftTransferFlags.HostCashOutMode) != 0
                        ? HostCashOutMode.Hard
                        : HostCashOutMode.Soft;
                }

                return HostCashOutMode.None;
            }
        }

        /// <inheritdoc />
        public WatTransaction CashOutTransaction { get; private set; }

        /// <inheritdoc />
        public bool LockedUp => (CashOutMode == HostCashOutMode.Hard) && _hardCashOutLock.Locked;

        /// <inheritdoc />
        public Task<bool> HandleHostCashOut(WatTransaction transaction)
        {
            if (CashOutMode == HostCashOutMode.None)
            {
                // We do not need to do anything if we are not in host cashout mode
                return Task.FromResult(false);
            }

            _transferAccepted = false;
            CashOutTransaction = transaction;
            _hostCashOutResetEvent.Reset();
            
            var aftBalance = _bank.QueryBalance();
            var transactionTotalAmount = transaction.NonCashAmount +
                                         transaction.CashableAmount +
                                         transaction.PromoAmount;
            _watCashoutException = (aftBalance > transactionTotalAmount)
                ? GeneralExceptionCode.AftRequestForHostToCashOutWin
                : GeneralExceptionCode.AftRequestForHostCashOut;

            _sasExceptionTimer.StartTimer();

            return WaitForHostCashOutResponse();
        }

        /// <inheritdoc />
        public void CashOutAccepted()
        {
            _transferAccepted = true;
            _hostCashOutResetEvent.Set();
        }

        /// <inheritdoc />
        public void CashOutDenied()
        {
            _transferAccepted = false;
            _hostCashOutResetEvent.Set();
        }

        /// <inheritdoc />
        public void ResetCashOutExceptionTimer()
        {
            if (!_watCashoutException.HasValue)
            {
                return;
            }
            
            _sasExceptionTimer.StartTimer(true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Handles the disposing of the unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether or not to dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _hostCashOutResetEvent.Dispose();
                _sasExceptionTimer.Dispose();
            }

            _disposed = true;
        }

        private Task<bool> WaitForHostCashOutResponse()
        {
            return Task.Run(
                () =>
                {
                    var requestAccepted = _hostCashOutResetEvent.WaitOne(HostCashoutTimeout) && _transferAccepted;

                    if ((CashOutMode == HostCashOutMode.Hard) && !requestAccepted)
                    {
                        _hostCashOutResetEvent.Reset();
                        _hardCashOutLock.PresentLockup();

                        // We need to lock up and not proceed any further until we either get a key off of the host takes the money
                        _hostCashOutResetEvent.WaitOne();
                        _hardCashOutLock.RemoveLockupPresentation();
                        requestAccepted = _transferAccepted;
                    }

                    CashOutTransaction = null;
                    _watCashoutException = null;
                    _sasExceptionTimer.StopTimer();
                    return requestAccepted;
                });
        }
    }
}