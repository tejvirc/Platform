namespace Aristocrat.Monaco.Application.Tests.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Application.Authentication;
    using Application.Contracts.Authentication;
    using Common.Storage;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ComponentHashServiceTest
    {
        private static readonly Component TestModule = new Component
        {
            Available = true,
            ComponentId = "ATI_Test_Module",
            FileSystemType = FileSystemType.File,
            Path = "C:\\TestingOSPath\\ATI_Test_Module.iso",
            Size = 1024,
            Type = ComponentType.Module
        };

        private static readonly Component TestOs = new Component
        {
            Available = true,
            ComponentId = "ATI_Test_OS",
            FileSystemType = FileSystemType.File,
            Path = "C:\\TestingOSPath",
            Size = 1024,
            Type = ComponentType.OS
        };

        private static readonly Component TestBoot = new Component
        {
            Available = true,
            ComponentId = "ATI_Test_Boot",
            FileSystemType = FileSystemType.File,
            Path = "C:\\TestingBootPath",
            Size = 1024,
            Type = ComponentType.OS
        };

        private static readonly IList<Component> TestComponents = new List<Component> { TestOs, TestBoot, TestModule };

        private Component _component;
        private Mock<IComponentRegistry> _componentRegistry;

        private ComponentVerification _componentVerification;
        private Mock<IEventBus> _eventBus;
        private Mock<IFileSystemProvider> _fileSystem = new Mock<IFileSystemProvider>();

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _componentRegistry = new Mock<IComponentRegistry>();

            _component = new Component();
            _componentVerification = new ComponentVerification();
            _eventBus = new Mock<IEventBus>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullComponentHashCalculatorExpectException()
        {
            var service = new AuthenticationService(null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var service = new AuthenticationService(_componentRegistry.Object, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCalculateHashWithNullComponentExpectException()
        {
            var service = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            service.CalculateHash(null, _componentVerification);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCalculateHashWithNullComponentVerificationExpectException()
        {
            var service = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            service.CalculateHash(_component, null);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void WhenCalculateHashWithNotSupportedAlgorithmExpectException()
        {
            _componentVerification.AlgorithmType = (AlgorithmType)10000;
            var service = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            service.CalculateHash(_component, _componentVerification);
        }

        [TestMethod]
        public void WhenCalculateHashWithHardwareFileSystemTypeExpectSuccess()
        {
            _component.FileSystemType = FileSystemType.File;
            _component.Path = "Bad.config.xml";
            _componentVerification.AlgorithmType = AlgorithmType.Sha512;

            _componentVerification.Salt = ConvertExtensions.FromPackedHexString("FFFF");

            var service = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            service.CalculateHash(
                _component,
                _componentVerification);
        }

        [TestMethod]
        public void WhenCalculateHashWithInvalidSaltExpectSuccess()
        {
            _component.FileSystemType = FileSystemType.File;
            _component.Path = "Bad.config.xml";
            _componentVerification.AlgorithmType = AlgorithmType.Sha512;

            _componentVerification.Salt = ConvertExtensions.FromPackedHexString("FFF");

            var service = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            service.CalculateHash(
                _component,
                _componentVerification);
        }
    }
}
