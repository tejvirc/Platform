namespace Aristocrat.Monaco.Sas.UI.Settings
{
    using Application.Contracts.Extensions;
    using MVVM.Model;
    using Newtonsoft.Json;
    using Storage.Models;

    /// <summary>
    ///     Gets the settings for a particular SAS host
    /// </summary>
    public class SasHostSetting : BaseNotify
    {
        private long _accountingDenom;
        private byte _sasAddress;
        private int _hostComPort;

        /// <summary>
        ///     Gets or sets the com port
        /// </summary>
        public int HostComPort
        {
            get => _hostComPort;
            set => SetProperty(ref _hostComPort, value);
        }

        /// <summary>
        ///     Gets or sets the address to use
        /// </summary>
        public byte SasAddress
        {
            get => _sasAddress;
            set => SetProperty(ref _sasAddress, value);
        }

        /// <summary>
        ///     Gets or sets the accounting denom
        /// </summary>
        public long AccountingDenom
        {
            get => _accountingDenom;
            set => SetProperty(ref _accountingDenom, value);
        }

        /// <summary>
        ///     Gets the display value for the accounting denom
        /// </summary>
        [JsonIgnore]
        public string AccountingDenomDisplay => AccountingDenom.CentsToDollars().FormattedCurrencyString();

        /// <summary>
        ///     Performs conversion from <see cref="Host"/> to <see cref="SasHostSetting"/>.
        /// </summary>
        /// <param name="host">The <see cref="Host"/> object.</param>
        public static explicit operator SasHostSetting(Host host) => new SasHostSetting
        {
            HostComPort = host.ComPort,
            AccountingDenom = host.AccountingDenom,
            SasAddress = host.SasAddress
        };

        /// <summary>
        ///     Performs conversion from <see cref="SasHostSetting"/> to <see cref="Host"/>.
        /// </summary>
        /// <param name="setting">The <see cref="SasHostSetting"/> setting.</param>
        public static explicit operator Host(SasHostSetting setting) => new Host
        {
            ComPort = setting.HostComPort,
            AccountingDenom = setting.AccountingDenom,
            SasAddress = setting.SasAddress
        };
    }
}
