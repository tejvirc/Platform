namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Windows.Controls;
    using Kernel;

    /// <summary>
    ///     Test tool plug-in event.
    /// </summary>
    public class TestToolPluginEvent : BaseEvent
    {
        /// <summary>
        ///     Construct <see cref="TestToolPluginEvent"/>.
        /// </summary>
        /// <param name="tab">Tab to add</param>
        public TestToolPluginEvent(TabItem tab)
        {
            Tab = tab;
        }

        /// <summary>
        ///     Get the tab.
        /// </summary>
        public TabItem Tab { get; }
    }
}
