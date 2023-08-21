namespace Aristocrat.SasClient.Tests
{
    using System.Reflection;
    using FakeParsers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Client;

    /// <summary>
    ///     Contains test for the SasLongPollParserFactory class
    /// </summary>
    [TestClass]
    public class SasLongPollParserFactoryTest
    {
        private readonly SasLongPollParserFactory _target = new SasLongPollParserFactory();

        [TestMethod]
        public void LoadParsersTest()
        {
            var configuration = new SasClientConfiguration
            {
                HandlesAft = true,
                HandlesGameStartEnd = true,
                HandlesGeneralControl = true,
                HandlesLegacyBonusing = true,
                HandlesValidation = true
            };

            _target.LoadParsers(configuration, Assembly.GetExecutingAssembly());

            // we should have all the fake parsers loaded now. Try to get them.
            // check unhandled parser. This should always be there.
            Assert.AreEqual(typeof(UnhandledLongPollParser), _target.GetParserForLongPoll(LongPoll.None).GetType());

            // these parsers should be present if the LoadParsers call worked.
            Assert.AreEqual(typeof(FakeAftParser), _target.GetParserForLongPoll(LongPoll.AftTransferFunds).GetType());
            Assert.AreEqual(typeof(FakeGeneralControlParser), _target.GetParserForLongPoll(LongPoll.EnableDisableGameN).GetType());
            Assert.AreEqual(typeof(FakeLegacyBonusParser), _target.GetParserForLongPoll(LongPoll.InitiateLegacyBonusPay).GetType());
            Assert.AreEqual(typeof(FakeValidationParser), _target.GetParserForLongPoll(LongPoll.RedeemTicket).GetType());
            Assert.AreEqual(typeof(FakeReadSingleMeterParser), _target.GetParserForLongPoll(LongPoll.SendCanceledCreditsMeter).GetType());
        }

        [TestMethod]
        public void InjectHandlerTest()
        {
            // nothing to test for now
            _target.InjectHandler(null, LongPoll.None);
        }
    }
}
