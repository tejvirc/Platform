namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Authentication;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Base;
    using Sas.Handlers;
    using Test.Common;

    /// <summary>
    ///     Contains tests for LP6ESendAuthenticationInfoHandler
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class LP6ESendAuthenticationInfoHandlerTest
    {
        private const string CompName1 = "Comp1";
        private const string CompName2 = "Comp2";
        private const string CompNameUnavailable = "CompUnavailable";
        private const long CompSize = 1_234_567_890;
        private int FakeCalculationDurationMs = 100;
        private readonly byte[] _twoByteHash = { 0x0F, 0xDD };
        private const ushort CrcHash = 0xBFDB; // Change if adding/editing components in SetupComponentRegistryMock()

        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
        private readonly Mock<IComponentRegistry> _componentRegistry = new Mock<IComponentRegistry>();
        private readonly Mock<IAuthenticationService> _authenticationService = new Mock<IAuthenticationService>(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _propertiesManger = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly ManualResetEventSlim _waiter = new ManualResetEventSlim(false);

        private LP6ESendAuthenticationInfoHandler _target;
        private readonly ComponentVerification _currentVerification = new ComponentVerification();
        private Action<ComponentAddedEvent> _componentAddedAction;
        private Action<ComponentRemovedEvent> _componentRemovedAction;
        private Action<ComponentHashCompleteEvent> _calculationCompletedAction;
        private Action<AllComponentsHashCompleteEvent> _allCalculationsCompletedAction;
        private Action<PropertyChangedEvent> _propertyChangedAction;
        private CancellationToken _cancellationToken;

        [TestInitialize]
        public void MyTestInitialize()
        {
            SetupEventBusMock();
            SetupComponentRegistryMock();
            SetupAuthenticationServiceMock();
            _exceptionHandler.Setup(
               m => m.ReportException(
                   It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.ComponentListChanged || ex.ExceptionCode == GeneralExceptionCode.AuthenticationComplete))).Verifiable();

            _target = new LP6ESendAuthenticationInfoHandler(_exceptionHandler.Object, _eventBus.Object, _componentRegistry.Object, _authenticationService.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            // dispose twice to ensure 2nd dispose just returns
            _target.Dispose();
            _target.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullSasHostTest()
        {
            _target = new LP6ESendAuthenticationInfoHandler(null, _eventBus.Object, _componentRegistry.Object, _authenticationService.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullEventBusTest()
        {
            _target = new LP6ESendAuthenticationInfoHandler(_exceptionHandler.Object, null, _componentRegistry.Object, _authenticationService.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullComponentRegistryTest()
        {
            _target = new LP6ESendAuthenticationInfoHandler(_exceptionHandler.Object, _eventBus.Object, null, _authenticationService.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAuthenticationServiceTest()
        {
            _target = new LP6ESendAuthenticationInfoHandler(_exceptionHandler.Object, _eventBus.Object, _componentRegistry.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            var commands = _target.Commands;
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual(LongPoll.SendAuthenticationInformation, commands[0]);
        }

        [TestMethod]
        public void HandleInvalidActionTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = (AuthenticationAction)20
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.InvalidCommand, output.Status);
        }

        [TestMethod]
        public void HandleRequestNumberComponentsTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateNumberOfInstalledComponents
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.InstalledComponentResponse, output.Status);
            Assert.AreEqual(_componentRegistry.Object.Components.Count(), output.ComponentSize);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
            Assert.AreEqual(AuthenticationMethods.None, output.AvailableMethods);
        }

        [TestMethod]
        public void HandleRequestKnownComponentStatusByIndexTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.ReadStatusOfComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.Success, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
            Assert.AreEqual(CompName1, output.ComponentName);
            Assert.AreEqual(CompSize, output.ComponentSize);
            Assert.AreEqual(_target.AvailableAuthenticationMethods, output.AvailableMethods);
        }

        [TestMethod]
        public void HandleRequestKnownComponentStatusByNameTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.ReadStatusOfComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByName,
                ComponentName = CompName1
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.Success, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
            Assert.AreEqual(CompName1, output.ComponentName);
            Assert.AreEqual(CompSize, output.ComponentSize);
            Assert.AreEqual(_target.AvailableAuthenticationMethods, output.AvailableMethods);
        }

        [TestMethod]
        public void HandleRequestUnknownComponentStatusByIndexTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.ReadStatusOfComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = _componentRegistry.Object.Components.Count() + 1 // invalid
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.ComponentDoesNotExist, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
        }

        [TestMethod]
        public void HandleRequestUnknownComponentStatusByNameTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.ReadStatusOfComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByName,
                ComponentName = "HereBeDragons" // invalid
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.ComponentDoesNotExist, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
        }

        [TestMethod]
        public void HandleRequestAuthenticationInvalidComponentTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = _componentRegistry.Object.Components.Count() + 1, // invalid
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.ComponentDoesNotExist, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
        }

        [TestMethod]
        public void HandleRequestAuthenticationComponentUnavailableTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByName,
                ComponentName = CompNameUnavailable
            };
            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.ComponentDisabledOrUnavailable, output.Status);
        }

        [TestMethod]
        public void HandleRequestAuthenticationInvalidMethodTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Kobetron1, // invalid
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.RequestedAuthenticationMethodNotSupported, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
        }

        [TestMethod]
        public void HandleRequestAuthenticationInvalidSeedTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = new byte[20], // invalid for CRC16
                AuthenticationOffset = 0
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.InvalidDataForRequestedAuthenticationMethod, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
        }

        [TestMethod]
        public void HandleRequestAuthenticationInvalidOffsetTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = CompSize + 100 // invalid
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.InvalidDataForRequestedAuthenticationMethod, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
        }

        [TestMethod]
        public void HandleRequestAuthenticationCorrectTest()
        {
            _exceptionHandler.Setup(
                m => m.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AuthenticationComplete))).Verifiable();
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            _waiter.Reset();
            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationCurrentlyInProgress, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);

            // Authentication is running still; interrogate.
            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationCurrentlyInProgress, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
            Assert.AreEqual(0, output.AuthenticationData.Length);

            // Look for results after authentication completes.
            _waiter.Wait();

            output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationComplete, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
            Assert.AreEqual(_twoByteHash, output.AuthenticationData);
            _exceptionHandler.Verify();
        }

        [TestMethod]
        public void HandleRequestAuthenticationStatusNoneStartedTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.NoAuthenticationDataAvailable, output.Status);
        }

        [TestMethod]
        public void HandleRequestAuthenticationAbortedTest()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            _target.Handle(input);

            // Abort the authentication by changing the component list.
            Thread.Sleep(FakeCalculationDurationMs / 2);
            _componentAddedAction.Invoke(new ComponentAddedEvent(new Component()));

            // Look for results; should be "aborted".
            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            Console.WriteLine($"[{DateTime.Now}] - [{nameof(HandleRequestAuthenticationAbortedTest)}-0] - [{GetHashCode()}] - [{Environment.CurrentManagedThreadId}]");

            var output = _target.Handle(input);

            Console.WriteLine($"[{DateTime.Now}] - [{nameof(HandleRequestAuthenticationAbortedTest)}-1] - [{GetHashCode()}] - [{Environment.CurrentManagedThreadId}]");

            Assert.AreEqual(AuthenticationStatus.AuthenticationAborted, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);

            // Test that subsequent Authentication commands may be finished to completion.
            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            SendAuthenticateComponentToCompletion(input);
        }

        [TestMethod]
        public void HandleRequestAuthenticationAbortedTest2()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            _target.Handle(input);

            // Abort the authentication by changing the component list.
            Thread.Sleep(FakeCalculationDurationMs / 2);
            _componentRemovedAction.Invoke(new ComponentRemovedEvent(new Component()));

            // Look for results; should be "aborted".
            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationAborted, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
        }

        [TestMethod]
        public void HandleRequestAuthenticationDisabledTest1()
        {
            SetupDisconnectOfComponent1();

            // Test that subsequent Authentication commands may be finished to completion:

            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByName,
                ComponentName = CompName2,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            SendAuthenticateComponentToCompletion(input);
        }

        [TestMethod]
        public void HandleRequestAuthenticationDisabledTest2()
        {
            SetupDisconnectOfComponent1();

            // Test that the status of our component is disabled.
            // Test that the status of other components are correct.

            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.ReadStatusOfComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByName,
                ComponentName = CompName1
            };

            var output = _target.Handle(input);
            Assert.AreEqual(AuthenticationStatus.ComponentDisabledOrUnavailable, output.Status);

            input.ComponentName = CompName2;

            output = _target.Handle(input);
            Assert.AreEqual(AuthenticationStatus.Success, output.Status);
        }

        [TestMethod]
        public void HandleRequestAuthenticationDisabledTest3()
        {
            SetupDisconnectOfComponent1();

            // Test that after checking the authentication status,
            // (this happens in the above setup function)
            // a subsequent authentication status is 'unavailable'.

            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            var output = _target.Handle(input);
            Assert.AreEqual(AuthenticationStatus.NoAuthenticationDataAvailable, output.Status);
        }

        [TestMethod]
        public void HandleRequestAuthenticationDisabledTest4()
        {
            SetupDisconnectOfComponent1();

            // Test that we can make available a disabled component,
            // but we need to resend the authentication request.

            // Enable our device.
            _componentRegistry.Object.Get(CompName1).Available = true;

            // Prove our device is enabled.
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.ReadStatusOfComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByName,
                ComponentName = CompName1
            };

            var output = _target.Handle(input);
            Assert.AreEqual(AuthenticationStatus.Success, output.Status);

            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            output = _target.Handle(input);
            Assert.AreEqual(AuthenticationStatus.NoAuthenticationDataAvailable, output.Status);

            // Resend the authentication request.
            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByName,
                ComponentName = CompName1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            SendAuthenticateComponentToCompletion(input);
        }

        [TestMethod]
        public void HandleRequestAuthenticationTwiceTest()
        {
            _exceptionHandler.Setup(
                m => m.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AuthenticationComplete))).Verifiable();

            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            _waiter.Reset();
            _target.Handle(input); // First authentication; do not allow to complete.

            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 2,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            SetupAuthenticationServiceMock(); // Allows the authentication task to be run again.
            var output = _target.Handle(input); // Second authentication; first authentication is cancelled.

            Assert.AreEqual(AuthenticationStatus.AuthenticationCurrentlyInProgress, output.Status);

            _waiter.Wait();

            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            output = _target.Handle(input);

            Assert.AreEqual(CompName2, output.ComponentName);
            Assert.AreEqual(AuthenticationStatus.AuthenticationComplete, output.Status);

            _exceptionHandler.Verify();
        }

        [TestMethod]
        public void HandleSasExceptionTimerTest()
        {
            ChangeTimeoutToHalfSecond();
            _exceptionHandler.Setup(
                m => m.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AuthenticationComplete))).Verifiable();
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByIndex,
                ComponentIndex = 1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            _waiter.Reset();
            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationCurrentlyInProgress, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);

            // Look for results after authentication completes.
            _waiter.Wait();

            // Wait longer than the timeout value so the timer re-sends the exception
            Thread.Sleep(1500);

            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationComplete, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
            Assert.AreEqual(_twoByteHash, output.AuthenticationData);

            // check that the exception was sent once, and then again after the timeout
            _exceptionHandler.Verify(
                m => m.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AuthenticationComplete)), Times.Exactly(2));
        }

        [TestMethod]
        public void HandleNackTest()
        {
            dynamic accessor = new DynamicPrivateObject(_target);
            accessor._currentVerification = new ComponentVerification
            {
                ComponentId = "component 1",
                AlgorithmType = AlgorithmType.Crc16,
                Result = new byte[0],
                ResultTime = DateTime.MinValue
            };

            accessor.HandleNack();

            var id = accessor._currentVerification.ComponentId;

            Assert.IsTrue(string.IsNullOrEmpty(id));
        }

        /// <summary>
        ///     Change the exception timer from 15 seconds to half a second for the unit test
        /// </summary>
        private void ChangeTimeoutToHalfSecond()
        {
            dynamic privateTarget = new DynamicPrivateObject(_target);
            dynamic privateTimer = new DynamicPrivateObject((SasExceptionTimer)privateTarget._exceptionTimer);
            privateTimer._timerTimeout = 1000.0f;
            privateTarget._exceptionTimer = (SasExceptionTimer)privateTimer.Target;
        }

        private void SetupEventBusMock()
        {
            _eventBus.Setup(
                    c => c.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<ComponentAddedEvent>>()))
                .Callback<object, Action<ComponentAddedEvent>>((obj, eventProcessor) => _componentAddedAction = eventProcessor);

            _eventBus.Setup(
                    c => c.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<ComponentRemovedEvent>>()))
                .Callback<object, Action<ComponentRemovedEvent>>((obj, eventProcessor) => _componentRemovedAction = eventProcessor);

            _eventBus.Setup(
                    c => c.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<ComponentHashCompleteEvent>>()))
                .Callback<object, Action<ComponentHashCompleteEvent>>((obj, eventProcessor) => _calculationCompletedAction = eventProcessor);

            _eventBus.Setup(
                    c => c.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<AllComponentsHashCompleteEvent>>()))
                .Callback<object, Action<AllComponentsHashCompleteEvent>>((obj, eventProcessor) => _allCalculationsCompletedAction = eventProcessor);

            _eventBus.Setup(
                c => c.Subscribe(
                    It.IsAny<LP6ESendAuthenticationInfoHandler>(),
                    It.IsAny<Action<PropertyChangedEvent>>(),
                    It.IsAny<Predicate<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>, Predicate<PropertyChangedEvent>>(
                    (obj, eventProcessor, filter) => _propertyChangedAction = eventProcessor);

            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<LP6ESendAuthenticationInfoHandler>())).Verifiable();
        }

        private void SetupAuthenticationServiceMock()
        {
            // The next two differ only in the offset parameter
            _authenticationService.Setup(
                    a => a.GetComponentHashesAsync(
                        It.Is<AlgorithmType>(t => t == AlgorithmType.Crc16),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<byte[]>(),
                        It.IsAny<string>(),
                        It.Is<long>(o => o == 0)))
                .Callback<AlgorithmType, CancellationToken, byte[], string, long>(
                    (at, ct, seed, componentName, offset) => { _cancellationToken = ct; _currentVerification.ComponentId = componentName; })
                .Returns(new Task(
                    () =>
                    {
                        Thread.Sleep(FakeCalculationDurationMs);
                        FakeCrcCalculation(_cancellationToken, ushort.MinValue, _twoByteHash);
                        _waiter?.Set();
                    }));

            _authenticationService.Setup(
                    a => a.GetComponentHashesAsync(
                        It.Is<AlgorithmType>(t => t == AlgorithmType.Crc16),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<byte[]>(),
                        It.IsAny<string>(),
                        It.Is<long>(o => o != 0)))
                .Callback<AlgorithmType, CancellationToken, byte[], string, long>(
                    (at, ct, seed, componentName, offset) => { _cancellationToken = ct; _currentVerification.ComponentId = componentName; })
                .Returns(new Task(
                    () =>
                    {
                        Thread.Sleep(FakeCalculationDurationMs);
                        FakeCrcCalculation(_cancellationToken, ushort.MinValue, _twoByteHash);
                        _waiter?.Set();
                    }));
        }

        private void SetupComponentRegistryMock()
        {
            var componentList = new List<Component>
            {
                new Component
                {
                    ComponentId = CompName1,
                    Size = CompSize
                },
                new Component
                {
                    ComponentId = CompName2,
                    Size = CompSize
                },
                new Component
                {
                    ComponentId = CompNameUnavailable,
                    Available = false,
                    Size = CompSize
                }
            };
            _componentRegistry.SetupGet(r => r.Components)
                .Returns(componentList);

            _componentRegistry.Setup(r => r.Get(It.Is<string>(c => c == CompName1)))
                .Returns(componentList[0]);

            _componentRegistry.Setup(r => r.Get(It.Is<string>(c => c == CompName2)))
                .Returns(componentList[1]);

            _componentRegistry.Setup(r => r.Get(It.Is<string>(c => c == CompNameUnavailable)))
                .Returns(componentList[2]);
        }

        private void FakeCrcCalculation(CancellationToken cancellationToken, ushort seed, byte[] result)
        {
            var componentVerification = new ComponentVerification
            {
                ComponentId = _currentVerification.ComponentId,
                AlgorithmType = AlgorithmType.Crc16,
                Seed = Utilities.ToBinary(seed, sizeof(ushort)),
                Result = result,
                ResultTime = DateTime.Now
            };

            if (!cancellationToken.IsCancellationRequested)
            {
                _calculationCompletedAction(new ComponentHashCompleteEvent(componentVerification, cancellationToken));
            }
        }

        private SendAuthenticationInfoResponse SendAuthenticateComponentToCompletion(SendAuthenticationInfoCommand input)
        {
            _exceptionHandler.Setup(
                m => m.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.AuthenticationComplete))).Verifiable();

            SetupAuthenticationServiceMock(); // Allows the authentication task to be run again if run or cancelled already.
            _target.Handle(input);

            // Authentication is running still; interrogate.
            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationCurrentlyInProgress, output.Status);

            // Look for results after authentication completes.
            _waiter.Wait();

            output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationComplete, output.Status);

            _exceptionHandler.Verify();

            return output;
        }

        private void SetupDisconnectOfComponent1()
        {
            var input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.AuthenticateComponent,
                AddressingMode = AuthenticationAddressingMode.AddressingByName,
                ComponentName = CompName1,
                Method = AuthenticationMethods.Crc16,
                AuthenticationSeed = _twoByteHash,
                AuthenticationOffset = 0
            };

            _waiter.Reset();
            _target.Handle(input);

            // Look for results; should be disabled or unavailable.
            input = new SendAuthenticationInfoCommand
            {
                Action = AuthenticationAction.InterrogateAuthenticationStatus
            };

            var output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.AuthenticationCurrentlyInProgress, output.Status);

            // Disable our device.
            _componentRegistry.Object.Get(CompName1).Available = false;

            output = _target.Handle(input);

            Assert.AreEqual(AuthenticationStatus.ComponentDisabledOrUnavailable, output.Status);
            Assert.AreEqual(CrcHash, output.ComponentListCrc);
        }
    }
}
