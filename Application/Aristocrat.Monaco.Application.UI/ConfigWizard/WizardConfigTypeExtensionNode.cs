namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Mono.Addins;
    using System;

    [CLSCompliant(false)]
    public class WizardConfigTypeExtensionNode : TypeExtensionNode
    {
        /// <summary>
        ///     Specifies the optional order value for this wizard config page.
        /// </summary>
        [NodeAttribute("order", typeof(int), false)] private readonly int _order = -1;

        /// <summary>
        ///     Gets the order value
        /// </summary>
        public int Order => _order;
    }
}
