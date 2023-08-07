namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Forms;
    using Contracts;
    using Hardware.Contracts.Button;
    using Kernel;
    using log4net;

    /// <summary>
    ///     The list of keys we can simulate
    /// </summary>
    public enum VirtualKeyCode
    {
        /// <summary> no key. (FxCop requirement) </summary>
        None = 0,

        /// <summary> The up arrow key </summary>
        Up = 38,

        /// <summary> The left arrow key </summary>
        Left = 37,

        /// <summary> The right arrow key </summary>
        Right = 39,

        /// <summary> The down arrow key </summary>
        Down = 40,

        /// <summary> The shift key </summary>
        Shift = 0x10,

        /// <summary> The space key </summary>
        Space = 0x20,

        /// <summary> The tab key </summary>
        Tab = 9
    }

    /// <summary>
    ///     This class provides configuration and operator screen navigation
    ///     using the button deck keys.
    /// </summary>
    public sealed class ButtonDeckNavigator : IDisposable
    {
        /// <summary> The Id of the logical button to use as the next button. </summary>
        private const int NextButtonId = 8;

        /// <summary> The Id of the logical button to use as the Previous button. </summary>
        private const int PreviousButtonId = 3;

        /// <summary> The Id of the logical button to use as the select button. </summary>
        private const int SelectButtonId = 10;

        /// <summary> The Id of the logical button to use as the scroll down button. </summary>
        private const int ScrollDownButtonId = 11;

        /// <summary> The Id of the logical button to use as the scroll up button. </summary>
        private const int ScrollUpButtonId = 9;

        /// <summary> The Id of the logical button to use as the right button. </summary>
        private const int RightButtonId = 0;

        /// <summary> The Id of the logical button to use as the left button. </summary>
        private const int LeftButtonId = 2;

        /// <summary>
        ///     Create a logger for use in this class
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Called when it is time to exit this component.
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();
            ServiceManager.GetInstance().GetService<IEventBus>().Publish(new ButtonDeckNavigatorEndedEvent());
        }

        /// <summary>
        ///     Initialize the component. Start another thread to listen for button press events.
        /// </summary>
        public void Initialize()
        {
            SubscribeToEvents();
            ServiceManager.GetInstance().GetService<IEventBus>().Publish(new ButtonDeckNavigatorStartedEvent());
        }

        /// <summary>
        ///     Handle button up events
        /// </summary>
        /// <param name="data">The Event to handle</param>
        private static void HandleEvent(IEvent data)
        {
            var theEvent = data as UpEvent;
            if (theEvent == null)
            {
                return;
            }

            Logger.DebugFormat("Received Logical ID of {0}.", theEvent.LogicalId);
            switch (theEvent.LogicalId)
            {
                // play button maps to shift tab
                case PreviousButtonId:
                    SimulateShiftedKeyPress(VirtualKeyCode.Tab);
                    break;

                // bet button maps to tab
                case NextButtonId:
                    SimulateKeyPress(VirtualKeyCode.Tab);
                    break;

                // play button maps to space. This will cause a checkbox state to toggle or
                // a button to be clicked.
                case SelectButtonId:
                    SimulateKeyPress(VirtualKeyCode.Space);
                    break;

                // cash out button maps to down arrow. This is used for combo box selections.
                case ScrollDownButtonId:
                    SimulateKeyPress(VirtualKeyCode.Down);
                    break;

                // scroll up button maps to up arrow. This is used for combo box selections.
                case ScrollUpButtonId:
                    SimulateKeyPress(VirtualKeyCode.Up);
                    break;

                // left button maps to left arrow. This is used for tab control selections.
                case LeftButtonId:
                    SimulateKeyPress(VirtualKeyCode.Left);
                    break;

                // right button maps to right arrow. This is used for tab control selections.
                case RightButtonId:
                    SimulateKeyPress(VirtualKeyCode.Right);
                    break;
            }
        }

        /// <summary>
        ///     Simulates a key press
        /// </summary>
        /// <param name="keyCode">The key to press</param>
        private static void SimulateKeyPress(VirtualKeyCode keyCode)
        {
            if (keyCode == VirtualKeyCode.Space)
            {
                Logger.Debug("Sending space");
                SendKeys.SendWait(" ");
            }
            else
            {
                var sendString = "{" + keyCode.ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
                Logger.DebugFormat("Sending {0}", sendString);
                SendKeys.SendWait(sendString);
            }
        }

        /// <summary>
        ///     Simulate a key press with a modifier key also pressed (e.g. the shift key).
        /// </summary>
        /// <param name="keyCode">The key modifier to press</param>
        private static void SimulateShiftedKeyPress(VirtualKeyCode keyCode)
        {
            var sendString = "+{" + keyCode.ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            Logger.DebugFormat("Sending +{0}", sendString);
            SendKeys.SendWait(sendString);
        }

        /// <summary>Subscribe to the button up event</summary>
        private void SubscribeToEvents()
        {
            ServiceManager.GetInstance().GetService<IEventBus>().Subscribe<UpEvent>(this, HandleEvent);
        }

        /// <summary>
        ///     Unsubscribe from the button up event
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
        }
    }
}