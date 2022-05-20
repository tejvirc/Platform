namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.MeterPage;
    using Contracts.OperatorMenu;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;

    public class VerificationTicket : MeterTextTicket
    {
        private const string MetersExtensionPath = "/Application/OperatorMenu/DisplayMeters";
        private const string EgmPaidGameWonAmt = "TotalEgmPaidGameWonAmt";

        private readonly long _lastCashoutSequence;
        private readonly string _title;

        private static readonly string[] MeterOutNames =
        {
            "VoucherOutCashable", "VoucherOutCashablePromotional",
            "VoucherOutNonCashablePromotional"
        };

        private static readonly Dictionary<string, string> VTicketPage2MeterNames = new Dictionary<string, string>();

        private readonly int _pageNumber;

        public VerificationTicket(int pageNumber = 0, string titleOverride = null, long lastCashoutSequence = 0)
        {
            _pageNumber = pageNumber;
            _lastCashoutSequence = lastCashoutSequence;
            _title = string.IsNullOrEmpty(titleOverride) ? TicketLocalizer.GetString(ResourceKeys.VerificationTicketTitle) : titleOverride;
            Title = $"{_title} - PAGE {_pageNumber + 1}";

            var configMeters = ConfigurationUtilities.GetConfiguration(MetersExtensionPath, () => new DisplayMetersConfiguration
            {
                MeterNodes = new MeterNode[0]
            });

            if (!VTicketPage2MeterNames.Any())
            {
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.MainDoorOpenCount), ApplicationMeters.MainDoorOpenCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.MainDoorOpenPowerOffCount), ApplicationMeters.MainDoorOpenPowerOffCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.CashDoorOpenCount), ApplicationMeters.CashDoorOpenCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.CashDoorOpenPowerOffCount), ApplicationMeters.CashDoorOpenPowerOffCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.LogicDoorOpenCount), ApplicationMeters.LogicDoorOpenCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.LogicDoorOpenPowerOffCount), ApplicationMeters.LogicDoorOpenPowerOffCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.TopBoxDoorOpenCount), ApplicationMeters.TopBoxDoorOpenCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.TopBoxDoorOpenPowerOffCount), ApplicationMeters.TopBoxDoorOpenPowerOffCount);

                if (configMeters.MeterNodes.Any(m => m.Name == ApplicationMeters.MainOpticDoorOpenCount))
                {
                    VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.MainOpticDoorOpenCount), ApplicationMeters.MainOpticDoorOpenCount);
                }

                if (configMeters.MeterNodes.Any(m => m.Name == ApplicationMeters.MainOpticDoorOpenPowerOffCount))
                {
                    VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.MainOpticDoorOpenPowerOffShort), ApplicationMeters.MainOpticDoorOpenPowerOffCount);
                }

                if (configMeters.MeterNodes.Any(m => m.Name == ApplicationMeters.TopBoxOpticDoorOpenCount))
                {
                    VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.TopBoxOpticDoorOpenCount), ApplicationMeters.TopBoxOpticDoorOpenCount);
                }

                if (configMeters.MeterNodes.Any(m => m.Name == ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount))
                {
                    VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.TopBoxOpticDoorOpenPowerOffShort), ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount);
                }

                if (configMeters.MeterNodes.Any(m => m.Name == ApplicationMeters.UniversalInterfaceBoxDoorOpenCount))
                {
                    VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.UniversalInterfaceBoxDoorOpenCount), ApplicationMeters.UniversalInterfaceBoxDoorOpenCount);
                }

                if (configMeters.MeterNodes.Any(m => m.Name == ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount))
                {
                    VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.UniversalInterfaceBoxDoorOpenPowerOffCount), ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount);
                }

                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.RetailerAccess), ApplicationMeters.AdministratorAccessCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.TechnicianAccess), ApplicationMeters.TechnicianAccessCount);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.AmountIn), ApplicationMeters.TotalIn);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.AmountOut), ApplicationMeters.TotalOut);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.AmountPlayed), "WageredAmount");
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.AmountWon), EgmPaidGameWonAmt);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.PowerResets), ApplicationMeters.PowerResetCount);
            }
        }

        public override void AddTicketContent()
        {
            var meterManager = ServiceManager.GetService<IMeterManager>();

            using (var scope = TicketLocalizer.NewScope())
            {
                AddLine(scope.GetString(ResourceKeys.MasterText), " ", scope.GetString(ResourceKeys.PeriodText));
                AddDashesLine();
                AddLine(
                    Time.GetLocationTime(meterManager.LastMasterClear).ToString(DateTimeFormat),
                    null,
                    Time.GetLocationTime(meterManager.LastPeriodClear).ToString(DateTimeFormat));

                if (_pageNumber == 0)
                {
                    //page 1
                    var config = ServiceManager.GetService<IOperatorMenuConfiguration>();
                    var showCoinData = config.GetSetting(
                        OperatorMenuSetting.MainMetersPageViewModel,
                        OperatorMenuSetting.CoinsDataVisibility,
                        false);
                    if (!showCoinData)
                    {
                        TicketPageContent(false);
                    }
                    else
                    {
                        TicketPage1Content();
                    }
                }
                else if (_pageNumber == 1)
                {
                    /* Page 2*/
                    long totalVoucherOutCountM = 0;
                    long totalVoucherOutCountP = 0;

                    foreach (var name in MeterOutNames)
                    {
                        var meter = meterManager.GetMeter(name + "Count");
                        totalVoucherOutCountM += meter.Lifetime;
                        totalVoucherOutCountP += meter.Period;
                    }

                    AddLine(
                        string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", totalVoucherOutCountM),
                        "Cashout Tickets",
                        string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", totalVoucherOutCountP));

                    var access = ServiceManager.GetService<IOperatorMenuAccess>();
                    var technicianMode = access?.HasTechnicianMode ?? false;
                    foreach (var dict in VTicketPage2MeterNames)
                    {
                        // VLT-10481 : ignore technician access meter

                        if (!technicianMode && dict.Key == "Technician Access")
                        {
                            continue;
                        }

                        if (dict.Key == "Retailer Access")
                        {
                            AddDashesLine();
                        }

                        if (meterManager.IsMeterProvided(dict.Value))
                        {
                            var meter = meterManager.GetMeter(dict.Value);

                            AddLine(
                                meter.Classification.CreateValueString(meter.Lifetime),
                                dict.Key.Replace(
                                    "Count",
                                    string.Empty), // VLT-11715 : dropped "Count" to give more space
                                meter.Classification.CreateValueString(meter.Period));
                        }
                    }

                    AddDashesLine();
                }
                else if (_pageNumber == 2)
                {
                    /* Page 3*/

                    if (meterManager.IsMeterProvided(ApplicationMeters.TotalIn) &&
                        meterManager.IsMeterProvided(ApplicationMeters.TotalOut))
                    {
                        var meter = meterManager.GetMeter(ApplicationMeters.TotalIn);
                        var meterMasterNet = meter.Lifetime;

                        meter = meterManager.GetMeter(ApplicationMeters.TotalOut);
                        meterMasterNet -= meter.Lifetime;

                        var valueString = meter.Classification.CreateValueString(meterMasterNet);
                        valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");

                        AddLine(
                            scope.GetString(ResourceKeys.NetAmountText),
                            null,
                            valueString);
                    }

                    var creditBalance =
                        Convert.ToDouble(
                            PropertiesManager.GetProperty(PropertyKey.CurrentBalance, 0L)); /* This is in milli-cents*/
                    creditBalance /= 100000.0;

                    AddLine(
                        scope.GetString(ResourceKeys.CreditBalanceText),
                        null,
                        creditBalance.FormattedCurrencyString());

                    AddLine(
                        scope.GetString(ResourceKeys.LastCashoutTicketSequenceText),
                        null,
                        _lastCashoutSequence.ToString());

                    AddDashesLine();

                    AddTicketFooter();
                }
            }
        }

        private void TicketPage1Content()
        {
            long countMaster = 0;
            long countPeriod = 0;
            long totalCoinInMaster = 0;
            long totalCoinInPeriod = 0;
            long totalBillInMaster = 0;
            long totalBillInPeriod = 0;

            var meterManager = ServiceManager.GetService<IMeterManager>();
            var moneyInLifetimeCountDictionary = new Dictionary<string, long>();
            var moneyInCountPeriodDictionary = new Dictionary<string, long>();
            PopulateValidMeters(
                meterManager,
                ref countMaster,
                CoinInMeterNames,
                moneyInLifetimeCountDictionary,
                moneyInCountPeriodDictionary,
                ref countPeriod);

            AddLine(string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", countMaster),
                "Number of Coins Inserted",
                string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", countPeriod));

            /*Add txt to the left string which will be displayed on the left side of the ticket*/
            AddTicketText(moneyInLifetimeCountDictionary, moneyInCountPeriodDictionary, ref totalCoinInMaster, ref totalCoinInPeriod, false);

            AddLine(totalCoinInMaster.FormattedCurrencyString(),
                "Total - Coin In",
                totalCoinInPeriod.FormattedCurrencyString());


            moneyInLifetimeCountDictionary.Clear();
            moneyInCountPeriodDictionary.Clear();
            countMaster = 0;
            countPeriod = 0;

            PopulateValidMeters(
                meterManager,
                ref countMaster,
                BillInMeterNames,
                moneyInLifetimeCountDictionary,
                moneyInCountPeriodDictionary,
                ref countPeriod);

            AddLine(string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", countMaster),
                "Number of Bills Inserted",
                string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", countPeriod));

            /*Add txt to the left string which will be displayed on the left side of the ticket*/
            AddTicketText(moneyInLifetimeCountDictionary, moneyInCountPeriodDictionary, ref totalBillInMaster, ref totalBillInPeriod, false);

            AddLine(totalBillInMaster.FormattedCurrencyString(),
                "Total - Bill In",
                totalBillInPeriod.FormattedCurrencyString());

            var sumMaster = totalBillInMaster + totalCoinInMaster;
            var sumPeriod = totalBillInPeriod + totalCoinInPeriod;
            AddLine(sumMaster.FormattedCurrencyString(),
                "Total - Coins and Bills",
                sumPeriod.FormattedCurrencyString());

            AddDashesLine();

            VoucherData(meterManager);

            GameData(meterManager);
        }
    }
}
