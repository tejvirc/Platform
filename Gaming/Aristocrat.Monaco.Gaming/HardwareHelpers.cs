namespace Aristocrat.Monaco.Gaming
{
    using System;
    //using Cabinet.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;

    /// <summary>
    ///     Helper utility class.
    /// </summary>
    public static class HardwareHelpers
    {
        /// <summary>
        ///     Checks is virtual button deck hardware exists.
        /// </summary>
        /// <returns>True if the virtual button deck exists as 3rd monitor.</returns>
        ///
        public const string BartopLCD9BD = "VBD_Bartop";

        public static bool CheckForVirtualButtonDeckHardware()
        {
            // Are we simulating the hardware?
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var simulateVirtualButtonDeck = properties.GetValue(HardwareConstants.SimulateVirtualButtonDeck, "FALSE");
            if (simulateVirtualButtonDeck.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            //// Check for the expected third monitor of the right size.
            //return ServiceManager.GetInstance().GetService<ICabinetDetectionService>()
            //    .GetDisplayDeviceByItsRole(DisplayRole.VBD) != null;
            return false;
        }

        /// <summary>
        ///     Checks is usb button deck hardware exists.
        /// </summary>
        /// <returns>True if the usb button deck exists.</returns>
        public static bool CheckForUsbButtonDeckHardware()
        {
            //if (CheckForVirtualButtonDeckHardware())
            //{
            //    //// for the one who needs to simulate VBD on an LCD rig.
            //    return false;
            //}

            //// Are we simulating the hardware?
            //var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            //var usbButtonDeck = properties.GetValue(HardwareConstants.UsbButtonDeck, "FALSE");

            //return usbButtonDeck.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);

            return false;
        }
    }
}
