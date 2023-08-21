namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using Mono.Addins;
    using System;

    [CLSCompliant(false)]
    public class OperatorMenuTypeExtensionNode : TypeExtensionNode
    {
        /// <summary>
        ///     Specifies the optional order value for this operator menu page.
        /// </summary>
        [NodeAttribute("order", typeof(int), false)] private readonly int _order = -1;

        /// <summary>
        ///     Gets the order value
        /// </summary>
        public int Order => _order;
    }
}