namespace Aristocrat.Monaco.Gaming.Tests.Runtime
{
    using System;
    using System.Collections.Generic;
    using Gaming.Runtime;
    using Gaming.Runtime.Server;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Summary description for GameProcessCommunicationTests
    /// </summary>
    [TestClass]
    public class GameProcessCommunicationTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameSessionIsNullExpectException()
        {
            var comms = new GameProcessCommunication(null);

            Assert.IsNull(comms);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var comms = new GameProcessCommunication(new List<IServerEndpoint>());

            Assert.IsNotNull(comms);
        }

        [TestMethod]
        public void WhenStartExpectSuccess()
        {
            var comms = new GameProcessCommunication(Factory_CreateGameSession());

            comms.StartComms();
            comms.Dispose();
        }

        [TestMethod]
        public void WhenStopExpectSuccess()
        {
            var comms = new GameProcessCommunication(Factory_CreateGameSession());

            comms.EndComms();
            comms.Dispose();
        }

        [TestMethod]
        public void WhenStopAfterStartExpectSuccess()
        {
            var comms = new GameProcessCommunication(Factory_CreateGameSession());

            comms.StartComms();
            comms.EndComms();
            comms.Dispose();
        }

        private static IEnumerable<IServerEndpoint> Factory_CreateGameSession()
        {
            return new List<IServerEndpoint> { new Mock<IServerEndpoint>().Object, new Mock<IServerEndpoint>().Object };
        }
    }
}