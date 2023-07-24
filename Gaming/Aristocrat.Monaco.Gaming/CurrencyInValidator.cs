namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Lobby;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.PWM;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using log4net;

    public class CurrencyInValidator : ICurrencyInValidator, IService
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private const string CreditsInKey = @"CreditsIn";

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IPropertiesManager _propertiesManager;
        private IMessageDisplay _messageDisplay;
        private IPersistentStorageAccessor _accessor;

        private bool _disposed;

        private long? _sessionIn;
        private long _currencyMultiplier;
        private DisplayableMessage _creditLimitExceededMessage;
        private IBank _bank;
        private INoteAcceptor _noteAcceptor;

        private long AmountIn
        {
            get => (long)(_sessionIn ?? (_sessionIn = (long)_accessor[CreditsInKey]));

            set
            {
                using (var transaction = _accessor.StartTransaction())
                {
                    transaction[CreditsInKey] = _sessionIn = value;

                    transaction.Commit();
                }
            }
        }

        public bool IsValid(long credit = 0L)
        {
            var noteAcceptor = GetNoteAcceptor();
            if (noteAcceptor == null)
            {
                return true;
            }

            var isValid = InternalIsValid(credit);
            if (credit > 0)
            {
                return isValid;
            }

            if (ServiceManager.GetInstance().GetService<IPropertiesManager>().GetValue(PropertyKey.VoucherIn, false))
            {
                // We don't want to disable the note acceptors if vouchers are enabled
                return isValid;
            }

            if (isValid)
            {
                EnableNoteAcceptorBasedOnConfig(noteAcceptor);

                return true;
            }

            DisableNoteAcceptorBasedOnConfig(noteAcceptor);

            return false;
        }

        private void EnableNoteAcceptorBasedOnConfig(INoteAcceptor noteAcceptor)
        {
            if (!noteAcceptor.Enabled && (noteAcceptor.ReasonDisabled & DisabledReasons.Configuration) > 0)
            {
                Logger.Info("Enabling the note acceptor per configuration override");

                // TODO: This can and does override the disabled state that is driven by the deviceCoordinator
                //  This was disabling with the reason of System, but it caused some side effects in particular when doing a force cashout when the system was disabled
                noteAcceptor.Enable(EnabledReasons.Configuration);
            }
        }

        private void DisableNoteAcceptorBasedOnConfig(INoteAcceptor noteAcceptor)
        {
            if (noteAcceptor.Enabled || !noteAcceptor.Enabled &&
                !((noteAcceptor.ReasonDisabled & DisabledReasons.Configuration) > 0))
            {
                Logger.Info("Disabling the note acceptor per configuration override");

                // TODO: See above in EnableNoteAcceptorBasedOnConfig
                noteAcceptor.Disable(DisabledReasons.Configuration);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(ICurrencyInValidator).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(ICurrencyInValidator) };

        public void Initialize()
        {
            var blockName = GetType().ToString();
            _accessor = ServiceManager.GetInstance().GetService<IPersistentStorageManager>()
                .GetAccessor(Level, blockName);

            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _currencyMultiplier = (long)_propertiesManager
                .GetValue(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier);

            _messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();
            var bus = ServiceManager.GetInstance().GetService<IEventBus>();

            bus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
            bus.Subscribe<TransactionSavedEvent>(this, HandleEvent);
            bus.Subscribe<PropertyChangedEvent>(this, HandleEvent, e => e.PropertyName == PropertyKey.MaxCreditsIn);
            bus.Subscribe<GameIdleEvent>(this, HandleEvent);
            bus.Subscribe<LobbyInitializedEvent>(this, _ =>
            {
                IsValid();
                CheckMaxCreditLimit();
            });

            _creditLimitExceededMessage = new DisplayableMessage(
                () => GetCreditLimitExceededRejectionMessage(),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(BankBalanceChangedEvent),
                new Guid("{98685495-22DD-4A05-A4EA-DAC6D0F76F63}"));
        }

        private bool InternalIsValid(long credit)
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            long balance;

            switch (properties.GetValue(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None))
            {
                case CheckCreditsStrategy.Balance:
                    balance = GetBank()?.QueryBalance() ?? 0;
                    break;
                case CheckCreditsStrategy.Session:
                    balance = AmountIn;
                    break;
                default:
                    return true;
            }

            var limit = properties.GetValue(
                PropertyKey.MaxCreditsIn,
                AccountingConstants.DefaultMaxTenderInLimit);

            if (limit == 0)
            {
                limit = AccountingConstants.DefaultMaxTenderInLimit;
            }

            if (properties.GetValue(AccountingConstants.AllowCreditUnderLimit, false))
            {
                return balance < limit;
            }

            if (credit > 0)
            {
                return balance + credit <= limit;
            }

            return balance <= limit - GetMinimumDenom();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleEvent(BankBalanceChangedEvent @event)
        {
            CheckMaxCreditLimit();

            if (_propertiesManager
                    .GetValue(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None) !=
                CheckCreditsStrategy.Balance)
            {
                return;
            }

            if (@event.NewBalance == 0 &&
                (ServiceManager.GetInstance().TryGetService<IGamePlayState>()?.InGameRound ?? false))
            {
                return;
            }

            IsValid();
        }

        private void CheckMaxCreditLimit()
        {
            if (_propertiesManager.GetValue(AccountingConstants.AllowCreditsInAboveMaxCredit, false))
            {
                return;
            }

            long balance;
            long limit;
            var bank = GetBank();
            if (bank == null)
            {
                balance = 0;
                limit = _propertiesManager.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue);
            }
            else
            {
                balance = bank.QueryBalance();
                limit = bank.Limit;
            }

            bool isMaxCreditLimitReached = false;
            if (balance >= limit)
            {
                if (_propertiesManager.GetValue(
                    AccountingConstants.ShowMessageWhenCreditLimitReached,
                    false))
                {
                    _messageDisplay.DisplayMessage(_creditLimitExceededMessage);
                    Logger.Info($"MaxCreditLimit {limit} reached, current balance {balance}");
                }

                isMaxCreditLimitReached = true;
            }
            else
            {
                _messageDisplay.RemoveMessage(_creditLimitExceededMessage);
            }

            DisableBankNoteAcceptorOnMaxCreditLimit(isMaxCreditLimitReached);

            DisableCoinAcceptorOnMaxCreditLimit(isMaxCreditLimitReached);
        }

        private string GetCreditLimitExceededRejectionMessage()
        {
            string displayMessage = Localizer.For(CultureFor.Player).GetString(ResourceKeys.CreditLimitExceeded);
            var limit = GetBank()?.Limit ??
                        _propertiesManager.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue);
            // For CreditLimit exceeded message we need Limit in some markets
            return string.Format(displayMessage, limit.MillicentsToDollars());
        }

        private void DisableCoinAcceptorOnMaxCreditLimit(bool isMaxCreditLimitReached)
        {
            if (_propertiesManager.GetValue(AccountingConstants.DisableCoinAcceptorWhenCreditLimitReached, false))
            {
                var coinAcceptor = ServiceManager.GetInstance().TryGetService<ICoinAcceptor>();

                if (coinAcceptor == null)
                {
                    return;
                }

                if (!isMaxCreditLimitReached)
                {
                    EnableCoinAcceptorBasedOnConfig(coinAcceptor);
                }
                else
                {
                    DisableCoinAcceptorBasedOnConfig(coinAcceptor);
                }
            }
        }

        private void EnableCoinAcceptorBasedOnConfig(ICoinAcceptor coinAcceptor)
        {
            if (!coinAcceptor.Enabled && (coinAcceptor.ReasonDisabled & DisabledReasons.Configuration) > 0)
            {
                Logger.Info("Enabling the coin acceptor per configuration override");
                coinAcceptor.Enable(EnabledReasons.Configuration);
            }
        }

        private void DisableCoinAcceptorBasedOnConfig(ICoinAcceptor coinAcceptor)
        {
            if (coinAcceptor.Enabled || !coinAcceptor.Enabled &&
                !((coinAcceptor.ReasonDisabled & DisabledReasons.Configuration) > 0))
            {
                Logger.Info("Disabling the coin acceptor per configuration override");
                coinAcceptor.Disable(DisabledReasons.Configuration);
            }
        }

        private void DisableBankNoteAcceptorOnMaxCreditLimit(bool isMaxCreditLimitReached)
        {
            if (_propertiesManager.GetValue(AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached, false))
            {
                INoteAcceptor noteAcceptor = GetNoteAcceptor();

                if (noteAcceptor == null)
                {
                    return;
                }

                if (!isMaxCreditLimitReached)
                {
                    EnableNoteAcceptorBasedOnConfig(noteAcceptor);
                }
                else
                {
                    DisableNoteAcceptorBasedOnConfig(noteAcceptor);
                }
            }
        }

        private void HandleEvent(TransactionSavedEvent @event)
        {
            if (ServiceManager.GetInstance().GetService<IPropertiesManager>()
                    .GetValue(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None) !=
                CheckCreditsStrategy.Session)
            {
                return;
            }

            switch (@event.Transaction)
            {
                case BillTransaction trans:
                    AmountIn += trans.Amount;
                    IsValid();
                    break;
                case VoucherOutTransaction _:
                    var bank = GetBank();

                    if ((bank?.QueryBalance() ?? 0) == 0)
                    {
                        AmountIn = 0;
                        _sessionIn = null;
                        IsValid();
                    }

                    break;
            }
        }

        private void HandleEvent(PropertyChangedEvent @event)
        {
            IsValid();
        }

        private void HandleEvent(GameIdleEvent @event)
        {
            var strategy = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None);

            if (strategy == CheckCreditsStrategy.None)
            {
                return;
            }

            if ((GetBank()?.QueryBalance(AccountType.Cashable) ?? 0) == 0)
            {
                if (strategy == CheckCreditsStrategy.Session)
                {
                    //VLT-4859: Reset Session $$$ if we hit $0 on Game Idle
                    AmountIn = 0;
                }

                IsValid();
            }
        }

        private long GetMinimumDenom()
        {
            var noteAcceptor = GetNoteAcceptor();

            if (noteAcceptor.Denominations.Count > 0)
            {
                return noteAcceptor.Denominations[0] * _currencyMultiplier;
            }

            return 0L;
        }

        private IBank GetBank() => _bank ??= ServiceManager.GetInstance().TryGetService<IBank>();

        private INoteAcceptor GetNoteAcceptor() => _noteAcceptor ??= ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
    }
}