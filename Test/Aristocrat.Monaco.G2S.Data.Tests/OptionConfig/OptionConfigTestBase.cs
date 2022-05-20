namespace Aristocrat.Monaco.G2S.Data.Tests.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using Data.Model;
    using Data.OptionConfig;
    using Data.OptionConfig.ChangeOptionConfig;

    public class OptionConfigTestBase
    {
        protected ChangeOptionConfigRequest CreateChangeOptionConfigRequest()
        {
            return new ChangeOptionConfigRequest
            {
                ApplyCondition = "G2S_disable",
                ConfigurationId = 1,
                DisableCondition = "G2S_immediate",
                EndDateTime = DateTime.MaxValue,
                StartDateTime = DateTime.MaxValue,
                RestartAfter = true,
                Options = new List<Option> { CreateOption() }
            };
        }

        protected Option CreateOption()
        {
            return new Option
            {
                DeviceClass = "G2S_OptionConfig",
                DeviceId = 1,
                OptionGroupId = "1",
                OptionId = "2",
                OptionValues = new[] { CreateOptionCurrentValue() }
            };
        }

        protected OptionCurrentValue CreateOptionCurrentValue()
        {
            return new OptionCurrentValue
            {
                ParameterType = OptionConfigParameterType.Complex,
                Value = string.Empty,
                ChildValues = new List<OptionCurrentValue>
                {
                    new OptionCurrentValue
                    {
                        ParameterType = OptionConfigParameterType.Integer,
                        ParamId = "1",
                        Value = "2"
                    }
                }
            };
        }

        protected OptionParameterDescriptor CreateOptionParameterDescriptor()
        {
            return new OptionParameterDescriptor
            {
                ParameterId = "1",
                ParameterType = OptionConfigParameterType.Complex,
                ChildItems = new List<OptionParameterDescriptor>
                {
                    new OptionParameterDescriptor
                    {
                        ParameterId = "2",
                        ChildItems = new List<OptionParameterDescriptor>(),
                        ParameterType = OptionConfigParameterType.Integer
                    }
                }
            };
        }
    }
}