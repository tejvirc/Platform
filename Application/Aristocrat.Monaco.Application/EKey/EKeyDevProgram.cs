namespace Aristocrat.Monaco.Application.EKey
{
    using System.Threading;
    using SmartCard;

#pragma warning disable 0162
    /// <summary>
    ///    Development EKey  program.
    /// </summary>
    internal class EKeyDevProgram : EKeyProgram
    {
        /// <inheritdoc />
        public override bool Run(SmartCardConnection connection, CancellationToken cancellation)
        {
#if (RETAIL)
            return false;
#endif
            return base.Run(connection, cancellation);

        }

        /// <inheritdoc />
        protected override string[] GetAuthTokens()
        {
            return new[] { "DEVELOPER" };
        }
    }
}
#pragma warning restore 0162
