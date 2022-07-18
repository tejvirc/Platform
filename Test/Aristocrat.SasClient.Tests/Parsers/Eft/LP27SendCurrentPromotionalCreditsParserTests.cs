namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client.EFT;
    using Aristocrat.Sas.Client.Eft.Response;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;

    [TestClass]
    public class LP27SendCurrentPromotionalCreditsParserTests
    {
        private LP27SendCurrentPromotionalCreditsParser _target;

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void ShouldThrowExceptionIfReturnNullFromHandlerTest()
        {
            var theParser = new LP27SendCurrentPromotionalCreditsParser();
            theParser.Parse(new List<byte>());
        }

        [TestMethod]
        public void CommandTest()
        {
            var handler = new Mock<ISasLongPollHandler<EftSendCurrentPromotionalCreditsResponse, LongPollData>>(MockBehavior.Strict);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(new EftSendCurrentPromotionalCreditsResponse(1000ul));
            _target = new LP27SendCurrentPromotionalCreditsParser();
            _target.InjectHandler(handler.Object);
            Assert.AreEqual(LongPoll.EftSendCurrentPromotionalCredits, _target.Command);
            handler.Verify(m => m.Handle(It.IsAny<LongPollData>()), Times.Never);
        }

        [TestMethod]
        public void ShouldHaveCorrectBytesTest()
        {        
            var handler = new Mock<ISasLongPollHandler<EftSendCurrentPromotionalCreditsResponse, LongPollData>>(MockBehavior.Strict);
            var mockedCurrentPromoCredit = 1000ul;
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(new EftSendCurrentPromotionalCreditsResponse(mockedCurrentPromoCredit));
            _target = new LP27SendCurrentPromotionalCreditsParser();
            _target.InjectHandler(handler.Object);

            var result = _target.Parse(new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftSendCurrentPromotionalCredits });
            handler.Verify(m => m.Handle(It.IsAny<LongPollData>()), Times.Once);
            Assert.AreEqual(6, result.Count); //6 bytes: 1 address, 1 command, 4 current promo credits
            Assert.AreEqual(TestConstants.SasAddress, result.ElementAt(0));
            Assert.AreEqual((byte)LongPoll.EftSendCurrentPromotionalCredits, result.ElementAt(1));
            var parsedValue = Utilities.FromBcdWithValidation(result.Skip(2).Take(4).ToArray());
            Assert.IsTrue(parsedValue.validBcd);
            Assert.AreEqual(mockedCurrentPromoCredit, parsedValue.number);
        }
    }
}