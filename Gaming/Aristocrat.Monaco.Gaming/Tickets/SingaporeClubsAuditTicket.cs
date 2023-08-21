namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Tickets;
    using Common;
    using Contracts;
    using Contracts.Progressives;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;

    public class SingaporeClubsAuditTicket : MeterTextTicket, ISingaporeClubsAuditTicket
    {
        private const int ProgHistoryCount = 10;

        private readonly List<Tuple<string, IList<string>>> _mainMeters = new List<Tuple<string, IList<string>>>
        {
            // Label: Turnover
            Tuple.Create<string, IList<string>>(
                ResourceKeys.TurnoverText,
                new List<string> { GamingMeters.WageredAmount }),
            // Label: Total Wins
            Tuple.Create<string, IList<string>>(
                ResourceKeys.TotalWins,
                new List<string> { GamingMeters.TotalEgmPaidAmt }),
            // Label: Cash Box
            Tuple.Create<string, IList<string>>(ResourceKeys.CashBox, new List<string> { AccountingMeters.CoinDrop }),
            // Label: Cancelled Credits
            Tuple.Create<string, IList<string>>(
                ResourceKeys.CanceledCreditsTransactionName,
                new List<string> { AccountingMeters.HandpaidCancelAmount }),
            // Label: Coin In
            Tuple.Create<string, IList<string>>(
                ResourceKeys.CoinInText,
                new List<string> { AccountingMeters.TrueCoinIn }),
            // Label: Coin Out
            Tuple.Create<string, IList<string>>(
                ResourceKeys.CoinOutText,
                new List<string> { AccountingMeters.TrueCoinOut }),
            // Label: Bills In
            Tuple.Create<string, IList<string>>(
                ResourceKeys.BillIn,
                new List<string> { AccountingMeters.CurrencyInAmount }),
            // Label: Money In
            Tuple.Create<string, IList<string>>(
                ResourceKeys.AmountIn,
                new List<string> { ApplicationMeters.TotalIn }),
            // Label: Money Out
            Tuple.Create<string, IList<string>>(
                ResourceKeys.AmountOut,
                new List<string> { ApplicationMeters.TotalOut }),
            // Label: Cashless In
            Tuple.Create<string, IList<string>>(
                ResourceKeys.CashlessInText,
                new List<string> { AccountingMeters.ElectronicTransfersOnTotalAmount }),
            // Label: Cashless Out
            Tuple.Create<string, IList<string>>(
                ResourceKeys.CashlessOutText,
                new List<string> { AccountingMeters.ElectronicTransfersOffTotalAmount }),
            // Label: Games Played
            Tuple.Create<string, IList<string>>(
                ResourceKeys.GamesPlayed,
                new List<string> { GamingMeters.PlayedCount }),
            // Label: Main Door Access
            Tuple.Create<string, IList<string>>(
                ResourceKeys.MainDoorAccessCount,
                new List<string> { ApplicationMeters.MainDoorOpenTotalCount }),
            // Label: Power Reset
            Tuple.Create<string, IList<string>>(
                ResourceKeys.PowerResetText,
                new List<string> { ApplicationMeters.PowerResetCount }),
            // Label: Ticket In
            Tuple.Create<string, IList<string>>(
                ResourceKeys.TicketInText,
                new List<string> { AccountingMeters.TotalVoucherInCashableAndPromoAmount }),
            // Label: Ticket Out
            Tuple.Create<string, IList<string>>(
                ResourceKeys.TicketOutText,
                new List<string> { AccountingMeters.TotalVoucherOutCashableAndPromoAmount }),
            // Label: Jackpot Wins
            Tuple.Create<string, IList<string>>(
                ResourceKeys.JackpotWinsText,
                new List<string> { GamingMeters.TotalHandPaidAmt }),
            // Label: Jackpot Occurrences
            Tuple.Create<string, IList<string>>(
                ResourceKeys.JackpotOccurencesText,
                new List<string> { GamingMeters.TotalJackpotWonCount }),
            // Label: Attendant Paid Progressive
            Tuple.Create<string, IList<string>>(
                ResourceKeys.AttendantPaidProgText,
                new List<string> { GamingMeters.HandPaidProgWonAmount }),
            // Label: Machine Paid Progressive
            Tuple.Create<string, IList<string>>(
                ResourceKeys.MachinePaidProgText,
                new List<string> { GamingMeters.EgmPaidProgWonAmount }),
            // Label: Progressive Occurrences
            Tuple.Create<string, IList<string>>(
                ResourceKeys.ProgOccurencesText,
                new List<string> { GamingMeters.TotalProgWonCount }),
            // Label: Current Credits
            Tuple.Create<string, IList<string>>(
                ResourceKeys.CreditText,
                new List<string> { AccountingMeters.CurrentCredits })
        };

        private readonly IMeterManager _meterManager;
        private readonly IGameProvider _gameProvider;
        private readonly IProgressiveLevelProvider _progressives;
        private readonly ITransactionHistory _transactions;

        public SingaporeClubsAuditTicket()
        {
            Title = string.Empty;

            _meterManager = ServiceManager.GetService<IMeterManager>();
            _gameProvider = ServiceManager.GetService<IGameProvider>();
            _transactions = ServiceManager.GetService<ITransactionHistory>();
            _progressives = ServiceManager.GetService<IProgressiveLevelProvider>();
        }

        public Ticket CreateMainMetersPage()
        {
            ClearFields();

            AddTicketHeader();

            var title = TicketLocalizer.GetString(ResourceKeys.MainMetersLabel);
            AddLine(string.Empty, string.Empty, string.Empty);
            AddLine(string.Empty, title, string.Empty);
            AddLine(string.Empty, string.Empty, string.Empty);

            foreach (var (label, meters) in _mainMeters)
            {
                var availableMeters = meters.Where(x => _meterManager.IsMeterProvided(x)).ToList();
                if (!availableMeters.Any())
                {
                    continue;
                }

                var classification = _meterManager.GetMeter(availableMeters.First()).Classification; // Both should match so just grab the first one
                var meterSum = availableMeters.Sum(meter => _meterManager.GetMeter(meter).Lifetime);

                if (classification.GetType() == typeof(CurrencyMeterClassification))
                {
                    AddMeterLine(TicketLocalizer.GetString(label), FromMillicents(meterSum));
                }
                else
                {
                    AddMeterLine(TicketLocalizer.GetString(label), meterSum, "{0}");
                }
            }

            return CreateTicket(Title);
        }

        public Ticket CreateProgressiveHistoryPage()
        {
            ClearFields();

            AddTicketHeader();

            var title = TicketLocalizer.GetString(ResourceKeys.LinkProgHistoryLabel);
            AddLine(string.Empty, string.Empty, string.Empty);
            AddLine(string.Empty, title, string.Empty);
            AddLine(string.Empty, string.Empty, string.Empty);

            var progressiveLevels = _progressives.GetProgressiveLevels();
            var index = 1;
            var ordinal = TicketLocalizer.GetString(ResourceKeys.Last);

            var linkedProgressiveData = _transactions.RecallTransactions<JackpotTransaction>()
                .OrderByDescending(t => t.TransactionId).Select(
                    j => (level: progressiveLevels.FirstOrDefault(
                        l => l.GameId == j.GameId && l.Denomination.Contains(j.DenomId) &&
                             l.ProgressiveId == j.ProgressiveId && l.LevelId == j.LevelId), jackpot: j))
                .Where(x => x.level?.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked)
                .Take(ProgHistoryCount);
            foreach (var (level, jackpot) in linkedProgressiveData)
            {
                AddLine(
                ordinal,
                $"{level.LevelName}",
                $"{jackpot.PaidAmount.MillicentsToDollars().FormattedCurrencyString()}");
                index++;
                ordinal = index.ToOrdinal(TicketLocalizer.CurrentCulture).CapitalizeFirstCharacter();
            }

            for (; index <= ProgHistoryCount; ++index)
            {
                AddLine(ordinal, string.Empty, ResourceKeys.NotAvailable);
                ordinal = (index + 1).ToOrdinal(TicketLocalizer.CurrentCulture).CapitalizeFirstCharacter();
            }

            return CreateTicket(Title);
        }

        /// <inheritdoc />
        public override int TicketHeaderLineCount => 8;

        /// <summary>
        /// </summary>
        public override void AddTicketHeader()
        {
            using (var scope = new CultureScope(CultureFor.Player))
            {
                var now = ServiceManager.GetService<ITime>().GetLocationTime(DateTime.UtcNow);
                // Label: Date
                AddLine(
                    $"{scope.GetString(ResourceKeys.DateText)}:",
                    string.Empty,
                    now.ToString(DateTimeFormatInfo.ShortDatePattern)
                );

                // Label: Time
                AddLine(
                    $"{scope.GetString(ResourceKeys.TimeText)}:",
                    string.Empty,
                    now.ToString(TimeFormat)
                );
            }

            // Label: Asset Number
            var assetNum = PropertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
            AddLine($"{Resources.AssetNumber}:", string.Empty, assetNum.ToString());

            // Label: Serial Number
            var serialNum = PropertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            AddLine($"{Resources.SerialNumberLabel}:", string.Empty, serialNum);

            // Label: Platform ID
            var systemId = PropertiesManager.GetProperty(KernelConstants.SystemVersion, "0.0.0.0");
            AddLine($"{Resources.PlatformIdText}:", string.Empty, systemId.ToString());

            // Label: Game Id - only report it if there is 1 game enabled
            var enabledGames = _gameProvider.GetEnabledGames();
            var themeId = enabledGames.Count == 1 ? enabledGames.First().Version : Resources.MultiGameText;
            AddLine($"{Resources.GameId}:", string.Empty, themeId);

            // Label: Game Name - only report it if there is 1 game enabled, otherwise report "Multi-Game" if more than 1
            var gameName = enabledGames.Count == 0 ? string.Empty :
                enabledGames.Count == 1 ? enabledGames.First().ThemeName :
                Resources.MultiGameText;
            AddLine($"{Resources.GameName}:", string.Empty, gameName);

            // Label: Game Denomination - report it if there is 1 denom enabled, otherwise report "Multi-Denom" if more than 1
            var gameDenom = string.Empty;
            if (enabledGames.Count > 0)
            {
                var uniqueActiveDenoms = enabledGames
                    .SelectMany(game => game.Denominations)
                    .Where(denom => denom.Active)
                    .Select(denom => denom.Value)
                    .Distinct()
                    .ToList();

                gameDenom = uniqueActiveDenoms.Count == 1
                    ? (uniqueActiveDenoms.First().MillicentsToDollars()).FormattedCurrencyString()
                    : Resources.MultiDenomText;
            }

            AddLine($"{Resources.GameDenomText}:", string.Empty, gameDenom);
        }

        private void AddMeterLine(string labelText, long meterVal, string meterFormatString = "")
        {
            AddLine(
                $"{labelText}:",
                string.Empty,
                string.IsNullOrEmpty(meterFormatString)
                    ? meterVal.FormattedCurrencyString()
                    : string.Format(CultureInfo.CurrentCulture, meterFormatString, meterVal));
        }

        private void AddMeterLine(string labelText, decimal meterVal, string meterFormatString = "")
        {
            AddLine(
                $"{labelText}:",
                string.Empty,
                string.IsNullOrEmpty(meterFormatString)
                    ? meterVal.FormattedCurrencyString()
                    : string.Format(CultureInfo.CurrentCulture, meterFormatString, meterVal));
        }
    }
}