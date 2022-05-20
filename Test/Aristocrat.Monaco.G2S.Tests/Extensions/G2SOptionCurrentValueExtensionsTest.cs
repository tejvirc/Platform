namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class G2SOptionCurrentValueExtensionsTest
    {
        private const string ParameterId = "G2S_paramId";

        private const string Value = "1";

        [TestMethod]
        public void WhenCreateOptionCurrentValueFromG2SItemAndItemsAreEmptyExpectEmpty()
        {
            var currentValues = new optionCurrentValues();

            var converted = currentValues.CreateOptionCurrentValueFromG2SItem();

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.Length);
        }

        [TestMethod]
        public void WhenCreateOptionCurrentValueFromG2SItemItemIsIntegerExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            currentValues.Items = new[] { new integerValue1() };
            var converted = currentValues.CreateOptionCurrentValueFromG2SItem().First();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.Integer, converted.ParameterType);
        }

        [TestMethod]
        public void WhenCreateOptionCurrentValueFromG2SItemItemIsDecimalExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            currentValues.Items = new[] { new decimalValue1() };
            var converted = currentValues.CreateOptionCurrentValueFromG2SItem().First();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.Decimal, converted.ParameterType);
        }

        [TestMethod]
        public void WhenCreateOptionCurrentValueFromG2SItemItemIsStringExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            currentValues.Items = new[] { new stringValue1() };
            var converted = currentValues.CreateOptionCurrentValueFromG2SItem().First();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.String, converted.ParameterType);
        }

        [TestMethod]
        public void WhenCreateOptionCurrentValueFromG2SItemItemIsBooleanExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            currentValues.Items = new[] { new booleanValue1() };
            var converted = currentValues.CreateOptionCurrentValueFromG2SItem().First();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.Boolean, converted.ParameterType);
        }

        [TestMethod]
        public void WhenCreateOptionCurrentValueFromG2SItemItemIsComplexExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            var complexValue = new complexValue();
            complexValue.Items = new[] { new integerValue1() };
            currentValues.Items = new[] { complexValue };

            var converted = currentValues.CreateOptionCurrentValueFromG2SItem().First();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.Complex, converted.ParameterType);
            Assert.AreEqual(1, converted.ChildValues.Count());
        }

        [TestMethod]
        public void WhenCreateOptionParameterFromG2SItemIsNullExpectNull()
        {
            var currentValues = new optionCurrentValues();
            var converted = currentValues.CreateOptionParameterFromG2SItem();

            Assert.IsNull(converted);
        }

        [TestMethod]
        public void WhenCreateOptionParameterFromG2SItemItemIsIntegerExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            currentValues.Items = new[] { new integerValue1() };
            var converted = currentValues.CreateOptionParameterFromG2SItem();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.Integer, converted.ParameterType);
        }

        [TestMethod]
        public void WhenCreateOptionParameterFromG2SItemItemIsDecimalExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            currentValues.Items = new[] { new decimalValue1() };
            var converted = currentValues.CreateOptionParameterFromG2SItem();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.Decimal, converted.ParameterType);
        }

        [TestMethod]
        public void WhenCreateOptionParameterFromG2SItemItemIsStringExpectSuceess()
        {
            var currentValues = new optionCurrentValues();
            currentValues.Items = new[] { new stringValue1() };
            var converted = currentValues.CreateOptionParameterFromG2SItem();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.String, converted.ParameterType);
        }

        [TestMethod]
        public void WhenCreateOptionParameterromG2SItemItemIsBooleanExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            currentValues.Items = new[] { new booleanValue1() };
            var converted = currentValues.CreateOptionParameterFromG2SItem();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.Boolean, converted.ParameterType);
        }

        [TestMethod]
        public void WhenCreateOptionParameterFromG2SItemItemIsComplexExpectSuccess()
        {
            var currentValues = new optionCurrentValues();
            var complexValue = new complexValue();
            complexValue.Items = new[] { new integerValue1() };
            currentValues.Items = new[] { complexValue };

            var converted = currentValues.CreateOptionParameterFromG2SItem();

            Assert.IsNotNull(converted);

            Assert.AreEqual(OptionConfigParameterType.Complex, converted.ParameterType);
            Assert.AreEqual(1, converted.ChildItems.Count());
        }

        [TestMethod]
        public void WhenAddSubValueItemsIsNullExpectSuccess()
        {
            var value = new OptionConfigValue
            {
                ValueType = OptionConfigValueType.Complex
            };

            var subvalue = new OptionConfigValue
            {
                ValueType = OptionConfigValueType.Integer
            };

            value.AddSubValue(subvalue);

            Assert.IsNotNull(value);

            Assert.AreEqual(1, value.Items.Count());
        }

        [TestMethod]
        public void WhenCreateG2SOptionIntegerValueExpectSuccess()
        {
            var value = new OptionConfigValue
            {
                ValueType = OptionConfigValueType.Integer,
                ParameterId = ParameterId,
                Value = Value
            };

            var convertedDefault = value.CreateG2SOptionCurrentValue().Item() as integerValue1;
            var convertedCurrent = value.CreateG2SOptionDefaultValue().Item() as integerValue1;

            Assert.IsNotNull(convertedDefault);
            Assert.IsNotNull(convertedCurrent);

            Assert.AreEqual(ParameterId, convertedCurrent.paramId);
            Assert.AreEqual(Value, convertedCurrent.Value.ToString());
        }

        [TestMethod]
        public void WhenCreateG2SOptionDecimalValueExpectSuccess()
        {
            var value = new OptionConfigValue
            {
                ValueType = OptionConfigValueType.Decimal,
                ParameterId = ParameterId,
                Value = Value
            };

            var convertedDefault = value.CreateG2SOptionCurrentValue().Item() as decimalValue1;
            var convertedCurrent = value.CreateG2SOptionDefaultValue().Item() as decimalValue1;

            Assert.IsNotNull(convertedDefault);
            Assert.IsNotNull(convertedCurrent);

            Assert.AreEqual(ParameterId, convertedCurrent.paramId);
            Assert.AreEqual(Value, convertedCurrent.Value.ToString());
        }

        [TestMethod]
        public void WhenCreateG2SOptionStringValueExpectSuccess()
        {
            var value = new OptionConfigValue
            {
                ValueType = OptionConfigValueType.String,
                ParameterId = ParameterId,
                Value = Value
            };

            var convertedDefault = value.CreateG2SOptionCurrentValue().Item() as stringValue1;
            var convertedCurrent = value.CreateG2SOptionDefaultValue().Item() as stringValue1;

            Assert.IsNotNull(convertedDefault);
            Assert.IsNotNull(convertedCurrent);

            Assert.AreEqual(ParameterId, convertedCurrent.paramId);
            Assert.AreEqual(Value, convertedCurrent.Value);
        }

        [TestMethod]
        public void WhenCreateG2SOptionBooleanValueExpectSuccess()
        {
            var value = new OptionConfigValue
            {
                ValueType = OptionConfigValueType.Boolean,
                ParameterId = ParameterId,
                Value = true.ToString()
            };

            var convertedDefault = value.CreateG2SOptionCurrentValue().Item() as booleanValue1;
            var convertedCurrent = value.CreateG2SOptionDefaultValue().Item() as booleanValue1;

            Assert.IsNotNull(convertedDefault);
            Assert.IsNotNull(convertedCurrent);

            Assert.AreEqual(ParameterId, convertedCurrent.paramId);
            Assert.AreEqual(true, convertedCurrent.Value);
        }

        [TestMethod]
        public void WhenCreateG2SOptionComplexValueExpectSuccess()
        {
            var value = new OptionConfigValue
            {
                ValueType = OptionConfigValueType.Complex,
                ParameterId = ParameterId,
                Items = new List<OptionConfigValue>
                {
                    new OptionConfigValue
                    {
                        ValueType = OptionConfigValueType.Integer,
                        ParameterId = ParameterId,
                        Value = Value
                    }
                }
            };

            var convertedDefault = value.CreateG2SOptionCurrentValue().Item() as complexValue;
            var convertedCurrent = value.CreateG2SOptionDefaultValue().Item() as complexValue;

            Assert.IsNotNull(convertedDefault);
            Assert.IsNotNull(convertedCurrent);

            Assert.AreEqual(1, convertedDefault.Items.Length);
            Assert.AreEqual(1, convertedCurrent.Items.Length);

            var subvalue = convertedCurrent.Items.First() as integerValue1;

            Assert.AreEqual(ParameterId, convertedCurrent.paramId);
            Assert.AreEqual(Value, subvalue.Value.ToString());
            Assert.AreEqual(ParameterId, subvalue.paramId);
        }
    }
}