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
    public class G2SOptionConfigParameterExtensionsTest
    {
        private const long Id = 1;

        private const string ParameterId = "G2S_paramId";

        private const string ParameterName = "paramId";

        private const string ParameterHelp = "paramId";

        private const bool ParameterKey = true;

        private const string AllowedValues = "[1]";

        private const bool CanModLocal = true;

        private const bool CanModRemote = true;

        private const long MinInclude = 1;

        private const long MaxInclude = 2;

        private const int Fractional = 15;

        private const int MinLen = 1;

        private const int MaxLen = 2;

        [TestMethod]
        public void WhenCreateG2SOptionConfigIntegerParameterExpectSuccess()
        {
            var parameter = new OptionConfigIntegerParameter
            {
                ParameterId = ParameterId,
                ParameterName = ParameterName,
                ParameterHelp = ParameterHelp,
                ParameterKey = ParameterKey,
                AllowedValues = AllowedValues,
                CanModLocal = CanModLocal,
                CanModRemote = CanModRemote,
                MinInclude = MinInclude,
                MaxInclude = MaxInclude
            };

            var convertedParameter = parameter
                .CreateG2SOptionConfigParameter().Item as integerParameter;

            Assert.AreEqual(parameter.ParameterId, convertedParameter.paramId);
            Assert.AreEqual(parameter.ParameterName, convertedParameter.paramName);
            Assert.AreEqual(parameter.ParameterHelp, convertedParameter.paramHelp);
            Assert.AreEqual(parameter.ParameterKey, convertedParameter.paramKey);
            // Assert.AreEqual(parameter.AllowedValues, JsonConvert.SerializeObject(convertedParameter.integerEnum)); // TODO: commented until correct integerEnum
            Assert.AreEqual(parameter.CanModLocal, convertedParameter.canModLocal);
            Assert.AreEqual(parameter.CanModRemote, convertedParameter.canModRemote);
            Assert.AreEqual(parameter.MinInclude, convertedParameter.minIncl);
            Assert.AreEqual(parameter.MaxInclude, convertedParameter.maxIncl);
        }

        [TestMethod]
        public void WhenCreateG2SOptionConfigDecimalParameterExpectSuccess()
        {
            var parameter = new OptionConfigDecimalParameter
            {
                ParameterId = ParameterId,
                ParameterName = ParameterName,
                ParameterHelp = ParameterHelp,
                ParameterKey = ParameterKey,
                AllowedValues = AllowedValues,
                CanModLocal = CanModLocal,
                CanModRemote = CanModRemote,
                MinInclude = MinInclude,
                MaxInclude = MaxInclude,
                Fractional = Fractional
            };

            var convertedParameter = parameter
                .CreateG2SOptionConfigParameter().Item as decimalParameter;

            Assert.AreEqual(parameter.ParameterId, convertedParameter.paramId);
            Assert.AreEqual(parameter.ParameterName, convertedParameter.paramName);
            Assert.AreEqual(parameter.ParameterHelp, convertedParameter.paramHelp);
            Assert.AreEqual(parameter.ParameterKey, convertedParameter.paramKey);
            // Assert.AreEqual(parameter.AllowedValues, JsonConvert.SerializeObject(convertedParameter.decimalEnum)); // TODO: commented until correct decimalEnum
            Assert.AreEqual(parameter.CanModLocal, convertedParameter.canModLocal);
            Assert.AreEqual(parameter.CanModRemote, convertedParameter.canModRemote);
            Assert.AreEqual(parameter.MinInclude, convertedParameter.minIncl);
            Assert.AreEqual(parameter.MaxInclude, convertedParameter.maxIncl);
            Assert.AreEqual(parameter.Fractional, convertedParameter.fracDig);
        }

        [TestMethod]
        public void WhenCreateG2SOptionConfigStringParameterExpectSuccess()
        {
            var parameter = new OptionConfigStringParameter
            {
                ParameterId = ParameterId,
                ParameterName = ParameterName,
                ParameterHelp = ParameterHelp,
                ParameterKey = ParameterKey,
                AllowedValues = AllowedValues,
                CanModLocal = CanModLocal,
                CanModRemote = CanModRemote,
                MinLen = MinLen,
                MaxLen = MaxLen
            };

            var convertedParameter = parameter
                .CreateG2SOptionConfigParameter().Item as stringParameter;

            Assert.AreEqual(parameter.ParameterId, convertedParameter.paramId);
            Assert.AreEqual(parameter.ParameterName, convertedParameter.paramName);
            Assert.AreEqual(parameter.ParameterHelp, convertedParameter.paramHelp);
            Assert.AreEqual(parameter.ParameterKey, convertedParameter.paramKey);
            // Assert.AreEqual(parameter.AllowedValues, JsonConvert.SerializeObject(convertedParameter.stringEnum)); // TODO: commented until correct stringEnum
            Assert.AreEqual(parameter.CanModLocal, convertedParameter.canModLocal);
            Assert.AreEqual(parameter.CanModRemote, convertedParameter.canModRemote);
            Assert.AreEqual(parameter.MinLen, convertedParameter.minLen);
            Assert.AreEqual(parameter.MaxLen, convertedParameter.maxLen);
        }

        [TestMethod]
        public void WhenCreateG2SOptionConfigBooleanParameterExpectSuccess()
        {
            var parameter = new OptionConfigBooleanParameter
            {
                ParameterId = ParameterId,
                ParameterName = ParameterName,
                ParameterHelp = ParameterHelp,
                ParameterKey = ParameterKey,
                AllowedValues = AllowedValues,
                CanModLocal = CanModLocal,
                CanModRemote = CanModRemote,
            };

            var convertedParameter = parameter
                .CreateG2SOptionConfigParameter().Item as booleanParameter;

            Assert.AreEqual(parameter.ParameterId, convertedParameter.paramId);
            Assert.AreEqual(parameter.ParameterName, convertedParameter.paramName);
            Assert.AreEqual(parameter.ParameterHelp, convertedParameter.paramHelp);
            Assert.AreEqual(parameter.ParameterKey, convertedParameter.paramKey);
            Assert.AreEqual(parameter.CanModLocal, convertedParameter.canModLocal);
            Assert.AreEqual(parameter.CanModRemote, convertedParameter.canModRemote);
        }

        [TestMethod]
        public void WhenCreateG2SOptionConfigComplexParameterExpectSuccess()
        {
            var parameter = new OptionConfigComplexParameter
            {
                ParameterId = ParameterId,
                ParameterName = ParameterName,
                ParameterHelp = ParameterHelp,
                ParameterKey = ParameterKey,
                Items = new List<OptionConfigParameter>
                {
                    new OptionConfigBooleanParameter
                    {
                        ParameterId = ParameterId,
                        ParameterName = ParameterName,
                        ParameterHelp = ParameterHelp,
                        ParameterKey = ParameterKey,
                        AllowedValues = AllowedValues,
                        CanModLocal = CanModLocal,
                        CanModRemote = CanModRemote,
                    }
                }
            };

            var convertedParameter = parameter
                .CreateG2SOptionConfigParameter().Item as complexParameter;

            Assert.AreEqual(parameter.ParameterId, convertedParameter.paramId);
            Assert.AreEqual(parameter.ParameterName, convertedParameter.paramName);
            Assert.AreEqual(parameter.ParameterHelp, convertedParameter.paramHelp);
            Assert.AreEqual(parameter.ParameterKey, convertedParameter.paramKey);

            var subparameter = parameter.Items.First() as OptionConfigBooleanParameter;
            var convertedSubparameter = convertedParameter.Items.First() as booleanParameter;

            Assert.AreEqual(subparameter.ParameterId, convertedSubparameter.paramId);
            Assert.AreEqual(subparameter.ParameterName, convertedSubparameter.paramName);
            Assert.AreEqual(subparameter.ParameterHelp, convertedSubparameter.paramHelp);
            Assert.AreEqual(subparameter.ParameterKey, convertedSubparameter.paramKey);
            Assert.AreEqual(subparameter.CanModLocal, convertedSubparameter.canModLocal);
            Assert.AreEqual(subparameter.CanModRemote, convertedSubparameter.canModRemote);
        }

        [TestMethod]
        public void WhenCreateG2SOptionConfigUnknownParameterExpectNullItem()
        {
            var parameter = new OptionConfigParameter
            {
                ParameterType = (OptionConfigParameterType)1000
            };

            var convertedParameter = parameter.CreateG2SOptionConfigParameter();

            Assert.IsNull(convertedParameter.Item);
        }

        [TestMethod]
        public void WhenAddSubParameterItemsIsNullExpectSuccess()
        {
            var parameter = new OptionConfigComplexParameter();

            var subparameter = new OptionConfigIntegerParameter();

            parameter.AddSubParameter(subparameter);

            Assert.IsNotNull(parameter);

            Assert.AreEqual(1, parameter.Items.Count());
        }
    }
}