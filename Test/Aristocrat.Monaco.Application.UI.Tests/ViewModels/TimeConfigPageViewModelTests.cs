namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using System;
    using System.Globalization;
    using System.Windows;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.UI.Events;
    using Aristocrat.Monaco.Application.UI.ViewModels;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class TimeConfigPageViewModelTests
    {
        private Mock<ILocalizerFactory> _localizerFactory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private TimeConfigPageViewModel _target;
        private dynamic _accessor;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IConfigWizardNavigator>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Default);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Loose);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                  name =>
                  {
                      var localizer = new Mock<ILocalizer>();
                      localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("es-US"));
                      localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns(" ");
                      return localizer.Object;
                  });

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
            _target = new TimeConfigPageViewModel(false);
            _accessor = new DynamicPrivateObject(_target);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            if (AddinManager.IsInitialized)
            {
                AddinManager.Shutdown();
            }
        }

        [TestMethod]
        public void VerifyEmptyOrderNumberErrorMessage()
        {
            _target.OrderNumber = string.Empty;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsNotNull(_accessor.GetErrors(nameof(_target.OrderNumber)));
        }

        [TestMethod]
        public void VerifyOrderNumberErrorMessage()
        {
            _target.OrderNumber = "Anything";
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsNull(_accessor.GetErrors(nameof(_target.OrderNumber)));
        }

        [TestMethod]
        public void VerifyEmptyInspctorInitialsErrorMessage()
        {
            _target.InspectorInitials = string.Empty;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsNotNull(_accessor.GetErrors(nameof(_target.InspectorInitials)));
        }

        [TestMethod]
        public void VerifyInspctorInitialsErrorMessage()
        {
            _target.InspectorInitials = "Anything";
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsNull(_accessor.GetErrors(nameof(_target.InspectorInitials)));
        }
        [TestMethod]
        public void VerifyHoursSetterUpdatesItemPickedFlag()
        {
            _accessor._ItemsPicked = 0;
            _target.Hour = 0;
            Assert.AreEqual(4, Convert.ToInt32(_accessor._ItemsPicked));
        }

        [TestMethod]
        public void VerifyMinutesSetterUpdatesItemPickedFlag()
        {
            _accessor._ItemsPicked = 0;
            _target.Minute = 0;
            Assert.AreEqual(2, Convert.ToInt32(_accessor._ItemsPicked));
        }

        [TestMethod]
        public void VerifySecondsSetterUpdatesItemPickedFlag()
        {
            _accessor._ItemsPicked = 0;
            _target.Second = 0;
            Assert.AreEqual(1, Convert.ToInt32(_accessor._ItemsPicked));
        }
    }
}