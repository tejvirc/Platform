namespace Aristocrat.Monaco.Application.UI.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    public static class ProtocolNameToDisplayNameMapper
    {
        //Key = protocol name, value = display name
        private static Dictionary<string, string> RemappedDisplayNames { get; } = new Dictionary<string, string>
        {
            { CommsProtocol.MGAM.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Mgam) }
        };

        public static string ToDisplayName(string protocolName)
        {
            return RemappedDisplayNames.ContainsKey(protocolName) ? RemappedDisplayNames[protocolName] : protocolName;
        }

        public static string ToProtocolName(string protocolName)
        {
            return RemappedDisplayNames.ContainsValue(protocolName) ? RemappedDisplayNames.Single(s => s.Value == protocolName).Key : protocolName;
        }
    }
}