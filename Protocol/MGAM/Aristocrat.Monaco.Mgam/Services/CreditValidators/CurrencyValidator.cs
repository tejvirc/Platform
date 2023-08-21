namespace Aristocrat.Monaco.Mgam.Services.CreditValidators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Common;
    using Common.Data.Models;
    using Common.Data.Repositories;
    using Common.Events;
    using Gaming.Contracts;
    using Kernel;
    using Lockup;
    using Protocol.Common.Storage.Entity;
    using BeginSession = Commands.BeginSession;
    using CreditCash = Commands.CreditCash;
    using EndSession = Commands.EndSession;
    using EscrowCash = Commands.EscrowCash;

    /// <summary>
    ///     Used to validate currency amounts requested by the EGM with the host.
    /// </summary>
    public class CurrencyValidator : ICurrencyValidator, IDisposable
    {
        private const int Cents = 100;
        private readonly ILogger _logger;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILockup _lockoutHandler;
        private readonly IEventBus _eventBus;
        private readonly IPlayerBank _bank;
        private bool _disposed;

        /// <inheritdoc />
        public string Name => typeof(CurrencyValidator).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ICurrencyValidator) };

        /// <inheritdoc />
        public bool HostOnline { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyValidator" /> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="commandFactory">Command factory.</param>
        /// <param name="unitOfWorkFactory">Unit of work factory.</param>
        /// <param name="lockoutHandler"><see cref="ILockup" />.</param>
        /// <param name="eventBus"><see cref="IEventBus" />.</param>
        /// <param name="bank"><see cref="IPlayerBank"/>.</param>
        public CurrencyValidator(
            ILogger<CurrencyValidator> logger,
            ICommandHandlerFactory commandFactory,
            IUnitOfWorkFactory unitOfWorkFactory,
            ILockup lockoutHandler,
            IEventBus eventBus,
            IPlayerBank bank)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _lockoutHandler = lockoutHandler ?? throw new ArgumentNullException(nameof(lockoutHandler));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));

            SubscribeToEvents();
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public async Task<CurrencyInExceptionCode> ValidateNote(int note)
        {
            var amount = note * Cents;

            try
            {
                _logger.LogInfo($"Sending Escrow Cash {amount}");
                await _commandFactory.Execute(new EscrowCash { Value = amount });
            }
            catch (ServerResponseException response)
            {
                _logger.LogInfo($"Escrow Cash failed ResponseCode:{response.ResponseCode}");

                switch (response.ResponseCode)
                {
                    case ServerResponseCode.CreditFailedSessionBalanceLimit:
                        return CurrencyInExceptionCode.CreditInLimitExceeded;
                    case ServerResponseCode.InvalidAmount:
                        return CurrencyInExceptionCode.InvalidBill;
                    default:
                        return CurrencyInExceptionCode.Other;
                }
            }

            try
            {
                if (!await CheckSession())
                {
                    return CurrencyInExceptionCode.Other;
                }
            }
            catch (ServerResponseException response)
            {
                var errorMessage = $"Begin Session failed ResponseCode:{response.ResponseCode}";
                _logger.LogInfo(errorMessage);

                _lockoutHandler.LockupForEmployeeCard(errorMessage);

                return CurrencyInExceptionCode.Other;
            }

            return CurrencyInExceptionCode.None;
        }

        /// <inheritdoc />
        public async Task<bool> StackedNote(int note)
        {
            var amount = note * Cents;

            try
            {
                if (!await CheckSession())
                {
                    throw new ServerResponseException(ServerResponseCode.InvalidSessionId);
                }

                _logger.LogInfo($"Sending Credit Cash Amount:{amount}");
                await _commandFactory.Execute(new CreditCash { Amount = amount });
            }
            catch (ServerResponseException response)
            {
                if (HostOnline)
                {
                    if (_bank.Balance == 0)
                    {
                        bool endSession;
                        using (var unitOfWork = _unitOfWorkFactory.Create())
                        {
                            endSession = unitOfWork.Repository<Session>().Queryable().SingleOrDefault() != null;
                        }

                        if (endSession)
                        {
                            try
                            {
                                _logger.LogInfo(
                                    $"Sending End Session Amount:{0L.MillicentsToDollars().FormattedCurrencyString()}");
                                await _commandFactory.Execute(
                                    new EndSession { Balance = 0L.MillicentsToDollars().FormattedCurrencyString() });
                            }
                            catch (ServerResponseException endSessionResponse)
                            {
                                _logger.LogInfo($"Failed End Session Reason:{endSessionResponse.ResponseCode}");
                            }
                        }
                    }

                    _logger.LogInfo(
                        $"Credit Cash failed ResponseCode:{response.ResponseCode}");

                    var voucherData = new Voucher();
                    voucherData.Validate();
                    voucherData.OfflineReason = VoucherOutOfflineReason.Credit;

                    using (var unitOfWork = _unitOfWorkFactory.Create())
                    {
                        unitOfWork.Repository<Voucher>().AddVoucher(voucherData);

                        unitOfWork.SaveChanges();
                    }

                    return false;
                }
            }

            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<AttributesUpdatedEvent>(this, evt => HostOnline = true);
            _eventBus.Subscribe<HostOfflineEvent>(this, evt => HostOnline = false);
        }

        private async Task<bool> CheckSession()
        {
            bool beginSession;
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                beginSession = unitOfWork.Repository<Session>().GetSessionId() == null;
            }

            if (beginSession)
            {
                _logger.LogInfo("Sending Begin Session");
                await _commandFactory.Execute(new BeginSession());

                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    if (unitOfWork.Repository<Session>().GetSessionId() == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}