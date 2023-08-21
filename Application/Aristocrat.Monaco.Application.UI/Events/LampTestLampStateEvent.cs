// ReSharper disable UnusedAutoPropertyAccessor.Global Used for Automation
namespace Aristocrat.Monaco.Application.UI.Events
{
    using Kernel;
    using System;

    /// <summary>
    ///     Definition of the LampTestLampStateEvent class. [Test automation purposes only for now.]
    /// </summary>
    public class LampTestLampStateEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LampTestLampStateEvent" /> class.
        /// </summary>
        /// <param name="lamps">The lamps that are updated.</param>
        /// <param name="isOn">The value set for for lamps in above bitmap.</param>
        [CLSCompliant(false)]
        public LampTestLampStateEvent(string lamps, bool isOn)
        {
            Lamps = lamps;
            IsOn = isOn;
        }

        /// <summary>
        ///     Gets the Lamps 
        /// </summary>
        public string Lamps { get; }

        /// <summary>
        ///     Gets the state of Lamps in LampBits bitmap
        /// </summary>
        public bool IsOn { get; }
    }
}
