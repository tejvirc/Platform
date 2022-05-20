namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Hardware.Contracts.Audio;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    ///     Tests for the SoundHandler class
    /// </summary>
    [TestClass]
    public class SoundHandlerTest
    {
        private Mock<IAudio> _audio;
        private Mock<IGameService> _gameService;
        private SoundHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _audio = new Mock<IAudio>(MockBehavior.Default);
            _gameService = new Mock<IGameService>();

            _target = new SoundHandler(_audio.Object, _gameService.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(3, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SoundOff));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SoundOn));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.GameSoundsDisable));
        }

        [TestMethod]
        public void HandleTest()
        {
            // TODO: Actually test something

            var dataAllOff = new LongPollSingleValueData<SoundActions> { Value = SoundActions.AllOff };
            var actual = _target.Handle(dataAllOff);
            Assert.IsNull(actual);

            var dataGameOff = new LongPollSingleValueData<SoundActions> { Value = SoundActions.GameOff };
            actual = _target.Handle(dataGameOff);
            Assert.IsNull(actual);

            var dataAllOn = new LongPollSingleValueData<SoundActions> { Value = SoundActions.AllOn };
            actual = _target.Handle(dataAllOn);
            Assert.IsNull(actual);
        }
    }
}