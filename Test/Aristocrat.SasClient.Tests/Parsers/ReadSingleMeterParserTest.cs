namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    /// <summary>
    ///     Uses parameterized testing to test the long polls that read single meter values
    /// </summary>
    [TestClass]
    public class ReadSingleMeterParserTest
    {
        private static readonly SasClientConfiguration Configuration =
            new SasClientConfiguration { AccountingDenom = 1 };

        // contains the parameters for the tests and the expected responses
        private static IEnumerable<object[]> ReadSingleMeterTestData =>
            new List<object[]>
            {
                // each object array contains: the parser being tested, the long poll for the parser, the meter accessed by the handler, and
                // the meter value to test with. Note that the meter value returned by the Parse command ***MUST*** be 12345678 or the
                // hard-coded test result in the ParseTest method won't match.
                // Use (ulong)12345678000 for currency values and (ulong)12345678 for occurrence values.
                new object[]
                {
                    new LP10SendCanceledCreditsParser(Configuration),
                    LongPoll.SendCanceledCreditsMeter,
                    SasMeters.TotalCanceledCredits,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP11SendTotalCoinInParser(Configuration),
                    LongPoll.SendCoinInMeter,
                    SasMeters.TotalCoinIn,
                    true,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP12SendTotalCoinOutParser(Configuration),
                    LongPoll.SendCoinOutMeter,
                    SasMeters.TotalCoinOut,
                    true,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP13SendTotalDropParser(Configuration),
                    LongPoll.SendDropMeter,
                    SasMeters.TotalDrop,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP14SendTotalJackpotParser(Configuration),
                    LongPoll.SendJackpotMeter,
                    SasMeters.TotalJackpot,
                    true,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP15SendGamesPlayedParser(Configuration),
                    LongPoll.SendGamesPlayedMeter,
                    SasMeters.GamesPlayed,
                    true,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP16SendGamesWonParser(Configuration),
                    LongPoll.SendGamesWonMeter,
                    SasMeters.GamesWon,
                    true,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP17SendGamesLostParser(Configuration),
                    LongPoll.SendGamesLostMeter,
                    SasMeters.GamesLost,
                    true,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP2ASendTrueCoinInParser(Configuration),
                    LongPoll.SendTrueCoinIn,
                    SasMeters.TrueCoinIn,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP2BSendTrueCoinOutParser(Configuration),
                    LongPoll.SendTrueCoinOut,
                    SasMeters.TrueCoinOut,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP2CSendCurrentHopperLevelParser(Configuration),
                    LongPoll.SendCurrentHopperLevel,
                    SasMeters.CurrentHopperLevel,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP31Send1DollarBillsInParser(Configuration),
                    LongPoll.SendOneDollarInMeter,
                    SasMeters.DollarIn1,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP32Send2DollarBillsInParser(Configuration),
                    LongPoll.SendTwoDollarInMeter,
                    SasMeters.DollarsIn2,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP33Send5DollarBillsInParser(Configuration),
                    LongPoll.SendFiveDollarInMeter,
                    SasMeters.DollarsIn5,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP34Send10DollarBillsInParser(Configuration),
                    LongPoll.SendTenDollarInMeter,
                    SasMeters.DollarsIn10,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP35Send20DollarBillsInParser(Configuration),
                    LongPoll.SendTwentyDollarInMeter,
                    SasMeters.DollarsIn20,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP36Send50DollarBillsInParser(Configuration),
                    LongPoll.SendFiftyDollarInMeter,
                    SasMeters.DollarsIn50,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP37Send100DollarBillsInParser(Configuration),
                    LongPoll.SendOneHundredDollarInMeter,
                    SasMeters.DollarsIn100,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP38Send500DollarBillsInParser(Configuration),
                    LongPoll.SendFiveHundredDollarInMeter,
                    SasMeters.DollarsIn500,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP39Send1000DollarBillsInParser(Configuration),
                    LongPoll.SendOneThousandDollarInMeter,
                    SasMeters.DollarsIn1000,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP3ASend200DollarBillsInParser(Configuration),
                    LongPoll.SendTwoHundredDollarInMeter,
                    SasMeters.DollarsIn200,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP3BSend25DollarBillsInParser(Configuration),
                    LongPoll.SendTwentyFiveDollarInMeter,
                    SasMeters.DollarsIn25,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP3CSend2000DollarBillsInParser(Configuration),
                    LongPoll.SendTwoThousandDollarInMeter,
                    SasMeters.DollarsIn2000,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP3ESend2500DollarBillsInParser(Configuration),
                    LongPoll.SendTwoThousandFiveHundredDollarInMeter,
                    SasMeters.DollarsIn2500,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP3FSend5000DollarBillsInParser(Configuration),
                    LongPoll.SendFiveThousandDollarInMeter,
                    SasMeters.DollarsIn5000,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP40Send10000DollarBillsInParser(Configuration),
                    LongPoll.SendTenThousandDollarInMeter,
                    SasMeters.DollarsIn10000,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP41Send20000DollarBillsInParser(Configuration),
                    LongPoll.SendTwentyThousandDollarInMeter,
                    SasMeters.DollarsIn20000,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP42Send25000DollarBillsInParser(Configuration),
                    LongPoll.SendTwentyFiveThousandDollarInMeter,
                    SasMeters.DollarsIn25000,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP43Send50000DollarBillsInParser(Configuration),
                    LongPoll.SendFiftyThousandDollarInMeter,
                    SasMeters.DollarsIn50000,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP44Send100000DollarBillsInParser(Configuration),
                    LongPoll.SendOneHundredThousandDollarInMeter,
                    SasMeters.DollarsIn100000,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP45Send250DollarBillsInParser(Configuration),
                    LongPoll.SendTwoHundredFiftyDollarInMeter,
                    SasMeters.DollarsIn250,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP47SendCoinAmountFromExternalAcceptorParser(Configuration),
                    LongPoll.SendCoinAcceptedFromExternalAcceptor,
                    SasMeters.CoinAmountAcceptedFromExternal,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP49SendNumberOfBillsInStackerParser(Configuration),
                    LongPoll.SendNumberOfBillsInStacker,
                    SasMeters.NumberOfBillsInStacker,
                    false,
                    (ulong)12345678
                },
                new object[]
                {
                    new LP20TotalBillInValueParser(Configuration),
                    LongPoll.SendTotalBillInValueMeter,
                    SasMeters.TotalBillIn,
                    false,
                    (ulong)12345678
                }
            };

        [DynamicData(nameof(ReadSingleMeterTestData))]
        [DataTestMethod]
        public void CommandTest(SasSingleMeterLongPollParserBase target, LongPoll longPoll, SasMeters meter, bool multiDenomAware, ulong meterValue)
        {
            Assert.AreEqual(longPoll, target.Command);
        }

        [DynamicData(nameof(ReadSingleMeterTestData))]
        [DataTestMethod]
        public void ParseTest(SasSingleMeterLongPollParserBase target, LongPoll longPoll, SasMeters meter, bool multiDenomAware, ulong meterValue)
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)longPoll };

            var response = new LongPollReadMeterResponse(meter, meterValue)
            {
                ErrorCode = MultiDenomAwareErrorCode.NoError
            };

            var handler = new Mock<ISasLongPollHandler<LongPollReadMeterResponse, LongPollReadMeterData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollReadMeterData>())).Returns(response);
            target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)longPoll,
                0x12, 0x34, 0x56, 0x78,
            };

            var actual = target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [DynamicData(nameof(ReadSingleMeterTestData))]
        [DataTestMethod]
        public void MultiDenomParseTest(
            SasSingleMeterLongPollParserBase target,
            LongPoll longPoll,
            SasMeters meter,
            bool multiDenomAware,
            ulong meterValue)
        {
            const long denom = 1;
            var command = new List<byte> { TestConstants.SasAddress, (byte)longPoll, TestConstants.FakeCrc, TestConstants.FakeCrc };
            List<byte> expected;

            if (multiDenomAware)
            {
                expected = new List<byte>
                {
                    TestConstants.SasAddress, (byte)longPoll,
                    0x12, 0x34, 0x56, 0x78,
                };

                var response = new LongPollReadMeterResponse(meter, meterValue)
                {
                    ErrorCode = MultiDenomAwareErrorCode.NoError
                };

                var handler = new Mock<ISasLongPollHandler<LongPollReadMeterResponse, LongPollReadMeterData>>(MockBehavior.Default);
                handler.Setup(m => m.Handle(It.IsAny<LongPollReadMeterData>())).Returns(response);
                target.InjectHandler(handler.Object);
            }
            else
            {
                expected = new List<byte>
                {
                    TestConstants.SasAddress, 0x00, (byte)MultiDenomAwareErrorCode.NotMultiDenomAware
                };
            }

            var actual = target.Parse(command, denom).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WrongMeterNamesTest()
        {
            var target = new LP11SendTotalCoinInParser(Configuration);
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.SendCoinInMeter };

            // send the wrong meter in the response
            var response = new LongPollReadMeterResponse(SasMeters.TotalCoinOut, 0);

            var handler = new Mock<ISasLongPollHandler<LongPollReadMeterResponse, LongPollReadMeterData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollReadMeterData>())).Returns(response);
            target.InjectHandler(handler.Object);

            var expected = new List<byte> { TestConstants.SasAddress | TestConstants.Nack };

            var actual = target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
