namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Localization.Properties;

    public class MeterNameConfigurationBuilder
    {
        private const string MetersExtensionPath = "/Gaming/OperatorMenu/GameHistoryMeters";

        public static Dictionary<string,
                                (string label, bool indent, bool occurrence, int order)>
            BuildMeterNames(bool isGameTransactionPage)
        {
            var configMeters = ConfigurationUtilities.GetConfiguration(MetersExtensionPath,
                () => new GameHistoryMetersConfiguration()
                {
                    MeterNodes = Array.Empty<GameHistoryMeterNode>()
                });

            var meters = configMeters.MeterNodes;

            if (!meters.Any())
            {
                // If no GameHistoryMeters.config.xml provided, fall back to this list of meters.
                // TODO: This should be cleaned up (either a default xml, or adding GameHistoryMeters.config.xml to all juris.)
                return new Dictionary<string, (string label, bool indent, bool occurrence, int order)>
                {
                    { "Credit", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Credit), false, false, 1) },
                    { "BetTurnover", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BetTurnover), false, false, 2) },
                    { "TotalWin", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TotalWin), false, false, 3) },
                    { "MachinePaid", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MachinePaid), true, false, 4) },
                    { "AttendantPaidJackpot", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AttendantPaidJackpot), true, false, 5) },
                    { "TotalMoneyIn", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TotalMoneyIn), false, false, 6) },
                    { "PhysicalCoinIn", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalCoinIn), true, false, 7) },
                    { "BillIn", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BillIn), true, false, 8) },
                    { "VoucherIn", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherIn), true, false, 9) },
                    { "TransferIn", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TransferIn), true, false, 10) },
                    { "TotalMoneyOut", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TotalMoneyOut), false, false, 11) },
                    { "PhysicalCoinOut", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalCoinOut), true, false, 12) },
                    { "VoucherOut", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherOut), true, false, 13) },
                    { "TransferOut", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TransferOut), true, false, 14) },
                    { "AttendantPaidCancelledCredit", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AttendantPaidCancelledCredit), true, false, 15) },
                    { "CoinDropCashBox", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinDropCashBox), false, false, 16) },
                    { "ExtraCoinOutRunaway", (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExtraCoinOutRunaway), false, false, 17) }
                };
            }

            var meterNames = new Dictionary<string, (string label, bool indent, bool occurrence, int order)>();

            var order = 1;
            foreach (var meter in meters)
            {
                if (isGameTransactionPage && meter.HideFromTransactionsPage) continue;

                meter.Order = order;
                meterNames.Add(meter.Name, (meter.DisplayName, meter.Indent, meter.Occurrence, meter.Order));
                order++;
            }

            return meterNames;
        }
    }
}
