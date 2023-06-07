namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    using System;
    using Mono.Addins;

    #region Usings

    #endregion

    /// <summary>
    ///     Definition of the CustomExtensionNode class.
    /// </summary>
    [ExtensionNode("CustomExtension")]
    public class CustomExtensionNode : ExtensionNode
    {
        //// fields are initialized by MonoAddins
#pragma warning disable 0649

        /// <summary>
        ///     simple data for testing
        /// </summary>
        [NodeAttribute("data")] private string data;

#pragma warning restore 0649

        /// <summary>
        ///     Gets the string data of this
        /// </summary>
        public string Data
        {
            get { return data; }
        }
    }
}