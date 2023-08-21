namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;

    [TestClass]
    public class OptionListCommandBuilderTest
    {
        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<OptionListCommandBuilder>();
        }
    }
}