namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Markup;
    using DataModel;
    using Hardware.Contracts;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Gds;
    using Hardware.Printer;
    using G2S;
    using Kernel;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    public partial class TestControllerEngine
    {
        private const string PrinterConnectMessage = "PrinterConnect";

        private const string PrinterDisconnectMessage = "PrinterDisconnect";

        [HttpPost]
        [Route("Printer/{id}/Connect")]
        public ActionResult PrinterConnect([FromRoute] string id)
        {
            bool result = false;
            var printer = ServiceManager.GetInstance().GetService<IPrinter>();

            if (printer != null)
            {
                PrinterAdapter adapter = printer as PrinterAdapter;
                GdsDeviceBase device = adapter?.PrinterImplementation as GdsDeviceBase;

                if (device != null)
                {
                    device.Open();
                    result = device.Initialize(device.Communicator).Result;
                }
            }

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Connect",
                ["Command"] = PrinterConnectMessage
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Disconnect")]
        public ActionResult PrinterDisconnect([FromRoute] string id)
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

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Disconnect",
                ["Command"] = PrinterDisconnectMessage
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Jam")]
        public ActionResult PrinterJam([FromRoute] string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperJam = true });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Jam",
                ["Command"] = "PrinterJam"
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/Out")]
        public ActionResult PrinterEmpty([FromRoute] string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperEmpty = true });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Paper/Out",
                ["Command"] = "PrinterEmpty"
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/PaperInChute")]
        public ActionResult PrinterPaperInChute([FromRoute] string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperInChute = true });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Paper/PaperInChute",
                ["Command"] = "PrinterPaperInChute"
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Status")]
        public ActionResult PrinterStatus([FromRoute] string id)
        {
            var faults = Enum.GetValues(typeof(PrinterFaultTypes)).Cast<PrinterFaultTypes>()
                .Where(val => (val & _printerFaults) != 0).ToList();

            var warnings = Enum.GetValues(typeof(PrinterWarningTypes)).Cast<PrinterWarningTypes>()
                .Where(val => (val & _printerWarnings) != 0).ToList();

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Status",
                ["faults"] = string.Join(",", faults),
                ["warnings"] = string.Join(",", warnings)
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/Status")]
        public ActionResult PrinterPaperStatus([FromRoute] string id)
        {
            var faults = Enum.GetValues(typeof(PrinterFaultTypes)).Cast<PrinterFaultTypes>()
                .Where(val => (val & _printerFaults) != 0).ToList();

            var warnings = Enum.GetValues(typeof(PrinterWarningTypes)).Cast<PrinterWarningTypes>()
                .Where(val => (val & _printerWarnings) != 0).ToList();

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Paper/Status",
                ["status"] = MapPrinterState(warnings, faults)
            });
        }

        [HttpPost]
        [Route("Printer/{id}/ChassisOpen")]
        public ActionResult PrinterChassisOpen([FromRoute] string id)
        {
            _eventBus.Publish(new FakePrinterEvent { ChassisOpen = true });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/ChassisOpen",
                ["Command"] = "PrinterChassisOpen"
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/Low")]
        public ActionResult PrinterPaperLow([FromRoute] string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperLow = true });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Paper/Low",
                ["Command"] = "PrinterLow"
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/Fill")]
        public ActionResult PrinterPaperFill([FromRoute] string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperLow = false, PaperEmpty = false });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Paper/Fill",
                ["Command"] = "PrinterPaperFill"
            });
        }

        [HttpGet]
        [Route("Printer/{id}/Ticket/Read/{count=1}")]
        public ActionResult PrinterGetTicketList([FromRoute] string id, [FromRoute] string count = "1")
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

            responseInfo.Add("Info", $"returning last {currentCount} printed tickets");
            return Ok(responseInfo);
        }

        [HttpPost]
        [Route("Printer/{id}/Ticket/Remove")]
        public ActionResult PrinterTicketRemove([FromRoute]string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PaperInChute = false });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Ticket/Remove",
                ["Command"] = "PrinterPaperRemove"
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Head/Lift")]
        public ActionResult PrinterHeadLift([FromRoute] string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PrintHeadOpen = true });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/HeadLift",
                ["Command"] = "PrinterHeadLift"
            });
        }

        [HttpPost]
        [Route("Printer/{id}/Head/Lower")]
        public ActionResult PrinterHeadLower([FromRoute] string id)
        {
            _eventBus.Publish(new FakePrinterEvent { PrintHeadOpen = false });

            return Ok(new Dictionary<string, object>
            {
                ["response-to"] = $"/Printer/{id}/Head/Lower",
                ["Command"] = "PrinterHeadLower"
            });
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