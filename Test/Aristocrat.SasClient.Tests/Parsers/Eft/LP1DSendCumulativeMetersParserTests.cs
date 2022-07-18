namespace Aristocrat.SasClient.Tests.Parsers.Eft
{
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client.EFT;

    [TestClass]
    public class LP1DSendCumulativeMetersParserTests
    {
        private LP1DSendCumulativeMetersParser _target;
        private Mock<ISasLongPollHandler<CumulativeEftMeterData, LongPollData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<CumulativeEftMeterData, LongPollData>>(MockBehavior.Strict);
            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(It.IsAny<CumulativeEftMeterData>());
            _target = new LP1DSendCumulativeMetersParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest() => Assert.AreEqual(LongPoll.EftSendCumulativeMeters, _target.Command);


        [DataRow((ulong)0, (ulong)0, (ulong)0, (ulong)0, DisplayName = "All Credits Zero")]
        [DataRow((ulong)1000, (ulong)0, (ulong)0, (ulong)0, DisplayName = "Cashable 1000")]
        [DataRow((ulong)0, (ulong)1000, (ulong)0, (ulong)0, DisplayName = "NonCashableCredits 1000")]
        [DataRow((ulong)0, (ulong)0, (ulong)1000, (ulong)0, DisplayName = "PromotionalCredits 1000")]
        [DataRow((ulong)0, (ulong)0, (ulong)0, (ulong)1000, DisplayName = "TransferredCredits 1000")]
        [DataRow((ulong)1000, (ulong)1000, (ulong)1000, (ulong)1000, DisplayName = "All Credits 1000")]
        [DataRow((ulong)0, (ulong)1000, (ulong)1000, (ulong)0, DisplayName = "NonCashableCredits and PromotionalCredits 1000")]
        [DataRow((ulong)1000, (ulong)0, (ulong)1000, (ulong)0, DisplayName = "PromotionalCredits and Cashable 1000")]
        [DataRow((ulong)1000, (ulong)1000, (ulong)0, (ulong)0, DisplayName = "NonCashableCredits and Cashable 1000")]
        [DataRow((ulong)0, (ulong)0, (ulong)1000, (ulong)1000, DisplayName = "TransferredCredits and PromotionalCredits 1000")]
        [DataTestMethod]
        public void ParseTest(ulong CashableCredits, ulong NonCashableCredits, ulong PromotionalCredits, ulong TransferredCredits)
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.EftSendCumulativeMeters
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(
                new CumulativeEftMeterData {
                    PromotionalCredits = PromotionalCredits,
                    NonCashableCredits = NonCashableCredits,
                    TransferredCredits = TransferredCredits,
                    CashableCredits = CashableCredits
                });

            var actual = _target.Parse(command).ToArray();
            
            var expectedResults = new List<byte>
            {
                
                TestConstants.SasAddress,
                (byte)LongPoll.EftSendCumulativeMeters
            };
            expectedResults.AddRange(Utilities.ToBcd(PromotionalCredits, SasConstants.Bcd8Digits));
            expectedResults.AddRange(Utilities.ToBcd(NonCashableCredits, SasConstants.Bcd8Digits));
            expectedResults.AddRange(Utilities.ToBcd(TransferredCredits, SasConstants.Bcd8Digits));
            expectedResults.AddRange(Utilities.ToBcd(CashableCredits, SasConstants.Bcd8Digits));
            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }
    }
}
