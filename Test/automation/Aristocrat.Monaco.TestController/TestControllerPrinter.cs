namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Hardware.Gds;
    using Aristocrat.Monaco.Hardware.Printer;
    using DataModel;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Newtonsoft.Json;

    public partial class TestControllerEngine
    {
        private const string PrinterConnectMessage = "PrinterConnect";

        private const string PrinterDisconnectMessage = "PrinterDisconnect";

        public CommandResult PrinterConnect(string id)
        {
            bool result = false;
            var printer = ServiceManager.GetInstance().GetService<IPrinter>();

            if (printer != null)
            {
                var adapter = printer as PrinterAdapter;
                switch (adapter?.PrinterImplementation)
                {
                    case GdsDeviceBase gds:
                        gds.Open();
                        result = gds.Initialize(gds.Communicator).Result;
                        break;
                    case IPrinterImplementation impl:
                        result = impl.Open();
                        break;
                }
            }

            return new CommandResult
            {
                data = new Dictionary<string, object> { [ResponseTo] = $"/Printer/{id}/Connect" },
                Command = PrinterConnectMessage,
                Result = result
            };
        }

        public CommandResult PrinterDisconnect(string id)
        {
            bool result = false;
            var printer = ServiceManager.GetInstance().GetService<IPrinter>();

            if (printer != null)
            {
                PrinterAdapter adapter = printer as PrinterAdapter;
                GdsDeviceBase device = adapter?.PrinterImplementation as GdsDeviceBase;

                if (device != null)
                {
                    result = device.Close();
                }
            }

            return new CommandResult
            {
                data = new Dictionary<string, object> { [ResponseTo] = $"/Printer/{id}/Disconnect" },
                Command = PrinterDisconnectMessage,
                Result = result
            };
        }

        public CommandResult PrinterJam(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperJam = true });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/Jam" },
                Command = "PrinterJam",
                Result = true
            };
        }

        public CommandResult PrinterEmpty(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperEmpty = true });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/Paper/Out" },
                Command = "PrinterEmpty",
                Result = true
            };
        }

        public CommandResult PrinterPaperInChute(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperInChute = true });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/Paper/PaperInChute" },
                Command = "PrinterPaperInChute",
                Result = true
            };
        }

        public CommandResult PrinterStatus(string id)
        {
            var faults = Enum.GetValues(typeof(PrinterFaultTypes)).Cast<PrinterFaultTypes>()
                .Where(val => (val & _printerFaults) != 0).ToList();

            var warnings = Enum.GetValues(typeof(PrinterWarningTypes)).Cast<PrinterWarningTypes>()
                .Where(val => (val & _printerWarnings) != 0).ToList();

            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/Printer/{id}/Status",
                    ["faults"] = string.Join(",", faults),
                    ["warnings"] = string.Join(",", warnings)
                }
            };
        }

        public CommandResult PrinterPaperStatus(string id)
        {
            var faults = Enum.GetValues(typeof(PrinterFaultTypes)).Cast<PrinterFaultTypes>()
                .Where(val => (val & _printerFaults) != 0).ToList();

            var warnings = Enum.GetValues(typeof(PrinterWarningTypes)).Cast<PrinterWarningTypes>()
                .Where(val => (val & _printerWarnings) != 0).ToList();

            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/Printer/{id}/Paper/Status",
                    ["status"] = MapPrinterState(warnings, faults)
                }
            };
        }

        public CommandResult PrinterChassisOpen(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { ChassisOpen = true });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/ChassisOpen" },
                Command = "PrinterChassisOpen",
                Result = true
            };
        }

        public CommandResult PrinterPaperLow(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperLow = true });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/Paper/Low" },
                Command = "PrinterLow",
                Result = true
            };
        }

        public CommandResult PrinterPaperFill(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperLow = false, PaperEmpty = false });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/Paper/Fill" },
                Command = "PrinterPaperFill",
                Result = true
            };
        }

        public CommandResult PrinterGetTicketList(string id, string count)
        {
            if (!int.TryParse(count, out var requestCount))
            {
                requestCount = 1;
            }

            if (!int.TryParse(id, out var requestId))
            {
                requestId = 1;
            }

            var responseInfo =
                new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/Ticket/Read/{count}" };

            var currentCount = 0;

            if (_vouchersIssued != null)
            {
                foreach (var voucher in _vouchersIssued.AsEnumerable().Reverse())
                {
                    if (currentCount >= requestCount)
                    {
                        break;
                    }

                    var ticketInfo = new Dictionary<string, object>(voucher.PrintedTicket.Data);

                    if (requestId != 1)
                    {
                        MapTicket(ticketInfo);
                    }

                    var ticketSequence = "Ticket" + ++currentCount;
                    responseInfo.Add(ticketSequence, JsonConvert.SerializeObject(ticketInfo));
                }
            }

            return new CommandResult
            {
                data = responseInfo,
                Result = true,
                Info = $"returning last {currentCount} printed tickets"
            };
        }

        public CommandResult PrinterTicketRemove(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperInChute = false });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/Ticket/Remove" },
                Command = "PrinterPaperRemove",
                Result = true
            };
        }

        public CommandResult PrinterHeadLift(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PrintHeadOpen = true });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/HeadLift" },
                Command = "PrinterHeadLift",
                Result = true
            };
        }

        public CommandResult PrinterHeadLower(string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PrintHeadOpen = false });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Printer/{id}/Head/Lower" },
                Command = "PrinterHeadLower",
                Result = true
            };
        }

        private static string MapPrinterState(List<PrinterWarningTypes> warnings, List<PrinterFaultTypes> faults)
        {
            var status = string.Empty;

            if (warnings.Contains(PrinterWarningTypes.PaperLow))
            {
                status = "PaperLow";
            }

            if (warnings.Contains(PrinterWarningTypes.PaperInChute))
            {
                status = "PaperInChute";
            }

            if (warnings.Contains(PrinterWarningTypes.None))
            {
                status = "Fill";
            }

            if (faults.Contains(PrinterFaultTypes.PrintHeadOpen))
            {
                status = "HeadUp";
            }

            if (faults.Contains(PrinterFaultTypes.PaperJam))
            {
                status = "Jam";
            }

            if (faults.Contains(PrinterFaultTypes.PaperEmpty))
            {
                status = "PaperOut";
            }

            return status;
        }

        private static void MapTicket(IDictionary<string, object> ticket)
        {
            ticket["1"] = ticket["barcode"];
            ticket["2"] = ticket["establishment name"];
            ticket["3"] = ticket["location address"];
            ticket["4"] = ticket["title"];
            ticket["5"] = $"{ticket["validation label"]} {ticket["validation number"]}";
            ticket["6"] = $"{ticket["datetime"]} {ticket["vlt sequence number"]}";
            ticket["7"] = ticket["value in words 1"];
            ticket["8"] = ticket["value"];
            ticket["9"] = ticket["machine id 2"];
        }
    }
}