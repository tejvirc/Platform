namespace Aristocrat.Monaco.Gaming.Commands
{
    using Runtime.Client;

    /// <summary>
    ///     Runtime Flag Changed command
    /// </summary>
    public class RuntimeFlagChanged
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeFlagChanged" /> class.
        /// </summary>
        /// <param name="condition">The flag that changed</param>
        /// <param name="state">The new state</param>
        public RuntimeFlagChanged(RuntimeCondition condition, bool state)
        {
            Condition = condition;
            State = state;
        }

        /// <summary>
        ///     Gets the condition that changed
        /// </summary>
        public RuntimeCondition Condition { get; }

        /// <summary>
        ///     Gets the new state
        /// </summary>
        public bool State { get; }
    }
}
