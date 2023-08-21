namespace Aristocrat.Monaco.Application.EKey
{
    /// <summary>
    ///    Production EKey program.
    /// </summary>
    internal class EKeyProdProgram : EKeyProgram
    {
        /// <inheritdoc />
        protected override string[] GetAuthTokens()
        {
            return new[] { "EKEY AUTHORIZATION" };
        }
    }
}
