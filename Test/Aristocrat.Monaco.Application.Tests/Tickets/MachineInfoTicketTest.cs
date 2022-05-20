namespace Aristocrat.Monaco.Application.Tests.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Application.Tickets;
    using Cabinet.Contracts;
    using Contracts;
    using Contracts.Tickets;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Summary description for MetersTicketCreatorTest
    /// </summary>
    [TestClass]
    public class MachineInfoTicketTest
    {
        // Mock Services
        private Mock<IServiceManager> _serviceManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITime> _time;
        private Mock<IIO> _io;
        private Mock<ICabinetDetectionService> _cabinetDetectionService;

        // Test values
        private const string TestBootLoader = "Test Boot Loader";
        private const string TestOsVersion = "0.0";
        private const string TestDisplay1 = "Display1";
        private const string TestDisplay2 = "Display2";

        // Test target
        MachineInfoTicketCreator _target;
        

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _io = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
            _cabinetDetectionService = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Strict);

            _serviceManager.Setup(s => s.TryGetService<INoteAcceptor>()).Returns((INoteAcceptor)null);
            _serviceManager.Setup(s => s.TryGetService<IPrinter>()).Returns((IPrinter)null);

            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.LocalizationOperatorTicketSelectable, It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.LocalizationOperatorTicketLocale, It.IsAny<string>())).Returns(CultureInfo.CurrentCulture);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SelectedLocaleCode, It.IsAny<string>())).Returns("en-us");
            _propertiesManager.Setup(p => p.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(p => p.GetProperty(PropertyKey.TicketTextLine2, It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(p => p.GetProperty(PropertyKey.TicketTextLine3, It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.Zone, It.IsAny<string>())).Returns("Data Unavailable");
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>())).Returns((uint)0);
            _propertiesManager.Setup(p => p.GetProperty(KernelConstants.SystemVersion, It.IsAny<string>())).Returns("Data Unavailable");
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>())).Returns("Data Unavailable");
            
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            _io.Setup(i => i.GetFirmwareVersion(FirmwareData.Bios)).Returns((string)null);
            _io.Setup(i => i.GetFirmwareVersion(FirmwareData.Fpga)).Returns((string)null);

            _cabinetDetectionService.Setup(i => i.ExpectedTouchDevices).Returns(new List<ITouchDevice>());

            _target = new MachineInfoTicketCreator();
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.AreNotEqual(null, _target);
        }

        [TestMethod]
        public void InitializeTest()
        {
            _target.Initialize();
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Gaming Machine Info Ticket Creator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(IMachineInfoTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void CreateNoServicesTest()
        {
            //_printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Default);
            //_printer.Setup(i => i.GetCharactersPerLine(false,0)).Returns(40);

            //var ticket = _target.Create();

            //AssertLabelHasValue(ticket, Resources.SerialNumberText, "Data Unavailable");
            //AssertLabelHasValue(ticket, Resources.AssetNumber, "0");
            //AssertLabelHasValue(ticket, "BIOS Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "FPGA Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "MBR/BL Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "OS Image Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Platform Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Graphics Card(s)", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Edge Lighting", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Touch Screen", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Note Acceptor", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Printer", "Data Unavailable");
        }

        [TestMethod]
        public void CreateNoValuesTest()
        {
            //_edge = MoqServiceManager.CreateAndAddService<IEdgeLightingController>(MockBehavior.Strict);
            //_edge.Setup(e => e.Devices).Returns(new List<ElDeviceType>());


            //_iOsService = MoqServiceManager.CreateAndAddService<IOSService>(MockBehavior.Strict);
            //_iOsService.Setup(i => i.BootloaderVersion).Returns((string)null);
            //_iOsService.Setup(i => i.OsImageVersion).Returns(Version.Parse(TestOsVersion));

            //_printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Default);
            //_printer.Setup(i => i.GetCharactersPerLine(false, 0)).Returns(40);

            //var ticket = _target.Create();

            ////AssertLabelHasValue(ticket, Resources.SerialNumberText, "Data Unavailable");
            ////AssertLabelHasValue(ticket, Resources.AssetNumber, "0");
            //AssertLabelHasValue(ticket, "BIOS Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "FPGA Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "MBR/BL Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "OS Image Version", TestOsVersion);
            //AssertLabelHasValue(ticket, "Platform Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Graphics Card(s)", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Edge Lighting", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Touch Screen", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Note Acceptor", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Printer", "Data Unavailable");
        }

        [TestMethod]
        public void CreateHasValuesTest()
        {
            //_edge = MoqServiceManager.CreateAndAddService<IEdgeLightingController>(MockBehavior.Strict);
            //_edge.Setup(e => e.Devices).Returns(new List<ElDeviceType> {ElDeviceType.Cabinet, ElDeviceType.Topper});

            //_iOsService = MoqServiceManager.CreateAndAddService<IOSService>(MockBehavior.Strict);
            //_iOsService.Setup(i => i.BootloaderVersion).Returns(TestBootLoader);
            //_iOsService.Setup(i => i.OsImageVersion).Returns(Version.Parse(TestOsVersion));
            //_touchscreens.Setup(i => i.Touchscreens).Returns(new List<string> {TestDisplay1, TestDisplay2});

            //_printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Default);
            //_printer.Setup(i => i.GetCharactersPerLine(false, 0)).Returns(40);

            //var ticket = _target.Create();

            ////AssertLabelHasValue(ticket, Resources.SerialNumberText, "Data Unavailable");
            ////AssertLabelHasValue(ticket, Resources.AssetNumber, "0");
            //AssertLabelHasValue(ticket, "BIOS Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "FPGA Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "MBR/BL Version", TestBootLoader);
            //AssertLabelHasValue(ticket, "OS Image Version", TestOsVersion);
            //AssertLabelHasValue(ticket, "Platform Version", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Graphics Card(s)", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Edge Lighting", ElDeviceType.Cabinet + " | " + ElDeviceType.Topper);
            //AssertLabelHasValue(ticket, "Touch Screen", TestDisplay1);
            //AssertLabelHasValue(ticket, "Touch Screen", TestDisplay2, 2);
            //AssertLabelHasValue(ticket, "Note Acceptor", "Data Unavailable");
            //AssertLabelHasValue(ticket, "Printer", "Data Unavailable");
        }

        /// <summary>
        ///     Checks to see whether the given label has the given value
        /// </summary>
        /// <param name="ticket">The ticket to check</param>
        /// <param name="label">The string in the "left" column to check</param>
        /// <param name="value">The string in the "right" column to check</param>
        /// <param name="instance">One based index of the label to check.  If you want the second instance of "Serial Number", use 2</param>
        /// <returns>True if value matches the "right" column</returns>
        private void AssertLabelHasValue(Ticket ticket, string label, string value, int instance = 1)
        {
            using (var leftReader = new StringReader(ticket["left"]))
            {
                string leftValue;
                var leftLineNum = 0;
                var timesMatched = 0;
                while ((leftValue = leftReader.ReadLine()) != null)
                {
                    if (leftValue.Contains(label))
                    {
                        timesMatched++;

                        if (timesMatched == instance)
                        {
                            using (var rightReader = new StringReader(ticket["right"]))
                            {
                                string rightValue;
                                var rightLineNum = 0;
                                while ((rightValue = rightReader.ReadLine()) != null)
                                {
                                    if (rightLineNum == leftLineNum)
                                    {
                                        if (!rightValue.Equals(value))
                                        {
                                            Assert.Fail($"{label}: \"{value}\" does not match \"{rightValue}\"");
                                        }

                                        return;
                                    }

                                    rightLineNum++;
                                }
                            }
                        }
                    }

                    leftLineNum++;
                }
            }

            Assert.Fail($"No matches found in AssertLabelHasValue(ticket, {label}, {value}, {instance}");
        }
    }
}