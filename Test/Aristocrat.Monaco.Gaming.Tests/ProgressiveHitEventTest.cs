namespace Aristocrat.Monaco.Gaming.Tests
{
    using Contracts.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     This is a test class for ProgressiveHitEventTest and is intended
    ///     to contain all ProgressiveHitEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class ProgressiveHitEventTest
    {
        /// <summary>
        ///     A test for the three parameter constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var jackpot = new JackpotTransaction();
            var level = new Mock<IViewableProgressiveLevel>();

            var target = new ProgressiveHitEvent(jackpot, level.Object, false);

            Assert.IsNotNull(target);
            Assert.IsNotNull(target.Jackpot);
            Assert.IsFalse(target.IsRecovery);
        }
    }
}