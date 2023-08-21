namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Globalization;

    /// <summary>
    ///     This event is posted whenever the system transitions from being disabled to enabled.
    ///     This event is not posted if the system is enabled when already enabled.
    /// </summary>
    /// <seealso cref="SystemDisabledEvent" />
    /// <seealso cref="ISystemDisableManager" />
    [Serializable]
    public class SystemEnabledEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"{GetType().Name} (EGM enabled)");
        }
    }
}