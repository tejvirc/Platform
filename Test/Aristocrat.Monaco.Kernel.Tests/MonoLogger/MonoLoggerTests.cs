namespace Aristocrat.Monaco.Kernel.Tests.MonoLogger
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MonoLogger = Kernel.MonoLogger;

    /// <summary>
    ///     This is a test class for MonoLoggerTest and is intended
    ///     to contain all MonoLoggerTest Unit Tests
    /// </summary>
    [TestClass]
    public class MonoLoggerTests
    {
        /// <summary>Test for constructors.</summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var logger = new MonoLogger();
            Assert.AreEqual(1, logger.LogLevel);
            logger = new MonoLogger(42);
            Assert.AreEqual(42, logger.LogLevel);
        }

        /// <summary>
        ///     Other than constructors, there isn't really anything that's testable in this class, so
        ///     just call all methods so NCover doesn't complain.
        /// </summary>
        [TestMethod]
        public void CallAllMethodsToMakeNCoverHappy()
        {
            var logger = new MonoLogger();
            Assert.IsFalse(logger.IsCanceled);
            logger.Cancel();
            logger.Log(string.Empty);
            logger.ReportError(string.Empty, new InvalidOperationException());
            logger.ReportWarning(string.Empty);
            logger.SetMessage(string.Empty);
            logger.SetProgress(0);
        }
    }
}