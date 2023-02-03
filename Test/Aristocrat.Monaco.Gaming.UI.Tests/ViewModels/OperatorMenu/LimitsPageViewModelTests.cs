namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels.OperatorMenu
{
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class LimitsPageViewModelTests
    {
        private Mock<ILocalizerFactory> _localizerFactory;
        private Mock<IPropertiesManager> _propertiesManager;
        private LimitsPageViewModel _target;
        private dynamic _accessor;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IConfigWizardNavigator>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);

            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Loose);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                  name =>
                  {
                      var localizer = new Mock<ILocalizer>();
                      localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("es-US"));
                      localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns(" ");
                      return localizer.Object;
                  });
            _target = new LimitsPageViewModel();
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
        public void ConstructorTest()
        {
            var target = new LimitsPageViewModel();
            Assert.IsNotNull(target);
        }


        [TestMethod]
        public void VerifyInvalidHandpayLimitIsNotSet()
        {
            _target.HandpayLimitIsChecked = true;
            _target.HandpayLimit = -1;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.HandpayLimit)));
        }

        [TestMethod]
        public void VerifyValidHandpayLimitIsSet()
        {
            _target.HandpayLimitIsChecked = true;
            _target.HandpayLimit = AccountingConstants.DefaultHandpayLimit.MillicentsToDollars();
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.HandpayLimit)));
        }

        [TestMethod]
        public void VerifyLargeWinLimitDoesNotExceedDefaultLargeWinAllowed()
        {
            _target = new LimitsPageViewModel();
            _accessor = new DynamicPrivateObject(_target);

            _target.LargeWinLimitIsChecked = true;
            _target.LargeWinLimit = AccountingConstants.DefaultLargeWinLimit.MillicentsToDollars() + 1;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.LargeWinLimit)));
        }

        [TestMethod]
        public void VerifyHandpayLimitDoesNotExceedDefaultHandpayLimitAllowed()
        {
            _target = new LimitsPageViewModel();
            _accessor = new DynamicPrivateObject(_target);

            _target.HandpayLimitIsChecked = true;
            _target.HandpayLimit = AccountingConstants.DefaultHandpayLimit.MillicentsToDollars() + 1;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.HandpayLimit)));
        }

        [TestMethod]
        public void VerifyLargeWinLimitDoesNotExceedHandpayLimit()
        {
            _target = new LimitsPageViewModel();
            _accessor = new DynamicPrivateObject(_target);

            _target.LargeWinLimitIsChecked = true;
            _target.LargeWinLimit = AccountingConstants.DefaultLargeWinLimit.MillicentsToDollars();
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.LargeWinLimit)));

            _target.HandpayLimitIsChecked = true;
            _target.HandpayLimit = AccountingConstants.DefaultLargeWinLimit.MillicentsToDollars() - 1;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.HandpayLimit)));

            _target.LargeWinLimit = 2000L;
            _target.HandpayLimit = 3000L;
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.HandpayLimit)));

        }

        [TestMethod]
        public void VerifyLargeWinLimitErrorIsClearedWhenLargeWinLimitLessThanAllowedMaxLargeWinIsSet()
        {
            _target = new LimitsPageViewModel();
            _accessor = new DynamicPrivateObject(_target);

            _target.HandpayLimitIsChecked = true;
            _target.HandpayLimit = AccountingConstants.DefaultHandpayLimit.MillicentsToDollars();
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.HandpayLimit)));

            _target.LargeWinLimitIsChecked = true;
            _target.LargeWinLimit = long.MaxValue.MillicentsToDollars();
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.LargeWinLimit)));

            _target.LargeWinLimit = AccountingConstants.DefaultHandpayLimit.MillicentsToDollars() - 100;
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.LargeWinLimit)));
        }

        [TestMethod]
        public void VerifyHandpayLimitErrorIsNotClearedWhenValidHandpayLimitIsSet()
        {
            _target = new LimitsPageViewModel();
            _accessor = new DynamicPrivateObject(_target);

            _target.LargeWinLimitIsChecked = true;
            _target.LargeWinLimit = AccountingConstants.DefaultHandpayLimit.MillicentsToDollars();
            Assert.IsFalse(_target.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.LargeWinLimit)));

            _target.HandpayLimitIsChecked = true;
            _target.HandpayLimit = long.MaxValue.MillicentsToDollars();
            Assert.IsTrue(_target.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.HandpayLimit))); // HandpayLimit error is created

            _target.LargeWinLimit = AccountingConstants.DefaultHandpayLimit.MillicentsToDollars();
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.LargeWinLimit))); // LargeWinLimit error is not created
            
            Assert.IsTrue(_target.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.HandpayLimit))); // HandpayLimit error is still exists
        }
    }
}