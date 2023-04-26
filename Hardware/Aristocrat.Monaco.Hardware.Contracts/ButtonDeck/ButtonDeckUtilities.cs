namespace Aristocrat.Monaco.Hardware.Contracts.ButtonDeck
{
    using Button;
    using Cabinet;
    using Contracts;
    using Kernel;
    using System;
    using System.Linq;
    //using Aristocrat.Cabinet.Contracts;
    using Common;

    /// <summary>
    /// LcdButtonDeckLobby maps LCD Button Deck Logical Ids to their corresponding lobby function
    /// </summary>
    public enum LcdButtonDeckLobby
    {
        /// <summary>NextGame</summary>
        NextGame = ButtonLogicalId.Play4,

        /// <summary>NextTab</summary>
        NextTab = ButtonLogicalId.Play5,

        /// <summary>CashOut</summary>
        CashOut = ButtonLogicalId.Collect,

        /// <summary>PreviousGame</summary>
        PreviousGame = ButtonLogicalId.Bet3,

        /// <summary>PreviousTab</summary>
        PreviousTab = ButtonLogicalId.Bet4,

        /// <summary>ChangeDenom</summary>
        ChangeDenom = ButtonLogicalId.Bet5,

        /// <summary>LaunchGame</summary>
        LaunchGame = ButtonLogicalId.Play
    }

    /// <summary>Definition of the ButtonDeckUtilities class</summary>
    [CLSCompliant(false)]
    public static class ButtonDeckUtilities
    {
        /// <summary>Data type that lists the different types of supported button decks</summary>
        public enum ButtonDeckType
        {
            /// <summary>No button deck</summary>
            NoButtonDeck,

            /// <summary>Virtual button deck</summary>
            Virtual,

            /// <summary>LCD button deck</summary>
            LCD,

            /// <summary>Simulated virtual button deck</summary>
            SimulatedVirtual,

            /// <summary>Simulated LCD button deck</summary>
            SimulatedLCD,

            /// <summary>Physical Button Deck</summary>
            PhysicalButtonDeck
        }

        /// <summary>Method to obtain the button deck type</summary>
        /// <returns>The type of button deck being used</returns>
        public static ButtonDeckType GetButtonDeckType()
        {
            var buttonDeckType = ButtonDeckType.PhysicalButtonDeck;
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            if (string.Equals(
                properties.GetValue(HardwareConstants.SimulateLcdButtonDeck, "FALSE"),
                "TRUE",
                StringComparison.InvariantCultureIgnoreCase))
            {
                buttonDeckType = ButtonDeckType.SimulatedLCD;
            }
            else if (string.Equals(
                properties.GetValue(HardwareConstants.SimulateVirtualButtonDeck, "FALSE"),
                "TRUE",
                StringComparison.InvariantCultureIgnoreCase))
            {
                buttonDeckType = ButtonDeckType.SimulatedVirtual;
            }
            else if (string.Equals(
                properties.GetValue(HardwareConstants.UsbButtonDeck, "FALSE"),
                "TRUE",
                StringComparison.InvariantCultureIgnoreCase))
            {
                buttonDeckType = ButtonDeckType.LCD;
            }
            else if (VirtualButtonDeckScreenMatch())
            {
                buttonDeckType = ButtonDeckType.Virtual;
            }

            return buttonDeckType;
        }

        /// <summary>Method to obtain the button deck firmware version</summary>
        /// <returns>The firmware version of the button deck being used</returns>
        public static string GetButtonDeckFirmwareVersion()
        {
            var firmwareVersion = string.Empty;
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            if (string.Equals(
                properties.GetValue(HardwareConstants.UsbButtonDeck, "FALSE"),
                "TRUE",
                StringComparison.InvariantCultureIgnoreCase))
            {
                var buttonDeckDisplay = ServiceManager.GetInstance().TryGetService<IButtonDeckDisplay>();
                firmwareVersion += buttonDeckDisplay.GetFirmwareId(0); // LCD button deck has two displays, just get the firmware Id for the first one.
            }

            return firmwareVersion;
        }

        /// <summary>Gets the button deck firmware CRC</summary>
        /// <returns>The firmware CRC and the Seed used to generate it</returns>
        public static (string crc, string seed) GetButtonDeckFirmwareCrc()
        {
            var crc = string.Empty;
            var seed = string.Empty;
            var serviceManager = ServiceManager.GetInstance();
            var properties = serviceManager.GetService<IPropertiesManager>();
            if (string.Equals(
               properties.GetValue(HardwareConstants.UsbButtonDeck, "FALSE"),
               "TRUE",
               StringComparison.InvariantCultureIgnoreCase))
            {
                var buttonDisplay = serviceManager.GetService<IButtonDeckDisplay>();
                if (buttonDisplay.Crc != 0)
                {
                    crc = $"0x{buttonDisplay.Crc:X8}";
                    seed = $"0x{buttonDisplay.Seed:X8}";
                }
            }

            return (crc, seed);
        }

        /// <summary>Method to determine if a virtual button deck is attached</summary>
        /// <remarks>
        ///     Logic moved from HardwareHelpers.  A different way should be found for determining whether a virtual button deck is attached,
        ///     since this may not work correctly with attached LCD toppers.
        /// </remarks>
        public static bool VirtualButtonDeckScreenMatch()
        {
            //// Check for the expected third monitor of the right size.
            //return ServiceManager.GetInstance().GetService<ICabinetDetectionService>()
            //    .GetDisplayDeviceByItsRole(DisplayRole.VBD) != null;
            return false;
        }

        /// <summary>Checks if the connected button deck has lamps.</summary>
        /// <returns>True if button deck has lamps, otherwise false.</returns>
        public static bool HasLamps()
        {
            //// Query button deck type
            //var buttonDeckType = GetButtonDeckType();

            //switch (buttonDeckType)
            //{
            //    case ButtonDeckType.Virtual:

            //        // Query the VBD type
            //        var cabinetDetectionService = ServiceManager.GetInstance().TryGetService<ICabinetDetectionService>();
            //        var externalVbdType = cabinetDetectionService?.GetDisplayDeviceByItsRole(DisplayRole.VBD)?.Name;
            //        if (string.IsNullOrEmpty(externalVbdType))
            //        {
            //            return false;
            //        }

            //        var vbdType = ExternalVbdTypeToInternal(externalVbdType);

            //        switch (vbdType)
            //        {
            //            case VbdType.Helix:
            //            case VbdType.HelixXT:
            //            case VbdType.Bartop:
            //            case VbdType.Flame: // TODO: Verify
            //            case VbdType.EdgeX: // TODO: Verify
            //                return true;

            //            case VbdType.MarsX:
            //                return false;
            //        }
            //        return false;

            //    case ButtonDeckType.PhysicalButtonDeck:
            //        // E.g. the Bartop mechanical button deck has lamps
            //        return true;

            //    case ButtonDeckType.SimulatedVirtual:
            //    case ButtonDeckType.LCD:
            //    case ButtonDeckType.SimulatedLCD:
            //    case ButtonDeckType.NoButtonDeck:
            //        return false;
            //}

            return false;
        }

        ///// <summary>Converts internal VBD type to external VBD type.</summary>
        ///// <param name="internalType">Internal VBD type.</param>
        ///// <returns>External VBD type</returns>
        //public static string InternalVbdTypeToExternal(VbdType internalType)
        //{
        //    return internalType.GetDescription(typeof(VbdType));
        //}

        ///// <summary>Converts external VBD type to internal VBD type.</summary>
        ///// <param name="externalType">External VBD type.</param>
        ///// <returns>Internal VBD type</returns>
        //public static VbdType ExternalVbdTypeToInternal(string externalType)
        //{
        //    var internalTypes = Enum.GetValues(typeof(VbdType)).Cast<VbdType>();

        //    var externalToInternalMap = internalTypes.ToDictionary(
        //        keySelector: iType => iType.GetDescription(typeof(VbdType)),
        //        elementSelector: iType => iType);

        //    if (!externalToInternalMap.TryGetValue(externalType, out VbdType internalType))
        //    {
        //        internalType = default(VbdType);
        //    }

        //    return internalType;
        //}

    }
}