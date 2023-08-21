namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    /// <summary>
    ///     Localize options.
    /// </summary>
    public enum LocalizeOptions
    {
        /// <summary>Use provider current culture.</summary>
        None,

        /// <summary>Use the invariant culture.</summary>
        UseInvariant,

        /// <summary>Use current culture outside of the scope.</summary>
        UseCurrent
    }
}
