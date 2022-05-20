namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LPB5SendExtendedGameNInformationParserTest
    {
        private const int MaxBet = 100;
        private const byte ProgressiveGroup = 3;
        private const uint ProgressiveLevels = 0x89ABCDEF;
        private const string GoodGameName = "Wilds";
        private const string GoodPaytableName = "PT12";
        private const string OversizedGameName = "WildsWayTooLongGameName";
        private const string OversizedPaytableName = "PT12IsWayTooLongPaytableName";
        private const int NumberOfWagerCategories = 10;

        private static readonly IReadOnlyCollection<byte> GoodCommand = new List<byte>
        {
            TestConstants.SasAddress,
            (byte)LongPoll.SendExtendedGameNInformation,
            0x00, 0x13
        };

        private static readonly IReadOnlyCollection<byte> BadCommand = new List<byte>
        {
            TestConstants.SasAddress,
            (byte) LongPoll.SendExtendedGameNInformation,
            0xAB, 0xCD
        };

        private static readonly IReadOnlyCollection<byte> Denominations = new List<byte>
        {
            0x01, 0x18, 0x02, 0x00, 0x03, 0x19, 0x04, 0x08
        };

        private static readonly IReadOnlyCollection<byte> TooManyDenominations = new List<byte>
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13
        };

        private Mock<ISasLongPollHandler<LongPollExtendedGameNInformationResponse, LongPollExtendedGameNInformationData>> _handler;

        private LPB5SendExtendedGameNInformationParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LPB5SendExtendedGameNInformationParser();

            _handler =
                new Mock<ISasLongPollHandler<LongPollExtendedGameNInformationResponse, LongPollExtendedGameNInformationData>>(
                    MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendExtendedGameNInformation, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var response = new LongPollExtendedGameNInformationResponse(
                MaxBet,
                ProgressiveGroup,
                ProgressiveLevels,
                GoodGameName,
                GoodPaytableName,
                NumberOfWagerCategories,
                Denominations);
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns(response);

            var actual = _target.Parse(GoodCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendExtendedGameNInformation,
                0x1F,// Length of data
                0x00, 0x13, // Game number
                0x01, 0x00, // Max bet
                0x03, // Progressive group
                0xEF, 0xCD, 0xAB, 0x89, // Progressive levels
                0x05, // Length of game name
                0x57, 0x69, 0x6C, 0x64, 0x73, // Game name
                0x04, // Length of paytable name
                0x50, 0x54, 0x31, 0x32, // Paytable name
                0x00, 0x10, // Number of Wager Categories
                0x08, // Number of denominations to follow
                0x01, 0x18, 0x02, 0x00, 0x03, 0x19, 0x04, 0x08
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleOversizedGameNameTest()
        {
            var response = new LongPollExtendedGameNInformationResponse(
                MaxBet,
                ProgressiveGroup,
                ProgressiveLevels,
                OversizedGameName,
                GoodPaytableName,
                NumberOfWagerCategories,
                Denominations);
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns(response);

            var actual = _target.Parse(GoodCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendExtendedGameNInformation,
                0x2E,// Length of data
                0x00, 0x13, // Game number
                0x01, 0x00, // Max bet
                0x03, // Progressive group
                0xEF, 0xCD, 0xAB, 0x89, // Progressive levels
                0x14, // Length of game name
                0x57, 0x69, 0x6C, 0x64, 0x73, 0x57, 0x61, 0x79, 0x54, 0x6F, 0x6F, 0x4C, 0x6F, 0x6E, 0x67, 0x47, 0x61, 0x6D, 0x65, 0x4E, // Game name
                0x04, // Length of paytable name
                0x50, 0x54, 0x31, 0x32, // Paytable name
                0x00, 0x10, // Number of Wager Categories
                0x08, // Number of denominations to follow
                0x01, 0x18, 0x02, 0x00, 0x03, 0x19, 0x04, 0x08
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleOversizedPayTableNameTest()
        {
            var response = new LongPollExtendedGameNInformationResponse(
                MaxBet,
                ProgressiveGroup,
                ProgressiveLevels,
                GoodGameName,
                OversizedPaytableName,
                NumberOfWagerCategories,
                Denominations);
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns(response);

            var actual = _target.Parse(GoodCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendExtendedGameNInformation,
                0x2F,// Length of data
                0x00, 0x13, // Game number
                0x01, 0x00, // Max bet
                0x03, // Progressive group
                0xEF, 0xCD, 0xAB, 0x89, // Progressive levels
                0x05, // Length of game name
                0x57, 0x69, 0x6C, 0x64, 0x73, // Game name
                0x14, // Length of paytable name
                0x50, 0x54, 0x31, 0x32, 0x49, 0x73, 0x57, 0x61, 0x79, 0x54, 0x6F, 0x6F, 0x4C, 0x6F, 0x6E, 0x67, 0x50, 0x61, 0x79, 0x74, // Paytable name
                0x00, 0x10, // Number of Wager Categories
                0x08, // Number of denominations to follow
                0x01, 0x18, 0x02, 0x00, 0x03, 0x19, 0x04, 0x08
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleNoGameNameTest()
        {
            var response = new LongPollExtendedGameNInformationResponse(
                MaxBet,
                ProgressiveGroup,
                ProgressiveLevels,
                string.Empty,
                GoodPaytableName,
                NumberOfWagerCategories,
                Denominations);
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns(response);

            var actual = _target.Parse(GoodCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendExtendedGameNInformation,
                0x1A, // Length of data
                0x00, 0x13, // Game number
                0x01, 0x00, // Max bet
                0x03, // Progressive group
                0xEF, 0xCD, 0xAB, 0x89, // Progressive levels
                0x00, // Length of game name
                0x04, // Length of paytable name
                0x50, 0x54, 0x31, 0x32, // Paytable name
                0x00, 0x10, // Number of Wager Categories
                0x08, // Number of denominations to follow
                0x01, 0x18, 0x02, 0x00, 0x03, 0x19, 0x04, 0x08
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleNoPaytableNameTest()
        {
            var response = new LongPollExtendedGameNInformationResponse(
                MaxBet,
                ProgressiveGroup,
                ProgressiveLevels,
                GoodGameName,
                string.Empty,
                NumberOfWagerCategories,
                Denominations);
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns(response);

            var actual = _target.Parse(GoodCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendExtendedGameNInformation,
                0x1B, // Length of data
                0x00, 0x13, // Game number
                0x01, 0x00, // Max bet
                0x03, // Progressive group
                0xEF, 0xCD, 0xAB, 0x89, // Progressive levels
                0x05, // Length of game name
                0x57, 0x69, 0x6C, 0x64, 0x73, // Game name
                0x00, // Length of paytable name
                0x00, 0x10, // Number of Wager Categories
                0x08, // Number of denominations to follow
                0x01, 0x18, 0x02, 0x00, 0x03, 0x19, 0x04, 0x08
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleNullPaytableNameAndGameNameTest()
        {
            var response = new LongPollExtendedGameNInformationResponse(
                MaxBet,
                ProgressiveGroup,
                ProgressiveLevels,
                null,
                null,
                NumberOfWagerCategories,
                Denominations);
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns(response);

            var actual = _target.Parse(GoodCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendExtendedGameNInformation,
                0x16, // Length of data
                0x00, 0x13, // Game number
                0x01, 0x00, // Max bet
                0x03, // Progressive group
                0xEF, 0xCD, 0xAB, 0x89, // Progressive levels
                0x00, // Length of game name
                0x00, // Length of paytable name
                0x00, 0x10, // Number of Wager Categories
                0x08, // Number of denominations to follow
                0x01, 0x18, 0x02, 0x00, 0x03, 0x19, 0x04, 0x08
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleNumberDenomsExceededTest()
        {
            var response = new LongPollExtendedGameNInformationResponse(
                MaxBet,
                ProgressiveGroup,
                ProgressiveLevels,
                GoodGameName,
                GoodPaytableName,
                NumberOfWagerCategories,
                TooManyDenominations);
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns(response);

            var actual = _target.Parse(GoodCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendExtendedGameNInformation,
                255, // Length of data
                0x00, 0x13, // Game number
                0x01, 0x00, // Max bet
                0x03, // Progressive group
                0xEF, 0xCD, 0xAB, 0x89, // Progressive levels
                0x05, // Length of game name
                0x57, 0x69, 0x6C, 0x64, 0x73, // Game name
                0x04, // Length of paytable name
                0x50, 0x54, 0x31, 0x32, // Paytable name
                0x00, 0x10, // Number of Wager Categories
                232, // Number of denominations to follow: 255-23 (max length of data, minus the data from Game number through this field)
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleDenomErrorTest()
        {
            const int targetDenom = 1;
            var response = new LongPollExtendedGameNInformationResponse(
                0,
                0,
                0,
                string.Empty,
                string.Empty,
                0,
                new List<byte>()
            ) { ErrorCode = MultiDenomAwareErrorCode.SpecificDenomNotSupported };
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns(response);

            var actualResults = _target.Parse(GoodCommand, targetDenom);

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                0x0,
                (byte)MultiDenomAwareErrorCode.SpecificDenomNotSupported
            };

            CollectionAssert.AreEqual(expectedResults, actualResults.ToList());
        }

        [TestMethod]
        public void HandleInvalidGameNumberTest()
        {
            var actual = _target.Parse(BadCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleNullResponseTest()
        {
            _handler.Setup(c => c.Handle(It.IsAny<LongPollExtendedGameNInformationData>())).Returns((LongPollExtendedGameNInformationResponse)null);

            var actual = _target.Parse(GoodCommand).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }
    }
}