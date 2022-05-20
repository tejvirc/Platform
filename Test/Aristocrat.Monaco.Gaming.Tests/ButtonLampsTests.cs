namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Contracts;
    using Gaming;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ButtonLampsTests
    {
        private readonly Mock<IIO> _iio = new Mock<IIO>(MockBehavior.Default);
        private readonly Mock<IButtonService> _buttonService = new Mock<IButtonService>(MockBehavior.Default);

        private ButtonLamps _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new ButtonLamps(_iio.Object, _buttonService.Object);
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
            _target.Dispose();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(bool nullIIo, bool nullButtonService)
        {
            _target = new ButtonLamps(nullIIo ? null : _iio.Object, nullButtonService ? null : _buttonService.Object);
        }

        [DataRow((int)LampName.MaxBet, LampState.Off, 12, true, false)]
        [DataRow((int)LampName.MaxBet, LampState.On, 13, true, true)]
        [DataRow((int)LampName.MaxBet, LampState.On, -1, false, true)]
        [DataTestMethod]
        public void ButtonLampsTest(int buttonId, LampState state, int lampBit, bool lampExists, bool lampOn)
        {
            _buttonService.Setup(x => x.GetButtonLampBit(buttonId + (int)ButtonLogicalId.ButtonBase))
                .Returns(lampBit);
            _target.SetLampState(buttonId, state);

            if (lampExists)
            {
                _iio.Verify(x => x.SetButtonLamp((uint)lampBit, lampOn), Times.Once);
            }
            else
            {
                _iio.Verify(x => x.SetButtonLamp(It.IsAny<uint>(), It.IsAny<bool>()), Times.Never);
            }
        }

        [DataRow((int)LampName.MaxBet, LampState.Off)]
        [DataRow((int)LampName.MaxBet, LampState.On)]
        [DataRow((int)LampName.MaxBet, LampState.Blink)]
        [DataTestMethod]
        public void GetButtonState(int buttonId, LampState state)
        {
            _buttonService.Setup(x => x.GetButtonLampBit(buttonId + (int)ButtonLogicalId.ButtonBase)).Returns(-1);
            _target.SetLampState(buttonId, state);
            Assert.AreEqual(state, _target.GetLampState(buttonId));
        }

        [TestMethod]
        public void GetUnknownButtonState()
        {
            Assert.AreEqual(LampState.Off, _target.GetLampState((int)LampName.MaxBet));
        }
    }
}