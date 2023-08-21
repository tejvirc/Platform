namespace Aristocrat.Monaco.Application.EKey
{
    using System.Threading;
    using SmartCard;

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
#else
            return base.Run(connection, cancellation);
#endif
        }

        /// <inheritdoc />
        protected override string[] GetAuthTokens()
        {
            return new[] { "DEVELOPER" };
        }
    }
}
