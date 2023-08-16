namespace Aristocrat.Monaco.Kernel
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     A helper class that finds addins within the selected configuration
    /// </summary>
    internal static class ConfiguredAddinFinder
    {
        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Returns the extension nodes specified by an addin configuration group,
        /// </summary>
        /// <param name="group">the addin configuration group to recursively search</param>
        /// <param name="extensionPath">The extension path to find addins</param>
        /// <param name="appendExtras">true if unspecified nodes should be appended to the result</param>
        /// <param name="configurationFound">true if the provided group configures the extension point</param>
        /// <returns>The extension nodes specified by an addin configuration group</returns>
        public static ICollection<ExtensionNode> GetConfiguredAddins(
            AddinConfigurationGroupNode group,
            string extensionPath,
            bool appendExtras,
            ref bool configurationFound)
        {
            IDictionary<string, AddinConfigurationGroupNode> groups =
                new Dictionary<string, AddinConfigurationGroupNode>();
            ICollection<ExtensionNode> nodes = new LinkedList<ExtensionNode>();

            foreach (ExtensionNode node in AddinManager.GetExtensionNodes<ExtensionNode>(extensionPath))
            {
                nodes.Add(node);
            }

            foreach (AddinConfigurationGroupNode configurationGroup in AddinManager.GetExtensionNodes<AddinConfigurationGroupNode>(
                MonoAddinsHelper.AddinConfigurationGroupExtensionPoint))
            {
                groups.Add(configurationGroup.Name, configurationGroup);
            }

            var nodeOrderings = new List<KeyValuePair<int, ExtensionNode>>();
            ICollection<ExtensionNode> result;
            configurationFound = false;

            FindNodes(group, extensionPath, nodes, groups, nodeOrderings, ref configurationFound);

            if (configurationFound)
            {
                nodeOrderings.Sort(CompareOrderedExtensionNodes);
                result = new LinkedList<ExtensionNode>();

                foreach (var orderedNode in nodeOrderings)
                {
                    result.Add(orderedNode.Value);
                }

                if (appendExtras)
                {
                    foreach (var node in nodes)
                    {
                        if (!result.Contains(node))
                        {
                            result.Add(node);
                        }
                    }
                }
            }
            else
            {
                result = nodes;
            }

            return result;
        }

        /// <summary>
        ///     Recursively finds the extension nodes specified by an addin configuration group,
        /// </summary>
        /// <param name="group">The current group being searched for selected nodes</param>
        /// <param name="extensionPath">The extension path to find nodes on</param>
        /// <param name="nodes">All available extension nodes on the extension path</param>
        /// <param name="groups">Maps names to corresponding AddinConfigurationGroupNodes, for all groups</param>
        /// <param name="result">The resulting collection of ExtensionNode orderings</param>
        /// <param name="configurationFound">true if an ExtensionPointConfiguration has been found for extensionPath</param>
        private static void FindNodes(
            AddinConfigurationGroupNode group,
            string extensionPath,
            ICollection<ExtensionNode> nodes,
            IDictionary<string, AddinConfigurationGroupNode> groups,
            ICollection<KeyValuePair<int, ExtensionNode>> result,
            ref bool configurationFound)
        {
            var configuration = group.GetExtensionPointConfiguration(extensionPath);

            if (configuration != null)
            {
                configurationFound = true;

                foreach (var specification in configuration.ExtensionNodeSpecifications)
                {
                    foreach (var node in nodes)
                    {
                        if (NodeMatchesSpecification(node, specification))
                        {
                            result.Add(new KeyValuePair<int, ExtensionNode>(specification.Order, node));
                        }
                    }
                }
            }

            foreach (var node in group.GroupReferences)
            {
                if (groups.ContainsKey(node.Name))
                {
                    FindNodes(groups[node.Name], extensionPath, nodes, groups, result, ref configurationFound);
                }
                else
                {
                    if (!node.Optional)
                    {
                        Logger.FatalFormat(
                            CultureInfo.InvariantCulture,
                            "Addin Configuration Group not found: {0}, referenced by: {1}",
                            node.Name,
                            group.Name);
                        throw new ConfigurationErrorsException(
                            "Addin Configuration Group not found: " + node.Name + ", referenced by: " + group.Name);
                    }
                }
            }
        }

        /// <summary>
        ///     Compare the order of two extension nodes
        /// </summary>
        /// <param name="node">the extension node to compare to the specification</param>
        /// <param name="specification">the specification to use for the comparison</param>
        /// <returns>True if the node matches the specification, false otherwise.</returns>
        private static bool NodeMatchesSpecification(ExtensionNode node, NodeSpecificationNode specification)
        {
            // If the addin doesn't match, then look no further
            if (node.Addin.Id != specification.AddinId)
            {
                return false;
            }

            if (specification.TypeName != null)
            {
                var typeExtensionNode = node as TypeExtensionNode;
                if (specification.TypeName == typeExtensionNode?.Type.FullName)
                {
                    return true;
                }
            }
            else if (specification.FilterId != null)
            {
                var filterableExtensionNode = node as FilterableExtensionNode;
                if (specification.FilterId == filterableExtensionNode?.FilterId)
                {
                    return true;
                }
            }
            else
            {
                // If the specification does not provide a TypeName or FilterId, the user must want to match all extension in that addin.
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Compare the order of two extension nodes
        /// </summary>
        /// <param name="orderedNode1">an extension node with an order</param>
        /// <param name="orderedNode2">another extension node with an order</param>
        /// <returns>an integer indicating the order, as defined by Comparison(T)"/></returns>
        private static int CompareOrderedExtensionNodes(
            KeyValuePair<int, ExtensionNode> orderedNode1,
            KeyValuePair<int, ExtensionNode> orderedNode2)
        {
            return orderedNode1.Key - orderedNode2.Key;
        }
    }
}
