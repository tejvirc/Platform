namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>Class to handle Touch specific events. This class must be inherited from to use.</summary>
    [Serializable]
    public class TouchEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TouchEvent" /> class.
        /// </summary>
        public TouchEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TouchEvent" /> class.
        /// </summary>
        /// <param name="ti">The associated info of touch action.</param>
        public TouchEvent(TouchInfo ti)
        {
            Info = ti;
        }

        /// <summary>Gets a X coordinate value .</summary>
        public TouchInfo Info { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1}",
                Info.X,
                Info.Y);
        }
    }
}