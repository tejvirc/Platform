namespace Aristocrat.Monaco.Application.UI.Common
{
    /// <summary>The possible modes for the state</summary>
    public enum StateMode
    {
        /// <summary>Normal status</summary>
        Normal,

        /// <summary>Not initialized</summary>
        Uninitialized,

        /// <summary>Error</summary>
        Error,

        /// <summary>Printing, Inspecting, Initializing</summary>
        Processing,

        /// <summary>Warning </summary>
        Warning
    }
}
