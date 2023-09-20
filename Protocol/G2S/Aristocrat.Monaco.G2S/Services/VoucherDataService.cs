namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Data.Model;
    using Data.Voucher;
    using Handlers;
    using Handlers.Voucher;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Meters;
    using Monaco.Common.Storage;
    using VoucherExtensions = Handlers.Voucher.VoucherExtensions;

    /// <summary>
    ///     An <see cref="IVoucherDataService" /> implementation
    /// </summary>
    public class VoucherDataService : IVoucherDataService, IService, IDisposable
    {
        private const int DefaultRetryInterval = 30000;
        private const int VoucherExpirationOverlapInSeconds = 2;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;

        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly ICommandBuilder<IVoucherDevice, voucherStatus> _statusCommandBuilder;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IVoucherDataRepository _voucherData;
        private readonly object _voucherDataLock = new();
        private readonly IMeterAggregator<IVoucherDevice> _voucherMeters;
        private readonly ITransactionReferenceProvider _references;

        private readonly object _voucherUpdateLock = new();

        private bool _commsOfflineDisable;
        private DateTime _nextVoucherRefresh = DateTime.MinValue;

        private bool _disposed;
        private CancellationTokenSource _refreshCancelSource;
        private bool _retrievingVoucherIdsDisable;
        private CancellationTokenSource _voucherDataCancellationToken;
        private bool _voucherStateDisable;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherDataService" /> class.
        /// </summary>
        public VoucherDataService(
            IG2SEgm egm,
            IVoucherDataRepository voucherData,
            IMonacoContextFactory contextFactory,
            IEventLift eventLift,
            ITransactionHistory transactionHistory,
            ITransactionReferenceProvider references,
            IEventBus eventBus,
            ICommandBuilder<IVoucherDevice, voucherStatus> statusCommandBuilder,
            IMeterAggregator<IVoucherDevice> voucherMeters,
            IMeterAggregator<ICabinetDevice> cabinetMeters)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _voucherData = voucherData ?? throw new ArgumentNullException(nameof(voucherData));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _references = references ?? throw new ArgumentNullException(nameof(references));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _statusCommandBuilder =
                statusCommandBuilder ?? throw new ArgumentNullException(nameof(statusCommandBuilder));
            _voucherMeters = voucherMeters ?? throw new ArgumentNullException(nameof(voucherMeters));
            _cabinetMeters = cabinetMeters ?? throw new ArgumentNullException(nameof(cabinetMeters));

            _eventBus.Subscribe<DeviceConfigurationChangedEvent>(
                this,
                evt =>
                {
                    if (evt.Device is IVoucherDevice device)
                    {
                        SetVoucherIdListRefresh(device);
                    }
                });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IVoucherDataService) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing the G2S VoucherDataService.");
        }

        /// <inheritdoc />
        public bool HostOnline { get; private set; }

        /// <inheritdoc />
        public void Start()
        {
            Logger.Info("Initializing the G2S voucher data service.");

            var device = _egm.GetDevice<IVoucherDevice>();
            if (device == null)
            {
                Logger.Error("Voucher device is not available");
                return;
            }

            SetState();
        }

        /// <inheritdoc />
        public void CommunicationsStateChanged(int hostId, bool online)
        {
            var device = _egm.GetDevice<IVoucherDevice>();

            if (hostId != device?.Owner)
            {
                return;
            }

            HostOnline = online;

            if (HostOnline)
            {
                if (_nextVoucherRefresh < DateTime.UtcNow)
                {
                    FreeCancellationTokens();

                    Logger.Debug("Comms is online. Will update voucher data.");

                    SetVoucherIdListRefresh(device);
                }

                Logger.Debug("Retrying pending commands");

                AttachTaskExceptionHandler(Task.Run(() => SendPendingIssueVoucherCommands()));
                AttachTaskExceptionHandler(Task.Run(() => SendPendingCommitVoucherCommands()));
            }
            else
            {
                FreeCancellationTokens();
            }

            if (!device.PrintOffLine)
            {
                if (HostOnline)
                {
                    EnableVoucherDevice(DeviceDisableReason.CommsOffline);
                }
                else
                {
                    DisableVoucherDevice(DeviceDisableReason.CommsOffline);
                }
            }
        }

        /// <inheritdoc />
        public int VoucherIdAvailable()
        {
            using var context = _contextFactory.CreateDbContext();
            return _voucherData.Count(context);
        }

        /// <inheritdoc />
        public (authorizeVoucher, voucherLog) SendRedeemVoucher(
            long transactionId,
            string validationId,
            long logSequence,
            long amount)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            if (!(bool)propertiesManager.GetProperty(PropertyKey.VoucherIn, false))
            {
                return (null, null);
            }

            var device = _egm.GetDevice<IVoucherDevice>();

            var voucher = new redeemVoucher
            {
                transactionId = transactionId,
                /*TODO: idReaderType, idNumber, playerId*/
                idReaderType = "G2S_none",
                validationId = validationId
            };

            var currentVoucherLog = VoucherExtensions.GetVoucherLog(
                true,
                logSequence,
                device?.Id ?? 0,
                voucher.transactionId,
                voucher.idReaderType,
                voucher.idNumber,
                voucher.playerId,
                voucher.validationId,
                amount);

            var result = device?.SendRedeemVoucher(voucher, currentVoucherLog).Result;

            if (result == null || result.voucherAmt == 0)
            {
                var commitVoucher = VoucherExtensions.GetCommitVoucherRejected(transactionId, validationId, result);

                device?.SendCommitVoucher(commitVoucher, currentVoucherLog, a => { }, a => null);
            }

            return (result, currentVoucherLog);
        }

        /// <inheritdoc />
        public void SendCommitVoucher(VoucherInTransaction voucherInTransaction)
        {
            SendCommitVoucher(voucherInTransaction, null);
        }

        /// <inheritdoc />
        public VoucherData GetVoucherData()
        {
            VoucherData result = null;

            var device = _egm.GetDevice<IVoucherDevice>();

            if (device == null)
            {
                return null;
            }

            lock (_voucherDataLock)
            {
                using var context = _contextFactory.CreateDbContext();
                var count = _voucherData.Count(context);
                if (count > 0)
                {
                    var data = _voucherData.GetAll(context).ToList();

                    if (data.Count > 0)
                    {
                        if (data.Count == 1)
                        {
                            DisableVoucherDevice(DeviceDisableReason.RetrievingVoucherIds);
                        }

                        result = data[0];
                        _voucherData.Delete(context, result);
                    }
                }
            }

            AttachTaskExceptionHandler(Task.Run(() => CheckForVoucherUpdate()));

            return result;
        }

        /// <inheritdoc />
        public VoucherData ReadVoucherData()
        {
            using var context = _contextFactory.CreateDbContext();
            return GetLastVoucher(context);
        }

        /// <inheritdoc />
        public void VoucherStateChanged()
        {
            var device = _egm.GetDevice<IVoucherDevice>();
            if (!device.HostEnabled && !device.Enabled)
            {
                return;
            }

            if (device.HostEnabled)
            {
                EnableVoucherDevice(DeviceDisableReason.VoucherState);
                AttachTaskExceptionHandler(
                    Task.Run(
                        () =>
                        {
                            if (_nextVoucherRefresh >= DateTime.UtcNow)
                            {
                                return;
                            }

                            FreeCancellationTokens();
                            CheckForVoucherUpdate();
                            SetVoucherIdListRefresh(device);
                        }));
            }
            else
            {
                FreeCancellationTokens();
                DeleteAllValidationIds(device);
                DisableVoucherDevice(DeviceDisableReason.VoucherState);
            }
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                if (_refreshCancelSource != null)
                {
                    _refreshCancelSource.Cancel(false);
                    _refreshCancelSource.Dispose();
                }

                if (_voucherDataCancellationToken != null)
                {
                    _voucherDataCancellationToken.Cancel(false);
                    _voucherDataCancellationToken.Dispose();
                }
            }

            _refreshCancelSource = null;
            _voucherDataCancellationToken = null;
            _disposed = true;
        }

        private void SendCommitVoucher(VoucherInTransaction voucherInTransaction, ICollection<VoucherInTransaction> pendingTransactions)
        {
            if (voucherInTransaction == null)
            {
                return;
            }

            var device = _egm.GetDevice<IVoucherDevice>();

            if (device == null)
            {
                return;
            }

            var voucher = VoucherExtensions.GetCommitVoucher(voucherInTransaction);

            var currentVoucherLog = VoucherExtensions.GetVoucherLog(voucherInTransaction, device.PrefixedDeviceClass());

            device.SendCommitVoucher(
                voucher,
                currentVoucherLog,
                command =>
                {
                    var trans = VoucherExtensions.FindTransaction<VoucherInTransaction>(_transactionHistory, command.transactionId);
                    if (trans == null)
                    {
                        return;
                    }

                    Logger.Debug($"Acknowledged pending issued voucher - {command.transactionId}");
                    trans.CommitAcknowledged = true;
                    _transactionHistory.UpdateTransaction(trans);
                    pendingTransactions?.Remove(voucherInTransaction);
                },
                GetMeterList);
        }

        private static void AttachTaskExceptionHandler(Task task)
        {
            task.ContinueWith(
                t =>
                {
                    if (t.Exception != null)
                    {
                        var aggException = t.Exception.Flatten();
                        foreach (var ex in aggException.InnerExceptions)
                        {
                            Logger.Error(ex.Message, ex);
                        }
                    }
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task GetValidationData(
            IVoucherDevice device,
            int currentValidVoucherCount,
            bool expired,
            long listId,
            bool timerExpired = false)
        {
            if (!HostOnline)
            {
                _voucherDataCancellationToken?.Cancel();
                _voucherDataCancellationToken?.Dispose();
                _voucherDataCancellationToken = null;
            }

            var vouchersToRequest = device.MaxValueIds - currentValidVoucherCount;
            if (vouchersToRequest > 0 || timerExpired)
            {
                Logger.Debug($"Attempting to get validation data.  Requesting {vouchersToRequest} vouchers.");

                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                var success = (bool)propertiesManager.GetProperty(AccountingConstants.VoucherOut, false);
                if (success)
                {
                    success = device.GetValidationData(vouchersToRequest, expired, listId, UpdateVoucherData);
                }

                Logger.Debug($"GetValidationData: {success}");
                if (!success)
                {
                    _voucherDataCancellationToken ??= new CancellationTokenSource();

                    AttachTaskExceptionHandler(
                        await Task.Delay(DefaultRetryInterval, _voucherDataCancellationToken.Token)
                            .ContinueWith(
                                async task =>
                                {
                                    if (!task.IsCanceled)
                                    {
                                        await GetValidationData(device, currentValidVoucherCount, expired, listId);
                                    }
                                }));
                }
                else
                {
                    _voucherDataCancellationToken?.Cancel();
                    _voucherDataCancellationToken?.Dispose();
                    _voucherDataCancellationToken = null;
                }
            }
        }

        private void SetVoucherIdListRefresh(IVoucherDevice device)
        {
            _refreshCancelSource?.Cancel();
            _refreshCancelSource?.Dispose();
            _refreshCancelSource = null;

            if (!device.HostEnabled)
            {
                return;
            }

            _refreshCancelSource = new CancellationTokenSource();

            SetVoucherDataRefresh(_refreshCancelSource);
        }

        private void SetVoucherDataRefresh(CancellationTokenSource refreshCancelSource)
        {
            var device = _egm.GetDevice<IVoucherDevice>();
            if (!device.HostEnabled)
            {
                return;
            }

            TimeSpan? time = null;
            var now = DateTime.UtcNow;

            using (var context = _contextFactory.CreateDbContext())
            {
                var lastData = GetLastVoucher(context);
                if (lastData == null)
                {
                    //we have no vouchers--refresh immediately
                    Logger.Debug("We have no vouchers--refresh immediately");
                    time = TimeSpan.FromSeconds(1);
                }
                else
                {
                    var refreshTime = lastData.ListTime + TimeSpan.FromMilliseconds(device.ValueIdListRefresh);
                    if (refreshTime > now)
                    {
                        Logger.Debug($"Refresh based on ValueIdListRefresh");
                        time = refreshTime - now;
                    }
                }

                //if we are going to refresh immediately, don't check for expired vouchers
                if (!time.HasValue || time.Value.TotalSeconds > 1)
                {
                    var (expiredVouchers, nextVoucherExpiration) = GetVoucherExpirationData(device, context);
                    if (expiredVouchers > 0)
                    {
                        //we have expired vouchers.  Refresh immediately
                        Logger.Debug("We have expired vouchers--refresh immediately");
                        time = TimeSpan.FromSeconds(1);
                    }
                    else
                    {
                        if (nextVoucherExpiration.HasValue)
                        {
                            var expirationTime = nextVoucherExpiration - now;
                            if (!time.HasValue || expirationTime < time)
                            {
                                Logger.Debug("Refresh based on the next voucher expiration time");
                                time = expirationTime;
                            }
                        }
                    }
                }
            }

            if (!time.HasValue)
            {
                //this will only happen if we are past the refresh time but all vouchers are expired.
                Logger.Debug("Refresh time not set--set to 1 second");
                time = TimeSpan.FromSeconds(1);
            }

            SetVoucherDataRefresh(time.Value, refreshCancelSource);
        }

        private void SetVoucherDataRefresh(TimeSpan time, CancellationTokenSource refreshCancelSource)
        {
            var device = _egm.GetDevice<IVoucherDevice>();
            if (!device.HostEnabled)
            {
                return;
            }

            if (time > TimeSpan.FromMilliseconds(device.ValueIdListRefresh) || time < TimeSpan.FromMilliseconds(0))
            {
                Logger.Error($"Voucher data refresh time is invalid {time}");
                time = TimeSpan.FromMilliseconds(1000);
            }

            if (refreshCancelSource == null || refreshCancelSource.IsCancellationRequested)
            {
                Logger.Debug("Bailing out of Voucher Refresh Loop due to null cancel token or cancel requested");
                return;
            }

            Logger.Debug($"Refreshing voucher data in {time}");
            _nextVoucherRefresh = DateTime.UtcNow + time;
            AttachTaskExceptionHandler(
                Task.Delay(time, refreshCancelSource.Token)
                    .ContinueWith(
                        task =>
                        {
                            if (!task.IsCanceled)
                            {
                                CheckForVoucherUpdate(true);
                                SetVoucherDataRefresh(refreshCancelSource);
                            }
                        }));
        }

        private void FreeCancellationTokens()
        {
            _refreshCancelSource?.Cancel();
            _refreshCancelSource?.Dispose();
            _refreshCancelSource = null;

            _voucherDataCancellationToken?.Cancel(false);
            _voucherDataCancellationToken?.Dispose();
            _voucherDataCancellationToken = null;

            _nextVoucherRefresh = DateTime.MinValue;
        }

        private void SendPendingIssueVoucherCommands()
        {
            var device = _egm.GetDevice<IVoucherDevice>();

            if (device == null)
            {
                return;
            }

            var pendingIssuedVouchers = _transactionHistory.RecallTransactions<VoucherOutTransaction>()
                .Where(a => a.HostAcknowledged == false).ToList();

            while (HostOnline && pendingIssuedVouchers.Count > 0)
            {
                foreach (var transaction in pendingIssuedVouchers.ToArray())
                {
                    var issueVoucher = VoucherExtensions.GetIssueVoucher(transaction, _references);

                    Logger.Debug($"Retrying unacknowledged redeemed voucher - {transaction.TransactionId}");

                    device.SendIssueVoucher(
                        issueVoucher,
                        transactionId =>
                        {
                            var trans = VoucherExtensions.FindTransaction<VoucherOutTransaction>(_transactionHistory, transactionId);
                            return trans != null && trans.HostAcknowledged;
                        },
                        result =>
                        {
                            var trans = VoucherExtensions.FindTransaction<VoucherOutTransaction>(_transactionHistory, result);
                            if (trans == null)
                            {
                                return;
                            }

                            Logger.Debug($"Acknowledged redeemed voucher - {transaction.TransactionId}");

                            trans.HostAcknowledged = true;
                            _transactionHistory.UpdateTransaction(trans);

                            _eventLift.Report(
                                device,
                                EventCode.G2S_VCE105,
                                transaction.TransactionId,
                                device.TransactionList(trans.ToLog(_references)));

                            pendingIssuedVouchers.Remove(transaction);
                        });
                }
            }
        }

        private void SendPendingCommitVoucherCommands()
        {
            var device = _egm.GetDevice<IVoucherDevice>();

            if (device == null)
            {
                return;
            }

            var pendingCommitVouchers = _transactionHistory.RecallTransactions<VoucherInTransaction>()
                .Where(a => a.CommitAcknowledged == false).ToList();

            while (HostOnline && pendingCommitVouchers.Count > 0)
            {
                foreach (var transaction in pendingCommitVouchers.ToArray())
                {
                    SendCommitVoucher(transaction, pendingCommitVouchers);
                }
            }
        }

        private void CheckForVoucherUpdate(bool timerExpired = false)
        {
            lock (_voucherUpdateLock)
            {
                var device = _egm.GetDevice<IVoucherDevice>();
                if (device == null)
                {
                    Logger.Debug("Voucher device is null.  It's likely the G2S layer is still being constructed.");
                    return;
                }

                if (!device.HostEnabled)
                {
                    Logger.Debug("Voucher device host disabled cannot request validation Ids");
                    return;
                }

                Logger.Debug("Updating voucher data");

                lock (_voucherDataLock)
                {
                    using var context = _contextFactory.CreateDbContext();
                    var data = _voucherData.GetAll(context).ToList();

                    var expiredData = GetExpiredVouchers(data, device);

                    var voucherCount = _voucherData.Count(context);
                    Logger.Debug(
                        $"Voucher Count: {voucherCount} Expired Count: {expiredData.Count} Min Voucher Level: {device.MinLevelValueIds}");
                    var currentValidVoucherCount = voucherCount - expiredData.Count;
                    if (currentValidVoucherCount <= device.MinLevelValueIds || timerExpired)
                    {
                        RequestVoucherData(currentValidVoucherCount > 0 ? currentValidVoucherCount : 0, timerExpired);
                    }

                    if (expiredData.Count > 0)
                    {
                        Logger.Debug($"Deleting {expiredData.Count} Expired Vouchers");
                        DeleteVoucherData(expiredData, device);
                    }
                }
            }
        }

        private static IReadOnlyCollection<VoucherData> GetExpiredVouchers(
            List<VoucherData> voucherData,
            IVoucherDevice device,
            DateTime? checkTime = null)
        {
            if (voucherData == null)
            {
                Logger.Debug($"{nameof(voucherData)} is null");
                return Array.Empty<VoucherData>();
            }

            if (device == null)
            {
                Logger.Debug($"{nameof(device)} is null");
                return Array.Empty<VoucherData>();
            }

            checkTime ??= DateTime.UtcNow;
            return voucherData.Where(v => v.ListTime.AddMilliseconds(device.ValueIdListLife) <= checkTime).ToList();
        }

        // int is # of currently expired vouchers.
        // First DateTime? is time of next voucher to expire, adjusted for overlap.
        private (int expiredCount, DateTime? nextVoucherExpiration) GetVoucherExpirationData(
            IVoucherDevice device,
            DbContext context)
        {
            if (context == null || device == null)
            {
                return (0, null);
            }

            DateTime? nextVoucherExpiration = null;

            var voucherData = _voucherData.GetAll(context).ToList();
            var expiredVouchers = GetExpiredVouchers(voucherData, device);
            // exclude currently expired vouchers
            voucherData = voucherData
                .Where(o => o.ListTime.AddMilliseconds(device.ValueIdListLife) > DateTime.UtcNow)
                .OrderBy(o => o.ListTime).ToList();

            if (voucherData.Any())
            {
                nextVoucherExpiration = voucherData.First().ListTime.AddMilliseconds(device.ValueIdListLife).UtcDateTime;
                var overlapExpiredVouchers = GetExpiredVouchers(
                    voucherData,
                    device,
                    nextVoucherExpiration.Value.AddSeconds(VoucherExpirationOverlapInSeconds)).ToList();
                if (overlapExpiredVouchers.Any())
                {
                    //adjust the next voucher expiration by the overlap to batch these calls.
                    nextVoucherExpiration =
                        overlapExpiredVouchers.Last().ListTime.AddMilliseconds(device.ValueIdListLife).UtcDateTime;
                }
            }

            Logger.Debug(
                $"Time: {DateTime.UtcNow} Expired vouchers: {expiredVouchers.Count}  Next voucher expiration: {nextVoucherExpiration}");
            return (expiredVouchers.Count, nextVoucherExpiration);
        }

        private meterList GetMeterList(IVoucherDevice voucherDevice)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            var cabinetMeters = _cabinetMeters.GetMeters(
                cabinet,
                CabinetMeterName.PlayerCashableAmount,
                CabinetMeterName.PlayerPromoAmount,
                CabinetMeterName.PlayerNonCashableAmount);

            // TODO: Add SystemIn meters
            var voucherMeters = _voucherMeters.GetMeters(
                voucherDevice,
                VoucherMeterName.CashableInAmount,
                VoucherMeterName.CashableInCount,
                VoucherMeterName.PromoInAmount,
                VoucherMeterName.PromoInCount,
                VoucherMeterName.NonCashableInAmount,
                VoucherMeterName.NonCashableInCount);

            return new meterList { meterInfo = cabinetMeters.Concat(voucherMeters).ToArray() };
        }

        private void DeleteVoucherData(IReadOnlyCollection<VoucherData> toDeleteData, IVoucherDevice device)
        {
            if (toDeleteData == null)
            {
                Logger.Debug(nameof(toDeleteData) + " is null");
                return;
            }

            if (device == null)
            {
                Logger.Debug(nameof(device) + " is null");
                return;
            }

            lock (_voucherDataLock)
            {
                using var context = _contextFactory.CreateDbContext();
                foreach (var del in toDeleteData)
                {
                    _voucherData.DeleteAll(context, a => a.ValidationId == del.ValidationId);
                }

                if (_voucherData.Count(context) == 0)
                {
                    DisableVoucherDevice(DeviceDisableReason.RetrievingVoucherIds);
                }
            }

            if (toDeleteData.Count > 0)
            {
                var status = new voucherStatus();
                _statusCommandBuilder.Build(device, status);

                _eventLift.Report(device, EventCode.G2S_VCE101, device.DeviceList(status));
            }
        }

        private void DeleteAllValidationIds(IVoucherDevice device)
        {
            using var context = _contextFactory.CreateDbContext();
            var voucherData = _voucherData.GetAll(context).ToList();

            DeleteVoucherData(voucherData, device);
        }

        private void RequestVoucherData(int currentValidVoucherCount, bool timerExpired)
        {
            VoucherData lastVoucherData = null;
            int count;

            Logger.Debug("Requesting voucher data");

            using (var context = _contextFactory.CreateDbContext())
            {
                count = _voucherData.Count(context);

                if (count > 0)
                {
                    lastVoucherData = GetLastVoucher(context);
                }
            }

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var allowVoucherIssue = (bool)propertiesManager.GetProperty(AccountingConstants.VoucherOut, false);

            var device = _egm.GetDevice<IVoucherDevice>();
            if (allowVoucherIssue && count == 0)
            {
                DisableVoucherDevice(DeviceDisableReason.RetrievingVoucherIds);
            }

            var expired = true;
            long listId = 0;
            if (lastVoucherData != null)
            {
                expired = lastVoucherData.ListTime.AddMilliseconds(device.ValueIdListLife) <= DateTime.UtcNow;
                listId = lastVoucherData.ListId;
            }

            AttachTaskExceptionHandler(GetValidationData(device, currentValidVoucherCount, expired, listId, timerExpired));
        }

        private void UpdateVoucherData(validationData voucherData, IVoucherDevice device)
        {
            if (voucherData.validationIdItem == null)
            {
                return;
            }

            Logger.Debug("Updating Voucher Data.");

            _voucherDataCancellationToken?.Cancel();
            _voucherDataCancellationToken?.Dispose();
            _voucherDataCancellationToken = null;

            lock (_voucherDataLock)
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    List<VoucherData> vouchersToDelete = null;
                    if (voucherData.deleteCurrent)
                    {
                        Logger.Debug("Deleting all vouchers due to deleteCurrent == true");
                        vouchersToDelete = _voucherData.GetAll(context).ToList();
                    }

                    Logger.Debug($"Adding {voucherData.validationIdItem.Length} vouchers");

                    foreach (var data in voucherData.validationIdItem)
                    {
                        var voucher = new VoucherData
                        {
                            ListTime = DateTime.UtcNow,
                            ListId = voucherData.validationListId,
                            ValidationId = data.validationId,
                            ValidationSeed = data.validationSeed
                        };

                        _voucherData.Add(context, voucher);
                    }

                    if (vouchersToDelete is { Count: > 0 })
                    {
                        DeleteVoucherData(vouchersToDelete, device);
                    }

                    Logger.Debug($"Voucher Count After Update: {_voucherData.Count(context)}");
                }

                var status = new voucherStatus();
                _statusCommandBuilder.Build(device, status);

                _eventLift.Report(device, EventCode.G2S_VCE102, device.DeviceList(status));

                if (voucherData.validationIdItem.Length > 0)
                {
                    EnableVoucherDevice(DeviceDisableReason.RetrievingVoucherIds);
                }
            }
        }

        private VoucherData GetLastVoucher(DbContext context)
        {
            return _voucherData.GetAll(context).OrderByDescending(v => v.Id).FirstOrDefault();
        }

        private void DisableVoucherDevice(DeviceDisableReason reason)
        {
            switch (reason)
            {
                case DeviceDisableReason.CommsOffline:
                    _commsOfflineDisable = true;
                    break;
                case DeviceDisableReason.RetrievingVoucherIds:
                    _retrievingVoucherIdsDisable = true;
                    break;
                case DeviceDisableReason.VoucherState:
                    _voucherStateDisable = true;
                    break;
            }

            var device = _egm.GetDevice<IVoucherDevice>();
            if (device.Enabled && reason != DeviceDisableReason.VoucherState)
            {
                Logger.Debug($"Voucher service is disabling the voucher device: {reason}");
                device.Enabled = false;

                var status = new voucherStatus();
                _statusCommandBuilder.Build(device, status);
                _eventLift.Report(device, EventCode.G2S_VCE001, device.DeviceList(status));
            }
        }

        private void EnableVoucherDevice(DeviceDisableReason reason)
        {
            switch (reason)
            {
                case DeviceDisableReason.CommsOffline:
                    _commsOfflineDisable = false;
                    break;
                case DeviceDisableReason.RetrievingVoucherIds:
                    _retrievingVoucherIdsDisable = false;
                    break;
                case DeviceDisableReason.VoucherState:
                    _voucherStateDisable = false;
                    break;
            }

            var device = _egm.GetDevice<IVoucherDevice>();
            if (!device.Enabled && reason != DeviceDisableReason.VoucherState)
            {
                Logger.Debug($"Voucher service is enabling the voucher device: {reason}");

                device.Enabled = true;
                if (!_commsOfflineDisable && !_retrievingVoucherIdsDisable && !_voucherStateDisable)
                {
                    var status = new voucherStatus();
                    _statusCommandBuilder.Build(device, status);

                    _eventLift.Report(device, EventCode.G2S_VCE002, device.DeviceList(status));
                }
            }
        }

        private void SetState()
        {
            using var context = _contextFactory.CreateDbContext();
            if (_voucherData.Count(context) > 0)
            {
                return;
            }

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var allowVoucherIssue = (bool)propertiesManager.GetProperty(AccountingConstants.VoucherOut, false);

            if (allowVoucherIssue)
            {
                DisableVoucherDevice(DeviceDisableReason.RetrievingVoucherIds);
            }
        }

        private enum DeviceDisableReason
        {
            CommsOffline,
            RetrievingVoucherIds,
            VoucherState
        }
    }
}
