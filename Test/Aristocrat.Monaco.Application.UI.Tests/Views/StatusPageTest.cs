namespace Aristocrat.Monaco.Application.UI.Tests.Views
{
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.Views;

    [TestClass]
    public class StatusPageTest
    {
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private StatusPage _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _disableManager.Setup(m => m.IsDisabled).Returns(false);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            //_propertiesManager
            //    .Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuStatusPageOperatorDisableWithCredits, true))
            //    .Returns(true);

            _target = new StatusPage();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [RequireSTA]
        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }
    }
}