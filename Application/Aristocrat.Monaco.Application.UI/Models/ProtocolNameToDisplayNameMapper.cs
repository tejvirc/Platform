namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    public static class ProtocolNameToDisplayNameMapper
    {
        //Key = protocol name, value = display name
        private static Dictionary<string, Func<string>> RemappedDisplayNames { get; } = new Dictionary<string, Func<string>>
        {
            { CommsProtocol.MGAM.ToString(), () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Mgam) },
            { CommsProtocol.None.ToString(), () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None) }
        };

        public static string ToDisplayName(string protocolName)
        {
            return RemappedDisplayNames.ContainsKey(protocolName) ? RemappedDisplayNames[protocolName].Invoke() : protocolName;
        }

        public static string ToProtocolName(string protocolName)
        {
            var match = RemappedDisplayNames.FirstOrDefault(kvp => kvp.Value.Invoke() == protocolName);
            return !match.Equals(default(KeyValuePair<string, Func<string>>)) ? match.Key : protocolName;
        }
    }
}