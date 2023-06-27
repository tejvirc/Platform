namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Diagnostics;
    using Protocol.v21;

    /// <summary>
    ///     The voucher class is used to manage the process of issuing and redeeming payment vouchers(sometimes
    ///     referred to as tickets or coupons) at an EGM.Vouchers can be cashable, promotional, or nonCashable.The
    ///     voucher class includes commands and events to issue and redeem vouchers, and to set validation identifiers.
    ///     The class also includes commands to retrieve the voucher history log maintained by the EGM.
    ///     When a player cashes out at an EGM and the EGM is configured for printing payment vouchers, the EGM
    ///     may issue vouchers in lieu of coin or other payment. A voucher may only contain one type of credit.Thus, if
    ///     multiple types of credits (cashable, promotional, nonCashable) are on the EGM when the player cashes out,
    ///     the EGM may have to produce multiple vouchers.Vouchers issued by one EGM may be redeemed at another
    ///     EGM. If the credits are cashable or promotional, vouchers can be redeemed at a cashier’s station or at a
    ///     self-service
    ///     kiosk.
    ///     The voucher class is a multiple-device class. Multiple voucher devices may be active in an EGM.Class-level
    ///     meters and logs MUST include activity related to both active and inactive devices. Inactive devices may be
    ///     exposed through the commConfig class.
    ///     • If an EGM exposes multiple active voucher devices, the EGM MUST support the v2.1 namespace
    ///     extension to v1.0.3 and provide functional support for the cashOutToVoucher and redeemPrefix
    ///     attributes.
    ///     The voucher class supports both issuing and redeeming vouchers.The allowVoucherIssue profile attribute
    ///     controls functionality related to validation data for a voucher device; it does not(directly) control voucher
    ///     issuance.However, without validation data, a voucher device will be unable to issue cash-out vouchers and
    ///     other types of generic vouchers.If allowVoucherIssue is set to false for a device:
    ///     • The EGM MUST NOT place any getValidationData commands for the voucher device in the
    ///     outbound queue.
    ///     • The EGM MUST NOT generate the G2S_VCE101 Validation ID Data Expired event for the
    ///     voucher device.
    ///     • The EGM MUST NOT disable the voucher device due to not having validation ID data.
    ///     The allowVoucherRedeem profile attribute controls whether a voucher device MAY be used for voucher
    ///     redemption. If allowVoucherRedeem is set to false, the EGM MUST NOT generate the redeemVoucher
    ///     command for the voucher device.
    ///     The voucher class has been extended to support multiple active voucher devices.
    ///     A new attribute, cashOutToVoucher, was added to the voucherProfile command to provide additional
    ///     control for voucher issuance.
    ///     • For backwards compatibility, a default value of true is recommended.
    ///     A new attribute, redeemPrefix, was added to the voucherProfile command to provide additional control for
    ///     voucher redemption.
    ///     • For backwards compatibility, an empty string as the default value is recommended.
    /// </summary>
    public class VoucherDevice : ClientDeviceBase<voucher>, IVoucherDevice
    {
        /// <summary>
        ///     The ID reader to use default value.
        /// </summary>
        public const int DefaultIdReaderId = 0;

        /// <summary>
        ///     The combine cashable credit types default value.
        /// </summary>
        public const bool DefaultCombineCashableOut = false;

        /// <summary>
        ///     The allow non-cashable out default value.
        /// </summary>
        public const bool DefaultAllowNonCashOut = true;

        /// <summary>
        ///     The maximum validation ids default value.
        /// </summary>
        public const int DefaultMaxValIds = 20;

        /// <summary>
        ///     The minimum level for validation ids default value.
        /// </summary>
        public const int DefaultMinLevelValIds = 15;

        /// <summary>
        ///     The validation ID refresh time default value.
        /// </summary>
        public const int DefaultValIdListRefresh = 43200000;

        /// <summary>
        ///     The validation ID list life default value.
        /// </summary>
        public const int DefaultValIdListLife = 86400000;

        /// <summary>
        ///     The maximum voucher hold time default value.
        /// </summary>
        public const int DefaultVoucherHoldTime = 15000;

        /// <summary>
        ///     The print offline default value.
        /// </summary>
        public const bool DefaultPrintOffLine = true;

        /// <summary>
        ///     The expire days cash promo default value.
        /// </summary>
        public const int DefaultExpireCashPromo = 30;

        /// <summary>
        ///     The print cash promo expiration default value.
        /// </summary>
        public const bool DefaultPrintExpCashPromo = true;

        /// <summary>
        ///     The expire non-cashable default value.
        /// </summary>
        public const int DefaultExpireNonCash = 30;

        /// <summary>
        ///     The Print Non-Cashable Expiration default value.
        /// </summary>
        public const bool DefaultPrintExpNonCash = true;

        /// <summary>
        ///     The allow validation data parameter name.
        /// </summary>
        public const bool DefaultAllowVoucherIssue = true;

        /// <summary>
        ///     The voucher redemption allowed parameter name.
        /// </summary>
        public const bool DefaultAllowVoucherRedeem = true;

        /// <summary>
        ///     The maximum online voucher parameter name.
        /// </summary>
        public const long DefaultMaxOnLinePayOut = 0;

        /// <summary>
        ///     The maximum offline voucher parameter name.
        /// </summary>
        public const long DefaultMaxOffLinePayOut = 0;

        /// <summary>
        ///     The print non-cashable vouchers when offline parameter name.
        /// </summary>
        public const bool DefaultPrintNonCashOffLine = true;

        /// <summary>
        ///     The cash out to voucher parameter name.
        /// </summary>
        public const bool DefaultCashOutToVoucher = true;

        private const bool DefaultRestartStatus = true;

        private readonly IEventLift _eventLift;

        private bool _closed;

        private bool _disposed;

        private CancellationTokenSource _sendCommitVoucherCancellationToken;
        private CancellationTokenSource _sendIssueVoucherCancellationToken;
        private CancellationTokenSource _sendRedeemVoucherCancellationToken;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An IDeviceStateObserver instance.</param>
        /// <param name="eventLift">An IEventLift instance.</param>
        public VoucherDevice(IDeviceObserver deviceStateObserver, IEventLift eventLift)
            : base(1, deviceStateObserver)
        {
            _eventLift = eventLift;
            SetDefaults();
        }

        /// <inheritdoc />
        public int TimeToLive { get; protected set; }

        /// <inheritdoc />
        public int IdReaderId { get; protected set; }

        /// <inheritdoc />
        public bool CombineCashableOut { get; protected set; }

        /// <inheritdoc />
        public int MaxValueIds { get; protected set; }

        /// <inheritdoc />
        public int MinLevelValueIds { get; protected set; }

        /// <inheritdoc />
        public int ValueIdListRefresh { get; protected set; }

        /// <inheritdoc />
        public int ValueIdListLife { get; set; }

        /// <inheritdoc />
        public int VoucherHoldTime { get; protected set; }

        /// <inheritdoc />
        public bool PrintOffLine { get; protected set; }

        /// <inheritdoc />
        public int ExpireCashPromo { get; protected set; }

        /// <inheritdoc />
        public bool PrintExpirationCashPromo { get; protected set; }

        /// <inheritdoc />
        public int ExpireNonCash { get; protected set; }

        /// <inheritdoc />
        public bool PrintExpirationNonCash { get; protected set; }

        /// <inheritdoc />
        public long MaxOnLinePayOut { get; protected set; }

        /// <inheritdoc />
        public long MaxOffLinePayOut { get; protected set; }

        /// <inheritdoc />
        public bool PrintNonCashOffLine { get; protected set; }

        /// <inheritdoc />
        public bool CashOutToVoucher { get; protected set; }

        /// <inheritdoc />
        public int MinLogEntries { get; protected set; }

        /// <inheritdoc />
        public bool RestartStatus { get; protected set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
            _closed = false;
        }

        /// <inheritdoc />
        public override void Close()
        {
            _closed = true;
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.RestartStatusParameterName,
                optionConfigValues,
                parameterId => { RestartStatus = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.RequiredForPlayParameterName,
                optionConfigValues,
                parameterId => { RequiredForPlay = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.TimeToLiveParameterName,
                optionConfigValues,
                parameterId => { TimeToLive = optionConfigValues.Int32Value(parameterId); });

            // Voucher Options
            SetDeviceValue(
                G2SParametersNames.VoucherDevice.IdReaderIdParameterName,
                optionConfigValues,
                parameterId => { IdReaderId = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.CombineCashableOutParameterName,
                optionConfigValues,
                parameterId => { CombineCashableOut = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.MaxValidationIdsParameterName,
                optionConfigValues,
                parameterId => { MaxValueIds = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.MinLevelValidationIdsParameterName,
                optionConfigValues,
                parameterId => { MinLevelValueIds = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.ValidationIdListRefreshParameterName,
                optionConfigValues,
                parameterId => { ValueIdListRefresh = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.ValidationIdListLifeParameterName,
                optionConfigValues,
                parameterId => { ValueIdListLife = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.VoucherHoldTimeParameterName,
                optionConfigValues,
                parameterId => { VoucherHoldTime = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.PrintOfflineParameterName,
                optionConfigValues,
                parameterId => { PrintOffLine = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.ExpireCashPromoParameterName,
                optionConfigValues,
                parameterId => { ExpireCashPromo = GetTicketExpirationCashable(optionConfigValues, parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.PrintExpirationCashPromoParameterName,
                optionConfigValues,
                parameterId => { PrintExpirationCashPromo = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.ExpireNonCashParameterName,
                optionConfigValues,
                parameterId => { ExpireNonCash = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.PrintExpireNonCashParameterName,
                optionConfigValues,
                parameterId => { PrintExpirationNonCash = optionConfigValues.BooleanValue(parameterId); });

            // Voucher Options 2

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.PrintNonCashOfflineParameterName,
                optionConfigValues,
                parameterId => { PrintNonCashOffLine = optionConfigValues.BooleanValue(parameterId); });

            // Voucher Limits
            SetDeviceValue(
                G2SParametersNames.VoucherDevice.MaxOnLinePayOutParameterName,
                optionConfigValues,
                parameterId => { MaxOnLinePayOut = optionConfigValues.Int64Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.VoucherDevice.MaxOffLinePayOutParameterName,
                optionConfigValues,
                parameterId => { MaxOffLinePayOut = optionConfigValues.Int64Value(parameterId); });

            // Voucher Options 3
            SetDeviceValue(
                G2SParametersNames.VoucherDevice.CashOutToVoucherParameterName,
                optionConfigValues,
                parameterId => { CashOutToVoucher = optionConfigValues.BooleanValue(parameterId); });

            var status = new voucherStatus { configComplete = ConfigComplete };

            if (ConfigDateTime != default(DateTime))
            {
                status.configDateTime = ConfigDateTime;
                status.configDateTimeSpecified = true;
            }

            status.configurationId = ConfigurationId;
            status.egmEnabled = Enabled;
            status.hostEnabled = HostEnabled;

            var deviceList = this.DeviceList(status);
            _eventLift.Report(
                this,
                string.Empty,
                deviceList,
                "Voucher device configuration changed by Host",
                -1,
                null,
                null);
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE009);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE010);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE108);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE109);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_VCE111);
        }

        /// <inheritdoc />
        public async Task SendIssueVoucher(
            issueVoucher voucher,
            Func<long, bool> checkAcknowledgedCallBack,
            Action<long> acknowledgedCallBack)
        {
            if (checkAcknowledgedCallBack.Invoke(voucher.transactionId))
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"VoucherDevice.SendIssueVoucher : voucher has been acknowledged
    Device Id : {0}
    Transaction Id : {1}",
                    Id,
                    voucher.transactionId);

                return;
            }

            if (_closed || !Queue.CanSend)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"VoucherDevice.SendIssueVoucher : can't sent or device is closed
    Device Id : {0}
    Transaction Id : {1}",
                    Id,
                    voucher.transactionId);

                _sendIssueVoucherCancellationToken?.Cancel(false);
                _sendIssueVoucherCancellationToken?.Dispose();
                _sendIssueVoucherCancellationToken = null;
                return;
            }

            var request = InternalCreateClass();
            request.Item = voucher;
            var session = SendRequest(request);
            session.WaitForCompletion();

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0 &&
                session.Responses[0].IClass.Item is issueVoucherAck)
            {
                acknowledgedCallBack.Invoke(voucher.transactionId);
                return;
            }

            if (_sendIssueVoucherCancellationToken == null)
            {
                _sendIssueVoucherCancellationToken = new CancellationTokenSource();
            }

            if (session.SessionState == SessionStatus.TimedOut)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"VoucherDevice.SendIssueVoucher : issueVoucher Timed Out.  Will try again
    Device Id : {0}",
                    Id);

                await Task.Run(
                    async () => await SendIssueVoucher(voucher, checkAcknowledgedCallBack, acknowledgedCallBack),
                    _sendIssueVoucherCancellationToken.Token);
            }
            else
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"VoucherDevice.SendIssueVoucher : issueVoucher Failed.  Will try again
    Device Id : {0}",
                    Id);

                await Task.Delay(TimeToLive, _sendIssueVoucherCancellationToken.Token)
                    .ContinueWith(
                        async task =>
                        {
                            if (!task.IsCanceled)
                            {
                                await SendIssueVoucher(voucher, checkAcknowledgedCallBack, acknowledgedCallBack);
                            }
                        });
            }
        }

        /// <inheritdoc />
        public async Task<authorizeVoucher> SendRedeemVoucher(
            redeemVoucher voucher,
            voucherLog log,
            bool triggerEvent = true,
            DateTime endDateTime = default(DateTime))
        {
            if (endDateTime != default(DateTime) && endDateTime < DateTime.UtcNow)
            {
                return null;
            }

            if (endDateTime == default(DateTime))
            {
                endDateTime = DateTime.UtcNow + TimeSpan.FromMilliseconds(VoucherHoldTime);
            }

            if (triggerEvent)
            {
                _eventLift.Report(
                    this,
                    EventCode.G2S_VCE106,
                    null,
                    "Voucher Redemption Requested",
                    voucher.transactionId,
                    new transactionList
                    {
                        transactionInfo = new[]
                        {
                            new transactionInfo
                            {
                                deviceId = Id,
                                deviceClass = this.PrefixedDeviceClass(),
                                Item = log
                            }
                        }
                    },
                    null);
            }

            var request = InternalCreateClass();
            request.Item = voucher;

            var session = SendRequest(request);
            session.WaitForCompletion();

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0 &&
                session.Responses[0].IClass.Item is authorizeVoucher)
            {
                var aVoucher = (authorizeVoucher)session.Responses[0].IClass.Item;
                if (aVoucher != null)
                {
                    log.voucherState = t_voucherStates.G2S_redeemAuth;
                    log.voucherAmt = aVoucher.voucherAmt;
                    log.creditType = aVoucher.creditType;
                    log.voucherSource = aVoucher.voucherSource;
                    log.largeWin = aVoucher.largeWin;
                    log.voucherSequence = aVoucher.voucherSequence;
                    log.expireCredits = aVoucher.expireCredits;
                    log.expireDateTime = aVoucher.expireDateTime;
                    log.expireDays = 0;
                    log.hostAction = aVoucher.hostAction;
                    log.hostException = aVoucher.hostException;
                    log.transferAmt = 0;
                    log.transferDateTime = DateTime.UtcNow;
                    log.egmAction = t_egmVoucherActions.G2S_pending;
                    log.egmException = 0;

                    _eventLift.Report(
                        this,
                        EventCode.G2S_VCE107,
                        null,
                        "Voucher Authorized",
                        voucher.transactionId,
                        transactionList: new transactionList
                        {
                            transactionInfo = new[]
                            {
                                new transactionInfo
                                {
                                    deviceId = Id,
                                    deviceClass = this.PrefixedDeviceClass(),
                                    Item = log
                                }
                            }
                        },
                        null);
                }

                return aVoucher;
            }

            if (_sendRedeemVoucherCancellationToken == null)
            {
                _sendRedeemVoucherCancellationToken = new CancellationTokenSource();
            }

            if (session.SessionState == SessionStatus.TimedOut)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"VoucherDevice.SendRedeemVoucher : redeemVoucher Timed Out.  Will try again
    Device Id : {0}",
                    Id);

                return await Task.Run(
                    async () => await SendRedeemVoucher(voucher, log, false, endDateTime),
                    _sendRedeemVoucherCancellationToken.Token);
            }

            SourceTrace.TraceInformation(
                G2STrace.Source,
                @"VoucherDevice.SendRedeemVoucher : redeemVoucher  Failed.  Will try again
    Device Id : {0}",
                Id);
            authorizeVoucher result = null;
            await Task.Delay(TimeToLive, _sendRedeemVoucherCancellationToken.Token)
                .ContinueWith(
                    async task =>
                    {
                        if (!task.IsCanceled)
                        {
                            result = await SendRedeemVoucher(voucher, log, false, endDateTime);
                        }
                    });

            return result;
        }

        /// <inheritdoc />
        public async Task SendCommitVoucher(
            commitVoucher voucher,
            voucherLog log,
            Action<commitVoucher> ackCallback,
            Func<IVoucherDevice, meterList> getMetersCallback,
            bool triggerEvent = true)
        {
            if(voucher == null || log == null)
            {
                return;
            }

            if (triggerEvent)
            {
                if (voucher.egmAction == t_egmVoucherActions.G2S_redeemed)
                {
                    log.voucherState = t_voucherStates.G2S_commitSent;
                    ////TODO: expireCredits Set to true if date/time expiration assigned to non-cashable credits.
                    //// expireDateTime Set to date/time assigned to credits if expireCredits is set to true.
                    log.transferAmt = log.voucherAmt;
                    log.transferDateTime = voucher.transferDateTime;
                    log.egmAction = t_egmVoucherActions.G2S_redeemed;
                    log.egmException = 0;

                    _eventLift.Report(
                        this,
                        EventCode.G2S_VCE108,
                        null,
                        "Voucher Redeemed",
                        voucher.transactionId,
                        new transactionList
                        {
                            transactionInfo = new[]
                            {
                                new transactionInfo
                                {
                                    deviceId = Id,
                                    deviceClass = this.PrefixedDeviceClass(),
                                    Item = log
                                }
                            }
                        },
                        getMetersCallback(this));
                }
                else
                {
                    log.voucherState = t_voucherStates.G2S_commitSent;
                    log.transferDateTime = voucher.transferDateTime;
                    log.egmAction = t_egmVoucherActions.G2S_rejected;
                    ////TODO: 4 (four) if hostException is not set to 0 (zero) and voucher stacked.
                    //// 5 (five) if the EGM aborted the transaction before it was authorized (timed out).
                    log.egmException = voucher.egmException != 0 ? voucher.egmException : log.hostException != 0 ? 2 : 3;

                    _eventLift.Report(
                        this,
                        EventCode.G2S_VCE109,
                        null,
                        "Voucher Rejected",
                        voucher.transactionId,
                        new transactionList
                        {
                            transactionInfo = new[]
                            {
                                new transactionInfo
                                {
                                    deviceId = Id,
                                    deviceClass = this.PrefixedDeviceClass(),
                                    Item = log
                                }
                            }
                        },
                        null);
                }
            }

            var request = InternalCreateClass();
            request.Item = voucher;

            var session = SendRequest(request);
            session.WaitForCompletion();

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0 &&
                session.Responses[0].IClass.Item is commitVoucherAck)
            {
                _sendCommitVoucherCancellationToken?.Cancel(false);
                _sendCommitVoucherCancellationToken?.Dispose();
                _sendCommitVoucherCancellationToken = null;

                log.voucherState = t_voucherStates.G2S_commitAcked;

                _eventLift.Report(
                    this,
                    EventCode.G2S_VCE111,
                    null,
                    "Voucher Commit Command Acknowledged",
                    voucher.transactionId,
                    new transactionList
                    {
                        transactionInfo = new[]
                        {
                            new transactionInfo
                            {
                                deviceId = Id,
                                deviceClass = this.PrefixedDeviceClass(),
                                Item = log
                            }
                        }
                    },
                    null);

                ackCallback(voucher);
            }
            else
            {
                if (_sendCommitVoucherCancellationToken == null)
                {
                    _sendCommitVoucherCancellationToken = new CancellationTokenSource();
                }

                if (session.SessionState == SessionStatus.TimedOut)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"VoucherDevice.SendCommitVoucher : commitVoucher Timed Out.  Will try again
    Device Id : {0}",
                        Id);

                    await Task.Run(
                        () => SendCommitVoucher(voucher, log, ackCallback, getMetersCallback, false),
                        _sendCommitVoucherCancellationToken.Token);
                }
                else
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"VoucherDevice.SendCommitVoucher : commitVoucher Failed.  Will try again
    Device Id : {0}",
                        Id);

                    await Task.Delay(TimeToLive, _sendCommitVoucherCancellationToken.Token)
                        .ContinueWith(
                            async task =>
                            {
                                if (!task.IsCanceled)
                                {
                                    await SendCommitVoucher(voucher, log, ackCallback, getMetersCallback, false);
                                }
                            });
                }
            }
        }

        /// <inheritdoc />
        public bool GetValidationData(
            int numberValidationIds,
            bool isValidationIdListExpired,
            long validationListId,
            Action<validationData, IVoucherDevice> result)
        {
            if (!HostEnabled || !Queue.CanSend && !_closed)
            {
                return false;
            }

            if (numberValidationIds < 0)
            {
                numberValidationIds = 0;
            }

            var command = new getValidationData
            {
                configComplete = ConfigComplete,
                configurationId = ConfigurationId,
                numValidationIds = numberValidationIds,
                valIdListExpired = isValidationIdListExpired,
                validationListId = validationListId
            };
            if (ConfigDateTime != DateTime.MinValue)
            {
                command.configDateTimeSpecified = true;
                command.configDateTime = ConfigDateTime;
            }

            var request = InternalCreateClass();
            request.Item = command;

            var session = SendRequest(request);
            session.WaitForCompletion();

            if (session.SessionState == SessionStatus.Success)
            {
                result((session.Responses.Count > 0 ? session.Responses[0].IClass.Item : null) as validationData, this);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            if (_sendIssueVoucherCancellationToken != null)
            {
                _sendIssueVoucherCancellationToken.Cancel(false);
                _sendIssueVoucherCancellationToken.Dispose();
                _sendIssueVoucherCancellationToken = null;
            }

            if (_sendRedeemVoucherCancellationToken != null)
            {
                _sendRedeemVoucherCancellationToken.Cancel(false);
                _sendRedeemVoucherCancellationToken.Dispose();
                _sendRedeemVoucherCancellationToken = null;
            }

            if (_sendCommitVoucherCancellationToken != null)
            {
                _sendCommitVoucherCancellationToken.Cancel(false);
                _sendCommitVoucherCancellationToken.Dispose();
                _sendCommitVoucherCancellationToken = null;
            }

            _disposed = true;
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private void SetDefaults()
        {
            TimeToLive = (int)Constants.DefaultTimeout.TotalMilliseconds;
            RestartStatus = DefaultRestartStatus;
            IdReaderId = DefaultIdReaderId;
            CombineCashableOut = DefaultCombineCashableOut;
            MaxValueIds = DefaultMaxValIds;
            MinLevelValueIds = DefaultMinLevelValIds;
            ValueIdListRefresh = DefaultValIdListRefresh;
            ValueIdListLife = DefaultValIdListLife;
            VoucherHoldTime = DefaultVoucherHoldTime;
            PrintOffLine = DefaultPrintOffLine;
            ExpireCashPromo = Constants.ExpirationNotSet;
            PrintExpirationCashPromo = DefaultPrintExpCashPromo;
            ExpireNonCash = Constants.ExpirationNotSet;
            PrintExpirationNonCash = DefaultPrintExpNonCash;
            MaxOnLinePayOut = DefaultMaxOnLinePayOut;
            MaxOffLinePayOut = DefaultMaxOffLinePayOut;
            PrintNonCashOffLine = DefaultPrintNonCashOffLine;
            CashOutToVoucher = DefaultCashOutToVoucher;
            MinLogEntries = Constants.DefaultMinLogEntries;
            RequiredForPlay = true;
        }

        private int GetTicketExpirationCashable(DeviceOptionConfigValues optionConfigValues, string parameterId)
        {
            return optionConfigValues.Int32Value(parameterId);
        }

        ///// <summary>
        ///// Enables the system for the specific door and removes the displayable message.
        ///// </summary>
        // private void EnableDevice()
        // {
        //    ISystemDisableManager disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
        //    IMessageDisplay messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();
        //    disableManager.Enable(_uId);
        //    if (DisableMessage != null)
        //    {
        //        messageDisplay.RemoveMessage(DisableMessage);
        //    }
        //    DisableMessage = null;
        //    // TODO: start processing pending transactions
        // }

        ///// <summary>
        ///// Disables the system for event device handler and adds the displayable message.
        ///// </summary>
        // private void Disable()
        // {
        //    ISystemDisableManager disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
        //    // MessageDisplay messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();
        //    disableManager.Disable(_uId, SystemDisablePriority.Normal, DisableMessage != null ? DisableMessage.Message : string.Empty);
        //    // TODO: stop processing pending transactions
        // }

        ///// <summary>
        ///// Disables the system for event device handler and adds the displayable message.
        ///// </summary>
        // private void DisableDevice()
        // {
        //    IMessageDisplay messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();
        //    messageDisplay.DisplayMessage(DisableMessage);
        // }
    }
}
