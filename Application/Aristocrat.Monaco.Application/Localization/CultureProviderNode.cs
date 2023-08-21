namespace Aristocrat.Monaco.Application.Localization
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Extension node use to discover culture providers.
    /// </summary>
    [ExtensionNode("CultureProvider")]
    [CLSCompliant(false)]
    public class CultureProviderNode : TypeExtensionNode
    {
    }
}
