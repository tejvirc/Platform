namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Kernel;
    using Localization.Properties;

    [CLSCompliant(false)]
    public class CurrentCreditsPageViewModel : OperatorMenuPageViewModelBase
    {
        private readonly Dictionary<AccountType, string> _displayCredits = new Dictionary<AccountType, string>
        {
            { AccountType.Cashable, ResourceKeys.Cashable },
            { AccountType.Promo, ResourceKeys.CashablePromotional },
            { AccountType.NonCash, ResourceKeys.NonCashablePromotional }
        };

        private readonly IBank _bank;
        private List<Credit> _credits;
        private string _totalCredits;

        public CurrentCreditsPageViewModel()
            : this(
                ServiceManager.GetInstance().TryGetService<IBank>())
        {
        }

        public CurrentCreditsPageViewModel(
            IBank bank)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        public List<Credit> Credits
        {
            get => _credits;
            private set => SetProperty(ref _credits, value, nameof(Credits));
        }

        public string TotalCredits
        {
            get => _totalCredits;
            private set => SetProperty(ref _totalCredits, value, nameof(TotalCredits));
        }

        protected override void OnLoaded()
        {
            var credits = new List<Credit>();
            using (var scope = new CultureScope(CultureFor.Operator))
            {
                foreach (var credit in _displayCredits)
                {
                    var name = GetConfigSetting<string>(credit.Key.ToString(), null) ?? scope.GetString(credit.Value);

                    // If empty then do not show this credit type
                    if (name != string.Empty)
                    {
                        credits.Add(new Credit(name,
                            _bank.QueryBalance(credit.Key).MillicentsToDollars().FormattedCurrencyString()));
                    }
                }

                Credits = credits;
                TotalCredits = _bank.QueryBalance().MillicentsToDollars().FormattedCurrencyString();
            }
        }

        public class Credit
        {
            public Credit(string accountType, string formattedBalance)
            {
                AccountType = accountType;
                FormattedBalance = formattedBalance;
            }

            public string AccountType { get; }

            public string FormattedBalance { get; }
        }
    }
}