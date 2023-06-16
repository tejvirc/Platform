namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Aristocrat.Monaco.Hardware.Contracts.Ticket;
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
            Title = $"{_title} - {TicketLocalizer.GetString(ResourceKeys.PageText).ToUpper(TicketLocalizer.CurrentCulture)} {_pageNumber + 1}";

            var configMeters = ConfigurationUtilities.GetConfiguration(MetersExtensionPath, () => new DisplayMetersConfiguration
            {
                MeterNodes = new MeterNode[0]
            });

            if (pageNumber is 1) // No need to remake this dictionary for every page, only used on page 2 (index 1)
            {
                if (VTicketPage2MeterNames.Any())
                {
                    VTicketPage2MeterNames.Clear(); // Need to clear this to account for possible language change
                }

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
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.AmountInText), ApplicationMeters.TotalIn);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.AmountOutText), ApplicationMeters.TotalOut);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.AmountPlayed), "WageredAmount");
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.AmountWon), EgmPaidGameWonAmt);
                VTicketPage2MeterNames.Add(TicketLocalizer.GetString(ResourceKeys.PowerResets), ApplicationMeters.PowerResetCount);
            }
        }

        public override Ticket CreateSecondPageTextTicket()
        {
            Title = $"{Title} (2)";
            AddCasinoInfo();
            AddTicketHeader();
            AddLine(TicketLocalizer.GetString(ResourceKeys.MasterText), " ", TicketLocalizer.GetString(ResourceKeys.PeriodText));
            AddDashesLine();
            AddLine(
                Time.GetLocationTime(MeterManager.LastMasterClear).ToString(DateTimeFormat),
                null,
                Time.GetLocationTime(MeterManager.LastPeriodClear).ToString(DateTimeFormat));

            return CreateTicket(Title);
        }

        public override void AddTicketContent()
        {

            using (var scope = TicketLocalizer.NewScope())
            {
                AddLine(scope.GetString(ResourceKeys.MasterText), " ", scope.GetString(ResourceKeys.PeriodText));
                AddDashesLine();
                AddLine(
                    Time.GetLocationTime(MeterManager.LastMasterClear).ToString(DateTimeFormat),
                    null,
                    Time.GetLocationTime(MeterManager.LastPeriodClear).ToString(DateTimeFormat));

                if (_pageNumber == 0)
                {
                    //page 1
                    var showCoinData = OperatorMenuConfig?.GetSetting(OperatorMenuSetting.MainMetersPageViewModel, OperatorMenuSetting.CoinsDataVisibility, false) ?? false;
                    var useCountLines = OperatorMenuConfig?.GetSetting(OperatorMenuSetting.MainMetersPageViewModel, OperatorMenuSetting.BillDataInCountState, true) ?? true;

                    TicketPageContent(useCountLines, true, showCoinData);
                }
                else if (_pageNumber == 1)
                {
                    /* Page 2*/
                    long totalVoucherOutCountM = 0;
                    long totalVoucherOutCountP = 0;

                    foreach (var name in MeterOutNames)
                    {
                        var meter = MeterManager.GetMeter(name + "Count");
                        totalVoucherOutCountM += meter.Lifetime;
                        totalVoucherOutCountP += meter.Period;
                    }

                    AddLine(
                        string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", totalVoucherOutCountM),
                        TicketLocalizer.GetString(ResourceKeys.CashoutTickets),
                        string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", totalVoucherOutCountP));

                    var access = ServiceManager.GetService<IOperatorMenuAccess>();
                    var technicianMode = access?.HasTechnicianMode ?? false;
                    foreach (var dict in VTicketPage2MeterNames)
                    {
                        // VLT-10481 : ignore technician access meter

                        if (!technicianMode && dict.Key == TicketLocalizer.GetString(ResourceKeys.TechnicianAccess))
                        {
                            continue;
                        }

                        if (dict.Key == TicketLocalizer.GetString(ResourceKeys.RetailerAccess))
                        {
                            AddDashesLine();
                        }

                        if (MeterManager.IsMeterProvided(dict.Value))
                        {
                            var meter = MeterManager.GetMeter(dict.Value);

                            AddLine(
                                meter.Classification.CreateValueString(meter.Lifetime, TicketLocalizer.CurrentCulture),
                                dict.Key.Replace(
                                    "Count",
                                    string.Empty), // VLT-11715 : dropped "Count" to give more space
                                meter.Classification.CreateValueString(meter.Period, TicketLocalizer.CurrentCulture));
                        }
                    }

                    AddDashesLine();
                }
                else if (_pageNumber == 2)
                {
                    /* Page 3*/

                    if (MeterManager.IsMeterProvided(ApplicationMeters.TotalIn) &&
                        MeterManager.IsMeterProvided(ApplicationMeters.TotalOut))
                    {
                        var meter = MeterManager.GetMeter(ApplicationMeters.TotalIn);
                        var meterMasterNet = meter.Lifetime;

                        meter = MeterManager.GetMeter(ApplicationMeters.TotalOut);
                        meterMasterNet -= meter.Lifetime;

                        var valueString = meter.Classification.CreateValueString(meterMasterNet, TicketLocalizer.CurrentCulture);
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
                        creditBalance.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));

                    AddLine(
                        scope.GetString(ResourceKeys.LastCashoutTicketSequenceText),
                        null,
                        _lastCashoutSequence.ToString());

                    AddDashesLine();

                    AddTicketFooter();
                }
            }
        }
    }
}
