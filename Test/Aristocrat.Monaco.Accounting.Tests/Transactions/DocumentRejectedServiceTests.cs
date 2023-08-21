namespace Aristocrat.Monaco.Accounting.Tests.Transactions
{
    using System;
    using System.Collections.Generic;
    using Accounting.Transactions;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Kernel.MarketConfig.Models.Application;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass()]
    public class DocumentRejectedServiceTests
    {

        private Mock<IEventBus> _bus;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IAudio> _audioService;
        private Action<CurrencyInCompletedEvent> _currencyInCompletedEvent;
        private Action<VoucherRedeemedEvent> _voucherRedeemedEvent;
        private Action<VoucherRejectedEvent> _voucherRejectedEvent;

        private Action<ClosedEvent> _doorClosedEvent;
        private Action<DownEvent> _jpKeyResetEvent;
        private Mock<ILocalizerFactory> _localizerFactory;

        private DocumentRejectedService _target;
        private const int MaxBillReject = 2;
        private const string ReasonText = "Excessive Bills/Vouchers Rejected";

        /// <summary>
        ///     Tracks how many messages are displayed;
        /// </summary>
        private List<string> _displayedMessages;

        /// <summary>
        ///     Tracks how many messages are removed;
        /// </summary>
        private List<string> _removedMessages;

        // To keep track of keys with which system is disabled with immediate priority
        private List<Guid> _enableImmediateKeys;

        private dynamic _accessor;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Default);
            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Default);
            _bus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Default);
            _audioService = MoqServiceManager.CreateAndAddService<IAudio>(MockBehavior.Default);

            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
            _propertiesManager.Setup(
                    m => m.GetProperty(ApplicationConstants.ExcessiveDocumentRejectCount, It.IsAny<object>()))
                .Returns(MaxBillReject);

            _bus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<DocumentRejectedService>(),
                        It.IsAny<Action<CurrencyInCompletedEvent>>()))
                .Callback<object, Action<CurrencyInCompletedEvent>>(
                    (subscriber, callback) => _currencyInCompletedEvent = callback);
            _bus.Setup(
                    m => m.Subscribe(
                        It.IsAny<DocumentRejectedService>(),
                        It.IsAny<Action<VoucherRedeemedEvent>>()))
                .Callback<object, Action<VoucherRedeemedEvent>>((subscriber, callback) => _voucherRedeemedEvent = callback);

            _bus.Setup(
                    b => b.Subscribe(
                        It.IsAny<DocumentRejectedService>(),
                        It.IsAny<Action<ClosedEvent>>(),
                        It.IsAny<Predicate<ClosedEvent>>()))
                .Callback<object, Action<ClosedEvent>, Predicate<ClosedEvent>>((subscriber, callback, predicateFilter) => _doorClosedEvent = callback);

            _bus.Setup(
                    b => b.Subscribe(
                        It.IsAny<DocumentRejectedService>(),
                        It.IsAny<Action<DownEvent>>(),
                        It.IsAny<Predicate<DownEvent>>()))
                .Callback<object, Action<DownEvent>, Predicate<DownEvent>>((subscriber, callback, predicateFilter) => _jpKeyResetEvent = callback);

            _bus.Setup(b => b.Subscribe(
                It.IsAny<DocumentRejectedService>(),
                It.IsAny<Action<VoucherRejectedEvent>>()))
                .Callback<object, Action<VoucherRejectedEvent>>((subscriber, callback) => _voucherRejectedEvent = callback);

            var localizer = new Mock<ILocalizer>();
            localizer.Setup(x => x.GetString(It.IsAny<string>())).Returns(ReasonText);
            _localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(localizer.Object);
            _displayedMessages = new List<string>();

            _removedMessages = new List<string>();

            _enableImmediateKeys = new List<Guid>();
            _systemDisableManager.Setup(s => s.CurrentImmediateDisableKeys).Returns(_enableImmediateKeys);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }


        [DataRow (ExcessiveDocumentRejectLockupType.Soft)]
        [DataRow (ExcessiveDocumentRejectLockupType.Hard)]
        [DataTestMethod()]
        public void VerifyBillRejectLockupIsCreated(ExcessiveDocumentRejectLockupType lockupType)
        {
            if (lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                SetupForSoftLockup();
                MockMessageDisplay(true, ReasonText);
            }
            else
            {
                SetupForHardLockup();
                MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);
            }
            CreateTarget();

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send bill reject once
            _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            Assert.AreEqual(_displayedMessages.Count, 0);
            _bus.Verify(x => x.Publish(It.IsAny<ExcessiveDocumentRejectedEvent>()), Times.Never);

            //Send bill reject max-1 times
            for (var i = 0; i < MaxBillReject - 1; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            }

            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_displayedMessages[0], ReasonText);
            _bus.Verify(x => x.Publish(It.IsAny<ExcessiveDocumentRejectedEvent>()), Times.Exactly(_displayedMessages.Count));
            _systemDisableManager.Verify();
            _audioService.Verify(
                m => m.Play("Test.ogg", It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Never);
        }

        [TestMethod()]
        public void VerifyBillRejectHardLockupNotClearedByVoucherRedeemedEvent()
        {
            SetupForHardLockup();
            CreateTarget();

            MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);
            _systemDisableManager.Setup(m => m.Enable(It.IsAny<Guid>()))
                .Throws(new Exception("Shouldn't clear lockup on Voucher redeemed event"));

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send bill reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            }

            _voucherRedeemedEvent?.Invoke(new VoucherRedeemedEvent(new VoucherInTransaction()));

            _systemDisableManager.Verify();
        }

        [TestMethod()]
        public void VerifyBillRejectHardLockupNotClearedByValidCurrencyIn()
        {
            SetupForHardLockup();
            CreateTarget();

            MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);
            _systemDisableManager.Setup(m => m.Enable(It.IsAny<Guid>()))
                .Throws(new Exception("Shouldn't clear lockup on Valid currency in event"));

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send bill reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            }

            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_displayedMessages[0], ReasonText);

            //Send currency in with positive amount
            _currencyInCompletedEvent(new CurrencyInCompletedEvent(1));

            _systemDisableManager.Verify();
        }

        [DataRow (ExcessiveDocumentRejectLockupType.Soft)]
        [DataRow (ExcessiveDocumentRejectLockupType.Hard)]
        [DataTestMethod()]
        public void VerifyBillRejectLockupIsClearedByMainDoorClose(ExcessiveDocumentRejectLockupType lockupType)
        {
            if (lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                SetupForSoftLockup();
                MockMessageDisplay(true, ReasonText);
                MockMessageDisplay(false, ReasonText);

            }
            else
            {
                SetupForHardLockup();
                MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);
                MockDisableManager(false, ApplicationConstants.ExcessiveDocumentRejectGuid);
            }
            CreateTarget();

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send bill reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            }

            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_displayedMessages[0], ReasonText);

            Assert.IsNotNull(_doorClosedEvent);

            //Send Reset key event
            _doorClosedEvent(new ClosedEvent((int)DoorLogicalId.Main, "main"));

            _systemDisableManager.Verify();
        }

        [DataRow(ResetMethodKeyType.MainDoor)]
        [DataRow(ResetMethodKeyType.JackpotKey)]
        [DataTestMethod()]
        public void VerifyBillRejectLockupIsClearedByConfig(ResetMethodKeyType resetMethodKey)
        {

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ExcessiveDocumentRejectSoundFilePath, It.IsAny<object>())).Returns("Test.ogg");
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ExcessiveDocumentRejectResetMethodKey, It.IsAny<object>())).Returns(resetMethodKey);

            SetupForHardLockup();
            MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);
            MockDisableManager(false, ApplicationConstants.ExcessiveDocumentRejectGuid);
            CreateTarget();

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send bill reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            }

            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_displayedMessages[0], ReasonText);

            //If MainDoor then verify lockup is reset
            switch (resetMethodKey)
            {
                case ResetMethodKeyType.MainDoor:
                    Assert.IsNotNull(_doorClosedEvent);
                    _doorClosedEvent(new ClosedEvent((int)DoorLogicalId.Main, "main"));

                    _systemDisableManager.Verify();
                    _audioService.Verify(
                        m => m.Play("Test.ogg", It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Once);
                    break;
                case ResetMethodKeyType.JackpotKey:
                    Assert.IsNotNull(_jpKeyResetEvent);
                    _jpKeyResetEvent(new DownEvent((int)ButtonLogicalId.Button30));

                    _systemDisableManager.Verify();
                    _audioService.Verify(
                        m => m.Play("Test.ogg", It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Once);
                    break;
            }
        }

        [TestMethod()]
        public void VerifyBillRejectSoftLockupIsClearedByVoucherRedemption()
        {
            SetupForSoftLockup();
            CreateTarget();

            MockMessageDisplay(true, ReasonText);
            MockMessageDisplay(false, ReasonText);

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send bill reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            }

            Assert.IsNotNull(_voucherRedeemedEvent);
            _voucherRedeemedEvent(new VoucherRedeemedEvent(new VoucherInTransaction()));

            Assert.AreEqual(_displayedMessages.Count, _removedMessages.Count);
        }

        [TestMethod()]
        public void VerifyBillRejectSoftLockupIsClearedByValidCurrencyIn()
        {
            SetupForSoftLockup();
            CreateTarget();

            MockMessageDisplay(true, ReasonText);
            MockMessageDisplay(false, ReasonText);

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send bill reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            }

            _currencyInCompletedEvent(new CurrencyInCompletedEvent(1));

            Assert.AreEqual(_displayedMessages.Count, _removedMessages.Count);
        }

        [DataRow(ExcessiveDocumentRejectLockupType.Soft)]
        [DataRow(ExcessiveDocumentRejectLockupType.Hard)]
        [DataTestMethod()]
        public void VerifyBillRejectIsCreatedOnlyOnSuccessiveMaxRejections(ExcessiveDocumentRejectLockupType lockupType)
        {
            if (lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                SetupForSoftLockup();
                _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<DisplayableMessage>()))
                    .Throws(new Exception("Shouldn't create lockup if Max rejections are not in succession"));
            }
            else
            {
                SetupForHardLockup();
                _systemDisableManager.Setup(
                        m => m.Disable(
                            It.IsAny<Guid>(),
                            SystemDisablePriority.Immediate,
                            It.IsAny<Func<string>>(),
                            null))
                    .Throws(new Exception("Shouldn't create lockup if Max rejections are not in succession"));
            }

            CreateTarget();

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send bill reject max-1 times
            for (var i = 0; i < MaxBillReject - 1; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));
            }
            Assert.AreEqual(_accessor._consecutiveDocumentRejectCount, MaxBillReject - 1);

            //Send currency in with positive amount
            _currencyInCompletedEvent(new CurrencyInCompletedEvent(1));

            Assert.AreEqual(_accessor._consecutiveDocumentRejectCount, 0);

            //Send bill reject once more
            _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));

            Assert.AreEqual(_accessor._consecutiveDocumentRejectCount, 1);

            _systemDisableManager.Verify();
        }

        [DataRow(ExcessiveDocumentRejectLockupType.Soft)]
        [DataRow(ExcessiveDocumentRejectLockupType.Hard)]
        [DataTestMethod()]
        public void VerifyVoucherRejectLockupIsCreated(ExcessiveDocumentRejectLockupType lockupType)
        {
            if (lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                SetupForSoftLockup();
                MockMessageDisplay(true, ReasonText);
            }
            else
            {
                SetupForHardLockup();
                MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);
            }

            CreateTarget();

            Assert.IsNotNull(_voucherRejectedEvent);

            //Send voucher reject once
            _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            Assert.AreEqual(_displayedMessages.Count, 0);
            _bus.Verify(x => x.Publish(It.IsAny<ExcessiveDocumentRejectedEvent>()), Times.Never);

            //Send voucher reject max-1 times
            for (var i = 0; i < MaxBillReject - 1; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }

            Assert.AreEqual(_displayedMessages.Count, 1);
            _bus.Verify(x => x.Publish(It.IsAny<ExcessiveDocumentRejectedEvent>()), Times.Exactly(_displayedMessages.Count));
            Assert.AreEqual(_displayedMessages[0], ReasonText);
            _systemDisableManager.Verify();
        }

        [TestMethod()]
        public void VerifyVoucherRejectSoftLockupIsClearedByVoucherRedeemedEvent()
        {
            SetupForSoftLockup();
            CreateTarget();

            MockMessageDisplay(true, ReasonText);
            MockMessageDisplay(false, ReasonText);

            //Send voucher reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }
            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_displayedMessages[0], ReasonText);

            _voucherRedeemedEvent?.Invoke(new VoucherRedeemedEvent(new VoucherInTransaction()));

            Assert.AreEqual(_displayedMessages.Count, _removedMessages.Count);

            _systemDisableManager.Verify();
        }

        [TestMethod()]
        public void VerifyVoucherRejectHardLockupNotClearedByVoucherRedeemedEvent()
        {
            SetupForHardLockup();
            CreateTarget();

            MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);
            _systemDisableManager.Setup(m => m.Enable(It.IsAny<Guid>()))
                .Throws(new Exception("Shouldn't clear lockup on Voucher redeemed event"));

            Assert.IsNotNull(_voucherRejectedEvent);

            //Send voucher reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }

            _voucherRedeemedEvent?.Invoke(new VoucherRedeemedEvent(new VoucherInTransaction()));

            Assert.AreEqual(_removedMessages.Count, 0);

            _systemDisableManager.Verify();
        }


        [DataRow(ExcessiveDocumentRejectLockupType.Soft)]
        [DataRow(ExcessiveDocumentRejectLockupType.Hard)]
        [DataTestMethod()]
        public void VerifyVoucherRejectLockupIsClearedByMainDoorClose(ExcessiveDocumentRejectLockupType lockupType)
        {
            if (lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                SetupForSoftLockup();
                MockMessageDisplay(true, ReasonText);
                MockMessageDisplay(false, ReasonText);
            }
            else
            {
                SetupForHardLockup();
                MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);
                MockDisableManager(false, ApplicationConstants.ExcessiveDocumentRejectGuid);
            }

            CreateTarget();

            Assert.IsNotNull(_voucherRejectedEvent);

            //Send bill reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }

            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_displayedMessages[0], ReasonText);

            Assert.IsNotNull(_doorClosedEvent);

            //Send Reset key event
            _doorClosedEvent(new ClosedEvent((int)DoorLogicalId.Main, "main"));

            _systemDisableManager.Verify();
        }

        [TestMethod()]
        public void VerifyVoucherRejectSoftLockupIsClearedByValidCurrencyIn()
        {
            SetupForSoftLockup();
            CreateTarget();

            MockMessageDisplay(true, ReasonText);
            MockMessageDisplay(false, ReasonText);

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send voucher reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }

            _currencyInCompletedEvent(new CurrencyInCompletedEvent(1));

            Assert.AreEqual(_displayedMessages.Count, _removedMessages.Count);
        }

        [TestMethod()]
        public void VerifyVoucherRejectHardLockupNotClearedByValidCurrencyIn()
        {
            SetupForHardLockup();
            CreateTarget();

            MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);

            _systemDisableManager.Setup(
                m => m.Enable(
                    It.IsAny<Guid>()))
                .Throws(new Exception("Shouldn't clear this lockup on valid Currency In event"));

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send voucher reject max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }
            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_accessor._consecutiveDocumentRejectCount, MaxBillReject);

            _currencyInCompletedEvent(new CurrencyInCompletedEvent(1));

            Assert.AreEqual(_removedMessages.Count, 0);
        }


        [DataRow(ExcessiveDocumentRejectLockupType.Soft)]
        [DataRow(ExcessiveDocumentRejectLockupType.Hard)]
        [DataTestMethod()]
        public void VerifyVoucherRejectLockupIsCreatedOnlyOnSuccessiveMaxRejections(ExcessiveDocumentRejectLockupType lockupType)
        {
            if (lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                SetupForSoftLockup();
                _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<DisplayableMessage>()))
                    .Throws(new Exception("Shouldn't create lockup if Max rejections are not in succession"));
            }
            else
            {
                SetupForHardLockup();
                _systemDisableManager.Setup(
                        m => m.Disable(
                            It.IsAny<Guid>(),
                            SystemDisablePriority.Immediate,
                            It.IsAny<Func<string>>(),
                            null))
                    .Throws(new Exception("Shouldn't create lockup if Max rejections are not in succession"));
            }

            CreateTarget();

            Assert.IsNotNull(_voucherRejectedEvent);

            //Send voucher reject max-1 times
            for (var i = 0; i < MaxBillReject - 1; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }
            Assert.AreEqual(_accessor._consecutiveDocumentRejectCount, MaxBillReject - 1);

            //Send voucher redeemed event
            _voucherRedeemedEvent(new VoucherRedeemedEvent(new VoucherInTransaction()));
            Assert.AreEqual(_accessor._consecutiveDocumentRejectCount, 0);

            //Send voucher reject once more
            _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));

            Assert.AreEqual(_displayedMessages.Count, 0);
            Assert.AreEqual(_accessor._consecutiveDocumentRejectCount, 1);

            _systemDisableManager.Verify();
        }

        [TestMethod()]
        public void VerifyDocumentRejectSoftLockupIsCreatedForCombinationOfBillsAndVouchers()
        {
            SetupForSoftLockup();
            CreateTarget();

            MockMessageDisplay(true, ReasonText);

            Assert.AreEqual(_displayedMessages.Count, 0);

            // Send bill reject once
            _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));

            // Send voucher reject max-1 times
            for (var i = 0; i < MaxBillReject - 1; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }

            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_displayedMessages[0], ReasonText);

            _bus.Verify();
        }

        [TestMethod()]
        public void VerifyDocumentRejectHardLockupIsCreatedForCombinationOfBillsAndVouchers()
        {
            SetupForHardLockup();
            CreateTarget();

            MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);

            // Send bill reject once
            _currencyInCompletedEvent(new CurrencyInCompletedEvent(0));

            Assert.AreEqual(_displayedMessages.Count, 0);

            // Send voucher reject max-1 times
            for (var i = 0; i < MaxBillReject - 1; i++)
            {
                _voucherRejectedEvent(new VoucherRejectedEvent(new VoucherInTransaction()));
            }

            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_displayedMessages[0], ReasonText);

            _systemDisableManager.Verify();
        }

        [TestMethod()]
        public void VerifyBillRejectSoftLockupDueToCurrencyReturnedEventIsClearedByValidCurrencyIn()
        {
            SetupForSoftLockup();
            CreateTarget();

            MockMessageDisplay(true, ReasonText);
            MockMessageDisplay(false, ReasonText);

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send currency returned event max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0L));
            }

            _currencyInCompletedEvent(new CurrencyInCompletedEvent(1));

            Assert.AreEqual(_displayedMessages.Count, _removedMessages.Count);
        }

        [TestMethod()]
        public void VerifyBillRejectHardLockupDueToCurrencyReturnedEventNotClearedByValidCurrencyIn()
        {
            SetupForHardLockup();
            CreateTarget();

            MockDisableManager(true, ApplicationConstants.ExcessiveDocumentRejectGuid, ReasonText);

            _systemDisableManager.Setup(
                    m => m.Enable(
                        It.IsAny<Guid>()))
                .Throws(new Exception("Shouldn't clear this lockup on valid Currency In event"));

            Assert.IsNotNull(_currencyInCompletedEvent);

            //Send currency returned event max times
            for (var i = 0; i < MaxBillReject; i++)
            {
                _currencyInCompletedEvent(new CurrencyInCompletedEvent(0L));
            }
            Assert.AreEqual(_displayedMessages.Count, 1);
            Assert.AreEqual(_accessor._consecutiveDocumentRejectCount, MaxBillReject);

            _currencyInCompletedEvent(new CurrencyInCompletedEvent(1));

            Assert.AreEqual(_removedMessages.Count, 0);
        }

        private void MockDisableManager(
            bool disable,
            Guid disableGuid,
            string msg = "")
        {
            if (disable)
            {
                if (!string.IsNullOrEmpty(disableGuid.ToString()) && !string.IsNullOrEmpty(msg))
                {
                    _systemDisableManager.Setup(
                        m => m.Disable(
                            disableGuid,
                            SystemDisablePriority.Immediate,
                            It.Is<Func<string>>(x => x.Invoke() == msg),
                            null))
                        .Callback(
                            (
                                Guid enableKey,
                                SystemDisablePriority priority,
                                Func<string> disableReason,
                                Type type) =>
                            {
                                _displayedMessages.Add(disableReason.Invoke());
                                _enableImmediateKeys.Add(enableKey);
                            })
                        .Verifiable();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(disableGuid.ToString()))
                {
                    _systemDisableManager.Setup(m => m.Enable(disableGuid))
                        .Callback(
                            (
                                Guid id) => _enableImmediateKeys.Remove(id))
                        .Verifiable();
                }
            }
        }

        private void MockMessageDisplay(
            bool disable,
            string msg)
        {
            if (disable)
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<DisplayableMessage>())).Callback(
                         (DisplayableMessage message) =>
                         {
                             _displayedMessages.Add(message.Message);
                         }).Verifiable();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<DisplayableMessage>()))
                        .Callback((DisplayableMessage message) => { _removedMessages.Add(message.Message); })
                        .Verifiable();
                }
            }
        }

        private void SetupForSoftLockup()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.ExcessiveDocumentRejectLockupType, It.IsAny<object>()))
                .Returns(ExcessiveDocumentRejectLockupType.Soft);
        }

        private void SetupForHardLockup()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.ExcessiveDocumentRejectLockupType, It.IsAny<object>()))
                .Returns(ExcessiveDocumentRejectLockupType.Hard);
        }

        private void CreateTarget()
        {
            _target = new DocumentRejectedService();
            _accessor = new DynamicPrivateObject(_target);
            _target.Initialize();
        }
    }
}