namespace Aristocrat.Monaco.Accounting.UI.Tests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Accounting.Contracts.Hopper;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    /// <summary>
    ///     Contains the unit tests for the HopperPageViewModel class
    /// </summary>
    [TestClass]
    public class HopperPageViewModelTest
    {
        private HopperPageViewModel _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IMeterManager> _meterManager;
        private Mock<ITransactionHistory> _transactionHistory;

        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IInspectionService>(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Default);
            _transactionHistory = MoqServiceManager.CreateAndAddService<ITransactionHistory>(MockBehavior.Default);
            IReadOnlyCollection<HopperRefillTransaction> emptyTrans =
                new ReadOnlyCollection<HopperRefillTransaction>(new[] { new HopperRefillTransaction() });
            _transactionHistory.Setup(m => m.RecallTransactions<HopperRefillTransaction>()).Returns(emptyTrans);
            var meterMock = new Mock<IMeter>(MockBehavior.Default);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(meterMock.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
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
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCurrentRefillValue, It.IsAny<long>()))
                .Returns((long)5000000);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillDefaultValue, It.IsAny<long>()))
                .Returns(5000000L);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillMinValue, It.IsAny<long>()))
                .Returns(0L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillMaxValue, It.IsAny<long>()))
                .Returns(1000000000L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectDefaultValue, It.IsAny<long>()))
                .Returns((long)10000000);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectMinValue, It.IsAny<long>()))
                .Returns(0L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectMaxValue, It.IsAny<long>()))
                .Returns(1000000000L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperThresholdDefaultValue, It.IsAny<long>()))
                .Returns(2000000L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperThresholdMinValue, It.IsAny<long>()))
                .Returns(1000000L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectLimit, It.IsAny<long>()))
                .Returns((long)0);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketThreshold, It.IsAny<long>()))
                .Returns((long)0);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.VoucherOutLimit, It.IsAny<long>()))
                .Returns((long)100000000);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplitSupported, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplitConfigurable, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplit, It.IsAny<bool>()))
                .Returns(true);

            _target = new HopperPageViewModel(false);
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void CheckDefaultValueVSMinValue()
        {
            var _minHopperCollectLimit = 0;
            var _defaultHopperCollectLimit = 10000000;
            var _defaultHopperTicketThreshold = 2000000;
            var _minHopperTicketThreshold = 1000000;

            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCurrentRefillValue, It.IsAny<long>()))
                .Returns((long)5000000);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectDefaultValue, It.IsAny<long>()))
                .Returns((long)_defaultHopperCollectLimit);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectMinValue, It.IsAny<long>()))
                .Returns((long)_minHopperCollectLimit)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectMaxValue, It.IsAny<long>()))
                .Returns(1000000000L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillDefaultValue, It.IsAny<long>()))
                .Returns(5000000L);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillMinValue, It.IsAny<long>()))
                .Returns(0L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillMaxValue, It.IsAny<long>()))
                .Returns(1000000000L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperThresholdDefaultValue, It.IsAny<long>()))
                .Returns((long)_defaultHopperTicketThreshold)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperThresholdMinValue, It.IsAny<long>()))
                .Returns((long)_minHopperTicketThreshold)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectLimit, It.IsAny<long>()))
                .Returns((long)-10);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketThreshold, It.IsAny<long>()))
                .Returns((long)0);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.VoucherOutLimit, It.IsAny<long>()))
                .Returns((long)100000000);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplitSupported, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplitConfigurable, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplit, It.IsAny<bool>()))
                .Returns(true);

            _target = new HopperPageViewModel(false);
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void CheckDefaultValueVSMaxValue()
        {
            var _minHopperCollectLimit = 0;
            var _defaultHopperCollectLimit = 10000000;
            var _defaultHopperTicketThreshold = 2000000;
            var _minHopperTicketThreshold = 1000000;

            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCurrentRefillValue, It.IsAny<long>()))
                .Returns((long)5000000);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillDefaultValue, It.IsAny<long>()))
                .Returns(5000000L);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillMinValue, It.IsAny<long>()))
                .Returns(0L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperRefillMaxValue, It.IsAny<long>()))
                .Returns(1000000000L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectDefaultValue, It.IsAny<long>()))
                .Returns((long)_defaultHopperCollectLimit);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectMinValue, It.IsAny<long>()))
                .Returns((long)_minHopperCollectLimit)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectMaxValue, It.IsAny<long>()))
                .Returns(1000000000L)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperThresholdDefaultValue, It.IsAny<long>()))
                .Returns((long)_defaultHopperTicketThreshold)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperThresholdMinValue, It.IsAny<long>()))
                .Returns((long)_minHopperTicketThreshold)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperCollectLimit, It.IsAny<long>()))
                .Returns(10000000000L);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketThreshold, It.IsAny<long>()))
                .Returns(599000000L);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.VoucherOutLimit, It.IsAny<long>()))
                .Returns((long)100000000);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplitSupported, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplitConfigurable, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.HopperTicketSplit, It.IsAny<bool>()))
                .Returns(true);

            _target = new HopperPageViewModel(false);
            Assert.IsNotNull(_target);
        }
    }
}
