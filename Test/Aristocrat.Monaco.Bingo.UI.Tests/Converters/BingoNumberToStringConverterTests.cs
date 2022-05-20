namespace Aristocrat.Monaco.Bingo.UI.Tests.Converters
{
    using Aristocrat.Monaco.Bingo.UI.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BingoNumberToStringConverterTests
    {
        private const string CardCenterText = " * ";
        private const string CardNumberUnknown = " ? ";

        private readonly BingoNumberToStringConverter _target = new BingoNumberToStringConverter();

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ConvertTest()
        {
            Assert.AreEqual(CardCenterText, _target.Convert(0, null, null, null));
            Assert.AreEqual("02", _target.Convert(2, null, null, null));
            Assert.AreEqual(CardNumberUnknown, _target.Convert(2.0f, null, null, null));
            Assert.AreEqual(CardNumberUnknown, _target.Convert("2", null, null, null));
        }
    }
}
