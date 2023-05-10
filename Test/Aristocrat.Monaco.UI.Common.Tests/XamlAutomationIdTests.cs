namespace Aristocrat.Monaco.UI.Common.Tests
{
    #region Using
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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

            string failMessage = $"The following AutomationProperties.AutomationId value(s) already exist:{Environment.NewLine}{string.Join(Environment.NewLine, automationTracker.CollisionPairs.Select(x => $"filename: {x.Value}, value: {x.Key}"))}";

            Assert.AreEqual(0, automationTracker.CollisionPairs.Count(), failMessage);
        }

        /// <summary>
        /// Searches the solution level down for all *.xaml files to enforce the
        /// existence of an AutomationID property on eligible elements.
        /// </summary>
        [TestMethod]
        public void CheckForMissingXamlAutomationIds()
        {
            var missingAutomationIds = new ConcurrentDictionary<string, string>();

            Parallel.ForEach(_allXamlFiles, xamlFile =>
            {
                CollectMissingAutomationIdsFromXaml(missingAutomationIds, xamlFile);
            });

            string failMessage = $"The following xaml element(s) are missing the AutomationProperties.AutomationId attribute:{Environment.NewLine}{string.Join(Environment.NewLine, missingAutomationIds.Select(x => $"filename: {x.Key}, value: {x.Value}"))}";

            Assert.AreEqual(0, missingAutomationIds.Count(), failMessage);
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

        private void CollectMissingAutomationIdsFromXaml(ConcurrentDictionary<string, string> missingAutomationIdPairs, string xamlFilePath)
        {
            foreach (var element in XDocument.Load(xamlFilePath).Descendants())
            {
                if (_eligibleAutomationIdXamlElements.Contains(element.Name.LocalName) &&
                    !element.Name.Namespace.NamespaceName.Contains("System.Windows.Forms") &&
                    element.Attribute(_automationIdAttributeName) == null)
                {
                    missingAutomationIdPairs[$"{xamlFilePath} {element.Name.LocalName}"] = element.Name.LocalName;
                }
            }
        }
    }

    public class AutomationIdTracker
    {
        public HashSet<string> AutomationIds { get; } = new HashSet<string>();

        // Store the file and element information if the AutomationId is non-unique in the codebase
        public ConcurrentDictionary<string, string> CollisionPairs { get; } = new ConcurrentDictionary<string, string>();
    }
}