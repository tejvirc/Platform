namespace TestExtension1
{
    using TestExtensionCore;

    /// <summary>
    ///     Bootstrap unit test class that implements an interface exposed as an extension point.
    /// </summary>
    public class TestClass2 : ITest
    {
        /// <summary>
        ///     FxCop complains if you don't have a method in the interface,
        ///     even though we don't need one for this test purpose.
        /// </summary>
        public void SilenceFxCop()
        {
        }
    }
}