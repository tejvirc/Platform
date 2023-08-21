namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    /// <summary>
    ///     Contains tests for the LP6ESendAuthenticationInfoParser class
    /// </summary>
    [TestClass]
    public class LP6ESendAuthenticationInfoParserTest
    {
        private const byte LP6E = 0x6E;

        // Handler mock data
        private const ushort ListCrc = 0x1234;
        private const ushort NumComponents = 1;
        private const string CompName = "Comp1";
        private const long CompSize = 0x123456789;
        private const AuthenticationMethods AuthMethod = AuthenticationMethods.Crc32;
        private const uint Seed = 0x11223344; // proper length for CRC32
        private const ushort Offset = 10;
        private const uint Result = 0x11223344; // proper size for CRC32


        private readonly Mock<ISasLongPollHandler<SendAuthenticationInfoResponse, SendAuthenticationInfoCommand>> _handler =
            new Mock<ISasLongPollHandler<SendAuthenticationInfoResponse, SendAuthenticationInfoCommand>>();

        private readonly Collection<byte> _invalidCommandResponse = new Collection<byte>
        {
            TestConstants.SasAddress, LP6E,
            3, // content length
            Utilities.ToBinary(0, sizeof(ushort)), // unused list CRC
            AuthenticationStatus.InvalidCommand
        };


        private readonly Collection<byte> _knownComponentStatusResponse = new Collection<byte>
        {
            TestConstants.SasAddress, LP6E,
            17 + CompName.Length, // content length
            Utilities.ToBinary(ListCrc, sizeof(ushort)), // list CRC
            AuthenticationStatus.Success, // status
            CompName.Length, // length of name
            Encoding.UTF8.GetBytes(CompName),
            sizeof(ulong), // length of size
            Utilities.ToBinary(CompSize, sizeof(ulong)), // size
            Utilities.ToBinary((uint)AuthMethod, sizeof(uint)) // avail methods
        };

        private readonly Collection<byte> _unknownComponentStatusResponse = new Collection<byte>
        {
            TestConstants.SasAddress, LP6E,
            3, // content length
            Utilities.ToBinary(ListCrc, sizeof(ushort)), // list CRC
            AuthenticationStatus.ComponentDoesNotExist // status
        };

        private readonly Collection<byte> _knownAuthenticateResponse = new Collection<byte>
        {
            TestConstants.SasAddress, LP6E,
            3, // content length
            Utilities.ToBinary(ListCrc, sizeof(ushort)), // list CRC
            AuthenticationStatus.AuthenticationCurrentlyInProgress // status
        };

        private readonly Collection<byte> _knownAuthenticationStatusResponse = new Collection<byte>
        {
            TestConstants.SasAddress, LP6E,
            26 + CompName.Length, // content length
            Utilities.ToBinary(ListCrc, sizeof(ushort)), // list CRC
            AuthenticationStatus.AuthenticationComplete, // status
            CompName.Length, // length of name
            Encoding.UTF8.GetBytes(CompName),
            sizeof(ulong), // length of size
            Utilities.ToBinary(CompSize, sizeof(ulong)), // size
            Utilities.ToBinary((uint)AuthMethod, sizeof(uint)), // avail methods
            Utilities.ToBinary((uint)AuthMethod, sizeof(uint)), // used method
            sizeof(uint), // size of result
            Utilities.ToBinary(Result, sizeof(uint)) // result
        };


        private LP6ESendAuthenticationInfoParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            SetupHandlerMock();

            _target = new LP6ESendAuthenticationInfoParser();
            _target.InjectHandler(_handler.Object);
        }

        /// <summary>
        /// This won't go to handler
        /// </summary>
        [TestMethod]
        public void ParseRequestInvalidAction()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                1, // content length
                100, // action - invalid
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        /// <summary>
        /// This won't go to handler
        /// </summary>
        [TestMethod]
        public void ParseRequestInvalidLength()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                100, // content length - invalid
                AuthenticationAction.InterrogateNumberOfInstalledComponents,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        /// <summary>
        /// This won't go to handler
        /// </summary>
        [TestMethod]
        public void ParseRequestInvalidAddressingLength()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                4, // content length
                AuthenticationAction.ReadStatusOfComponent,
                AuthenticationAddressingMode.AddressingByIndex, // address mode
                100, // address length - invalid
                1, // address = index
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        /// <summary>
        /// This won't go to handler
        /// </summary>
        [TestMethod]
        public void ParseRequestInvalidComponentName()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                3 + CompName.Length, // content length
                AuthenticationAction.ReadStatusOfComponent,
                AuthenticationAddressingMode.AddressingByName, // address mode
                100, // address length - invalid
                Encoding.UTF8.GetBytes(CompName), // address = name
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        /// <summary>
        /// This won't go to handler
        /// </summary>
        [TestMethod]
        public void ParseRequestInvalidAddressingMode()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                3 + CompName.Length, // content length
                AuthenticationAction.ReadStatusOfComponent,
                55, // address mode - invalid
                CompName.Length, // address length - invalid
                Encoding.UTF8.GetBytes(CompName), // address = name
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        /// <summary>
        /// This won't go to handler
        /// </summary>
        [TestMethod]
        public void ParseRequestInvalidSeedLength()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                12 + CompName.Length, // content length
                AuthenticationAction.AuthenticateComponent,
                AuthenticationAddressingMode.AddressingByName, // address mode
                CompName.Length, // address length - invalid
                Encoding.UTF8.GetBytes(CompName), // address = name
                AuthMethod,
                100, // seed length - invalid,
                Utilities.ToBinary(Seed, sizeof(uint)),
                sizeof(ushort), // offset length
                Utilities.ToBinary(Offset, sizeof(ushort)),
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        /// <summary>
        /// This won't go to handler
        /// </summary>
        [TestMethod]
        public void ParseRequestInvalidOffsetLength()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                12 + CompName.Length, // content length
                AuthenticationAction.AuthenticateComponent,
                AuthenticationAddressingMode.AddressingByName, // address mode
                CompName.Length, // address length - invalid
                Encoding.UTF8.GetBytes(CompName), // address = name
                AuthMethod,
                sizeof(uint), // seed length
                Utilities.ToBinary(Seed, sizeof(uint)),
                3, // offset length = invalid
                Utilities.ToBinary(Offset, sizeof(ushort)),
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        /// <summary>
        /// This won't go to handler
        /// </summary>
        [TestMethod]
        public void ParseRequestTooLongOffsetLength()
        {
            UInt64 junk = 1;
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                20 + CompName.Length, // content length
                AuthenticationAction.AuthenticateComponent,
                AuthenticationAddressingMode.AddressingByName, // address mode
                CompName.Length, // address length - invalid
                Encoding.UTF8.GetBytes(CompName), // address = name
                AuthMethod,
                sizeof(uint), // seed length
                Utilities.ToBinary(Seed, sizeof(uint)),
                10, // offset length = valid per spec, but not for us
                Utilities.ToBinary(Offset, sizeof(ushort)),
                Utilities.ToBinary(junk, sizeof(ulong)), // 8 bytes
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        [TestMethod]
        public void ParseRequestCorrectInterrogateNumberComponents()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                1, // content length
                AuthenticationAction.InterrogateNumberOfInstalledComponents,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var expectedResponse = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                11, // content length
                Utilities.ToBinary(ListCrc, sizeof(ushort)), // list CRC
                AuthenticationStatus.InstalledComponentResponse, // status
                0, // 0-length name
                sizeof(ushort),
                Utilities.ToBinary(NumComponents, sizeof(ushort)), // size : number of components
                Utilities.ToBinary((uint)AuthenticationMethods.None, sizeof(uint)) // no avail methods
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(expectedResponse, actualResponse as ICollection);
        }

        [TestMethod]
        public void ParseRequestCorrectInterrogateComponentStatusByIndex()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                4, // content length
                AuthenticationAction.ReadStatusOfComponent,
                AuthenticationAddressingMode.AddressingByIndex, // address mode
                1, // address length
                1, // address = index
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_knownComponentStatusResponse, actualResponse as ICollection);
        }

        [TestMethod]
        public void ParseRequestCorrectInterrogateComponentStatusByName()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                3 + CompName.Length, // content length
                AuthenticationAction.ReadStatusOfComponent,
                AuthenticationAddressingMode.AddressingByName, // address mode
                CompName.Length, // address length
                Encoding.UTF8.GetBytes(CompName), // address = name
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_knownComponentStatusResponse, actualResponse as ICollection);
        }

        [TestMethod]
        public void ParseRequestIncorrectInterrogateComponentStatusByIndex()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                4, // content length
                AuthenticationAction.ReadStatusOfComponent,
                AuthenticationAddressingMode.AddressingByIndex, // address mode
                1, // address length
                50, // address = index (not valid)
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_unknownComponentStatusResponse, actualResponse as ICollection);
        }

        [TestMethod]
        public void ParseRequestIncorrectInterrogateComponentStatusByName()
        {
            var badName = "DEADFACE";
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                3 + badName.Length, // content length
                AuthenticationAction.ReadStatusOfComponent,
                AuthenticationAddressingMode.AddressingByName, // address mode
                badName.Length, // address length
                Encoding.UTF8.GetBytes(badName), // address = name
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_unknownComponentStatusResponse, actualResponse as ICollection);
        }

        [TestMethod]
        public void ParseRequestCorrectAuthenticate()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                13, // content length
                AuthenticationAction.AuthenticateComponent,
                AuthenticationAddressingMode.AddressingByIndex, // address mode
                1, // address length
                1, // address = index
                AuthMethod,
                sizeof(uint), // seed length
                Utilities.ToBinary(Seed, sizeof(uint)),
                sizeof(ushort), // offset length
                Utilities.ToBinary(Offset, sizeof(ushort)),
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_knownAuthenticateResponse, actualResponse as ICollection);
        }

        [TestMethod]
        public void ParseRequestCorrectAuthenticationStatus()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                1, // content length
                AuthenticationAction.InterrogateAuthenticationStatus,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_knownAuthenticationStatusResponse, actualResponse as ICollection);
        }


        [TestMethod]
        public void ParseRequestZeroAddressLength()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, LP6E,
                12, // content length
                AuthenticationAction.AuthenticateComponent,
                AuthenticationAddressingMode.AddressingByIndex, // address mode
                0, // address length zero
                32, // address = index
                0,
                0, // seed length
                0,
                2, // offset length
                0,
                0,
                1,
                0,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
            var actualResponse = _target.Parse(longPoll);
            CollectionAssert.AreEqual(_invalidCommandResponse, actualResponse as ICollection);
        }

        private void SetupHandlerMock()
        {
            // interrogate number of components ==> 1 component
            _handler.Setup(m => m.Handle(It.Is<SendAuthenticationInfoCommand>(c => c.Action == AuthenticationAction.InterrogateNumberOfInstalledComponents)))
                .Returns(new SendAuthenticationInfoResponse
                {
                    Status = AuthenticationStatus.InstalledComponentResponse,
                    ComponentListCrc = ListCrc,
                    ComponentSize = NumComponents,
                    AvailableMethods = AuthenticationMethods.None
                });

            // interrogate component 1 status, either by index or name => its name, size, allowed method, etc
            var goodResponseComponentStatus = new SendAuthenticationInfoResponse
            {
                Status = AuthenticationStatus.Success,
                ComponentListCrc = ListCrc,
                ComponentName = CompName,
                ComponentSize = CompSize,
                AvailableMethods = AuthMethod
            };
            _handler.Setup(m => m.Handle(It.Is<SendAuthenticationInfoCommand>(c => c.Action == AuthenticationAction.ReadStatusOfComponent &&
                                                                                   c.AddressingMode == AuthenticationAddressingMode.AddressingByIndex &&
                                                                                   c.ComponentIndex == NumComponents)))
                .Returns(goodResponseComponentStatus);
            _handler.Setup(m => m.Handle(It.Is<SendAuthenticationInfoCommand>(c => c.Action == AuthenticationAction.ReadStatusOfComponent &&
                                                                                   c.AddressingMode == AuthenticationAddressingMode.AddressingByName &&
                                                                                   c.ComponentName == CompName)))
                .Returns(goodResponseComponentStatus);

            // interrogate invalid component status, either by index or name => invalid component
            var badResponseComponentStatus = new SendAuthenticationInfoResponse
            {
                Status = AuthenticationStatus.ComponentDoesNotExist,
                ComponentListCrc = ListCrc,
                ComponentName = CompName,
                ComponentSize = CompSize,
                AvailableMethods = AuthMethod
            };
            _handler.Setup(m => m.Handle(It.Is<SendAuthenticationInfoCommand>(c => c.Action == AuthenticationAction.ReadStatusOfComponent &&
                                                                                   c.AddressingMode == AuthenticationAddressingMode.AddressingByIndex &&
                                                                                   c.ComponentIndex != NumComponents)))
                .Returns(badResponseComponentStatus);
            _handler.Setup(m => m.Handle(It.Is<SendAuthenticationInfoCommand>(c => c.Action == AuthenticationAction.ReadStatusOfComponent &&
                                                                                   c.AddressingMode == AuthenticationAddressingMode.AddressingByName &&
                                                                                   c.ComponentName != CompName)))
                .Returns(badResponseComponentStatus);

            // request authentication of component => authentication started
            _handler.Setup(m => m.Handle(It.Is<SendAuthenticationInfoCommand>(c => c.Action == AuthenticationAction.AuthenticateComponent &&
                                                                                   c.AddressingMode == AuthenticationAddressingMode.AddressingByIndex &&
                                                                                   c.ComponentIndex == NumComponents)))
                .Returns(new SendAuthenticationInfoResponse
                {
                    Status = AuthenticationStatus.AuthenticationCurrentlyInProgress,
                    ComponentListCrc = ListCrc
                });

            // request authentication status => authentication data
            _handler.Setup(
                    m => m.Handle(
                        It.Is<SendAuthenticationInfoCommand>(
                            c => c.Action == AuthenticationAction.InterrogateAuthenticationStatus)))
                .Returns(new SendAuthenticationInfoResponse
                {
                    Status = AuthenticationStatus.AuthenticationComplete,
                    ComponentListCrc = ListCrc,
                    ComponentName = CompName,
                    ComponentSize = CompSize,
                    AvailableMethods = AuthMethod,
                    Method = AuthMethod,
                    AuthenticationData = Utilities.ToBinary(Result, sizeof(uint))
                });
        }
    }
}
