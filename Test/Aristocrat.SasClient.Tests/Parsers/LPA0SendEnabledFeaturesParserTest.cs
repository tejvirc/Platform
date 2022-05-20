namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LPA0SendEnabledFeaturesParserTest
    {
        private LPA0SendEnabledFeaturesParser _target = new LPA0SendEnabledFeaturesParser();

        [TestInitialize]
        public void TestInitialize()
        {
            var handler = new Mock<ISasLongPollHandler<LongPollSendEnabledFeaturesResponse, LongPollSingleValueData<uint>>>(MockBehavior.Strict);

            var setupFeatures1Data = LongPollSendEnabledFeaturesResponse.Features1.JackpotMultiplier |
                LongPollSendEnabledFeaturesResponse.Features1.LegacyBonusAwards |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationExtensions |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationStyleBit1 |
                LongPollSendEnabledFeaturesResponse.Features1.TicketRedemption;
            var setupFeatures2Data = LongPollSendEnabledFeaturesResponse.Features2.MeterModelBit1 |
                LongPollSendEnabledFeaturesResponse.Features2.TicketsToTotalDropAndTotalCancelledCredits |
                LongPollSendEnabledFeaturesResponse.Features2.AdvancedFundTransfer |
                LongPollSendEnabledFeaturesResponse.Features2.MultidenomExtensions;
            var setupFeatures3Data = LongPollSendEnabledFeaturesResponse.Features3.MaxPollingRateBit0 |
                LongPollSendEnabledFeaturesResponse.Features3.MultipleSasProgressiveWinReporting |
                LongPollSendEnabledFeaturesResponse.Features3.MeterChangeNotification |
                LongPollSendEnabledFeaturesResponse.Features3.NonSasProgressiveHitReporting |
                LongPollSendEnabledFeaturesResponse.Features3.EnhancedProgressiveDataReporting;
            var setupFeatures4Data = (LongPollSendEnabledFeaturesResponse.Features4)0;

            handler.Setup(c => c.Handle(It.IsAny<LongPollSingleValueData<uint>>()))
                .Returns(new LongPollSendEnabledFeaturesResponse(
                        setupFeatures1Data,
                        setupFeatures2Data,
                        setupFeatures3Data,
                        setupFeatures4Data
                    )
                );
            _target.InjectHandler(handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendEnabledFeatures, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            byte expectedFeatures1Flags = 0b1101_0101;
            byte expectedFeatures2Flags = 0b1100_0110;
            byte expectedFeatures3Flags = 0b1100_0111;
            byte expectedFeatures4Flags = 0b0000_0000;

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnabledFeatures,
            };
            command.AddRange(Utilities.ToBcd(1, 2));
            var actual = _target.Parse(command).ToList();
            var expected = new List<Byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnabledFeatures
            };
            expected.AddRange(Utilities.ToBcd(1,2));
            expected.Add(expectedFeatures1Flags);
            expected.Add(expectedFeatures2Flags);
            expected.Add(expectedFeatures3Flags);
            expected.Add(expectedFeatures4Flags);
            expected.AddRange(new byte[2] { 0, 0 });

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
