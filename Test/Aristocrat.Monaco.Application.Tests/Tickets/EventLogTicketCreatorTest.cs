namespace Aristocrat.Monaco.Application.Tests.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Application.Tickets;
    using Aristocrat.Monaco.Application.Contracts.Currency;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Tickets;
    using Contracts.TiltLogger;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Summary description for EventLogTicketCreatorTest
    /// </summary>
    [TestClass]
    public class EventLogTicketCreatorTest
    {
        private const string Retailer = "RETAILER";
        private const string RetailerAddress = "RETAILER ADDRESS";
        private const string License = "6000";
        private const string CabinetSerial = "56789";
        private const string SystemVersion = "98765";
        private const uint SystemId = 9;
        private static readonly int ExpectedEventsPerPage = 5;
        private Mock<IPrinter> _printerMock;

        // Mock Services
        private Mock<IPropertiesManager> _propertiesManager;
        EventLogTicketCreator _target;
        private Mock<ITime> _time;
        private Mock<IIO> _iio;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationOperatorTicketSelectable, false))
                .Returns(false)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<string>()))
                .Returns(Retailer)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine2, It.IsAny<string>()))
                .Returns("Test Casino city")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine3, It.IsAny<string>()))
                .Returns(RetailerAddress)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.Zone, It.IsAny<string>()))
                .Returns(License)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(CabinetSerial)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns(SystemId)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(KernelConstants.SystemVersion, It.IsAny<string>()))
                .Returns(SystemVersion)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty("Locale.OperatorTicket.Locale", It.IsAny<string>()))
                .Returns(CultureInfo.GetCultureInfo("en-US"))
                .Verifiable();

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.AuditTicketEventsPerPage, 6))
                .Returns(6)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.AuditTicketLineLimit, 36))
                .Returns(36)
                .Verifiable();

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationOperatorTicketDateFormat, ApplicationConstants.DefaultDateFormat))
                .Returns(ApplicationConstants.DefaultDateFormat)
                .Verifiable();

            _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Loose);
            _iio.Setup(i => i.DeviceConfiguration).Returns(new Device { Manufacturer = "Manufacturer", Model = "Model" });

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);
            _time.Setup(t => t.TimeZoneInformation).Returns(TimeZoneInfo.Utc);

            _printerMock = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _printerMock.Setup(mock => mock.PaperState).Returns(PaperStates.Full);
            _printerMock.Setup(mock => mock.GetCharactersPerLine(false, 0)).Returns(36);

            _target = new EventLogTicketCreator();

            // set up currency
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
        }

        [TestMethod]
        public void ConstructsTest()
        {
            Assert.IsNotNull(_target);
            Assert.IsInstanceOfType(_target, typeof(EventLogTicketCreator));
        }

        [TestMethod]
        public void InitializeTest()
        {
            // No observable side effects
            _target.Initialize();

            // Go ahead and test other IService properties here.
            Assert.AreEqual(_target.Name, "Event Log Ticket Creator");

            var serviceTypes = _target.ServiceTypes;
            var expectedType = serviceTypes.Select(
                    t => typeof(IEventLogTicketCreator) == t)
                .FirstOrDefault();

            Assert.IsNotNull(expectedType);
        }

        //[TestMethod]
        //public void CreateTicketFailswithBadPageTest()
        //{
        //    var events = CreateEventDescriptions(DateTime.UtcNow);

        //    Assert.IsNull(_target.Create(0, events));

        //    var expectedPages = (events.Count / _target.EventsPerPage)
        //                        + (events.Count % _target.EventsPerPage != 0 ? 1 : 0);

        //    Assert.IsNull(_target.Create(expectedPages + 1, events));
        //}

        [TestMethod]
        public void CreateTicketTest()
        {
            DateTime now = DateTime.UtcNow;
            var events = CreateEventDescriptions(now);

            now = now.AddMinutes(15);
            _time.Setup(mock => mock.GetLocationTime(It.IsAny<DateTime>()))
                .Returns(now);

            //var separator = GetPrivateStringConstant("Dashes");
            //var dateFormat = GetPrivateStringConstant("DateAndTimeFormat");
            var eventsPerPage = _target.EventsPerPage;

            Assert.AreEqual(ExpectedEventsPerPage, eventsPerPage);

            var expectedPages = (events.Count / eventsPerPage) + (events.Count % eventsPerPage != 0 ? 1 : 0);
            var expectedHeaderData = new Dictionary<string, string>()
            {
                { "EmptyLine", string.Empty },
                { "Retailer", "Unit Test Retailer" },
                { "RetailerID", "88888" },
                { "Jurisdiction", "UNITTESTJURISDICTION" },
                { "Serial", "99999" },
                { "Version", "5.5.5" },
                //{ "Time", now.ToString(dateFormat)},
                { "Page", string.Empty },
                // { "Separator", separator}
            };
            var lookup = expectedHeaderData.Keys.ToArray();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(expectedHeaderData["Serial"]);

            _propertiesManager.Setup(
                    mock => mock.GetProperty(
                        PropertyKey.TicketTextLine1,
                        It.IsAny<string>()))
                .Returns(expectedHeaderData["Retailer"]);

            _propertiesManager.Setup(
                    mock => mock.GetProperty(
                        ApplicationConstants.Zone,
                        It.IsAny<string>()))
                .Returns(expectedHeaderData["RetailerID"]);

            _propertiesManager.Setup(
                    mock => mock.GetProperty(
                        ApplicationConstants.JurisdictionKey,
                        It.IsAny<string>()))
                .Returns(expectedHeaderData["Jurisdiction"]);

            _propertiesManager.Setup(
                    mock => mock.GetProperty(
                        ApplicationConstants.LocalizationOperatorTicketDateFormat,
                        ApplicationConstants.DefaultDateFormat))
                .Returns(ApplicationConstants.DefaultDateFormat);

            _propertiesManager.Setup(
                    mock => mock.GetProperty(
                        KernelConstants.SystemVersion,
                        It.IsAny<string>()))
                .Returns(expectedHeaderData["Version"]);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);

            for (var page = 1; page <= expectedPages; page++)
            {
                var ticket = _target.Create(page, events);
                Assert.IsNotNull(ticket);

                Assert.AreEqual("text", ticket["ticket type"]);
                //Assert.AreEqual(Resources.EventLogTicketTitle, ticket["title"]);

                //// Read/Verify the header
                //var headerLines = ticket["center"].Split(new[] { "\r\n" }, StringSplitOptions.None);
                //int line;
                //var headerLine = String.Empty;
                //for (line = 0; line < headerLines.Length; line++)// && !headerLine.Equals(separator)
                //{
                //    headerLine = headerLines[line];
                //    var key = lookup[line];
                //    switch (key)
                //    {
                //        case "Serial":
                //            Assert.AreEqual($"SERIAL NUMBER: {expectedHeaderData[key]}", headerLine);
                //            break;
                //        case "Time":
                //            Assert.AreEqual($"TIME: {expectedHeaderData[key]}", headerLine);
                //            break;
                //        case "Page":
                //            Assert.AreEqual($"PAGE: {page}", headerLine);
                //            break;
                //        default:
                //            Assert.AreEqual(expectedHeaderData[key], headerLine);
                //            break;
                //    }
                //}

                //Assert.AreEqual(lookup.Length, line);

                //// Read/Verify the ticket events
                //var leftColumn = ticket["left"].Split(new[] { "\r\n" }, StringSplitOptions.None).Skip(line);
                //var centerColumn = ticket["center"].Split(new[] { "\r\n" }, StringSplitOptions.None).Skip(line);
                //var rightColumn = ticket["right"].Split(new[] { "\r\n" }, StringSplitOptions.None).Skip(line);
                //var actualColumns = ZipReportColumns(leftColumn, centerColumn, rightColumn).ToArray();

                //// Select the events we expect to see on this page
                //var expectedEvents = events
                //                    .Skip((page - 1) * ExpectedEventsPerPage)
                //                    .Take(ExpectedEventsPerPage)
                //                    .ToArray();

                //Create lists of the columnar string data
                //var expectedLeftColumn = expectedEvents.SelectMany(
                //    e => new[] { $"{e.Timestamp.ToString(dateFormat)}", $"{e.Name}", $"{string.Empty}" }
                //    );
                //var expectedCenterColumn = expectedEvents.SelectMany(
                //    e => new[] { $"{e.Level}", $"{string.Empty}", $"{separator}" }
                //    );
                //var expectedRightColumn = expectedEvents.SelectMany(
                //    e => new[] { $"{string.Empty}", $"{string.Empty}", $"{string.Empty}" }
                //    );
                //var expectedColumns = ZipReportColumns(expectedLeftColumn, expectedCenterColumn, expectedRightColumn).ToArray();

                //for (int i = 0; i < expectedColumns.Length; i++)
                //{
                //    Assert.AreEqual(expectedColumns[i], actualColumns[i]);
                //}
            }
        }

        /// <summary>
        ///     Creates a simple Collection of EventDescriptions
        /// </summary>
        /// <param name="now">
        ///     Starting point for time stamps. Events are stamped as if they
        ///     occur 1 minute apart.
        /// </param>
        /// <returns>A Collection of EventDescriptions</returns>
        private static Collection<EventDescription> CreateEventDescriptions(DateTime now)
        {
            var events = new Collection<EventDescription>()
            {
                new EventDescription("NAME01", "LEVEL01", "TYPE01",  1, Guid.NewGuid(), now),
                new EventDescription("NAME02", "LEVEL02", "TYPE02",  2, Guid.NewGuid(), now.AddMinutes(1)),
                new EventDescription("NAME03", "LEVEL03", "TYPE03",  3, Guid.NewGuid(), now.AddMinutes(2)),
                new EventDescription("NAME04", "LEVEL04", "TYPE04",  4, Guid.NewGuid(), now.AddMinutes(3)),
                new EventDescription("NAME05", "LEVEL05", "TYPE05",  5, Guid.NewGuid(), now.AddMinutes(4)),
                new EventDescription("NAME06", "LEVEL06", "TYPE06",  6, Guid.NewGuid(), now.AddMinutes(5)),
                new EventDescription("NAME07", "LEVEL07", "TYPE07",  7, Guid.NewGuid(), now.AddMinutes(6)),
                new EventDescription("NAME08", "LEVEL08", "TYPE08",  8, Guid.NewGuid(), now.AddMinutes(7)),
                new EventDescription("NAME09", "LEVEL09", "TYPE09",  9, Guid.NewGuid(), now.AddMinutes(8)),
                new EventDescription("NAME10", "LEVEL10", "TYPE10", 10, Guid.NewGuid(), now.AddMinutes(9)),
                new EventDescription("NAME11", "LEVEL11", "TYPE11", 11, Guid.NewGuid(), now.AddMinutes(10)),
                new EventDescription("NAME12", "LEVEL12", "TYPE12", 12, Guid.NewGuid(), now.AddMinutes(11)),
                new EventDescription("NAME13", "LEVEL13", "TYPE13", 13, Guid.NewGuid(), now.AddMinutes(12)),
                new EventDescription("NAME14", "LEVEL14", "TYPE14", 14, Guid.NewGuid(), now.AddMinutes(13)),
                new EventDescription("NAME15", "LEVEL15", "TYPE15", 15, Guid.NewGuid(), now.AddMinutes(14)),
                new EventDescription("NAME16", "LEVEL16", "TYPE16", 16, Guid.NewGuid(), now.AddMinutes(15)),
                new EventDescription("NAME17", "LEVEL17", "TYPE17", 17, Guid.NewGuid(), now.AddMinutes(16)),
                new EventDescription("NAME18", "LEVEL18", "TYPE18", 18, Guid.NewGuid(), now.AddMinutes(17)),
                new EventDescription("NAME19", "LEVEL19", "TYPE19", 19, Guid.NewGuid(), now.AddMinutes(18)),
                new EventDescription("NAME20", "LEVEL20", "TYPE20", 20, Guid.NewGuid(), now.AddMinutes(19)),
                new EventDescription("NAME21", "LEVEL21", "TYPE21", 21, Guid.NewGuid(), now.AddMinutes(20))
            };
            return events;
        }

        /// <summary>
        ///     Combines the given lists of strings into a single IEnumerable of Tuples representing a row-oriented view
        ///     of the columnar data.
        /// </summary>
        /// <param name="leftColumn">String data representing the left column of the ticket</param>
        /// <param name="centerColumn">String data representing the center column of the ticket</param>
        /// <param name="rightColumn">String data representing the right column of the ticket</param>
        /// <returns>IEnumerable of Tuples, where each Tuple is a line on the ticket, and each Item in the tuple is a column</returns>
        private static IEnumerable<Tuple<string, string, string>> ZipReportColumns(
            IEnumerable<string> leftColumn,
            IEnumerable<string> centerColumn,
            IEnumerable<string> rightColumn)
        {
            var query = leftColumn
                .Zip(
                    centerColumn,
                    (a, b) => new
                    {
                        First = a,
                        Second = b
                    })
                .Zip(
                    rightColumn,
                    (a, b) => new Tuple<string, string, string>(
                        a.First,
                        a.Second,
                        b));
            return query;
        }
    }
}