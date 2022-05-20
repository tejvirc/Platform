namespace Aristocrat.Monaco.G2S.Handlers.Voucher
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Localization.Properties;
    using Kernel;
    using Kernel.Contracts;
    using Constants = G2S.Constants;

    /// <summary>
    ///     Handles the v21.getVoucherProfile G2S message
    /// </summary>
    public class GetVoucherProfile : ICommandHandler<voucher, getVoucherProfile>
    {
        private readonly IG2SEgm _egm;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVoucherProfile" /> class.
        ///     Creates a new instance of the GetDownloadProfile handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="propertiesManager">Properties Manager</param>
        public GetVoucherProfile(IG2SEgm egm, IPropertiesManager propertiesManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<voucher, getVoucherProfile> command)
        {
            return await Sanction.OwnerAndGuests<IVoucherDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<voucher, getVoucherProfile> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IVoucherDevice>(command.IClass.deviceId);
                if (device == null)
                {
                    return;
                }

                var response = command.GenerateResponse<voucherProfile>();
                var profile = response.Command;
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

                profile.useDefaultConfig = device.UseDefaultConfig;
                profile.allowVoucherIssue = propertiesManager.GetValue(AccountingConstants.VoucherOut, false);
                profile.allowVoucherRedeem = propertiesManager.GetValue(PropertyKey.VoucherIn, false);
                profile.cashOutToVoucher = device.CashOutToVoucher;
                profile.configComplete = device.ConfigComplete;
                if (device.ConfigDateTime != DateTime.MinValue)
                {
                    profile.configDateTimeSpecified = true;
                    profile.configDateTime = device.ConfigDateTime;
                }

                profile.maxOffLinePayOut = device.MaxOffLinePayOut;
                profile.maxOnLinePayOut = device.MaxOnLinePayOut;
                profile.printNonCashOffLine = device.PrintNonCashOffLine;
                profile.configurationId = device.ConfigurationId;
                profile.restartStatus = device.RestartStatus;
                profile.requiredForPlay = device.RequiredForPlay;
                profile.minLogEntries = device.MinLogEntries;
                profile.timeToLive = device.TimeToLive;
                profile.idReaderId = device.IdReaderId;
                profile.combineCashableOut = device.CombineCashableOut;
                profile.allowNonCashOut = propertiesManager.GetValue(AccountingConstants.VoucherOutNonCash, false);
                profile.maxValIds = device.MaxValueIds;
                profile.minLevelValIds = device.MinLevelValueIds;
                profile.valIdListRefresh = device.ValueIdListRefresh;
                profile.valIdListLife = device.ValueIdListLife;
                profile.voucherHoldTime = device.VoucherHoldTime;
                profile.printOffLine = device.PrintOffLine;

                profile.expireCashPromo = propertiesManager.GetValue(AccountingConstants.VoucherOutExpirationDays, AccountingConstants.DefaultVoucherExpirationDays);
                profile.expireNonCash = propertiesManager.GetValue(AccountingConstants.VoucherOutNonCashExpirationDays, AccountingConstants.DefaultVoucherExpirationDays);
                profile.printExpCashPromo = device.PrintExpirationCashPromo;
                profile.printExpNonCash = device.PrintExpirationNonCash;

                SetPropValues(profile);
            }

            await Task.CompletedTask;
        }

        private static string Truncate(string source, int length)
        {
            if (source.Length > length)
            {
                source = source.Substring(0, length);
            }

            return source;
        }

        private void SetPropValues(voucherProfile profile)
        {
            profile.propName = Truncate(
                _propertiesManager.GetValue(PropertyKey.TicketTextLine1, string.Empty),
                Constants.VoucherTitle40);
            profile.propLine1 = Truncate(
                _propertiesManager.GetValue(PropertyKey.TicketTextLine2, string.Empty),
                Constants.VoucherTitle40);
            profile.propLine2 = Truncate(
                _propertiesManager.GetValue(PropertyKey.TicketTextLine3, string.Empty),
                Constants.VoucherTitle40);

            profile.titleCash = Truncate(
                _propertiesManager.GetValue(AccountingConstants.TicketTitleCash, AccountingConstants.DefaultCashoutTicketTitle),
                Constants.VoucherTitle16);
            profile.titlePromo = Truncate(
                _propertiesManager.GetValue(
                    AccountingConstants.TicketTitlePromo,
                    AccountingConstants.DefaultNonCashTicketTitle),
                Constants.VoucherTitle16);
            profile.titleNonCash = Truncate(
                string.IsNullOrEmpty(_propertiesManager.GetValue(AccountingConstants.TicketTitleNonCash, string.Empty))
                    ? Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PlayableOnly)
                    : _propertiesManager.GetValue(AccountingConstants.TicketTitleNonCash, string.Empty),
                Constants.VoucherTitle16);
            profile.titleLargeWin = Truncate(
                _propertiesManager.GetValue(
                    AccountingConstants.TicketTitleLargeWin,
                    AccountingConstants.DefaultLargeWinTicketTitle),
                Constants.VoucherTitle16);
            profile.titleBonusCash = Truncate(
                _propertiesManager.GetValue(AccountingConstants.TicketTitleBonusCash, string.Empty),
                Constants.VoucherTitle16);
            profile.titleBonusPromo = Truncate(
                _propertiesManager.GetValue(AccountingConstants.TicketTitleBonusPromo, string.Empty),
                Constants.VoucherTitle16);
            profile.titleBonusNonCash = Truncate(
                _propertiesManager.GetValue(AccountingConstants.TicketTitleBonusNonCash, string.Empty),
                Constants.VoucherTitle16);
            profile.titleWatCash = Truncate(
                _propertiesManager.GetValue(AccountingConstants.TicketTitleWatCash, string.Empty),
                Constants.VoucherTitle16);
            profile.titleWatPromo = Truncate(
                _propertiesManager.GetValue(AccountingConstants.TicketTitleWatPromo, string.Empty),
                Constants.VoucherTitle16);
            profile.titleWatNonCash = Truncate(
                string.IsNullOrEmpty(_propertiesManager.GetValue(AccountingConstants.TicketTitleNonCash, string.Empty))
                    ? Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PlayableOnly)
                    : _propertiesManager.GetValue(AccountingConstants.TicketTitleNonCash, string.Empty),
                Constants.VoucherTitle16);

            profile.redeemPrefix = _propertiesManager.GetValue(AccountingConstants.RedeemText, string.Empty);
        }
    }
}