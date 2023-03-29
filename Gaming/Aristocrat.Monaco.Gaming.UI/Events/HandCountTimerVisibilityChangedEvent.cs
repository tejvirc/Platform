namespace Aristocrat.Monaco.Gaming.UI.Events
{
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class HandCountTimerOverlayVisibilityChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="isVisible">True if overlay window is visible</param>
        public HandCountTimerOverlayVisibilityChangedEvent(bool isVisible)
        {
            IsVisible = isVisible;
        }

        /// <summary>
        ///     True if visible
        /// </summary>
        public bool IsVisible { get; }
    }
}
