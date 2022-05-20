namespace Aristocrat.Monaco.G2S.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    /// <summary>
    ///     Initializer for test context.
    /// </summary>
    [TestClass]
    public class TestInitializer
    {
        /// <summary>
        ///     Initializes the test context.
        /// </summary>
        [AssemblyInitialize]
        public static void InitTestContext(TestContext context)
        {
            ModelMappingRules.Initialize();
            ConfigureJsonConverter();
        }

        private static void ConfigureJsonConverter()
        {
            JsonConvert.DefaultSettings =
                () =>
                    new JsonSerializerSettings
                    {
                        SerializationBinder = new DisplayNameSerializationBinder(),
                        TypeNameHandling = TypeNameHandling.Auto
                    };
        }
    }
}