namespace Aristocrat.Monaco.Hardware.Tests.ContractsTests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Contracts.Printer;
    using Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This class tests the classes in the Hardware.Contracts.Printer namespace
    /// </summary>
    [TestClass]
    public class PrinterContractsTests
    {
        private const int PrinterId = 1;

        [TestMethod]
        public void DisabledEventTest()
        {
            // constructor with DisabledReasons test
            var disabledEvent = new DisabledEvent(DisabledReasons.Backend);
            Assert.IsNotNull(disabledEvent);
            Assert.AreEqual(DisabledReasons.Backend, disabledEvent.Reasons);

            // constructor with printerId and DisabledReasons test
            disabledEvent = new DisabledEvent(PrinterId, DisabledReasons.Configuration);
            Assert.IsNotNull(disabledEvent);
            Assert.AreEqual(DisabledReasons.Configuration, disabledEvent.Reasons);
            Assert.AreEqual(PrinterId, disabledEvent.PrinterId);
        }

        [TestMethod]
        public void EnabledEventTest()
        {
            // constructor with EnabledReasons test
            var enabledEvent = new EnabledEvent(EnabledReasons.Backend);
            Assert.IsNotNull(enabledEvent);
            Assert.AreEqual(EnabledReasons.Backend, enabledEvent.Reasons);

            // constructor with printerId and EnabledReasons test
            enabledEvent = new EnabledEvent(PrinterId, EnabledReasons.Configuration);
            Assert.IsNotNull(enabledEvent);
            Assert.AreEqual(EnabledReasons.Configuration, enabledEvent.Reasons);
            Assert.AreEqual(PrinterId, enabledEvent.PrinterId);
        }

        [TestMethod]
        public void InspectedEventTest()
        {
            // plain constructor test
            Assert.IsNotNull(new InspectedEvent());

            // constructor with one parameter test
            var target = new InspectedEvent(PrinterId);
            Assert.IsNotNull(target);
            Assert.AreEqual(PrinterId, target.PrinterId);
        }

        [TestMethod]
        public void InspectionFailedEventTest()
        {
            // plain constructor test
            Assert.IsNotNull(new InspectionFailedEvent());

            // constructor with one parameter test
            var target = new InspectionFailedEvent(PrinterId);
            Assert.IsNotNull(target);
            Assert.AreEqual(PrinterId, target.PrinterId);
        }

        [TestMethod]
        public void PrintCompletedEventTest()
        {
            // plain constructor test
            Assert.IsNotNull(new PrintCompletedEvent());

            // constructor with one parameter test
            var target = new PrintCompletedEvent(PrinterId);
            Assert.IsNotNull(target);
            Assert.AreEqual(PrinterId, target.PrinterId);
        }

        [TestMethod]
        public void PrintStartedEventTest()
        {
            // plain constructor test
            Assert.IsNotNull(new PrintStartedEvent());

            // constructor with one parameter test
            var target = new PrintStartedEvent(PrinterId);
            Assert.IsNotNull(target);
            Assert.AreEqual(PrinterId, target.PrinterId);
        }

        [TestMethod]
        public void ResolverErrorEventTest()
        {
            // plain constructor test
            Assert.IsNotNull(new ResolverErrorEvent());

            // constructor with one parameter test
            var target = new ResolverErrorEvent(PrinterId);
            Assert.IsNotNull(target);
            Assert.AreEqual(PrinterId, target.PrinterId);
        }

        [TestMethod]
        public void LoadingRegionsAndTemplatesEventTest()
        {
            // plain constructor test
            Assert.IsNotNull(new LoadingRegionsAndTemplatesEvent());

            // constructor with one parameter test
            var target = new LoadingRegionsAndTemplatesEvent(PrinterId);
            Assert.IsNotNull(target);
            Assert.AreEqual(PrinterId, target.PrinterId);
        }

        [TestMethod]
        public void ResetEventTest()
        {
            // plain constructor test
            Assert.IsNotNull(new ResetEvent());

            // constructor with one parameter test
            var target = new ResetEvent(PrinterId);
            Assert.IsNotNull(target);
            Assert.AreEqual(PrinterId, target.PrinterId);
        }

        [TestMethod]
        public void PrintRequestedEventTest()
        {
            // constructor with parameters test
            int templateId = 123;
            var target = new PrintRequestedEvent(PrinterId, templateId);
            Assert.IsNotNull(target);
            Assert.AreEqual(PrinterId, target.PrinterId);
            Assert.AreEqual(templateId, target.TemplateId);
        }
    }
}
