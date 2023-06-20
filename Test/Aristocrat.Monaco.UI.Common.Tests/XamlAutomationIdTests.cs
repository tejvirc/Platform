namespace Aristocrat.Monaco.UI.Common.Tests
{
    #region Using
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Animation;
    using System.Xml.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    [TestClass]
    public class XamlAutomationIdTests
    {
        private static readonly string _automationIdAttributeName = "AutomationProperties.AutomationId";

        private IEnumerable<string> _eligibleAutomationIdXamlElements;

        private IEnumerable<string> _allXamlFiles;

        [TestInitialize]
        public void Initialize()
        {
            // Collect all xaml element types eligible for AutomationId assignment
            var _frameworkElements = Assembly.GetAssembly(typeof(FrameworkElement)).GetTypes()
               .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(DependencyObject)))
               .Select(myType => myType.Name);

            var _animateableElements = Assembly.GetAssembly(typeof(Animatable)).GetTypes()
                   .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(DependencyObject)))
                   .Select(myType => myType.Name);

            _eligibleAutomationIdXamlElements = _frameworkElements.Concat(_animateableElements);


            var genericSolutionFolderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));
            _allXamlFiles = Directory.GetFiles(genericSolutionFolderPath, "*.xaml", SearchOption.AllDirectories).Where(x => !x.StartsWith(genericSolutionFolderPath + "bin"));
        }

        /// <summary>
        /// Searches the solution level down for all *.xaml files to enforce the
        /// uniqueness of an AutomationID property on eligible elements.
        /// </summary>
        [TestMethod]
        public void CheckForDuplicateXamlAutomationIds()
        {
            var automationTracker = new AutomationIdTracker();

            Parallel.ForEach(_allXamlFiles, xamlFile =>
            {
                CollectDuplicateAutomationIdsFromXaml(automationTracker, xamlFile);
            });

            string failMessage = $"The following {_automationIdAttributeName} value(s) already exist:{Environment.NewLine}{string.Join(Environment.NewLine, automationTracker.CollisionPairs.Select(x => $"filename: {x.Value}, value: {x.Key}"))}";

            Assert.AreEqual(0, automationTracker.CollisionPairs.Count(), failMessage);
        }

        /// <summary>
        /// Searches the solution level down for all *.xaml files to enforce the
        /// existence of an AutomationID property on eligible elements.
        /// </summary>
        [TestMethod]
        public void CheckForMissingXamlAutomationIds()
        {
            var missingAutomationIds = new ConcurrentDictionary<string, List<string>>();

            Parallel.ForEach(_allXamlFiles, xamlFile =>
            {
                CollectMissingAutomationIdsFromXaml(missingAutomationIds, xamlFile);
            });

            var failMessage = new StringBuilder();
            failMessage.AppendLine($"The following xaml element(s) are missing the {_automationIdAttributeName} attribute:");
            foreach (var key in missingAutomationIds.Keys.OrderBy(x => x))
            {
                foreach (var value in missingAutomationIds[key].OrderBy(x => x))
                {
                    failMessage.AppendLine($"filename: {key}, value: {value}");
                }
            }

            Assert.AreEqual(0, missingAutomationIds.Count(), failMessage.ToString());
        }

        private void CollectDuplicateAutomationIdsFromXaml(AutomationIdTracker automationTracker, string xamlFilePath)
        {
            foreach (var element in XDocument.Load(xamlFilePath).Descendants())
            {
                if (_eligibleAutomationIdXamlElements.Contains(element.Name.LocalName) &&
                element.Attribute(_automationIdAttributeName) != null)
                {
                    string automationId = element.Attribute(_automationIdAttributeName).Value;

                    lock (automationTracker.AutomationIds)
                    {
                        if (!automationTracker.AutomationIds.Add(automationId))
                        {
                            automationTracker.CollisionPairs.TryAdd(automationId, xamlFilePath);
                        }
                    }
                }
            }
        }

        private void CollectMissingAutomationIdsFromXaml(ConcurrentDictionary<string, List<string>> missingAutomationIdPairs, string xamlFilePath)
        {
            foreach (var element in XDocument.Load(xamlFilePath).Descendants())
            {
                if (_eligibleAutomationIdXamlElements.Contains(element.Name.LocalName) &&
                    !element.Name.Namespace.NamespaceName.Contains("System.Windows.Forms") &&
                    (element.Attribute(_automationIdAttributeName) == null ||
                    element.Attribute(_automationIdAttributeName).Value == string.Empty))
                {
                    var key = $"{xamlFilePath} {element.Name.LocalName}";
                    if (!missingAutomationIdPairs.ContainsKey(key))
                    {
                        missingAutomationIdPairs[key] = new List<string>();
                    }
                    missingAutomationIdPairs[key].Add(GetAbsoluteXPath(element));
                }
            }
        }

        private string GetAbsoluteXPath(XElement element)
        {
            Func<XElement, string> relativeXPath = e =>
                {
                    int index = IndexPosition(e);
                    string name = e.Name.LocalName;

                    // If the element is the root or has no sibling elements, no index is required
                    return (index is -1 or -2) ? "/" + name : $"/{name}[{index}]";
                };

            var ancestors = from e in element.Ancestors()
                            select relativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) +
                   relativeXPath(element);
                }

        private int IndexPosition(XElement element)
        {
            // Element is root
            if (element.Parent == null)
                return -1;

            // Element has no sibling elements
            if (element.Parent.Elements(element.Name).Count() == 1)
                return -2;

            int i = 1; // Indexes for nodes start at 1, not 0

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                    return i;

                i++;
            }

            throw new InvalidOperationException
                ("element has been removed from its parent.");
        }
    }

    public class AutomationIdTracker
    {
        public HashSet<string> AutomationIds { get; } = new HashSet<string>();

        // Store the file and element information if the AutomationId is non-unique in the codebase
        public ConcurrentDictionary<string, string> CollisionPairs { get; } = new ConcurrentDictionary<string, string>();
    }
}