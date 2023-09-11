namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Common;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Helpers;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using Settings;

    public class MachineInfoTicket : TextTicket
    {
        public MachineInfoTicket()
            : base(Localizer.For(CultureFor.OperatorTicket))
        {
            Title = TicketLocalizer.GetString(ResourceKeys.MachineInfoTicketTitle);
        }

        public override void AddTicketContent()
        {
            AddMachineSettings();
        }

        public List<Ticket> CreateTickets()
        {
            var result = new List<Ticket>();

            AddCasinoInfo();
            AddTicketHeader();
            AddTicketContent();

            result.Add(CreateTicket(Title + " - " + TicketLocalizer.FormatString(ResourceKeys.PageNumber, 1, 2)));
            result.Add(CreateSecondPage());
            return result;
        }

        private Ticket CreateSecondPage()
        {
            ClearFields();
            AddMachineSettingsSecondPage();
            return CreateTicket(Title + " - " + TicketLocalizer.FormatString(ResourceKeys.PageNumber, 2, 2));
        }

        private void AddMachineSettingsSecondPage()
        {
            var lineLength = TicketCreatorHelper.MaxCharPerLine;
            AddLine(null, null, null);
            AddLine(null, Dashes, null);

            var model = ServiceManager.GetService<IIO>().DeviceConfiguration.Model;
            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.ModelLabel)}:",
                null,
                string.IsNullOrWhiteSpace(model) ? TicketLocalizer.GetString(ResourceKeys.DataUnavailable) : model);
            var electronics = ServiceManager.GetService<IIO>().GetElectronics();
            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.Electronics)}:",
                null,
                string.IsNullOrWhiteSpace(electronics) ? TicketLocalizer.GetString(ResourceKeys.DataUnavailable) : electronics);

            var cardInfo = ServiceManager.GetService<IDisplayService>().GraphicsCard;
            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.GraphicsCard)}:",
                null,
                string.IsNullOrWhiteSpace(cardInfo) ? TicketLocalizer.GetString(ResourceKeys.DataUnavailable) : cardInfo);

            var buttonDeck = MachineSettingsUtilities.GetButtonDeckIdentification(Localizer.For(CultureFor.Operator));
            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.ButtonDeck)}:",
                null,
                string.IsNullOrWhiteSpace(buttonDeck) ? TicketLocalizer.GetString(ResourceKeys.DataUnavailable) : buttonDeck);

            var edgeLighting = ServiceManager.TryGetService<IEdgeLightingController>();
            var edgeLightingStr = TicketLocalizer.GetString(ResourceKeys.DataUnavailable);
            if (edgeLighting != null)
            {
                edgeLightingStr = MachineSettingsUtilities.GetLightingIdentification(Localizer.For(CultureFor.Operator));
            }
            AddLine($"{TicketLocalizer.GetString(ResourceKeys.EdgeLighting)}:", null, edgeLightingStr);

            var displayCount = 1;
            foreach (var display in MachineSettingsUtilities.GetTouchScreenIdentificationsWithoutVbd(TicketLocalizer))
            {
                if (displayCount == 1)
                {
                    AddLine($"{TicketLocalizer.GetString(ResourceKeys.TouchScreen)}:", null, display);
                }
                else if (displayCount > 1)
                {
                    AddLine(null, null, display);
                }
                displayCount++;
            }

            var noteAcceptor = ServiceManager.TryGetService<INoteAcceptor>();
            var availableSpaceNote = lineLength - ($"{TicketLocalizer.GetString(ResourceKeys.NoteAcceptorText)}:".Length + 1);
            var wordsNote = (noteAcceptor?.DeviceConfiguration.GetDeviceStatus(false) ?? TicketLocalizer.GetString(ResourceKeys.DataUnavailable)).ConvertStringToWrappedWords(availableSpaceNote);

            if (string.IsNullOrEmpty(wordsNote[1]))
            {
                AddLine($"{TicketLocalizer.GetString(ResourceKeys.NoteAcceptorText)}:", null, $"{wordsNote[0]}");
            }
            else
            {
                var wordsCount = wordsNote.Count;

                for (var j = 0; j <= wordsCount - 1; j++)
                {
                    AddLine(j == 0 ? $"{TicketLocalizer.GetString(ResourceKeys.NoteAcceptorText)}:" : null, null, $"{wordsNote[j]}");
                }
            }

            var printer = ServiceManager.TryGetService<IPrinter>();
            var availableSpacePrinter = lineLength - ($"{TicketLocalizer.GetString(ResourceKeys.PrinterText)}:".Length + 1);
            var wordsPrinter = (printer?.DeviceConfiguration.GetDeviceStatus(false) ?? TicketLocalizer.GetString(ResourceKeys.DataUnavailable)).ConvertStringToWrappedWords(availableSpacePrinter);

            if (string.IsNullOrEmpty(wordsPrinter[1]))
            {
                AddLine($"{TicketLocalizer.GetString(ResourceKeys.PrinterText)}:", null, $"{wordsPrinter[0]}");
            }
            else
            {
                var wordsCount = wordsPrinter.Count;

                for (var j = 0; j <= wordsCount - 1; j++)
                {
                    AddLine(j == 0 ? $"{TicketLocalizer.GetString(ResourceKeys.PrinterText)}:" : null, null, $"{wordsPrinter[j]}");
                }
            }

            AddLine(null, null, null);
        }
        private void AddMachineSettings()
        {
            AddLine(null, Dashes, null);

            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.SerialNumberText)}:",
                null,
                string.Format(
                    CultureInfo.CurrentCulture,
                    $"{PropertiesManager.GetValue(ApplicationConstants.SerialNumber, TicketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));

            var machineId = PropertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
            var assetNumber = string.Empty;
            if (machineId != 0)
            {
                assetNumber = machineId.ToString();
            }

            AddLine(
            $"{TicketLocalizer.GetString(ResourceKeys.AssetNumber)}:",
            null,
            string.Format(
                CultureInfo.CurrentCulture,
               assetNumber));

            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageAreaOverride))
            {
                AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.AreaLabel)}:",
                null,
                string.Format(
                    CultureInfo.CurrentCulture,
                    $"{PropertiesManager.GetValue(ApplicationConstants.Area, TicketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));
            }

            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride))
            {
                AddLine(
               $"{TicketLocalizer.GetString(ResourceKeys.ZoneText)}:",
               null,
               string.Format(
                   CultureInfo.CurrentCulture,
                   $"{PropertiesManager.GetValue(ApplicationConstants.Zone, TicketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));
            }

            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageBankOverride))
            {
                AddLine(
               $"{TicketLocalizer.GetString(ResourceKeys.BankText)}:",
               null,
               string.Format(
                   CultureInfo.CurrentCulture,
                   $"{PropertiesManager.GetValue(ApplicationConstants.Bank, TicketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));
            }

            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPagePositionOverride))
            {
                AddLine(
               $"{TicketLocalizer.GetString(ResourceKeys.PositionText)}:",
               null,
               string.Format(
                   CultureInfo.CurrentCulture,
                   $"{PropertiesManager.GetValue(ApplicationConstants.Position, TicketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));
            }

            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageLocationOverride))
            {
                AddLine(
               $"{TicketLocalizer.GetString(ResourceKeys.Location)}:",
               null,
               string.Format(
                   CultureInfo.CurrentCulture,
                   $"{PropertiesManager.GetValue(ApplicationConstants.Location, TicketLocalizer.GetString(ResourceKeys.DataUnavailable))}"));
            }

            var bios = ServiceManager.GetService<IIO>().GetFirmwareVersion(FirmwareData.Bios);
            AddLine($"{TicketLocalizer.GetString(ResourceKeys.BiosVersion)}:", null, string.IsNullOrWhiteSpace(bios) ? TicketLocalizer.GetString(ResourceKeys.DataUnavailable) : bios);

            var fpga = ServiceManager.GetService<IIO>().GetFirmwareVersion(FirmwareData.Fpga);
            AddLine($"{TicketLocalizer.GetString(ResourceKeys.FpgaVersion)}:", null, string.IsNullOrWhiteSpace(fpga) ? TicketLocalizer.GetString(ResourceKeys.DataUnavailable) : fpga);

            AddLine($"{TicketLocalizer.GetString(ResourceKeys.WindowsVersionLabel)}:", null, Environment.OSVersion.Version.ToString());

            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.OSImageVersionText)}:",
                null,
                ServiceManager.TryGetService<IOSService>()?.OsImageVersion.ToString() ?? TicketLocalizer.GetString(ResourceKeys.DataUnavailable));

            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.PlatformVersionText)}:",
                null,
                PropertiesManager.GetValue(KernelConstants.SystemVersion, TicketLocalizer.GetString(ResourceKeys.DataUnavailable)));

            AddLine(null, null, null);
        }
    }
}
