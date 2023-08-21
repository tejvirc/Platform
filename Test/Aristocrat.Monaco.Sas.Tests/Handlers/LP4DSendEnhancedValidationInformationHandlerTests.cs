namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Aristocrat.Monaco.Sas.VoucherValidation;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP4DSendEnhancedValidationInformationHandlerTests
    {
        private LP4DSendEnhancedValidationInformationHandler _target;
        private Mock<IEnhancedValidationProvider> _enhancedValidationProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _enhancedValidationProvider = new Mock<IEnhancedValidationProvider>(MockBehavior.Default);
            _target = new LP4DSendEnhancedValidationInformationHandler(_enhancedValidationProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendEnhancedValidationInformation));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullSecureEnhancedValidationProviderTest()
        {
            _target = new LP4DSendEnhancedValidationInformationHandler(null);
        }
    }
}