namespace Aristocrat.Monaco.Kernel.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using log4net;
    using log4net.Config;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Tests for the PropertiesManager class.
    /// </summary>
    [TestClass]
    public class PropertiesManagerTests
    {
        /// <summary>
        ///     A mock IEventBus object.
        /// </summary>
        private Mock<IEventBus> _eventBus;

        /// <summary>
        ///     A collection of the events posted to the event bus
        /// </summary>
        List<PropertyChangedEvent> _postedEvents = new List<PropertyChangedEvent>();

        /// <summary>
        ///     Gets the log file name
        /// </summary>
        public static string LogFileName
        {
            get
            {
                // Beacuse the log gets put in the same test results directory as the DefaultPropertyProviderTests log
                // I gave them unique names based on their types
                var logFileName = MethodBase.GetCurrentMethod()!.DeclaringType + ".log";
                return logFileName;
            }
        }

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Initialize the class
        /// </summary>
        /// <param name="context">The test context is an unused parameter</param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Building the log4net configuration in memory to avoid dealing with a separate configuration file
            // while running the unit tests
            var builder = new StringBuilder();
            builder.Append("<log4net>");
            builder.Append(@"<appender name=""LogFileAppender"" type=""log4net.Appender.FileAppender"">");
            builder.AppendFormat(@"<file value=""{0}"" />", LogFileName);
            builder.Append(@"<appendToFile value=""true"" />");
            builder.Append(@"<layout type=""log4net.Layout.PatternLayout"">");
            builder.Append(@"<conversionPattern value=""%level %logger - %message%newline"" />");
            builder.Append(@"</layout>");
            builder.Append(@"</appender>");
            builder.Append(@"<!-- Set root logger level to DEBUG and its only appender to A1 -->");
            builder.Append(@"<root>");
            builder.Append(@"<level value=""ALL"" />");
            builder.Append(@"<appender-ref ref=""LogFileAppender"" />");
            builder.Append(@"</root>");
            builder.Append(@"</log4net>");

            var loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            Stream s = new MemoryStream(Encoding.Default.GetBytes(builder.ToString()));
            XmlConfigurator.Configure(loggerRepository, s);

            var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

            logger.Info("Class Initialized");
        }

        /// <summary>
        ///     Setups for each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(m => m.Publish(It.IsAny<PropertyChangedEvent>()))
                .Callback<object>(theEvent => _postedEvents.Add(theEvent as PropertyChangedEvent));
        }

        /// <summary>
        ///     Cleans up after each test
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

        /// <summary>
        ///     Test to verify that the log file has been created
        /// </summary>
        [TestMethod]
        public void VerifyLog()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var fileExists = File.Exists(currentDirectory + Path.DirectorySeparatorChar + LogFileName);
            Assert.IsTrue(fileExists);
        }

        /// <summary>
        ///     Test construction and verify the collection is empty
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var propertiesManager = new PropertiesManager();
            Assert.IsNotNull(propertiesManager);
        }

        /// <summary>
        ///     Test to get coverage of the Dispose method
        /// </summary>
        [TestMethod]
        public void DisposeTest()
        {
            var propertiesManager = new PropertiesManager();

            propertiesManager.Dispose();

            // after Dispose the properties should be cleared
            Assert.AreEqual(0, propertiesManager.Count);
        }

        /// <summary>
        ///     Test adding a property provider and ensure that it's
        ///     properties are entered in our collection
        /// </summary>
        [TestMethod]
        public void AddPropertyProviderTest()
        {
            var propertiesManager = new PropertiesManager();
            Assert.AreEqual(1, propertiesManager.Count);

            // mock a provider with a few properties
            List<KeyValuePair<string, object>> properties = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Property1", 111),
                new KeyValuePair<string, object>("Property2", "abc"),
                new KeyValuePair<string, object>("Property3", 4.5f)
            };
            Mock<IPropertyProvider> provider = new Mock<IPropertyProvider>(MockBehavior.Strict);
            provider.SetupGet(m => m.GetCollection).Returns(properties);

            // add the provider
            propertiesManager.AddPropertyProvider(provider.Object);

            // verify we now have 3 properties in our list + 1 from addin
            Assert.AreEqual(4, propertiesManager.Count);
            Assert.AreEqual(4, _postedEvents.Count);
            Assert.AreEqual("Property1", _postedEvents[1].PropertyName);
            Assert.AreEqual("Property2", _postedEvents[2].PropertyName);
            Assert.AreEqual("Property3", _postedEvents[3].PropertyName);
        }

        /// <summary>
        ///     Test setting a property that hasn't been defined.
        ///     Should increase our property count by 1
        /// </summary>
        [TestMethod]
        public void SetUnknownPropertyTest()
        {
            var propertiesManager = new PropertiesManager();
            var propertyValue = 3;
            var fakeDefaultValue = 11;
            var propertyName = "MyProperty";

            // write the new property
            propertiesManager.SetProperty(propertyName, propertyValue);

            // verify we now have 1 property in our list + 1 from addin
            Assert.AreEqual(2, propertiesManager.Count);
            Assert.AreEqual(propertyValue, propertiesManager.GetProperty(propertyName, fakeDefaultValue));

            // The SetProperty() call should have generated an event
            Assert.AreEqual(2, _postedEvents.Count);
            Assert.AreEqual(propertyName, _postedEvents[1].PropertyName);
        }

        /// <summary>
        ///     Test setting a known, non-null property to a new value.
        /// </summary>
        [TestMethod]
        public void SetKnownPropertyTest()
        {
            var propertiesManager = new PropertiesManager();
            var propertyValue = 3;
            var propertyName = "MyProperty";

            // write the property
            propertiesManager.SetProperty(propertyName, propertyValue);

            // set up a default value that is different from our expected property value
            var defaultValue = 2;

            // set it to a new value
            propertyValue = 10;
            propertiesManager.SetProperty(propertyName, propertyValue);

            // verify it reads back ok
            Assert.AreEqual(propertyValue, propertiesManager.GetProperty(propertyName, defaultValue));

            // Both SetProperty() calls should have generated an event
            Assert.AreEqual(3, _postedEvents.Count);
            Assert.AreEqual(propertyName, _postedEvents[1].PropertyName);
            Assert.AreEqual(propertyName, _postedEvents[2].PropertyName);
        }

        /// <summary>
        ///     Test setting a known, null property to a new value.
        /// </summary>
        [TestMethod]
        public void SetKnownPropertyWhenOldValueIsNullTest()
        {
            var propertiesManager = new PropertiesManager();
            object propertyValue = null;
            var propertyName = "MyProperty";

            // write the property
            propertiesManager.SetProperty(propertyName, propertyValue);

            // set up a default value that is different from our expected property value
            var defaultValue = 2;

            // set it to a new value
            propertyValue = 10;
            propertiesManager.SetProperty(propertyName, propertyValue);

            // verify it reads back ok
            Assert.AreEqual(propertyValue, propertiesManager.GetProperty(propertyName, defaultValue));

            // Both SetProperty() calls should have generated an event
            Assert.AreEqual(3, _postedEvents.Count);
            Assert.AreEqual(propertyName, _postedEvents[1].PropertyName);
            Assert.AreEqual(propertyName, _postedEvents[2].PropertyName);
        }

        /// <summary>
        ///     Test setting a known property to the same value.
        /// </summary>
        [TestMethod]
        public void SetKnownPropertyToSameValueTest()
        {
            var propertiesManager = new PropertiesManager();
            var propertyValue = 3;
            var propertyName = "MyProperty";

            // write the property
            propertiesManager.SetProperty(propertyName, propertyValue);

            // set up a default value that is different from our expected property value
            var defaultValue = 2;

            propertiesManager.SetProperty(propertyName, propertyValue);

            // verify it reads back ok
            Assert.AreEqual(propertyValue, propertiesManager.GetProperty(propertyName, defaultValue));

            // Only the first SetProperty() call should have generated an event
            Assert.AreEqual(2, _postedEvents.Count);
            Assert.AreEqual(propertyName, _postedEvents[1].PropertyName);
        }

        /// <summary>
        ///     Test setting a property that hasn't been defined.
        ///     Should increase our property count by 1
        /// </summary>
        [TestMethod]
        public void SetPropertyNotIsConfigureFalseTest()
        {
            var propertiesManager = new PropertiesManager();
            var propertyValue = 3;
            var fakeDefaultValue = 11;
            var isConfiguration = false;
            var propertyName = "MyProperty";

            // write the new property
            propertiesManager.SetProperty(propertyName, propertyValue, isConfiguration);

            // verify we now have 1 property in our list + 1 from addin
            Assert.AreEqual(2, propertiesManager.Count);
            Assert.AreEqual(propertyValue, propertiesManager.GetProperty(propertyName, fakeDefaultValue));

            // The SetProperty() call should have generated an event
            Assert.AreEqual(2, _postedEvents.Count);
            Assert.AreEqual(propertyName, _postedEvents[1].PropertyName);
        }

        /// <summary>
        ///     Test setting a property that hasn't been defined.
        ///     Should increase our property count by 1
        /// </summary>
        [TestMethod]
        public void SetPropertyNotIsConfigureTrueTest()
        {
            var propertiesManager = new PropertiesManager();
            var propertyValue = 3;
            var fakeDefaultValue = 11;
            var isConfiguration = true;
            var propertyName = "MyProperty";

            // write the new property
            propertiesManager.SetProperty(propertyName, propertyValue, isConfiguration);

            // verify we now have 1 property in our list + 1 from addin
            Assert.AreEqual(2, propertiesManager.Count);
            Assert.AreEqual(propertyValue, propertiesManager.GetProperty(propertyName, fakeDefaultValue));

            // The SetProperty() call should have generated an event
            Assert.AreEqual(2, _postedEvents.Count);
            Assert.AreEqual(propertyName, _postedEvents[1].PropertyName);
        }

        /// <summary>
        ///     Test getting an existing property
        /// </summary>
        [TestMethod]
        public void GetGoodPropertyTest()
        {
            var propertiesManager = new PropertiesManager();
            var propertyValue = 3;
            var propertyName = "MyProperty";

            // write the property
            propertiesManager.SetProperty(propertyName, propertyValue);

            // set up a default value that is different from our expected property value
            var defaultValue = 2;

            // verify it reads back ok
            Assert.AreEqual(propertyValue, propertiesManager.GetProperty(propertyName, defaultValue));
        }

        /// <summary>
        ///     Test getting an unknown property.
        ///     The default value from GetProperty should be returned.
        /// </summary>
        [TestMethod]
        public void GetUnknownPropertyTest()
        {
            var propertiesManager = new PropertiesManager();
            var defaultValue = 3;
            var propertyName = "FakeProperty";
            Assert.AreEqual(defaultValue, propertiesManager.GetProperty(propertyName, defaultValue));
        }

        /// <summary>
        ///     Test adding collections with duplicate keys
        /// </summary>
        [TestMethod]
        public void TestDuplicateKeys()
        {
            var propertiesManager = new PropertiesManager();
            var provider1 = new DefaultPropertyProvider();
            var provider2 = new DefaultPropertyProvider();
            var propertyName = "New Property";

            var firstPropertyValue = 1; // test value to assign the first property
            var secondPropertyValue = 3; // test value to assign the second property

            // assign the values to the same keys in different property providers
            provider1.SetProperty(propertyName, firstPropertyValue);
            provider2.SetProperty(propertyName, secondPropertyValue);

            // add the first provider
            propertiesManager.AddPropertyProvider(provider1);

            // add the second
            propertiesManager.AddPropertyProvider(provider2);

            // The value should be the second provider's value
            Assert.AreEqual(secondPropertyValue, propertiesManager.GetProperty(propertyName, null));
        }

        /// <summary>
        ///     Verify the service name is what was expected
        /// </summary>
        [TestMethod]
        public void NameTest()
        {
            var propertiesManager = new PropertiesManager();
            Assert.AreEqual("Properties Manager", propertiesManager.Name);
        }

        /// <summary>
        ///     Verify the service type is what was expected
        /// </summary>
        [TestMethod]
        public void ServiceTypesTest()
        {
            var propertiesManager = new PropertiesManager();
            Assert.AreEqual(1, propertiesManager.ServiceTypes.Count);
            Assert.IsTrue(propertiesManager.ServiceTypes.Contains(typeof(IPropertiesManager)));
        }

        /// <summary>
        ///     Test for the Initialize method
        /// </summary>
        [TestMethod]
        public void InitializeTest()
        {
            var propertiesManager = new PropertiesManager();

            // doesn't do anything since the method is empty. This is for code coverage
            propertiesManager.Initialize();
        }
    }
}
