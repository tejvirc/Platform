namespace Aristocrat.Monaco.G2S.Data.Tests.Model
{
    using System;
    using Data.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OptionConfigValueTypeConverterTest
    {
        [TestMethod]
        public void WhenConvertToIntegerValueExpectSuccess()
        {
            var value = "25";
            var result = OptionConfigValueTypeConverter.Convert(value, OptionConfigValueType.Integer);

            Assert.AreEqual(typeof(long), result.GetType());
            Assert.AreEqual(Convert.ToInt64(value), result);
        }

        [TestMethod]
        public void WhenConvertToDecimalValueExpectSuccess()
        {
            var value = "25,5123";
            var result = OptionConfigValueTypeConverter.Convert(value, OptionConfigValueType.Decimal);

            Assert.AreEqual(typeof(decimal), result.GetType());
            Assert.AreEqual(Convert.ToDecimal(value), result);
        }

        [TestMethod]
        public void WhenConvertToBooleanValueExpectSuccess()
        {
            var value = true.ToString();
            var result = OptionConfigValueTypeConverter.Convert(value, OptionConfigValueType.Boolean);

            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.AreEqual(Convert.ToBoolean(value), result);
        }

        [TestMethod]
        public void WhenConvertToStringValueExpectSuccess()
        {
            var value = "string_value";
            var result = OptionConfigValueTypeConverter.Convert(value, OptionConfigValueType.String);

            Assert.AreEqual(typeof(string), result.GetType());
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void WhenConvertToComplexValueExpectException()
        {
            var value = "complex_value";
            OptionConfigValueTypeConverter.Convert(value, OptionConfigValueType.Complex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenConvertToUnknownValueExpectException()
        {
            var value = "value";
            OptionConfigValueTypeConverter.Convert(value, (OptionConfigValueType)1000);
        }
    }
}