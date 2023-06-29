namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    public class LogicalStripInformation : ILogicalStripInformation
    {
        public string LogicalStripCreationRuleXmlPath { get; set; } = @".\EdgeLightStripsCreationRule.xml";
    }
}