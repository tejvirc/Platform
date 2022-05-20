namespace Aristocrat.Monaco.UI.Common.Tests
{
    using Aristocrat.Monaco.UI.Common.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumDescriptionConverterTests
    {
        private const string HasDescription = "HAS DESCRIPTION";

        private EnumDescriptionConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            _target = new EnumDescriptionConverter();
        }

        [TestMethod]
        public void WhenEnumValueHasDescriptionAttribute_ShouldReturnDescription()
        {
            Assert.AreEqual(HasDescription, Convert(TheTestEnum.HasDescription));
        }

        [TestMethod]
        public void WhenEnumValueDoesNotHaveDescriptionAttribute_ShouldReturnEnumAsString()
        {
            Assert.AreEqual("NoDescription", Convert(TheTestEnum.NoDescription));
        }

        [DataTestMethod]
        [DataRow("definitely not an enum", DisplayName = "non-empty string test")]
        [DataRow("", DisplayName = "empty string test")]
        [DataRow(42, DisplayName ="integer test")]
        [DataRow(null, DisplayName = "null test")]
        public void WhenValueIsNotAnEnum_ShouldReturnEmptyString(object val)
        {
            Assert.AreEqual(string.Empty, Convert(val));
        }

        private enum TheTestEnum
        {
            NoDescription,

            [System.ComponentModel.DescriptionAttribute(EnumDescriptionConverterTests.HasDescription)]
            HasDescription,
        }

        private object Convert(object val)
        {
            return _target.Convert(
                val,
                targetType: null,
                parameter: null,
                culture: null);
        }
    }
}
