namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Mgam.Client.Messaging;
    using Common;
    using Common.Events;
    using Kernel;
    using Services.CreditValidators;

    /// <summary>
    ///     TransactionalCommandHandlerBase
    /// </summary>
    public abstract class TransactionalCommandHandlerBase
    {
        private readonly ITransactionRetryHandler _transactionRetry;
        private readonly IIdProvider _idProvider;
        protected readonly IEventBus Bus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionalCommandHandlerBase" /> class.
        /// </summary>
        /// <param name="idProvider"><see cref="IIdProvider" />.</param>
        /// <param name="transactionRetry"><see cref="ITransactionRetryHandler"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        protected TransactionalCommandHandlerBase(IIdProvider idProvider, ITransactionRetryHandler transactionRetry, IEventBus bus)
        {
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _transactionRetry = transactionRetry ?? throw new ArgumentNullException(nameof(transactionRetry));
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <summary>
        ///     Checks for invalid server response codes that require re-connection.
        /// </summary>
        /// <param name="code"><see cref="IServerResponseCode" />.</param>
        protected void ValidateResponseCode(IServerResponseCode code)
        {
            if (code == null)
            {
                Bus.Publish(new ForceDisconnectEvent(DisconnectReason.InvalidServerResponse));
            }

            switch (code?.ResponseCode)
            {
                case ServerResponseCode.DeviceStillRegisteredWithLauncherSvc:
                case ServerResponseCode.InvalidInstanceId:
                case ServerResponseCode.VltServiceNotRegistered:
                case ServerResponseCode.ServerError:
                case ServerResponseCode.DuplicateVoucherRetry:
                    Bus.Publish(new ForceDisconnectEvent(DisconnectReason.InvalidServerResponse));
                    break;
            }
        }

        /// <summary>
        ///     SendRequest.
        /// </summary>
        /// <returns>Task.</returns>
        protected async Task SendRequest(Request message, CancellationToken cancellationToken = default(CancellationToken), long transactionId = 0)
        {
            if (message is ILocalTransactionId localTransactionId)
            {
                localTransactionId.LocalTransactionId =
                    (int)_idProvider.GetNextLogSequence<ILocalTransactionId>(int.MaxValue);

                _transactionRetry.Add(message);
                try
                {
                    await Handle(message, cancellationToken, transactionId);
                }
                catch (ServerResponseException se)
                {
                    if (se.ResponseCode == ServerResponseCode.DoNotPrintVoucher)
                    {
                        _transactionRetry.Remove(message);
                    }

                    throw;
                }

                _transactionRetry.Remove(message);
            }
        }

        /// <summary>
        ///     Handle message.
        /// </summary>
        /// <param name="arg">Request message</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
        /// <param name="transactionId">Central transaction Id.</param>
        /// <returns>Task.</returns>
        protected abstract Task Handle(Request arg, CancellationToken cancellationToken = default(CancellationToken), long transactionId = 0);

        /// <summary>
        ///     UpdateRequestTransactionId.
        /// </summary>
        protected void UpdateRequestTransactionId(Request message)
        {
            if (message is ILocalTransactionId localTransactionId)
            {
                localTransactionId.LocalTransactionId =
                    (int)_idProvider.GetNextLogSequence<ILocalTransactionId>(int.MaxValue);

                _transactionRetry.Remove(message);
                _transactionRetry.Add(message);
            }
        }
    }
}