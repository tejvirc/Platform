namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Bootstrap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;

    /// <summary>
    ///     This is a test class for CommandLineHelpTest and is intended
    ///     to contain all CommandLineHelpTest Unit Tests
    /// </summary>
    [TestClass]
    [CLSCompliant(false)]
    public class CommandLineHelpTest
    {
        private const int ChunkSize = 80;
        private const int MaxNameLength = 20;
        private const int MaxArgumentLength = 20;

        private string _argumentFormat;
        private string _descriptionFormat;
        private string _headerFormat;
        private CommandLineHelp _target;
        private PrivateObject _targetPrivateObject;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            // use reflection to access private constructor to create the object
            Type[] paramTypes = { };
            object[] paramValues = { };
            var ci = typeof(CommandLineHelp).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                paramTypes,
                null);
            _target = (CommandLineHelp)ci.Invoke(paramValues);
            _targetPrivateObject = new PrivateObject(_target);

            // default these to some reasonable values
            _headerFormat = string.Format(
                CultureInfo.InvariantCulture,
                (string)GetConstTargetField("HeaderFormatString"),
                MaxNameLength,
                MaxArgumentLength);

            _argumentFormat = string.Format(
                CultureInfo.InvariantCulture,
                (string)GetConstTargetField("ArgumentFormatString"),
                MaxNameLength,
                MaxArgumentLength);

            _descriptionFormat = string.Format(
                CultureInfo.InvariantCulture,
                (string)GetConstTargetField("DescriptionOnlyFormatString"),
                MaxNameLength + MaxArgumentLength);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (AddinManager.IsInitialized)
            {
                try
                {
                    AddinManager.Shutdown();
                }
                catch (InvalidOperationException)
                {
                    // temporarily swallow exception
                }
            }
        }

        [TestMethod]
        public void DiscoverAndDisplayCommandLineArgumentsTest()
        {
            CommandLineHelp.DiscoverAndDisplayCommandLineArguments();
        }

        [TestMethod]
        public void GetDescriptionPartsWithEmptyString()
        {
            var descriptionParts =
                (List<string>)InvokeTargetMethod("GetDescriptionParts", string.Empty, ChunkSize);

            Assert.AreEqual(0, descriptionParts.Count);
        }

        [TestMethod]
        public void GetDescriptionPartsWithSingleLine()
        {
            var description = "This should all be on one line.";

            var descriptionParts =
                (List<string>)InvokeTargetMethod("GetDescriptionParts", description, ChunkSize);

            Assert.AreEqual(1, descriptionParts.Count);
            Assert.AreEqual(descriptionParts[0], description);
        }

        [TestMethod]
        public void GetDescriptionPartsWithMultipleLines()
        {
            var descriptionPart1 = "This is a string that will need to be split. Part of it should go on the other";
            var descriptionPart2 = "line.";

            var description = descriptionPart1 + " " + descriptionPart2;

            var descriptionParts =
                (List<string>)InvokeTargetMethod("GetDescriptionParts", description, ChunkSize);

            Assert.AreEqual(2, descriptionParts.Count);
            Assert.AreEqual(descriptionParts[0], descriptionPart1);
            Assert.AreEqual(descriptionParts[1], descriptionPart2);
        }

        [TestMethod]
        public void DisplayArgumentsWithEmptyCollection()
        {
            var list = new List<ArgumentValueNode>();
            InvokeTargetMethod(
                "DisplayArguments",
                list,
                ChunkSize,
                _argumentFormat,
                _descriptionFormat,
                _headerFormat,
                null);
        }

        [TestMethod]
        public void CommandLineHelpConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void CommandLineArgumentExtensionNodeTests()
        {
            var target = new CommandLineArgumentExtensionNode();
            ArgumentValueNode[] nodes =
                { new ArgumentValueNode { ValidValue = "Test Value", Description = "Test Description" } };
            target.ValidValues = nodes;
            var result = target.ValidValues;
            Assert.AreEqual(nodes[0].ValidValue, result[0].ValidValue);
            Assert.AreEqual(nodes[0].Description, result[0].Description);
        }

        private object InvokeTargetMethod(string methodName, params object[] parameters)
        {
            return _targetPrivateObject.Invoke(methodName, BindingFlags.NonPublic | BindingFlags.Static, parameters);
        }

        private object GetConstTargetField(string fieldName)
        {
            return _targetPrivateObject.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}
