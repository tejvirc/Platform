namespace Aristocrat.Monaco.Sas.UI.Settings
{
    using Application.Contracts.Extensions;
    using Contracts.Client;
    using MVVM.Model;
    using Newtonsoft.Json;

    /// <summary>
    ///     Contains the settings for accounting denoms.
    /// </summary>
    public class AccountingDenomsSetting : BaseNotify
    {
        private long _host0Denom;
        private long _host1Denom;

        /// <summary>
        ///     Gets or sets host 0 denom.
        /// </summary>
        public long Host0Denom
        {
            get => _host0Denom;

            set
            {
                SetProperty(ref _host0Denom, value);
                RaisePropertyChanged(nameof(Host0DenomDisplay));
            }
        }

        /// <summary>
        ///     Gets the host 0 denom to display.
        /// </summary>
        [JsonIgnore]
        public string Host0DenomDisplay => _host0Denom.CentsToDollars().FormattedCurrencyString();

        /// <summary>
        ///     Gets or sets host 1 denom.
        /// </summary>
        public long Host1Denom
        {
            get => _host1Denom;

            set
            {
                SetProperty(ref _host1Denom, value);
                RaisePropertyChanged(nameof(Host1DenomDisplay));
            }
        }

        /// <summary>
        ///     Gets the host 1 denom to display.
        /// </summary>
        [JsonIgnore]
        public string Host1DenomDisplay => _host1Denom.CentsToDollars().FormattedCurrencyString();

        /// <summary>
        ///     Performs conversion from <see cref="AccountingDenoms"/> to <see cref="AccountingDenomsSetting"/>.
        /// </summary>
        /// <param name="accountingDenoms">The <see cref="AccountingDenoms"/> object.</param>
        public static explicit operator AccountingDenomsSetting(AccountingDenoms accountingDenoms) => new AccountingDenomsSetting
        {
            Host0Denom = accountingDenoms.Host0Denom,
            Host1Denom = accountingDenoms.Host1Denom
        };

        /// <summary>
        ///     Performs conversion from <see cref="AccountingDenomsSetting"/> to accounting denoms.
        /// </summary>
        /// <param name="setting">The <see cref="AccountingDenomsSetting"/> setting.</param>
        public static explicit operator AccountingDenoms(AccountingDenomsSetting setting) => new AccountingDenoms
        {
            Host0Denom = setting.Host0Denom,
            Host1Denom = setting.Host1Denom
        };
    }
}
