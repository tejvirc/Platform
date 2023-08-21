namespace TestExtensionCore
{
    /// <summary>
    ///     Definition of the IAddinInterface interface.
    /// </summary>
    public interface ITest
    {
        /// <summary>
        ///     FxCop complains if you don't have a method in the interface,
        ///     even though we don't need one for this test purpose.
        /// </summary>
        void SilenceFxCop();
    }
}