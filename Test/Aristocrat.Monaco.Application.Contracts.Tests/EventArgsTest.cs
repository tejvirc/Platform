namespace Aristocrat.Monaco.Application.Contracts.Tests
{
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TiltLogger;

    /// <summary>
    ///     Tests for the EventArgs classes
    /// </summary>
    [TestClass]
    public class EventArgsTest
    {
                [TestMethod]
        public void TiltLogAppendedEventArgsTest()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            var localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Default);
            localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);
            var localizer = new Mock<ILocalizer>();
            localizer.Setup(x => x.GetString(It.IsAny<string>())).Returns("empty");
            localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(localizer.Object);

            EventDescription msg = new EventDescription();

            var target = new TiltLogAppendedEventArgs(msg, null);
            Assert.IsNotNull(target);
            Assert.AreEqual(msg, target.Message);
        }

        [TestMethod]
        public void MeterChangedEventArgsTest()
        {
            long amount = 1234;
            var target = new MeterChangedEventArgs(amount);
            Assert.IsNotNull(target);
            Assert.AreEqual(amount, target.Amount);
        }
    }
}