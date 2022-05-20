namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using G2S.Handlers.OptionConfig.Builders;

    public class BaseOptionsTests
    {
        protected Option CreateOptionWithParameters(
            string optionId,
            bool isValid,
            Action<List<OptionCurrentValue>> createParameters)
        {
            var result = new Option();
            result.OptionId = optionId;

            var childs = new List<OptionCurrentValue>();

            createParameters(childs);

            result.OptionValues = childs.ToArray();

            return result;
        }

        protected Option CreateG2SProtocolParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.ProtocolOptionsId,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        G2SParametersNames.ConfigurationIdParameterName,
                        OptionConfigParameterType.Integer,
                        isValid);
                    AddAdditionalG2SProtocolParameters(list, isValid);
                });
        }

        protected virtual void AddAdditionalG2SProtocolParameters(
            List<OptionCurrentValue> parent,
            bool isValid)
        {
        }

        protected Option CreateG2Sv3ProtocolParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.ProtocolAdditionalOptionsId,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        G2SParametersNames.ConfigDateTimeParameterName,
                        OptionConfigParameterType.String,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.ConfigCompleteParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                });
        }

        protected void CreateParameter(
            List<OptionCurrentValue> parent,
            string name,
            OptionConfigParameterType type,
            bool isValid)
        {
            var result = new OptionCurrentValue();
            result.ParameterType = type;
            result.ParamId = name;

            result.Value = CreateParameterValue(type, isValid);

            parent.Add(result);
        }

        protected string CreateParameterValue(OptionConfigParameterType type, bool isValid)
        {
            var result = string.Empty;

            switch (type)
            {
                case OptionConfigParameterType.Boolean:
                    result = isValid ? Boolean.FalseString : GetRandomString();
                    break;
                case OptionConfigParameterType.Decimal:
                    result = isValid ? "12.01" : GetRandomString();
                    break;
                case OptionConfigParameterType.Integer:
                    result = isValid ? "12" : "12.01";
                    break;
                case OptionConfigParameterType.String:
                    result = GetRandomString();
                    break;
            }

            return result;
        }

        private string GetRandomString()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}