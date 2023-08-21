namespace Aristocrat.Monaco.Kernel.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using log4net;
    using log4net.Config;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;

    /// <summary>
    ///     The unit test class for the default property provider class.
    /// </summary>
    [TestClass]
    public class DefaultPropertyProviderTests
    {
        /// <summary>
        ///     Gets the log file name
        /// </summary>
        public static string LogFileName
        {
            get
            {
                // Beacuse the log gets put in the same test results directory as the PropertiesManagerTests log
                // I gave them unique names based on their types
                var logFileName = MethodBase.GetCurrentMethod().DeclaringType + ".log";
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

            var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            logger.Info("Assembly Initialized");
        }

        /// <summary>
        ///     Setups for each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
        }

        /// <summary>
        ///     Test construction of a property provider
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            var provider = new DefaultPropertyProvider();

            // we should have one property in our collection from the addin file
            Assert.AreEqual(1, provider.GetCollection.Count);
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
        ///     Set a property that doesn't exist and verify it gets created.
        /// </summary>
        [TestMethod]
        public void TestSettingNewProperty()
        {
            var provider = new DefaultPropertyProvider();
            var value = 1; // test value to assign the property
            var propertyName = "New Property";

            provider.SetProperty(propertyName, value);

            // verify the property was created with the value
            Assert.AreEqual(value, provider.GetProperty(propertyName));
        }

        /// <summary>
        ///     Test getting a non-existent property and verify it returns null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(UnknownPropertyException))]
        public void TestGettingNonexistentProperty()
        {
            var provider = new DefaultPropertyProvider();
            provider.GetProperty("Non-Existent Property");
        }

        /// <summary>
        ///     Test setting an existing property to a new value.
        /// </summary>
        [TestMethod]
        public void TestSettingExistingProperty()
        {
            var provider = new DefaultPropertyProvider();
            var value = 1; // test value to assign the property
            var propertyName = "New Property";

            // create a new property and assign it a value
            provider.SetProperty(propertyName, value);

            // verify the property was created with the value
            Assert.AreEqual(value, provider.GetProperty(propertyName));

            // change the property value
            var newValue = 2;
            provider.SetProperty(propertyName, newValue);

            // verify the property was set to the new value
            Assert.AreEqual(newValue, provider.GetProperty(propertyName));
        }

        /// <summary>
        ///     Get the collection of properites and verify a key/value pair
        /// </summary>
        [TestMethod]
        public void TestGettingCollection()
        {
            var provider = new DefaultPropertyProvider();

            // a collection of key/value pairs to test with
            KeyValuePair<string, object>[] testProperties =
            {
                new KeyValuePair<string, object>("New Property", 1),
                new KeyValuePair<string, object>("Property2", "Hello"),
                new KeyValuePair<string, object>("MyProperty", 478.38f)
            };

            // add the properties to the provider
            foreach (var testProperty in testProperties)
            {
                provider.SetProperty(testProperty.Key, testProperty.Value);
            }

            // verify there are testProperties.Length items in the collection + 1 from addin
            Assert.AreEqual(testProperties.Length+1, provider.GetCollection.Count);

            var propertyCollection = provider.GetCollection;

            // verify the collection contains the key/value pairs we provided
            foreach (var testProperty in testProperties)
            {
                Assert.IsTrue(propertyCollection.Contains(testProperty));
            }
        }
    }
}
