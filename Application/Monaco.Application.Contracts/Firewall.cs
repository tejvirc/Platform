namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Reflection;
    using log4net;
    using NetFwTypeLib;

    /// <summary>
    ///     Provides helper methods to allow interacting with the Windows Firewall
    /// </summary>
    [CLSCompliant(false)]
    public static class Firewall
    {
        /// <summary>
        ///     Firewall direction
        /// </summary>
        public enum Direction
        {
            /// <summary>
            ///     Incoming firewall rule
            /// </summary>
            In,

            /// <summary>
            ///     Outgoing firewall rule
            /// </summary>
            Out
        }

        private const string Group = "Aristocrat Monaco Firewall Rules";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Enables the specified port by adding a new rule to the Windows firewall
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="port">The port to be enabled</param>
        /// <param name="direction">The firewall direction</param>
        /// <returns>true if successful, otherwise false</returns>
        public static bool AddRule(string ruleName, ushort port, Direction direction = Direction.In)
        {
            return AddRule(ruleName, port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP, direction);
        }

        /// <summary>
        ///     Enables the specified port by adding a new rule to the Windows firewall
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="port">The port to be enabled</param>
        /// <param name="direction">The firewall direction</param>
        /// <returns>true if successful, otherwise false</returns>
        public static bool AddUdpRule(string ruleName, ushort port, Direction direction = Direction.In)
        {
            return AddRule(ruleName, port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP, direction);
        }

        /// <summary>
        ///     Enables the specified protocol by adding a new rule to the Windows firewall
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="protocol">The IP protocol <see href="https://www.iana.org/assignments/protocol-numbers/protocol-numbers.xhtml"/>.</param>
        /// <param name="direction">The firewall direction</param>
        /// <returns>true if successful, otherwise false</returns>
        public static bool AddProtocolRule(string ruleName, int protocol, Direction direction = Direction.In)
        {
            return AddRule(ruleName, null, protocol, direction, "System");
        }

        /// <summary>
        ///     Disables the specified port
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <returns>true if successful, otherwise false</returns>
        public static bool RemoveRule(string ruleName)
        {
            try
            {
                var firewallPolicy = GetFirewallPolicy();
                if (firewallPolicy == null)
                {
                    return false;
                }

                return RemoveRule(firewallPolicy, ruleName);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to remove the firewall rule - {ruleName}", e);

                return false;
            }
        }

        private static bool AddRule(string ruleName, ushort? port, NET_FW_IP_PROTOCOL_ protocol, Direction direction) =>
            AddRule(ruleName, port, (int)protocol, direction);

        private static bool AddRule(
            string ruleName,
            ushort? port,
            int protocol,
            Direction direction,
            string appName = null)
        {
            try
            {
                var firewallPolicy = GetFirewallPolicy();
                if (firewallPolicy == null)
                {
                    Logger.Error("Failed to load the firewall policy");

                    return false;
                }

                RemoveRule(firewallPolicy, ruleName);

                var firewallRule = GetFirewallRule();
                if (firewallRule == null)
                {
                    Logger.Error("Failed to load the firewall rule");

                    return false;
                }

                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Direction = direction == Direction.In
                    ? NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN
                    : NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Name = ruleName;
                firewallRule.Grouping = Group;
                firewallRule.Protocol = protocol;

                // For protocol rules
                if (appName != null)
                {
                    firewallRule.ApplicationName = appName;
                }

                // For all other rules
                if (port != null)
                {
                    if (direction == Direction.In)
                    {
                        firewallRule.LocalPorts = $"{port}";
                    }
                    else
                    {
                        firewallRule.RemotePorts = $"{port}";
                    }
                }

                firewallPolicy.Rules.Add(firewallRule);

                Logger.Info($"Applied the firewall rule: {ruleName} for port {port}");
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to apply the firewall rule: {ruleName} for port {port}", e);

                return false;
            }

            return true;
        }

        private static bool RemoveRule(INetFwPolicy2 firewallPolicy, string ruleName)
        {
            try
            {
                var enumerator = firewallPolicy.Rules.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is INetFwRule rule)
                    {
                        if (ruleName.Equals(rule.Name, StringComparison.InvariantCulture))
                        {
                            firewallPolicy.Rules.Remove(ruleName);

                            Logger.Info($"Removed the firewall rule: {ruleName}");

                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to remove the firewall rule - {ruleName}", e);
            }

            return false;
        }

        private static INetFwPolicy2 GetFirewallPolicy()
        {
            var policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            if (policyType == null)
            {
                return null;
            }

            return Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2")) as INetFwPolicy2;
        }

        private static INetFwRule GetFirewallRule()
        {
            var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
            if (ruleType == null)
            {
                return null;
            }

            return Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")) as INetFwRule;
        }
    }
}