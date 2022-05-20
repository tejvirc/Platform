namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Handlers;

    [TestClass]
    public class LP4FSendCurrentHopperStatusTest
    {
        private readonly LP4FSendCurrentHopperStatusHandler _target = new LP4FSendCurrentHopperStatusHandler();

        [TestMethod]
        public void CommandsTest()
        {
            var commands = _target.Commands;
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual(LongPoll.SendCurrentHopperStatus, commands[0]);
        }

        [TestMethod]
        public void HandleTest()
        {
            var output = _target.Handle(null);

            Assert.AreEqual(LongPollHopperStatusResponse.HopperStatus.HopperEmpty, output.Status);
            Assert.AreEqual(0xFF, output.PercentFull);
            Assert.AreEqual(0, output.Level);
        }
    }
}