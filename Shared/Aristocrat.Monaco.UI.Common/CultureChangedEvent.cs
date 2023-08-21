namespace Aristocrat.Monaco.UI.Common
{
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     An event that signals that the culture has changed.
    /// </summary>
    public class CultureChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CultureChangedEvent" /> class.
        /// </summary>
        public CultureChangedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CultureChangedEvent" /> class.
        /// </summary>
        /// <param name="newCulture">The newly selected culture</param>
        public CultureChangedEvent(CultureInfo newCulture)
        {
            NewCulture = newCulture;
        }

        /// <summary>
        ///     Gets the newly selected culture.
        /// </summary>
        public CultureInfo NewCulture { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [NewCulture={1}]",
                GetType().Name,
                NewCulture.Name);
        }
    }
}