namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Delegate used by tests that execute code and  capture any thrown exception.
    /// </summary>
    public delegate void TestDelegate();

    public static class AssertHelper
    {
        /// <summary>
        ///     Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        public static TActual Throws<TActual>(TestDelegate code) where TActual : Exception
        {
            return Throws<TActual>(code, string.Empty, null);
        }

        /// <summary>
        ///     Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static TActual Throws<TActual>(TestDelegate code, string message, params object[] args)
            where TActual : Exception
        {
            return (TActual)Throws(typeof(TActual), code, message, args);
        }

        /// <summary>
        ///     Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Throws(
            Type expectedExceptionType,
            TestDelegate code,
            string message,
            params object[] args)
        {
            Exception actual = null;

            try
            {
                code();
            }
            catch (Exception ex)
            {
                actual = ex;
            }

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, expectedExceptionType, message, args);

            return actual;
        }
    }
}