namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Transactions;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization.Properties;
    using MVVM.Command;
    using MVVM.Model;

    [CLSCompliant(false)]
    public class KeyedCreditsPageViewModel : OperatorMenuPageViewModelBase
    {
        private const int TransactionTimeout = 1000;
        private static readonly Guid RequestorId = new Guid("{E6CB4D8F-0B43-4CE1-BC14-5F6BD102FEC0}");
        private readonly IBank _bank;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly IMeterManager _meterManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IPersistentStorageManager _persistentStorageManager;
        private readonly ITransactionHistory _transactionHistory;
        private decimal _keyedOnCreditAmount;
        private Credit _selectedCredit;
        private List<Credit> _credits;
        private long _creditLimit;
        private long _currentCredits;
        private readonly IEventBus _eventBus;

        public KeyedCreditsPageViewModel()
            : this(
                ServiceManager.GetInstance().TryGetService<IBank>(),
                ServiceManager.GetInstance().TryGetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<ITransactionCoordinator>(),
                ServiceManager.GetInstance().TryGetService<IMeterManager>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>())
        {
        }

        public KeyedCreditsPageViewModel(
            IBank bank,
            IEventBus eventBus,
            ITransactionCoordinator transactionCoordinator,
            IMeterManager meterManager,
            IPropertiesManager propertiesManager,
            IPersistentStorageManager persistentStorageManager,
            ITransactionHistory transactionHistory)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _transactionCoordinator =
                transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _persistentStorageManager = persistentStorageManager ??
                                        throw new ArgumentNullException(nameof(persistentStorageManager));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));

            ConfirmKeyOnCreditsCommand = new ActionCommand<object>(ConfirmKeyOnCreditsPressed);
            ConfirmKeyOffCreditsCommand = new ActionCommand<object>(ConfirmKeyOffCreditsPressed);
        }

        public ICommand ConfirmKeyOnCreditsCommand { get; }

        public ICommand ConfirmKeyOffCreditsCommand { get; }

        public bool KeyedOnInputEnabled => InputEnabled && _currentCredits < _creditLimit;

        public bool KeyedOnCreditsAllowed => InputEnabled && KeyedOnCreditAmount > 0 && !PropertyHasErrors(nameof(KeyedOnCreditAmount));

        public bool KeyOffCreditsButtonEnabled => InputEnabled && Credits.Any(x => x.HasCredits);

        public Credit SelectedCredit
        {
            get => _selectedCredit;
            set => SetProperty(ref _selectedCredit, value, nameof(SelectedCredit));
        }

        public decimal KeyedOnCreditAmount
        {
            get => _keyedOnCreditAmount;
            set
            {
                if (SetProperty(ref _keyedOnCreditAmount, value, nameof(KeyedOnCreditAmount)))
                {
                    ValidateKeyedOnCredits();
                    RaisePropertyChanged(nameof(KeyedOnCreditsAllowed));
                }
            }
        }

        public List<Credit> Credits
        {
            get
            {
                if (_credits != null)
                {
                    return _credits;
                }

                using (var scope = new CultureScope(CultureFor.Operator))
                {
                    _credits = new List<Credit>
                    {
                        new Credit(
                            AccountType.Cashable,
                            scope.GetString(ResourceKeys.Cashable),
                            _bank.QueryBalance(AccountType.Cashable).MillicentsToDollars().FormattedCurrencyString(),
                            _bank.QueryBalance(AccountType.Cashable),
                            false),
                        new Credit(
                            AccountType.Promo,
                            scope.GetString(ResourceKeys.CashablePromotional),
                            _bank.QueryBalance(AccountType.Promo).MillicentsToDollars().FormattedCurrencyString(),
                            _bank.QueryBalance(AccountType.Promo),
                            false),
                        new Credit(
                            AccountType.NonCash,
                            scope.GetString(ResourceKeys.NonCashablePromotional),
                            _bank.QueryBalance(AccountType.NonCash).MillicentsToDollars().FormattedCurrencyString(),
                            _bank.QueryBalance(AccountType.NonCash),
                            false)
                    };
                    return _credits;
                }
            }
        }

        protected override void OnLoaded()
        {
            _creditLimit = PropertiesManager.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue);
            UpdateCurrentCredits();

            KeyedOnCreditAmount = 0m;
            SelectedCredit = Credits[0];
        }

        protected override void OnInputEnabledChanged()
        {
            RaisePropertyChanged(nameof(KeyedOnInputEnabled));
            RaisePropertyChanged(nameof(KeyedOnCreditsAllowed));
            RaisePropertyChanged(nameof(KeyOffCreditsButtonEnabled));
        }

        private void UpdateCreditData()
        {
            foreach (var credit in Credits)
            {
                credit.FormattedValue = _bank.QueryBalance(credit.AccountType).MillicentsToDollars()
                    .FormattedCurrencyString();
                credit.Value = _bank.QueryBalance(credit.AccountType);
            }

            UpdateCurrentCredits();
        }

        private void UpdateCurrentCredits()
        {
            _currentCredits = Credits.Sum(x => x.Value);
            RaisePropertyChanged(nameof(KeyedOnInputEnabled));
            RaisePropertyChanged(nameof(KeyOffCreditsButtonEnabled));
        }

        private void ConfirmKeyOnCreditsPressed(object obj)
        {
            var accountType = SelectedCredit.AccountType;
            var currencyMultiplier = (double)_propertiesManager.GetProperty(
                ApplicationConstants.CurrencyMultiplierKey,
                ApplicationConstants.DefaultCurrencyMultiplier);
            var amount = (long)(KeyedOnCreditAmount * (decimal)currencyMultiplier);
            var transactionId = _transactionCoordinator.RequestTransaction(
                RequestorId,
                TransactionTimeout,
                TransactionType.Write);
            if (transactionId == Guid.Empty)
            {
                return;
            }

            var keyedOnMeterDictionary = new Dictionary<AccountType, (string amountMeter, string countMeter)>
            {
                {
                    AccountType.Cashable,
                    (AccountingMeters.KeyedOnCashableAmount, AccountingMeters.KeyedOnCashableCount)
                },
                {
                    AccountType.Promo,
                    (AccountingMeters.KeyedOnCashablePromoAmount, AccountingMeters.KeyedOnCashablePromoCount)
                },
                {
                    AccountType.NonCash,
                    (AccountingMeters.KeyedOnNonCashableAmount, AccountingMeters.KeyedOnNonCashableCount)
                }
            };

            var trans = new KeyedCreditsTransaction(1, DateTime.UtcNow, accountType, true, amount);
            trans.TransferredCashableAmount = (accountType == AccountType.Cashable) ? amount : 0;
            trans.TransferredNonCashAmount = (accountType == AccountType.NonCash) ? amount : 0;
            trans.TransferredPromoAmount = (accountType == AccountType.Promo) ? amount : 0;
            using (var scope = _persistentStorageManager.ScopedTransaction())
            {
                if (_bank.CheckDeposit(accountType, amount, transactionId) && amount > 0)
                {
                    _bank.Deposit(accountType, amount, transactionId);
                    _meterManager.GetMeter(keyedOnMeterDictionary[accountType].amountMeter).Increment(amount);
                    _meterManager.GetMeter(keyedOnMeterDictionary[accountType].countMeter).Increment(1);

                    _transactionHistory.AddTransaction(trans);
                    Logger.Info(
                        $"Keyed on {amount} credits to {accountType} account type [TransactionId={transactionId}");
                }
                _transactionCoordinator.ReleaseTransaction(transactionId);
                scope.Complete();
            }
            _eventBus.Publish(new KeyedCreditOnEvent(trans));
            UpdateCreditData();
            KeyedOnCreditAmount = 0m;
        }

        private void ConfirmKeyOffCreditsPressed(object obj)
        {
            foreach (var credit in Credits.Where(credit => credit.KeyOff))
            {
                credit.KeyOff = false;
                var accountType = credit.AccountType;
                var amount = credit.Value;
                var transactionId = _transactionCoordinator.RequestTransaction(
                    RequestorId,
                    TransactionTimeout,
                    TransactionType.Write);
                if (transactionId == Guid.Empty)
                {
                    return;
                }

                var keyedOffMeterDictionary = new Dictionary<AccountType, (string amountMeter, string countMeter)>
                {
                    {
                        AccountType.Cashable,
                        (AccountingMeters.KeyedOffCashableAmount, AccountingMeters.KeyedOffCashableCount)
                    },
                    {
                        AccountType.Promo,
                        (AccountingMeters.KeyedOffCashablePromoAmount, AccountingMeters.KeyedOffCashablePromoCount)
                    },
                    {
                        AccountType.NonCash,
                        (AccountingMeters.KeyedOffNonCashableAmount, AccountingMeters.KeyedOffNonCashableCount)
                    }
                };

                var trans = new KeyedCreditsTransaction(1, DateTime.UtcNow, accountType, false, amount);
                trans.TransferredCashableAmount = (accountType == AccountType.Cashable) ? amount : 0;
                trans.TransferredNonCashAmount = (accountType == AccountType.NonCash) ? amount : 0;
                trans.TransferredPromoAmount = (accountType == AccountType.Promo) ? amount : 0;
                using (var scope = _persistentStorageManager.ScopedTransaction())
                {
                    if (_bank.CheckWithdraw(accountType, amount, transactionId) && amount > 0)
                    {
                        _bank.Withdraw(accountType, amount, transactionId);
                        _meterManager.GetMeter(keyedOffMeterDictionary[accountType].amountMeter).Increment(amount);
                        _meterManager.GetMeter(keyedOffMeterDictionary[accountType].countMeter).Increment(1);

                        _transactionHistory.AddTransaction(trans);
                        Logger.Info(
                            $"Keyed off {amount} credits from {accountType} account type [TransactionId={transactionId}");
                    }
                    _transactionCoordinator.ReleaseTransaction(transactionId);
                    scope.Complete();
                }

                _eventBus.Publish(new KeyedCreditOffEvent(trans));
            }

            UpdateCreditData();
            KeyedOnCreditAmount = 0m;
        }

        private void ValidateKeyedOnCredits()
        {
            var error = KeyedOnCreditAmount.Validate(true, _creditLimit - _currentCredits);

            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(nameof(KeyedOnCreditAmount));
            }
            else
            {
                SetError(nameof(KeyedOnCreditAmount), error);
            }
        }

        public class Credit : BaseNotify
        {
            private long _value;
            private string _formattedValue;
            private bool _keyOff;
            private bool _hasCredits;

            public Credit(AccountType accountType, string creditType, string formattedValue, long value, bool keyOff)
            {
                AccountType = accountType;
                CreditType = creditType;
                _formattedValue = formattedValue;
                _value = value;
                _hasCredits = value > 0;
                _keyOff = keyOff;
            }

            public AccountType AccountType { get; }

            public string CreditType { get; }

            public long Value
            {
                get => _value;
                set
                {
                    _value = value;
                    HasCredits = _value > 0;
                }
            }

            public string FormattedValue
            {
                get => _formattedValue;
                set => SetProperty(ref _formattedValue, value, nameof(FormattedValue));
            }

            public bool KeyOff
            {
                get => _keyOff;
                set => SetProperty(ref _keyOff, value, nameof(KeyOff));
            }

            public bool HasCredits
            {
                get => _hasCredits;
                set => SetProperty(ref _hasCredits, value, nameof(HasCredits));
            }
        }
    }
}