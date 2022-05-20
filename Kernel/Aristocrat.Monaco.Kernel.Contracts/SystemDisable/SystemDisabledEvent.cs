namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Globalization;

    /// <summary>
    ///     This event is posted whenever the system transitions from being enabled to disabled.
    ///     This event is not posted if the system is disabled when already disabled.
    /// </summary>
    /// <seealso cref="SystemEnabledEvent" />
    /// <seealso cref="ISystemDisableManager" />
    [Serializable]
    public class SystemDisabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisabledEvent" /> class.
        /// </summary>
        public SystemDisabledEvent()
        {
            Priority = SystemDisablePriority.Normal;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisabledEvent" /> class.
        /// </summary>
        /// <param name="priority">The priority of the disable.</param>
        public SystemDisabledEvent(SystemDisablePriority priority)
        {
            Priority = priority;
        }

        /// <summary>
        ///     Gets the priority of the disable.
        /// </summary>
        public SystemDisablePriority Priority { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"{GetType().Name} (EGM disabled)");
        }
    }
}
