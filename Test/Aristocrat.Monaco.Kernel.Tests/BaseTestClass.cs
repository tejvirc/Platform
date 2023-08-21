namespace Aristocrat.Monaco.Kernel.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Base class for all unit test classes.
    /// </summary>
    public class BaseTestClass
    {
        /// <summary>
        ///     This is the TestContext instance
        /// </summary>
        private TestContext testContextInstance;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        /// <summary>
        ///     Writes start unit test log message.
        /// </summary>
        protected void WriteStartLogMessage()
        {
            Debug(string.Format("Running {0}()...", TestContext.TestName));
        }

        /// <summary>
        ///     Writes end unit test log message.
        /// </summary>
        protected void WriteEndLogMessage()
        {
            Debug(string.Format("{0}()...completed!", TestContext.TestName));
        }

        /// <summary>
        ///     Writes specified message to Debug log session.
        /// </summary>
        /// <param name="message">Message.</param>
        protected void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        ///     Writes specified format message with parameters to Debug log session.
        /// </summary>
        /// <param name="message">Format message string.</param>
        /// <param name="args">Parameters.</param>
        protected void Debug(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }
    }
}