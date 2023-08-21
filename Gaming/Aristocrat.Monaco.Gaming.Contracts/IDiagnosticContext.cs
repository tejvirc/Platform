namespace Aristocrat.Monaco.Gaming.Contracts
{
    using ProtoBuf;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to build a list of parameters to be passed to the runtime for diagnostics like (combo test and
    ///     replay)
    /// </summary>
    [ProtoContract]
    [ProtoInclude(1, typeof(GameDiagnosticsCompletedEvent))]
    public interface IDiagnosticContext
    {
        /// <summary>
        ///     Builds a collection of key value pairs that defines the parameters to be passed to the runtime
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, string> GetParameters();
    }

    /// <summary>
    ///     Provides a mechanism to build a list of parameters to be passed to the runtime for diagnostics like (combo test and
    ///     replay)
    /// </summary>
    public interface IDiagnosticContext<out TArguments> : IDiagnosticContext
        where TArguments : class
    {
        /// <summary>
        ///     Gets the arguments for the diagnostics for this session
        /// </summary>
        TArguments Arguments { get; }
    }
}