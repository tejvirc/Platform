namespace Aristocrat.Monaco.Sas.Tests.AftTransferProvider
{
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.AftTransferProvider;

    [TestClass]
    public class AftRegistrationProviderTests
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IStorageDataProvider<AftRegistration>> _aftRegistrationDataProvider;
        private AftRegistrationProvider _target;

        private byte[] testZeroRegistrationKey = new byte[20];
        private byte[] testNonZeroRegistrationKey = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
        private const uint testNonZeroUint = 8;
        private const uint testWrongUint = 9;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _aftRegistrationDataProvider = new Mock<IStorageDataProvider<AftRegistration>>(MockBehavior.Default);
            _aftRegistrationDataProvider.Setup(x => x.GetData()).Returns(new AftRegistration
            {
                RegistrationStatus = AftRegistrationStatus.NotRegistered,
                AftRegistrationKey = testZeroRegistrationKey,
                PosId = 0
            });

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns((uint)0);
            _propertiesManager.Setup(x => x.SetProperty(It.IsAny<string>(), It.IsAny<object>()));

            _exceptionHandler.Setup(x => x.ReportException(It.IsAny<ISasExceptionCollection>()));

            _target = new AftRegistrationProvider(_aftRegistrationDataProvider.Object ,_exceptionHandler.Object, _propertiesManager.Object);
        }

        [TestMethod]
        public void InitialValuesTest()
        {
            Assert.AreEqual(AftRegistrationStatus.NotRegistered, _target.AftRegistrationStatus);
        }

        [TestMethod]
        public void ZeroRegistrationKeyTest()
        {
            Assert.IsTrue(testZeroRegistrationKey.SequenceEqual(_target.ZeroRegistrationKey));
        }

        [TestMethod]
        public void ForceAftNotRegisteredTest()
        {
            TransitionAftRegistrationStatusTo(AftRegistrationStatus.Registered);

            _target.ForceAftNotRegistered();

            Assert.AreEqual(AftRegistrationStatus.NotRegistered, _target.AftRegistrationStatus);
            _exceptionHandler.Verify(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AftRegistrationCanceled)),
                Times.Once);
        }

        [DataRow(AftRegistrationStatus.RegistrationReady, true, DisplayName = "RegistrationReady, Cancelled")]
        [DataRow(AftRegistrationStatus.Registered, false, DisplayName = "Registered, Not Cancelled")]
        [DataRow(AftRegistrationStatus.RegistrationPending, true, DisplayName = "RegistrationPending, Cancelled")]
        [DataRow(AftRegistrationStatus.NotRegistered, false, DisplayName = "NotRegistered, Not Cancelled")]
        [DataTestMethod]
        public void AftRegistrationCycleInterruptedTest(AftRegistrationStatus currentState, bool cycleCancelled)
        {
            TransitionAftRegistrationStatusTo(currentState);
            _target.AftRegistrationCycleInterrupted();
            Assert.AreEqual(cycleCancelled ? AftRegistrationStatus.NotRegistered : currentState, _target.AftRegistrationStatus);
            _exceptionHandler.Verify(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AftRegistrationCanceled)),
                cycleCancelled ? Times.Once() : Times.Never());
        }

        [TestMethod]
        public void RegistrationKeyMatchesTest()
        {
            Assert.IsTrue(testZeroRegistrationKey.SequenceEqual(_target.ZeroRegistrationKey));

            Assert.IsTrue(_target.RegistrationKeyMatches(testZeroRegistrationKey));
            Assert.IsFalse(_target.RegistrationKeyMatches(testNonZeroRegistrationKey));
            Assert.IsFalse(_target.RegistrationKeyMatches(new byte[] { 0, 0, 0 }));

            _aftRegistrationDataProvider.Setup(x => x.GetData()).Returns(new AftRegistration
            {
                RegistrationStatus = AftRegistrationStatus.NotRegistered,
                AftRegistrationKey = testNonZeroRegistrationKey,
                PosId = 0
            });

            Assert.IsFalse(_target.RegistrationKeyMatches(testZeroRegistrationKey));
            Assert.IsTrue(_target.RegistrationKeyMatches(testNonZeroRegistrationKey));
        }

        [TestMethod]
        public void RegistrationKeyMatchesComparisonTest()
        {
            Assert.IsFalse(_target.RegistrationKeyMatches(testZeroRegistrationKey, testNonZeroRegistrationKey));
            Assert.IsTrue(_target.RegistrationKeyMatches(testNonZeroRegistrationKey, testNonZeroRegistrationKey));
            Assert.IsTrue(_target.RegistrationKeyMatches(testZeroRegistrationKey, testZeroRegistrationKey));
        }

        /// <summary>
        ///     Tests a full sequence of AFT Registration States.
        ///     Full sequence:
        ///        NotRegistered->
        ///        RegistrationReady->
        ///        RegistrationPending->
        ///        RegistrationReady->
        ///        Registered
        ///     Expected Result: Success
        /// </summary>
        [TestMethod]
        public void AftRegistrationStatusValidSequencesTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(testNonZeroUint);

            TransitionAftRegistrationStatusTo(AftRegistrationStatus.NotRegistered);
            Assert.AreEqual(AftRegistrationStatus.NotRegistered, _target.AftRegistrationStatus);
            
            _target.ProcessAftRegistration(AftRegistrationCode.InitializeRegistration, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
            Assert.AreEqual(AftRegistrationStatus.RegistrationReady, _target.AftRegistrationStatus);

            _target.ProcessAftRegistration(AftRegistrationCode.RequestOperatorAcknowledgement, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
            _exceptionHandler.Verify(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AftRegistrationAcknowledged)),
                Times.Once);
            Assert.AreEqual(AftRegistrationStatus.RegistrationPending, _target.AftRegistrationStatus);

            _target.ProcessAftRegistration(AftRegistrationCode.RequestOperatorAcknowledgement, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
            Assert.AreEqual(AftRegistrationStatus.RegistrationReady, _target.AftRegistrationStatus);

            _target.ProcessAftRegistration(AftRegistrationCode.RegisterGamingMachine, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
            Assert.AreEqual(AftRegistrationStatus.Registered, _target.AftRegistrationStatus);

            _exceptionHandler.Verify(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AftRegistrationCanceled)),
                Times.Never);
        }

        [DataRow(AftRegistrationStatus.RegistrationReady, AftRegistrationCode.InitializeRegistration, AftRegistrationStatus.RegistrationReady, true, DisplayName = "RegistrationReady->InitializeRegistration, Success")]
        [DataRow(AftRegistrationStatus.RegistrationReady, AftRegistrationCode.RegisterGamingMachine, AftRegistrationStatus.Registered, true, DisplayName = "RegistrationReady->RegisterGamingMachine, Success")]
        [DataRow(AftRegistrationStatus.RegistrationReady, AftRegistrationCode.RequestOperatorAcknowledgement, AftRegistrationStatus.RegistrationPending, true, DisplayName = "RegistrationReady->RequestOperatorAcknowledgement, Success")]
        [DataRow(AftRegistrationStatus.RegistrationReady, AftRegistrationCode.UnregisterGamingMachine, AftRegistrationStatus.RegistrationReady, false, DisplayName = "RegistrationReady->UnregisterGamingMachine, Failure (Explicit)")]
        [DataRow(AftRegistrationStatus.RegistrationReady, AftRegistrationCode.ReadCurrentRegistration, AftRegistrationStatus.RegistrationReady, true, DisplayName = "RegistrationReady->ReadCurrentRegistration, Success")]
        [DataRow(AftRegistrationStatus.Registered, AftRegistrationCode.InitializeRegistration, AftRegistrationStatus.RegistrationReady, true, DisplayName = "Registered->InitializeRegistration, Success")]
        [DataRow(AftRegistrationStatus.Registered, AftRegistrationCode.RegisterGamingMachine, AftRegistrationStatus.Registered, false, DisplayName = "Registered->RegisterGamingMachine, Failure")]
        [DataRow(AftRegistrationStatus.Registered, AftRegistrationCode.RequestOperatorAcknowledgement, AftRegistrationStatus.RegistrationPending, false, DisplayName = "Registered->RequestOperatorAcknowledgement, Failure")]
        [DataRow(AftRegistrationStatus.Registered, AftRegistrationCode.UnregisterGamingMachine, AftRegistrationStatus.Registered, false, DisplayName = "Registered->UnregisterGamingMachine, Failure (Explicit)")]
        [DataRow(AftRegistrationStatus.Registered, AftRegistrationCode.ReadCurrentRegistration, AftRegistrationStatus.Registered, true, DisplayName = "Registered->ReadCurrentRegistration, Success")]
        [DataRow(AftRegistrationStatus.RegistrationPending, AftRegistrationCode.InitializeRegistration, AftRegistrationStatus.RegistrationReady, false, DisplayName = "RegistrationPending->InitializeRegistration, Failure")]
        [DataRow(AftRegistrationStatus.RegistrationPending, AftRegistrationCode.RegisterGamingMachine, AftRegistrationStatus.Registered, false, DisplayName = "RegistrationPending->RegisterGamingMachine, Failure")]
        [DataRow(AftRegistrationStatus.RegistrationPending, AftRegistrationCode.RequestOperatorAcknowledgement, AftRegistrationStatus.RegistrationReady, true, DisplayName = "RegistrationPending->RequestOperatorAcknowledgement, Success")]
        [DataRow(AftRegistrationStatus.RegistrationPending, AftRegistrationCode.UnregisterGamingMachine, AftRegistrationStatus.RegistrationReady, false, DisplayName = "RegistrationPending->UnregisterGamingMachine, Failure (Explicit)")]
        [DataRow(AftRegistrationStatus.RegistrationPending, AftRegistrationCode.ReadCurrentRegistration, AftRegistrationStatus.RegistrationPending, true, DisplayName = "RegistrationPending->ReadCurrentRegistration, Success")]
        [DataRow(AftRegistrationStatus.NotRegistered, AftRegistrationCode.InitializeRegistration, AftRegistrationStatus.RegistrationReady, true, DisplayName = "RegistrationReady->InitializeRegistration, Success")]
        [DataRow(AftRegistrationStatus.NotRegistered, AftRegistrationCode.RegisterGamingMachine, AftRegistrationStatus.NotRegistered, false, DisplayName = "RegistrationReady->RegisterGamingMachine, Success")]
        [DataRow(AftRegistrationStatus.NotRegistered, AftRegistrationCode.RequestOperatorAcknowledgement, AftRegistrationStatus.NotRegistered, false, DisplayName = "RegistrationReady->RequestOperatorAcknowledgement, Success")]
        [DataRow(AftRegistrationStatus.NotRegistered, AftRegistrationCode.UnregisterGamingMachine, AftRegistrationStatus.NotRegistered, true, DisplayName = "RegistrationReady->UnregisterGamingMachine, Success (No change)")]
        [DataRow(AftRegistrationStatus.NotRegistered, AftRegistrationCode.ReadCurrentRegistration, AftRegistrationStatus.NotRegistered, true, DisplayName = "RegistrationReady->ReadCurrentRegistration, Success")]
        [DataTestMethod]
        public void AftRegistrationStatusTransitionTest(AftRegistrationStatus oldState, AftRegistrationCode trigger, AftRegistrationStatus newState, bool success)
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(testNonZeroUint);
            _aftRegistrationDataProvider.Setup(x => x.GetData()).Returns(new AftRegistration
            {
                RegistrationStatus = AftRegistrationStatus.NotRegistered,
                AftRegistrationKey = testNonZeroRegistrationKey,
                PosId = testNonZeroUint
            });

            TransitionAftRegistrationStatusTo(oldState);
            Assert.AreEqual(oldState, _target.AftRegistrationStatus);

            _target.ProcessAftRegistration(trigger, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
            Assert.AreEqual(success ? newState : AftRegistrationStatus.NotRegistered, _target.AftRegistrationStatus);

            _exceptionHandler.Verify(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AftRegistrationCanceled)),
                success ? Times.Never() : Times.Once());
        }

        [DataRow(AftRegistrationStatus.RegistrationReady, testNonZeroUint, true, false, DisplayName = "RegistrationReady, Valid Asset Number, Valid Registration Key, Not Registered")]
        [DataRow(AftRegistrationStatus.Registered, testNonZeroUint, true, true, DisplayName = "Registered, Valid Asset Number, Valid Registration Key, Is Registered")]
        [DataRow(AftRegistrationStatus.RegistrationPending, testNonZeroUint, true, false, DisplayName = "RegistrationPending, Valid Asset Number, Valid Registration Key, Not Registered")]
        [DataRow(AftRegistrationStatus.NotRegistered, testNonZeroUint, true, false, DisplayName = "NotRegistered, Valid Asset Number, Valid Registration Key, Not Registered")]
        [DataRow(AftRegistrationStatus.Registered, (uint)0, true, false, DisplayName = "Registered, Invalid Asset Number, Valid Registration Key, Not Registered")]
        [DataRow(AftRegistrationStatus.Registered, testNonZeroUint, false, false, DisplayName = "Registered, Valid Asset Number, Invalid Registration Key, Not Registered")]
        [DataRow(AftRegistrationStatus.NotRegistered, (uint)0, false, false, DisplayName = "NotRegistered, Invalid Asset Number, Invalid Registration Key, Not Registered")]
        [DataTestMethod]
        public void IsAftRegisteredTest(AftRegistrationStatus registrationStatus, uint assetNumber, bool validRegistrationKey, bool isRegistered)
        {
            _aftRegistrationDataProvider.Setup(x => x.GetData()).Returns(new AftRegistration
            {
                RegistrationStatus = AftRegistrationStatus.NotRegistered,
                AftRegistrationKey = validRegistrationKey ? testNonZeroRegistrationKey : testZeroRegistrationKey,
                PosId = testNonZeroUint
            });

            // Everything is correct
            TransitionAftRegistrationStatusTo(registrationStatus);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);
            Assert.AreEqual(isRegistered, _target.IsAftRegistered);
        }

        [DataRow(true, true, testNonZeroUint, true, DisplayName = "Aft Registered, Debit Transfers Supported, Valid POS ID, Is Enabled")]
        [DataRow(false, true, testNonZeroUint, false, DisplayName = "Aft Not Registered, Debit Transfers Supported, Valid POS ID, Not Enabled")]
        [DataRow(true, false, testNonZeroUint, false, DisplayName = "Aft Registered, Debit Transfers Not Supported, Valid POS ID, Not Enabled")]
        [DataRow(true, true, (uint)0, false, DisplayName = "Aft Registered, Debit Transfers Supported, Invalid POS ID, Not Enabled")]
        [DataRow(false, false, (uint)0, false, DisplayName = "Aft Not Registered, Debit Transfers Not Supported, Invalid POS ID, Not Enabled")]
        [DataTestMethod]
        public void IsAftDebitTransferEnabledTest(bool isAftRegistered, bool debitTransfersSupported, uint posId, bool isDebitTransferEnabled)
        {
            _aftRegistrationDataProvider.Setup(x => x.GetData()).Returns(new AftRegistration
            {
                RegistrationStatus = AftRegistrationStatus.NotRegistered,
                AftRegistrationKey = testNonZeroRegistrationKey,
                PosId = posId
            });

            TransitionAftRegistrationStatusTo(isAftRegistered ? AftRegistrationStatus.Registered : AftRegistrationStatus.NotRegistered);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(testNonZeroUint);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { DebitTransfersAllowed = debitTransfersSupported });
            Assert.AreEqual(isDebitTransferEnabled, _target.IsAftDebitTransferEnabled);
        }

        [DataRow(AftRegistrationCode.InitializeRegistration, (uint)0, testNonZeroUint, true, testNonZeroUint, true, false, false, DisplayName = "InitializeRegistration: Zero Asset Number, Not Registered")]
        [DataRow(AftRegistrationCode.InitializeRegistration, testNonZeroUint, (uint)0, false, testNonZeroUint, true, false, false, DisplayName = "InitializeRegistration: EGM Asset Number is 0, Not Registered")]
        [DataRow(AftRegistrationCode.InitializeRegistration, testWrongUint, testNonZeroUint, false, testNonZeroUint, true, false, false, DisplayName = "InitializeRegistration: Wrong Asset Number, Not Registered")]
        [DataRow(AftRegistrationCode.InitializeRegistration, testNonZeroUint, testNonZeroUint, false, testNonZeroUint, false, true, false, DisplayName = "InitializeRegistration: Zero Reg Key, Registering, Delete Reg Key")]
        [DataRow(AftRegistrationCode.InitializeRegistration, testNonZeroUint, testNonZeroUint, true, (uint)0, false, false, true, DisplayName = "InitializeRegistration: Zero PosId, Registering, Delete PosId")]
        [DataRow(AftRegistrationCode.InitializeRegistration, testNonZeroUint, testNonZeroUint, true, testNonZeroUint, false, false, false, DisplayName = "InitializeRegistration: All filled in, Registering, No Deletions")]
        [DataRow(AftRegistrationCode.RegisterGamingMachine, (uint)0, testNonZeroUint, true, testNonZeroUint, true, false, false, DisplayName = "RegisterGamingMachine: Zero Asset Number, Not Registered")]
        [DataRow(AftRegistrationCode.RegisterGamingMachine, testNonZeroUint, (uint)0, false, testNonZeroUint, true, false, false, DisplayName = "RegisterGamingMachine: EGM Asset Number is 0, Not Registered")]
        [DataRow(AftRegistrationCode.RegisterGamingMachine, testWrongUint, testNonZeroUint, false, testNonZeroUint, true, false, false, DisplayName = "RegisterGamingMachine: Wrong Asset Number, Not Registered")]
        [DataRow(AftRegistrationCode.RegisterGamingMachine, testNonZeroUint, testNonZeroUint, false, testNonZeroUint, true, false, false, DisplayName = "RegisterGamingMachine: Zero Reg Key, Not Registered")]
        [DataRow(AftRegistrationCode.RegisterGamingMachine, testNonZeroUint, testNonZeroUint, true, (uint)0, false, true, true, DisplayName = "RegisterGamingMachine: Zero PosId, Registering (But can not do debits)")]
        [DataRow(AftRegistrationCode.RegisterGamingMachine, testNonZeroUint, testNonZeroUint, true, testNonZeroUint, false, true, true, DisplayName = "RegisterGamingMachine: All correct, Registering")]
        [DataRow(AftRegistrationCode.RegisterGamingMachine, testNonZeroUint, testNonZeroUint, true, (uint)AftPosIdDefinition.NoChange, false, true, false, DisplayName = "RegisterGamingMachine: AftPosIdDefinition.NoChange, Registering, no change to PosId")]
        [DataRow(AftRegistrationCode.RequestOperatorAcknowledgement, (uint)0, (uint)0, false, (uint)0, true, false, false, DisplayName = "RequestOperatorAcknowledgement: Asset numbers 0, Not Registered")]
        [DataRow(AftRegistrationCode.RequestOperatorAcknowledgement, testNonZeroUint, (uint)0, false, (uint)0, true, false, false, DisplayName = "RequestOperatorAcknowledgement: EGM Asset Number is 0, Not Registered")]
        [DataRow(AftRegistrationCode.RequestOperatorAcknowledgement, (uint)0, testNonZeroUint, false, (uint)0, true, false, false, DisplayName = "RequestOperatorAcknowledgement: Zero Asset Number, Not Registered")]
        [DataRow(AftRegistrationCode.RequestOperatorAcknowledgement, testWrongUint, testNonZeroUint, false, (uint)0, true, false, false, DisplayName = "RequestOperatorAcknowledgement: Wrong Asset Number, Not Registered")]
        [DataRow(AftRegistrationCode.RequestOperatorAcknowledgement, testNonZeroUint, testNonZeroUint, false, (uint)0, false, false, false, DisplayName = "RequestOperatorAcknowledgement: Asset numbers match, Registering")]
        [DataRow(AftRegistrationCode.UnregisterGamingMachine, testNonZeroUint, testNonZeroUint, true, testNonZeroUint, true, false, false, DisplayName = "UnregisterGamingMachine: All filled in, Still Not Registered")]
        [DataRow(AftRegistrationCode.UnregisterGamingMachine, (uint)0, (uint)0, false, (uint)0, true, false, false, DisplayName = "UnregisterGamingMachine: Everything wrong, Still Not Registered")]
        [DataRow(AftRegistrationCode.ReadCurrentRegistration, testNonZeroUint, testNonZeroUint, true, testNonZeroUint, false, false, false, DisplayName = "ReadCurrentRegistration: All filled in, Registering")]
        [DataRow(AftRegistrationCode.ReadCurrentRegistration, (uint)0, (uint)0, false, (uint)0, false, false, false, DisplayName = "ReadCurrentRegistration: Everything wrong, Registering")]
        [DataTestMethod]
        public void ProcessAftRegistrationTest(AftRegistrationCode registrationCode, uint dataAssetNumber, uint storedAssetNumber, bool useValidRegistrationKey, uint posId, bool notRegistered, bool modifyRegKey, bool modifyPosId)
        {
            TransitionAftRegistrationStatusFor(registrationCode);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(storedAssetNumber);

            _target.ProcessAftRegistration(
                registrationCode,
                dataAssetNumber,
                useValidRegistrationKey ? testNonZeroRegistrationKey : testZeroRegistrationKey,
                posId);

            CollectionAssert.AreEqual(
                modifyRegKey
                    ? (useValidRegistrationKey ? testNonZeroRegistrationKey : testZeroRegistrationKey)
                    : testZeroRegistrationKey,
                _target.AftRegistrationKey);
            Assert.AreEqual(modifyPosId ? posId : 0, _target.PosId);
            if (notRegistered)
            {
                Assert.AreEqual(AftRegistrationStatus.NotRegistered, _target.AftRegistrationStatus);
            }
            else
            {
                Assert.AreNotEqual(AftRegistrationStatus.NotRegistered, _target.AftRegistrationStatus);
            }
        }

        /// <summary>
        ///     Uses valid state transitions to modify the AftRegistrationStatus to the desired state
        /// </summary>
        /// <param name="newStatus">The desired state of AftRegistrationStatus</param>
        private void TransitionAftRegistrationStatusTo(AftRegistrationStatus newStatus)
        {
            // Save old property values
            uint oldAssetNumber = _propertiesManager.Object.GetValue(ApplicationConstants.MachineId, (uint)0);
            var oldData = _aftRegistrationDataProvider.Object.GetData();

            // Set valid property values so that state transitions don't fail
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(testNonZeroUint);
            _aftRegistrationDataProvider.Setup(x => x.GetData()).Returns(new AftRegistration
            {
                RegistrationStatus = AftRegistrationStatus.NotRegistered,
                AftRegistrationKey = testNonZeroRegistrationKey,
                PosId = testNonZeroUint
            });

            // Transition to correct state
            _target.ProcessAftRegistration(AftRegistrationCode.UnregisterGamingMachine, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
            switch (newStatus)
            {
                case AftRegistrationStatus.NotRegistered:
                    break;
                case AftRegistrationStatus.RegistrationReady:
                    _target.ProcessAftRegistration(AftRegistrationCode.InitializeRegistration, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
                    break;
                case AftRegistrationStatus.RegistrationPending:
                    _target.ProcessAftRegistration(AftRegistrationCode.InitializeRegistration, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
                    _target.ProcessAftRegistration(AftRegistrationCode.RequestOperatorAcknowledgement, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
                    break;
                case AftRegistrationStatus.Registered:
                    _target.ProcessAftRegistration(AftRegistrationCode.InitializeRegistration, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
                    _target.ProcessAftRegistration(AftRegistrationCode.RegisterGamingMachine, testNonZeroUint, testNonZeroRegistrationKey, testNonZeroUint);
                    break;
            }

            // Reset property values
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(oldAssetNumber);
            oldData.RegistrationStatus = newStatus;
            _aftRegistrationDataProvider.Setup(x => x.GetData()).Returns(oldData);

            // Reset any calls done here because they shouldn't count for testing
            _propertiesManager.ResetCalls();
            _exceptionHandler.ResetCalls();
            _aftRegistrationDataProvider.ResetCalls();
        }


        /// <summary>
        ///     Uses valid state transitions to modify the AftRegistrationStatus to a state where the registration code is valid
        /// </summary>
        /// <param name="registrationCode">The desired code to use</param>
        private void TransitionAftRegistrationStatusFor(AftRegistrationCode registrationCode)
        {
            switch (registrationCode)
            {
                case AftRegistrationCode.InitializeRegistration:
                case AftRegistrationCode.UnregisterGamingMachine:
                    TransitionAftRegistrationStatusTo(AftRegistrationStatus.NotRegistered);
                    break;
                case AftRegistrationCode.RegisterGamingMachine:
                case AftRegistrationCode.RequestOperatorAcknowledgement:
                case AftRegistrationCode.ReadCurrentRegistration:
                    TransitionAftRegistrationStatusTo(AftRegistrationStatus.RegistrationReady);
                    break;
            }
        }
    }
}