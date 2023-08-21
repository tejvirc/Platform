namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Kernel;
    using Services;

    /// <summary>
    ///     Handles the <see cref="setRemoteKeyOff" /> command.
    /// </summary>
    public class SetRemoteKeyOff : ICommandHandler<handpay, setRemoteKeyOff>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IHandpayProperties _properties;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetRemoteKeyOff" /> class.
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> instance.</param>
        /// <param name="eventBus"><see cref="IEventBus" /> instance.</param>
        /// <param name="eventLift"><see cref="IEventLift"/> instance</param>
        /// <param name="transactionHistory"><see cref="ITransactionHistory"/> instance</param>
        /// <param name="properties"><see cref="IHandpayProperties"/> instance</param>
        /// <param name="propertiesManager"><see cref="IPropertiesManager"/> instance</param>
        public SetRemoteKeyOff(
            IG2SEgm egm,
            IEventBus eventBus,
            IEventLift eventLift,
            ITransactionHistory transactionHistory,
            IHandpayProperties properties,
            IPropertiesManager propertiesManager
         )
        {
            _egm = egm;
            _eventBus = eventBus;
            _eventLift = eventLift;
            _transactionHistory = transactionHistory;
            _properties = properties;
            _propertiesManager = propertiesManager;
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<handpay, setRemoteKeyOff> command)
        {
            return await Sanction.OnlyOwner<IHandpayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<handpay, setRemoteKeyOff> command)
        {
            var device = _egm.GetDevice<IHandpayDevice>(command.IClass.deviceId);

            var transaction = _transactionHistory
                .RecallTransactions<HandpayTransaction>()
                .FirstOrDefault(x => x.TransactionId == command.Command.transactionId);

            if (transaction == null || transaction.State >= HandpayState.Committed)
            {
                command.Error.SetErrorCode(ErrorCode.G2S_JPX003);
                return;
            }

            if (transaction.CashableAmount != command.Command.keyOffCashableAmt)
            {
                command.Error.SetErrorCode(ErrorCode.G2S_JPX004);
                return;
            }

            var keyOffType = command.Command.keyOffType.KeyOffTypeFromG2SString();

            // TODO: Get clarification on the action to take if the specified key-off method is not allowed

            if (!_propertiesManager.GetValue(AccountingConstants.MenuSelectionHandpayInProgress, false) && (_properties.AllowRemoteHandpay && keyOffType == KeyOffType.RemoteHandpay ||
                                                                                                                          _properties.AllowRemoteVoucher && keyOffType == KeyOffType.RemoteVoucher ||
                                                                                                                          _properties.AllowRemoteCredit && keyOffType == KeyOffType.RemoteCredit ||
                                                                                                                          _properties.AllowRemoteWat && keyOffType == KeyOffType.RemoteWat ||
                                                                                                                          _properties.AllowLocalHandpay && keyOffType == KeyOffType.LocalHandpay ||
                                                                                                                          _properties.AllowLocalVoucher && keyOffType == KeyOffType.LocalVoucher ||
                                                                                                                          _properties.AllowLocalCredit && keyOffType == KeyOffType.LocalCredit ||
                                                                                                                          _properties.AllowLocalWat && keyOffType == KeyOffType.LocalWat))
            {
                _eventLift.Report(
                    device,
                    EventCode.G2S_JPE103,
                    transaction.TransactionId,
                    device.TransactionList(transaction.GetLog(_properties)));

                _eventBus.Publish(
                    new RemoteKeyOffEvent(
                        keyOffType,
                        command.Command.keyOffCashableAmt,
                        command.Command.keyOffPromoAmt,
                        command.Command.keyOffNonCashAmt));
            }

            var response = command.GenerateResponse<remoteKeyOffAck>();

            response.Command.transactionId = command.Command.transactionId;

            await Task.CompletedTask;
        }
    }
}