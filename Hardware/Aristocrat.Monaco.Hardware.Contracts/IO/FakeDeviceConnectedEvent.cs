namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Kernel;
    using SharedDevice;

    /// <summary>
    /// Class to handle fake device connection simulation.  Should be paired with the relevant disconnection event.
    /// </summary>
    public class FakeDeviceConnectedEvent : BaseEvent
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="FakeDeviceConnectedEvent"/>
        /// </summary>
        public FakeDeviceConnectedEvent()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeDeviceConnectedEvent"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="connected"></param>
        public FakeDeviceConnectedEvent(DeviceType type, bool connected)
        {
            Type = type;
            Connected = connected;
        }

        /// <summary>
        /// Type of device that is being simulated
        /// </summary>
        public DeviceType Type { get; }

        /// <summary>
        /// If the fake device is connected
        /// </summary>
        public bool Connected { get; }
    }
}
