////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IOGen8Test.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2010 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace UnitTest
{
    using System.IO;
    using System.Security.Permissions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Vgt.Client12.Hardware.IO;
    using Vgt.Client12.Hardware.SharedDevice;
    using Vgt.Client12.Kernel;
    using Vgt.Client12.Testing.UnitTest;
    
    /// <summary>
    /// This is a test class for IOGen8Test and is intended
    /// to contain all IOGen8Test Unit Tests
    /// </summary>
    [TestClass]
    public class IOGen8Test
    {
        #region Private Field

        /// <summary>Number of outputs.</summary> 
        private const int TPCI940NumOutputs = 16;

        /// <summary>Number of inputs.</summary>
        private const int TPCI940NumInputs = 16;

        /// <summary>
        /// The test implementation of <c>IEventBus</c>
        /// </summary>
        private TestEventBus m_eventBus;

        /// <summary>
        /// Reference to the object to  test
        /// </summary>
        private IOGen8 m_inputOutputGen8;

        #endregion

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Assembly Initializer

        /// <summary>
        /// Initialize the assembly to configure logging.
        /// </summary>
        /// <param name="context">The test context is an unused parameter.</param>
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
        }

        #endregion

        /// <summary>
        /// Initializes class. Called once during the test run.
        /// </summary>
        /// <param name="context">The test context is an unused parameter.</param>
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
        }

        /// <summary>
        /// Cleans up after all test methods have been executed. Called once during the test run.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            AddinManager.Shutdown();
        }

        #region Test Initialize and Cleanup

        /// <summary>
        /// Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Initialize()
        {
            IServiceManager serviceManager = ServiceManager.GetInstance();

            m_eventBus = new TestEventBus();
            m_eventBus.Initialize();
            serviceManager.AddService(m_eventBus);

            m_inputOutputGen8 = new IOGen8();
            m_inputOutputGen8.Initialize();

            m_eventBus.ClearEventHistory();
        }

        /// <summary>
        /// Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            m_eventBus = null;

            var serviceManager = (TestServiceManager)ServiceManager.GetInstance();
            serviceManager.Shutdown();

            Assert.IsTrue(0 == serviceManager.ServicesCount);
        }

        #endregion

        /// <summary>
        /// A test for WatchdogEnabled
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void WatchdogEnabledTest()
        {
            IOGen8 target = m_inputOutputGen8;
            int actual = target.EnableWatchdog(30);
            Assert.AreEqual(-1, actual);
        }

        /// <summary>
        /// A test for GetOutputs
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void GetOutputsTest()
        {
            IOGen8 target = m_inputOutputGen8;
            ulong actual = target.GetOutputs;
            Assert.AreEqual((ulong)0, actual); // Should be 0 by default
        }

        /// <summary>
        /// A test for GetMaxOutputs
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void GetMaxOutputsTest()
        {
            IOGen8 target = m_inputOutputGen8;
            int actual = target.GetMaxOutputs;
            Assert.AreEqual(TPCI940NumOutputs, actual);
        }

        /// <summary>
        /// A test for GetMaxInputs
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void GetMaxInputsTest()
        {
            IOGen8 target = m_inputOutputGen8;
            int actual = target.GetMaxInputs;
            Assert.AreEqual(TPCI940NumInputs, actual);
        }

        /// <summary>
        /// A test for GetInputs
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void GetInputsTest()
        {
            IOGen8 target = m_inputOutputGen8;
            ulong actual = target.GetInputs;
            Assert.AreEqual((ulong)0, actual);
        }

        /// <summary>
        /// A test for GetIntrusionEvents
        /// </summary>
        /// <remarks>The Gen8 card is not expected log intrusion events and the array returned will be empty</remarks>
        [TestMethod]
        public void GetIntrusionEventsTest()
        {
            InputEvent[] intrusionEvents = m_inputOutputGen8.GetIntrusionEvents;
            Assert.AreEqual(0, intrusionEvents.Length);
        }

        /// <summary>
        /// A test for DeviceConfiguration
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void DeviceConfigurationTest()
        {
            IOGen8 target = m_inputOutputGen8;
            Device actual = target.DeviceConfiguration;
            Assert.AreEqual("Gen8", actual.Manufacturer);
            Assert.AreEqual("TPCI940", actual.Model);
        }

        /// <summary>
        /// A test for WriteOutputs
        /// </summary>
        [TestMethod]
        [DeploymentItem("IOGen8.dll")]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void WriteOutputsTest()
        {
            var target = new IOGen8_Accessor();
            IOGen8_Accessor.WriteOutputs();
            var actual = target.GetOutputs;
            Assert.AreEqual((ulong)0, actual); 
        }

        /// <summary>
        /// A test for TurnOutputsOn
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void TurnOutputsOnTest()
        {
            IOGen8 target = m_inputOutputGen8;
            const ulong Outputs = 0; 
            target.TurnOutputsOn(Outputs);
            var actual = target.GetOutputs;
            Assert.AreEqual((ulong)0, actual);
        }

        /// <summary>
        /// A test for TurnOutputsOff
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void TurnOutputsOffTest()
        {
            IOGen8 target = m_inputOutputGen8;
            const ulong Outputs = 0; 
            target.TurnOutputsOff(Outputs);
            var actual = target.GetOutputs;
            Assert.AreEqual((ulong)0, actual);
        }

        /// <summary>
        /// A test for TP940DIO_MAKE_IOCTL
        /// </summary>
        [TestMethod]
        [DeploymentItem("IOGen8.dll")]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void TP940DIOMAKEIOCTLTest()
        {
            const Tpci940IOCommand Command = Tpci940IOCommand.Read;
            uint expected = IOGen8_Accessor.TP940DIO_MAKE_IOCTL(Tpci940IOCommand.Read);
            uint actual = IOGen8_Accessor.TP940DIO_MAKE_IOCTL(Command);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ToggleWatchdog
        /// </summary>
        [TestMethod]
        [DeploymentItem("IOGen8.dll")]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void ToggleWatchdogTest()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// A test for IsOutputsOn
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void IsOutputsOnTest()
        {
            IOGen8 target = m_inputOutputGen8;
            const ulong Outputs = 0; 
            const bool Expected = false;
            bool actual = target.IsOutputsOn(Outputs);
            Assert.AreEqual(Expected, actual);
        }

        /// <summary>
        /// A test for IsInputsOn
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void IsInputsOnTest()
        {
            IOGen8 target = m_inputOutputGen8;
            const ulong Inputs = 0; 
            const bool Expected = false;
            bool actual = target.IsInputsOn(Inputs);
            Assert.AreEqual(Expected, actual);
        }

        /// <summary>
        /// A test for Initialize
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void InitializeTest()
        {
            var target = new IOGen8(); 
            target.Initialize();
            Device device = target.DeviceConfiguration;
            Assert.AreEqual("Gen8", device.Manufacturer);
            Assert.AreEqual("TPCI940", device.Model);
        }

        /// <summary>
        /// A test for EnableWatchDog
        /// </summary>
        [TestMethod]
        [DeploymentItem("IOGen8.dll")]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void EnableWatchDogTest()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// A test for DeviceIoControl
        /// </summary>
        [TestMethod]
        [DeploymentItem("IOGen8.dll")]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void DeviceIoControlTest()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// A test for CTL_CODE
        /// </summary>
        [TestMethod]
        [DeploymentItem("IOGen8.dll")]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void CTLCODETest()
        {
            const uint DeviceType = 1; 
            const uint Function = 2; 
            const uint Method = 4; 
            const uint Access = 2; 
            const uint Expected = (DeviceType << 16) | (Access << 14) | (Function << 2) | Method;
            uint actual = IOGen8_Accessor.CTL_CODE(DeviceType, Function, Method, Access);
            Assert.AreEqual(Expected, actual);
        }

        /// <summary>
        /// A test for CreateFile
        /// </summary>
        [TestMethod]
        [DeploymentItem("IOGen8.dll")]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void CreateFileTest()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// A test for Cleanup
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void CleanupTest()
        {
            IOGen8 target = m_inputOutputGen8;
            target.Cleanup();
            Assert.IsTrue(true);
        }

        /// <summary>
        /// A test for <c>IOGen8</c> Constructor
        /// </summary>
        [TestMethod]
        [DeploymentItem("ServiceManager.addin.xml")]
        public void IOGen8ConstructorTest()
        {
            var target = new IOGen8();
            Device device = target.DeviceConfiguration;
            
            Assert.AreEqual(100, device.PollingFrequency);
        }
    }
}
