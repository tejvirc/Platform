namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using log4net;
    using Mono.Addins;
    using Newtonsoft.Json;

    /// <summary>
    ///     An enumeration describing what to do with ordered addins that don't appear in an
    ///     ordered addin configuration file.
    /// </summary>
    public enum DefaultOrderedAddinBehavior
    {
        /// <summary>
        ///     Insert un-named items at beginning of list
        /// </summary>
        InsertAtBeginning,

        /// <summary>
        ///     Insert un-named items at end of list
        /// </summary>
        InsertAtEnd,

        /// <summary>
        ///     Exclude un-named items from list
        /// </summary>
        ExcludeFromList
    }

    /// <summary>
    ///     Wraps commonly used functionality of the Mono.Addins framework.
    /// </summary>
    public static class MonoAddinsHelper
    {
        /// <summary>
        ///     The property manager key used to find the selected addin configuration
        /// </summary>
        internal const string AddinConfigurationsSelectedKey = "Mono.SelectedAddinConfigurationHashCode";

        /// <summary>
        ///     The extension path for finding addin configuration groups
        /// </summary>
        internal const string AddinConfigurationGroupExtensionPoint = "/Kernel/AddinConfigurationGroup";

        /// <summary>
        ///     The base extension path for selectable addin configuration extension points
        /// </summary>
        internal const string SelectableAddinConfigurationExtensionPointBase = "/Kernel/SelectableAddinConfiguration/";

        /// <summary>
        ///     The extension path for finding selectable addin configurations
        /// </summary>
        internal static readonly string SelectableAddinConfigurationExtensionPoint =
            "/Kernel/SelectableAddinConfiguration";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object ExtensionLock = new object();

        /// <summary>
        ///     Gets the selectable addin configuration groups as an ordered list
        /// </summary>
        [CLSCompliant(false)]
        public static IList<string> SelectableConfigurations
        {
            get
            {
                var nodes = AddinManager.GetExtensionNodes<SelectableAddinConfigurationNode>(SelectableAddinConfigurationExtensionPoint);
                var orderedNodes = new List<SelectableAddinConfigurationNode>();
                foreach (SelectableAddinConfigurationNode node in nodes)
                {
                    // Load the addin associated with the node, else the extension point hosted by the addin won't be queryable.
                    AddinManager.LoadAddin(null, node.Addin.Id);

                    // Query the extension path hosted by the selectable configuration addin and only keep this node if addins are found.
                    var path = GetSelectableConfigurationExtensionPath(node.Name);
                    var count = AddinManager.GetExtensionNodes(path).Count;
                    if (count > 0)
                    {
                        orderedNodes.Add(node);
                    }
                    else
                    {
                        Logger.Warn(
                            $"Ignoring selectable configuration \"{node.Name}\" because there are no addins for it.");
                    }
                }

                // Sort the nodes so that the configured order is maintained in the returned list.
                orderedNodes.Sort((x, y) => x.Order.CompareTo(y.Order));

                return orderedNodes.Select(x => x.Name).ToList();
            }
        }

        /// <summary>
        ///     Gets the selected configurations,
        ///     or null if none has been selected or persisted
        /// </summary>
        /// <exception cref="InvalidOperationException">thrown when the selection property maps to multiple selections</exception>
        [CLSCompliant(false)]
        public static ICollection<AddinConfigurationGroupNode> SelectedConfigurations
        {
            get
            {
                var selectedKey =
                    ServiceManager.GetInstance().GetService<IPropertiesManager>().GetProperty(AddinConfigurationsSelectedKey, null);

                if (selectedKey == null)
                {
                    return null;
                }

                if (!(selectedKey is Dictionary<string, string> configurationGroups) || configurationGroups.Count == 0)
                {
                    Logger.Warn("selected configuration property is not a dictionary or is empty");
                    return null;
                }

                var result = new List<AddinConfigurationGroupNode>();

                // Each type of configuration
                foreach (var selectionName in SelectableConfigurations)
                {
                    if (!configurationGroups.TryGetValue(selectionName, out var selectedConfigurationValue))
                    {
                        Logger.Info(
                            $"Selectable configuration \"{selectionName}\" is not in the selected configurations dictionary");
                        continue;
                    }

                    var nodes = AddinManager.GetExtensionNodes(GetSelectableConfigurationExtensionPath(selectionName));

                    if (IsValidJson(selectedConfigurationValue) && ParseAsJson(nodes, selectedConfigurationValue))
                    {
                        continue;
                    }

                    AddinConfigurationGroupNode node = null;
                    foreach (AddinConfigurationGroupNode checkNode in nodes)
                    {
                        if (checkNode.Name != selectedConfigurationValue)
                        {
                            continue;
                        }

                        if (node == null)
                        {
                            node = checkNode;
                            result.Add(node);
                        }
                        else
                        {
                            var message = $"Multiple addin configurations found for {selectionName} ({selectedConfigurationValue}): {node.Name} and {checkNode.Name}";
                            Logger.Error(message);
                            throw new InvalidOperationException(message);
                        }
                    }
                }

                return result;

                bool IsValidJson(string value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return false;
                    }

                    value = value.Trim();

                    return (value.StartsWith("[") && value.EndsWith("]")) || (value.StartsWith("{") && value.EndsWith("}"));
                }

                bool ParseAsJson(IEnumerable nodes, string selectedConfigurationValue)
                {
                    try
                    {
                        var selectedConfigurationValues = JsonConvert.DeserializeObject<List<string>>(selectedConfigurationValue);

                        foreach (AddinConfigurationGroupNode checkNode in nodes)
                        {
                            if (selectedConfigurationValues != null && selectedConfigurationValues.Contains(checkNode.Name))
                            {
                                result.Add(checkNode);
                            }
                        }
                    }
                    catch (JsonReaderException)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        /// <summary>
        ///     Gets the addin choice nodes for a given selectable configuration
        /// </summary>
        /// <param name="selectableConfiguration">The selectable configuration for which to retrieve addins</param>
        /// <returns>the addin choice nodes for a given selectable configuration</returns>
        [CLSCompliant(false)]
        public static ICollection<AddinConfigurationGroupNode> GetSelectableConfigurationAddinNodes(string selectableConfiguration)
        {
            if (!SelectableConfigurations.Contains(selectableConfiguration))
            {
                Logger.Warn($"{selectableConfiguration} is not a selectable configuration");
            }

            var extensionPath = GetSelectableConfigurationExtensionPath(selectableConfiguration);

            return GetSelectedNodes<AddinConfigurationGroupNode>(extensionPath);
        }

        /// <summary>
        ///     Gets the addin choices for a given selectable configuration
        /// </summary>
        /// <param name="selectableConfiguration">The selectable configuration for which to retrieve addins</param>
        /// <returns>the addin choices for a given selectable configuration</returns>
        public static ICollection<string> GetSelectableConfigurationAddins(string selectableConfiguration)
        {
            return GetSelectableConfigurationAddinNodes(selectableConfiguration).Select(x => x.Name).ToList();
        }

        /// <summary>
        ///     Returns all child nodes of a specific type
        /// </summary>
        /// <typeparam name="T">The type of child node to get</typeparam>
        /// <param name="node">The ExtensionNode to retrieve child nodes from</param>
        /// <returns>All child nodes of a specific type</returns>
        [CLSCompliant(false)]
        public static ICollection<T> GetChildNodes<T>(ExtensionNode node)
        {
            ICollection<T> result = new LinkedList<T>();

            try
            {
                if (node.GetChildNodes() != null)
                {
                    foreach (var child in node.GetChildNodes().OfType<T>())
                    {
                        result.Add(child);
                    }
                }
            }
            catch (NullReferenceException)
            {
                // ChildNodes has not been initialized by Mono.Addins
            }

            return result;
        }

        /// <summary>
        ///     Ensures there is only one type extension node at the specified extension path,
        ///     before returning it. The Mono.Addins.AddinManager must be initialized,
        ///     before calling this method.
        /// </summary>
        /// <param name="extensionPath">The extension path to search</param>
        /// <returns>The TypeExtensionNode for the only extension at the given path</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     Throws a ConfigurationErrorsException if the number of found extensions is not 1
        /// </exception>
        [CLSCompliant(false)]
        public static TypeExtensionNode GetSingleTypeExtensionNode(string extensionPath)
        {
            lock(ExtensionLock)
            {
                var typeExtensionNodes = AddinManager.GetExtensionNodes(extensionPath).OfType<TypeExtensionNode>().ToList();

                if (typeExtensionNodes.Count != 1)
                {
                    var error = $"{typeExtensionNodes.Count} selected extensions were found at path \\{extensionPath}\\ - There must be exactly one";

                    Logger.Error(error);
                    throw new ConfigurationErrorsException(error);
                }

                return typeExtensionNodes[0];
            }
        }

        /// <summary>
        ///     Returns the extension nodes configured by an addin configuration group,
        ///     cast to type T, which may be ExtensionNode
        /// </summary>
        /// <typeparam name="T">The type to cast ExtensionNodes to</typeparam>
        /// <param name="groups">the addin configuration groups to recursively search</param>
        /// <param name="extensionPath">the extension path to find extension nodes</param>
        /// <param name="appendExtras">true if addins not specified should be appended to the result</param>
        /// <returns>The extension nodes specified by an addin configuration group, cast to type T</returns>
        [CLSCompliant(false)]
        public static ICollection<T> GetConfiguredExtensionNodes<T>(
            ICollection<AddinConfigurationGroupNode> groups,
            string extensionPath,
            bool appendExtras)
            where T : ExtensionNode
        {
            ICollection<T> result = new LinkedList<T>();

            var filtered = false;
            foreach (var group in groups)
            {
                ICollection<T> nodes = new LinkedList<T>();
                var filteredByThisNode = false;
                CastCollection(
                    nodes,
                    ConfiguredAddinFinder.GetConfiguredAddins(
                        group,
                        extensionPath,
                        appendExtras,
                        ref filteredByThisNode));

                if (filtered)
                {
                    if (filteredByThisNode)
                    {
                        // Add ones identified by this node that are not already in the result set
                        foreach (var node in nodes)
                        {
                            if (!result.Contains(node))
                            {
                                result.Add(node);
                            }
                        }
                    }
                }
                else if (filteredByThisNode || result.Count == 0)
                {
                    result = nodes;
                    filtered = filteredByThisNode;
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns the extension nodes configured by an addin configuration group,
        ///     cast to type T, which may be ExtensionNode
        /// </summary>
        /// <typeparam name="T">The type to cast ExtensionNodes to</typeparam>
        /// <param name="extensionPath">the extension path to find extension nodes</param>
        /// <returns>The extension nodes specified by an addin configuration group, cast to type T</returns>
        [CLSCompliant(false)]
        public static T GetSelectedNode<T>(string extensionPath)
            where T : ExtensionNode
        {
            T result;

            var selectedConfigurations = SelectedConfigurations;
            if (selectedConfigurations == null || selectedConfigurations.Count == 0)
            {
                result = null;
            }
            else
            {
                result = GetConfiguredExtensionNodes<T>(selectedConfigurations, extensionPath, false).Single();
            }

            return result;
        }

        /// <summary>
        ///     Returns the extension nodes configured by an addin configuration group,
        ///     cast to type T, which may be ExtensionNode
        /// </summary>
        /// <typeparam name="T">The type to cast ExtensionNodes to</typeparam>
        /// <param name="extensionPath">the extension path to find extension nodes</param>
        /// <returns>The extension nodes specified by an addin configuration group, cast to type T</returns>
        [CLSCompliant(false)]
        public static ICollection<T> GetSelectedNodes<T>(string extensionPath)
            where T : ExtensionNode
        {
            ICollection<T> result;

            var selectedConfigurations = SelectedConfigurations;
            if (selectedConfigurations == null || selectedConfigurations.Count == 0)
            {
                result = new LinkedList<T>();
                CastCollection(result, AddinManager.GetExtensionNodes(extensionPath));
            }
            else
            {
                result = GetConfiguredExtensionNodes<T>(selectedConfigurations, extensionPath, false);
            }

            return result;
        }

        /// <summary>
        ///     Gets the one and only selected extension node on an extension path
        /// </summary>
        /// <typeparam name="T">the type to cast the result to</typeparam>
        /// <param name="extensionPath"> The extension path of the type extension node. </param>
        /// <returns> The single extension node. </returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     Throws a ConfigurationErrorsException if the number of selected extensions is not 1
        /// </exception>
        [CLSCompliant(false)]
        public static T GetSingleSelectedExtensionNode<T>(string extensionPath)
            where T : ExtensionNode
        {
            lock (ExtensionLock)
            {
                var nodes = GetSelectedNodes<T>(extensionPath);

                if (nodes.Count != 1)
                {
                    var error = $"{nodes.Count} selected extensions were found at path \\{extensionPath}\\ - There must be exactly one";

                    Logger.Error(error);
                    throw new ConfigurationErrorsException(error);
                }

                return nodes.First();
            }
        }

       private static string GetSelectableConfigurationExtensionPath(string selectableConfiguration)
        {
            return SelectableAddinConfigurationExtensionPointBase + selectableConfiguration;
        }

        private static void CastCollection<T>(ICollection<T> result, IEnumerable source)
        {
            foreach (var item in source)
            {
                if (item is T variable)
                {
                    result.Add(variable);
                }
            }
        }
    }
}
