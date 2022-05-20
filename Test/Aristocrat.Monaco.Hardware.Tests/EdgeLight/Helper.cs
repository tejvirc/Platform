namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class Helper
    {
        public static void AssertThrow<T>(Action action) where T : Exception
        {
            try
            {
                action();
                Assert.Fail();
            }
            catch (T)
            {
            }
        }

        public static void AssertNoThrow<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                Assert.Fail();
            }
        }
    }
}