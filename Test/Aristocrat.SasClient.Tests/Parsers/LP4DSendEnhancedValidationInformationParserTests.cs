namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP4DSendEnhancedValidationInformationParserTests
    {
        private static readonly SendEnhancedValidationInformationResponse SuccessfulResponse =
            new SendEnhancedValidationInformationResponse
            {
                Amount = 100,
                ExpirationDate = SasConstants.MaxTicketExpirationDays,
                Index = 10,
                PoolId = 3,
                Successful = true,
                TicketNumber = 1,
                ValidationDate = new DateTime(2019, 1, 3, 15, 30, 58),
                ValidationType = 0,
                ValidationSystemId = 1,
                ValidationNumber = 6429188185446104
            };

        private static readonly SendEnhancedValidationInformationResponse FailedResponse =
            new SendEnhancedValidationInformationResponse
            {
                Successful = false
            };

        private static readonly List<byte> SuccessfulResponseData = new List<byte>
        {
            TestConstants.SasAddress,
            (byte)LongPoll.SendEnhancedValidationInformation,
            (byte)SuccessfulResponse.ValidationType, // validation type
            0x0A, // Index
            0x01, 0x03, 0x20, 0x19, // Date
            0x15, 0x30, 0x58, // Time
            0x64, 0x29, 0x18, 0x81, 0x85, 0x44, 0x61, 0x04, // Validation number
            0x00, 0x00, 0x00, 0x01, 0x00, // Amount
            0x01, 0x00, // Ticket Number
            0x01, // Validation System Id
            0x00, 0x00, 0x99, 0x99, // Expiration Date
            0x03, 0x00 // Pool Id
        };

        private static IEnumerable<object[]> ParseValidFunctionCodesData => new List<object[]>
        {
            new object[]
            {
                new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.SendEnhancedValidationInformation,
                    SasConstants.CurrentValidation,
                    TestConstants.FakeCrc,
                    TestConstants.FakeCrc
                },
                SuccessfulResponse,
                SuccessfulResponseData
            },
            new object[]
            {
                new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.SendEnhancedValidationInformation,
                    SasConstants.MaxValidationIndex,
                    TestConstants.FakeCrc,
                    TestConstants.FakeCrc
                },
                SuccessfulResponse,
                SuccessfulResponseData
            },
            new object[]
            {
                new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.SendEnhancedValidationInformation,
                    SasConstants.LookAhead,
                    TestConstants.FakeCrc,
                    TestConstants.FakeCrc
                },
                SuccessfulResponse,
                SuccessfulResponseData
            },
            new object[]
            {
                new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.SendEnhancedValidationInformation,
                    SasConstants.CurrentValidation,
                    TestConstants.FakeCrc,
                    TestConstants.FakeCrc
                },
                FailedResponse,
                new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.SendEnhancedValidationInformation,
                    0x00, // validation type
                    0x00, // Index
                    0x00, 0x00, 0x00, 0x00, // Date
                    0x00, 0x00, 0x00, // Time
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Validation number
                    0x00, 0x00, 0x00, 0x00, 0x00, // Amount
                    0x00, 0x00, // Ticket Number
                    0x00, // Validation System Id
                    0x00, 0x00, 0x00, 0x00, // Expiration Date
                    0x00, 0x00 // Pool Id
                }
            },
        };

        private LP4DSendEnhancedValidationInformationParser _target;

        private Mock<ISasLongPollHandler<
            SendEnhancedValidationInformationResponse,
            SendEnhancedValidationInformation>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP4DSendEnhancedValidationInformationParser();
            _handler =
                new Mock<ISasLongPollHandler<SendEnhancedValidationInformationResponse,
                    SendEnhancedValidationInformation>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendEnhancedValidationInformation, _target.Command);
        }

        [DynamicData(nameof(ParseValidFunctionCodesData))]
        [DataTestMethod]
        public void ParseValidFunctionCodeTest(
            List<byte> command,
            SendEnhancedValidationInformationResponse handledResponse,
            List<byte> expectedResult)
        {
            _handler.Setup(c => c.Handle(It.IsAny<SendEnhancedValidationInformation>())).Returns(handledResponse);
            var response = _target.Parse(command);
            CollectionAssert.AreEquivalent(expectedResult, response.ToList());
        }

        [TestMethod]
        public void ParseInvalidFunctionCodeTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnhancedValidationInformation,
                SasConstants.MaxValidationIndex + 1,
                TestConstants.FakeCrc,
                TestConstants.FakeCrc
            };

            var expectedResult = new List<byte>
            {
                TestConstants.SasAddress | SasConstants.Nack
            };

            var response = _target.Parse(command);
            CollectionAssert.AreEquivalent(expectedResult, response.ToList());
        }
    }
}