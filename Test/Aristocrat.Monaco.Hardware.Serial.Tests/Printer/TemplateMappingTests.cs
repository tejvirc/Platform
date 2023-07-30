namespace Aristocrat.Monaco.Hardware.Serial.Tests.Printer
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Aristocrat.Monaco.Hardware.Serial;
    using Aristocrat.Monaco.Hardware.Serial.Printer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TemplateMappingTests
    {
        private const string TestProtocol = "JCM TCL";
        private const string TestFirmware1 = "5RUSAGE29";
        private const string TestFirmware2 = "GURUSAG";
        private const string TestFirmware3 = "abc";
        private const string PrinterOverridePath = @".\Printer\PrinterOverrides.xml";

        private Overrides _overrides;
        private OverridesOverride _override;

        private List<dpttype> _printableTemplate = new List<dpttype>
            {
                new dpttype() { name = "cashout", id = 102, t_dim_da = 508, t_dim_pa = 1240, Value = "107 121 122 135 106 117 124 125 126 114 115 118 119 120" },
                new dpttype() { name = "jackpot", id = 103, t_dim_da = 508, t_dim_pa = 1240, Value = "107 121 122 135 106 130 128 125 127 129 131 132 118 119 120" },
                new dpttype() { name = "handpay receipt", id = 104, t_dim_da = 508, t_dim_pa = 1240, Value = "107 121 122 135 106 130 128 125 127 129 131 132 118 119 120" },
                new dpttype() { name = "handpay receipt no barcode", id = 107, t_dim_da = 508, t_dim_pa = 1240, Value = "121 122 135 106 128 125 127 114 115 118" }
            };

        private List<dprtype> _printableRegions = new List<dprtype>
            {
                new dprtype() { name = "CashSlipTitle", id= "106" },
                new dprtype() { name = "ValidationNo", id= "107" },
                new dprtype() { name = "CashValue", id= "114" },
                new dprtype() { name = "CashValueText", id= "115" },
                new dprtype() { name = "CashSlipBarcode", id= "117" },
                new dprtype() { name = "ExpiryDate", id= "118" },
                new dprtype() { name = "AltValidationNo", id= "119" },
                new dprtype() { name = "ValidationTitle", id= "120" },
                new dprtype() { name = "EstablishmentName", id= "121" },
                new dprtype() { name = "LocationName", id= "122" },
                new dprtype() { name = "AltSeqNo", id= "124" },
                new dprtype() { name = "MachineId", id= "125" },
                new dprtype() { name = "AltDateTime", id= "126" },
                new dprtype() { name = "AltDateTime2", id= "127" },
                new dprtype() { name = "AltSeqNo2", id= "128" },
                new dprtype() { name = "CashValue2", id= "129" },
                new dprtype() { name = "CashSlipBarcode2", id= "130" },
                new dprtype() { name = "AltCashValueText", id= "131" },
                new dprtype() { name = "AltCashValueText2", id= "132" },
                new dprtype() { name = "LocationAddress", id= "135" },
            };

        [TestInitialize]
        public void TestInitialize()
        {
            PrinterOverrideParser.LoadOverrides(PrinterOverridePath);
            _overrides = TemplateMapping.LoadMappings(PrinterOverridePath);
            _override = PrinterOverrideParser.GetPrinterSpecificOverride(TestProtocol, TestFirmware1);
        }

        [TestMethod]
        public void GetTemplateOverridesTest()
        {
            var overrides = PrinterOverrideParser.GetTemplateOverrides(TestProtocol, TestFirmware1);
            Assert.IsNotNull(overrides);
            Assert.AreEqual(5, overrides.Count());
        }

        [TestMethod]
        public void GetMappingOverridesTest()
        {
            var overrides = PrinterOverrideParser.GetMappingOverrides(TestProtocol, TestFirmware1);
            Assert.IsNotNull(overrides);
            Assert.AreEqual(5, overrides.Count());
        }

        [TestMethod]
        public void GetTemplateOverridesNoMatchingProtocolTest()
        {
            var overrides = PrinterOverrideParser.GetTemplateOverrides("TEST", TestFirmware1);
            Assert.IsNull(overrides);
        }

        [TestMethod]
        public void GetMappingOverridesDefaultFirmwareTest()
        {
            // should map to the * firmware
            var overrides = PrinterOverrideParser.GetMappingOverrides(TestProtocol, TestFirmware3);
            Assert.IsNotNull(overrides);
            Assert.AreEqual(1, overrides.Count());
        }

        [TestMethod]
        public void IsAuditTicketTest()
        {
            var command1 = new PrintCommand() { Id = 101 };
            var command2 = new PrintCommand() { Id = 102 };

            Assert.IsTrue(_override.IsAuditTicket(command1));
            Assert.IsFalse(_override.IsAuditTicket(command2));
        }

        [TestMethod]
        public void GetSpecificFontTest()
        {
            var originalFont = "8";
            var region100ExpectedFont = "6";
            var region200ExpectedFont = string.Empty;

            // should match and give new font
            Assert.AreEqual(region100ExpectedFont, _override.GetSpecificFont(originalFont, "100"));

            // no match so return original font
            Assert.AreEqual(region200ExpectedFont, _override.GetSpecificFont(originalFont, "200"));
        }

        [TestMethod]
        public void MatchOtherFirmwareTest()
        {
            var @override = PrinterOverrideParser.GetPrinterSpecificOverride(TestProtocol, TestFirmware2);
            Assert.IsNotNull(@override);
        }

        [TestMethod]
        public void RemapCashoutPrintCommandTest()
        {
            const string barCode = "123456789";
            const string establishmentName = "My Casino";
            const string locationName = "My Test Location";
            const string locationAddress = "My Test Address";
            const string ticketTitle = "Cashout Voucher";
            const string sequenceNumber = "321";
            const string machineId = "Machine #: 111";
            const string dateTime = "2/23/2022 12:34:56";
            const string date = "2/23/2022";
            const string time = "12:34:56";
            const string cashValue = "10.00";
            const string cashValueText = "Ten Dollars and Zero Cents";
            const string expiryDate = "0";
            const string validationTitle = "Validation:";
            const string blank = "";

            var expectedData = 16;
            var printCommand = new PrintCommand
            {
                Id = 102,
                UsePrinterTemplate = true,
                IsAuditTicket = false,
                PrinterTemplateId = "0",
                DataFields = new PrintDataField[]
                {
                    new PrintDataField() { Data = barCode },    // 107 validation number
                    new PrintDataField() { Data = establishmentName },    // 121 establishment name
                    new PrintDataField() { Data = locationName }, // 122 location name
                    new PrintDataField() { Data = locationAddress }, // 135 location address
                    new PrintDataField() { Data = ticketTitle }, // 106 cash slip title
                    new PrintDataField() { Data = barCode },  // 117 cash slip barcode
                    new PrintDataField() { Data = sequenceNumber }, // 124 sequence number
                    new PrintDataField() { Data = machineId }, // 125 machine id
                    new PrintDataField() { Data = dateTime }, // 126 date/time
                    new PrintDataField() { Data = cashValue }, // 114 cash value
                    new PrintDataField() { Data = cashValueText }, // 115 cash value text
                    new PrintDataField() { Data = expiryDate }, // 118 expiry date
                    new PrintDataField() { Data = barCode }, // 119 validation number
                    new PrintDataField() { Data = validationTitle } // 120 validation title
                }
            };

            var newPrintCommand = _override.RemapPrintCommand(printCommand, _printableTemplate, _printableRegions);

            Assert.AreEqual(expectedData, newPrintCommand.DataFields.Count());
            Assert.AreEqual(barCode, newPrintCommand.DataFields[0].Data, "ticket edge barcode didn't match");
            Assert.AreEqual(establishmentName, newPrintCommand.DataFields[1].Data, "Establishment name didn't match");
            Assert.AreEqual(locationName, newPrintCommand.DataFields[2].Data, "location name didn't match");
            Assert.AreEqual(locationAddress, newPrintCommand.DataFields[3].Data, "location address didn't match");
            Assert.AreEqual(ticketTitle, newPrintCommand.DataFields[4].Data, "Ticket title didn't match");
            Assert.AreEqual(validationTitle, newPrintCommand.DataFields[5].Data, "validation title didn't match");
            Assert.AreEqual(barCode, newPrintCommand.DataFields[6].Data, "barcode didn't match");
            Assert.AreEqual(date, newPrintCommand.DataFields[7].Data, "date didn't match");
            Assert.AreEqual(time, newPrintCommand.DataFields[8].Data, "time didn't match");
            Assert.AreEqual(sequenceNumber, newPrintCommand.DataFields[9].Data, "sequence number didn't match");
            Assert.AreEqual(cashValueText, newPrintCommand.DataFields[10].Data, "cash value text didn't match");
            Assert.AreEqual(blank, newPrintCommand.DataFields[11].Data, "blank didn't match");
            Assert.AreEqual(cashValue, newPrintCommand.DataFields[12].Data, "cash value didn't match");
            Assert.AreEqual(expiryDate, newPrintCommand.DataFields[13].Data, "expiry date didn't match");
            Assert.AreEqual("MACHINE # 111", newPrintCommand.DataFields[14].Data, "machine id didn't match");
            Assert.AreEqual(barCode, newPrintCommand.DataFields[15].Data, "ticket bottom validation number didn't match");
        }

        [TestMethod]
        
        public void RemapHandpayNoBarcodePrintCommandTest()
        {
            const string establishmentName = "My Casino";
            const string locationName = "My Test Location";
            const string locationAddress = "My Test Address";
            const string ticketTitle = "Cashout Voucher";
            const string sequenceNumber = "321";
            const string machineId = "Machine #: 111";
            const string dateTime = "2/23/2022 12:34:56";
            const string date = "23/02/2022";
            const string time = "12:34:56";
            const string cashValue = "10.00";
            const string cashValueText = "Ten Dollars and Zero Cents";
            const string expiryDate = "0";
            const string blank = "";

            var expectedData = 13;
            var printCommand = new PrintCommand
            {
                Id = 107,
                UsePrinterTemplate = true,
                IsAuditTicket = false,
                PrinterTemplateId = "C",
                DataFields = new PrintDataField[]
                {
                    new PrintDataField() { Data = establishmentName },    // 121 establishment name
                    new PrintDataField() { Data = locationName }, // 122 location name
                    new PrintDataField() { Data = locationAddress }, // 135 location address
                    new PrintDataField() { Data = ticketTitle }, // 106 ticket title
                    new PrintDataField() { Data = sequenceNumber }, // 128 sequence number
                    new PrintDataField() { Data = machineId }, // 125 machine id
                    new PrintDataField() { Data = dateTime }, // 126/127 date/time
                    new PrintDataField() { Data = cashValue }, // 114 cash value
                    new PrintDataField() { Data = cashValueText }, // 115 cash value text
                    new PrintDataField() { Data = expiryDate }, // 118 expiry date
                }
            };
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var newPrintCommand = _override.RemapPrintCommand(printCommand, _printableTemplate, _printableRegions);

            Assert.AreEqual(expectedData, newPrintCommand.DataFields.Count());
            Assert.AreEqual(establishmentName, newPrintCommand.DataFields[0].Data, "Establishment name didn't match");
            Assert.AreEqual(locationName, newPrintCommand.DataFields[1].Data, "location name didn't match");
            Assert.AreEqual(locationAddress, newPrintCommand.DataFields[2].Data, "location address didn't match");
            Assert.AreEqual(ticketTitle, newPrintCommand.DataFields[3].Data, "Ticket title didn't match");
            Assert.AreEqual(cashValue, newPrintCommand.DataFields[4].Data, "cash value didn't match");
            Assert.AreEqual(cashValueText, newPrintCommand.DataFields[5].Data, "cash value text didn't match");
            Assert.AreEqual(blank, newPrintCommand.DataFields[6].Data, "barcode didn't match");
            Assert.AreEqual(date, newPrintCommand.DataFields[7].Data, "date didn't match");
            Assert.AreEqual(" " + time, newPrintCommand.DataFields[8].Data, "time didn't match");
            Assert.AreEqual(sequenceNumber, newPrintCommand.DataFields[9].Data, "sequence number didn't match");
            Assert.AreEqual(blank, newPrintCommand.DataFields[10].Data, "blank didn't match");
            Assert.AreEqual(expiryDate, newPrintCommand.DataFields[11].Data, "expiry date didn't match");
            Assert.AreEqual("MACHINE # 111", newPrintCommand.DataFields[12].Data, "machine id didn't match");
        }

        [TestMethod]
        
        public void RemapHandPayReceiptTest()
        {
            const string barCode = "123456789";
            const string establishmentName = "My Casino";
            const string locationName = "My Test Location";
            const string locationAddress = "My Test Address";
            const string ticketTitle = "Cashout Voucher";
            const string sequenceNumber = "321";
            const string machineId = "Machine #: 111";
            const string dateTime = "2/23/2022 12:34:56";
            const string date = "23/02/2022";
            const string time = "12:34:56";
            const string cashValue = "10.00";
            const string cashValueText = "Ten Dollars and Zero Cents";
            const string noValueText = "*** No Cash Value ***";
            const string expiryDate = "0";
            const string validationTitle = "Validation:";
            const string blank = "";
            const string longCashValueText = cashValueText + "~013~010" + noValueText;

            var expectedData = 16;
            var printCommand = new PrintCommand
            {
                Id = 104,
                UsePrinterTemplate = true,
                IsAuditTicket = false,
                PrinterTemplateId = "A",
                DataFields = new PrintDataField[]
                {
                    new PrintDataField() { Data = barCode },    // 107 validation number
                    new PrintDataField() { Data = establishmentName },    // 121 establishment name
                    new PrintDataField() { Data = locationName }, // 122 location name
                    new PrintDataField() { Data = locationAddress }, // 135 location address
                    new PrintDataField() { Data = ticketTitle }, // 106 cash slip title
                    new PrintDataField() { Data = barCode },  // 117 cash slip barcode
                    new PrintDataField() { Data = sequenceNumber }, // 124 sequence number
                    new PrintDataField() { Data = machineId }, // 125 machine id
                    new PrintDataField() { Data = dateTime }, // 126 date/time
                    new PrintDataField() { Data = cashValue }, // 114 cash value
                    new PrintDataField() { Data = cashValueText }, // 131 cash value text
                    new PrintDataField() { Data = noValueText }, // 132 no value text
                    new PrintDataField() { Data = expiryDate }, // 118 expiry date
                    new PrintDataField() { Data = barCode }, // 119 validation number
                    new PrintDataField() { Data = validationTitle } // 120 validation title
                }
            };
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var newPrintCommand = _override.RemapPrintCommand(printCommand, _printableTemplate, _printableRegions);

            Assert.AreEqual(expectedData, newPrintCommand.DataFields.Count());
            Assert.AreEqual(barCode, newPrintCommand.DataFields[0].Data, "ticket edge barcode didn't match");
            Assert.AreEqual(establishmentName, newPrintCommand.DataFields[1].Data, "Establishment name didn't match");
            Assert.AreEqual(locationName, newPrintCommand.DataFields[2].Data, "location name didn't match");
            Assert.AreEqual(locationAddress, newPrintCommand.DataFields[3].Data, "location address didn't match");
            Assert.AreEqual(ticketTitle, newPrintCommand.DataFields[4].Data, "Ticket title didn't match");
            Assert.AreEqual(cashValue, newPrintCommand.DataFields[5].Data, "cash value didn't match");
            Assert.AreEqual(longCashValueText, newPrintCommand.DataFields[6].Data, "cash value text didn't match");
            Assert.AreEqual(blank, newPrintCommand.DataFields[7].Data, "blank didn't match");
            Assert.AreEqual(date, newPrintCommand.DataFields[8].Data, "date didn't match");
            Assert.AreEqual(" " + time, newPrintCommand.DataFields[9].Data, "time didn't match");
            Assert.AreEqual(sequenceNumber, newPrintCommand.DataFields[10].Data, "sequence number didn't match");
            Assert.AreEqual(validationTitle, newPrintCommand.DataFields[11].Data, "validation title didn't match");
            Assert.AreEqual(barCode, newPrintCommand.DataFields[12].Data, "barcode didn't match");
            Assert.AreEqual(expiryDate, newPrintCommand.DataFields[13].Data, "expiry date didn't match");
            Assert.AreEqual("MACHINE # 111", newPrintCommand.DataFields[14].Data, "machine id didn't match");
            Assert.AreEqual(barCode, newPrintCommand.DataFields[15].Data, "ticket bottom validation number didn't match");
        }
    }
}
