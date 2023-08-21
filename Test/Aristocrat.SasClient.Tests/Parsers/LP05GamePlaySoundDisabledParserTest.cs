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
    ///     Contains the tests for the LP05GamePlaySoundDisabledParser class
    /// </summary>
    [TestClass]
    public class LP05GamePlaySoundDisabledParserTest
    {
        private LP05GamePlaySoundDisabledParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var handler =
                new Mock<ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<SoundActions>>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollSingleValueData<SoundActions>>())).Returns((LongPollResponse)null);

            _target = new LP05GamePlaySoundDisabledParser();
            _target.InjectHandler(handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.GameSoundsDisable, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.GameSoundsDisable, TestConstants.FakeCrc, TestConstants.FakeCrc };

            var actual = _target.Parse(command).ToArray();

            Assert.AreEqual(command[0], actual[0]);
        }
    }
}
