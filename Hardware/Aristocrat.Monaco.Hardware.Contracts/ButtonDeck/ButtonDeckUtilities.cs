namespace Aristocrat.Monaco.Hardware.Contracts.ButtonDeck
{
    using Cabinet;
    using Contracts;
    using Kernel;
    using System;
    using System.Linq;
    using Aristocrat.Cabinet.Contracts;
    using Common;

    /// <summary>Definition of the ButtonDeckUtilities class</summary>
    [CLSCompliant(false)]
    public static class ButtonDeckUtilities
    {
        /// <summary>Method to obtain the button deck type</summary>
        /// <returns>The type of button deck being used</returns>
        public static ButtonDeckType GetButtonDeckType(
            this ICabinetDetectionService cabinetDetectionService,
            IPropertiesManager properties)
        {
            var buttonDeckType = ButtonDeckType.PhysicalButtonDeck;
            if (string.Equals(
                    properties.GetValue(HardwareConstants.SimulateLcdButtonDeck, "FALSE"),
                    "TRUE",
                    StringComparison.OrdinalIgnoreCase))
            {
                buttonDeckType = ButtonDeckType.SimulatedLCD;
            }
            else if (string.Equals(
                         properties.GetValue(HardwareConstants.SimulateVirtualButtonDeck, "FALSE"),
                         "TRUE",
                         StringComparison.OrdinalIgnoreCase))
            {
                buttonDeckType = ButtonDeckType.SimulatedVirtual;
            }
            else if (string.Equals(
                         properties.GetValue(HardwareConstants.UsbButtonDeck, "FALSE"),
                         "TRUE",
                         StringComparison.OrdinalIgnoreCase))
            {
                buttonDeckType = ButtonDeckType.LCD;
            }
            else if (VirtualButtonDeckScreenMatch(cabinetDetectionService))
            {
                buttonDeckType = ButtonDeckType.Virtual;
            }

            return buttonDeckType;
        }

        /// <summary>Method to obtain the button deck firmware version</summary>
        /// <returns>The firmware version of the button deck being used</returns>
        public static string GetButtonDeckFirmwareVersion(
            this IButtonDeckDisplay buttonDeckDisplay,
            IPropertiesManager properties)
        {
            var firmwareVersion = string.Empty;
            if (!string.Equals(
                    properties.GetValue(HardwareConstants.UsbButtonDeck, "FALSE"),
                    "TRUE",
                    StringComparison.OrdinalIgnoreCase))
            {
                return firmwareVersion;
            }

            // LCD button deck has two displays, just get the firmware Id for the first one.
            firmwareVersion += buttonDeckDisplay.GetFirmwareId(0);
            return firmwareVersion;
        }

        /// <summary>Gets the button deck firmware CRC</summary>
        /// <returns>The firmware CRC and the Seed used to generate it</returns>
        public static (string crc, string seed) GetButtonDeckFirmwareCrc(this IButtonDeckDisplay buttonDisplay, IPropertiesManager properties)
        {
            if (!string.Equals(
                    properties.GetValue(HardwareConstants.UsbButtonDeck, "FALSE"),
                    "TRUE",
                    StringComparison.OrdinalIgnoreCase) || buttonDisplay.Crc == 0)
            {
                return (string.Empty, string.Empty);
            }

            return ($"0x{buttonDisplay.Crc:X8}", $"0x{buttonDisplay.Seed:X8}");
        }

        /// <summary>Checks if the connected button deck has lamps.</summary>
        /// <returns>True if button deck has lamps, otherwise false.</returns>
        public static bool HasLamps(
            this ICabinetDetectionService cabinetDetectionService,
            IPropertiesManager properties)
        {
            // Query button deck type
            var buttonDeckType = cabinetDetectionService.GetButtonDeckType(properties);

            return buttonDeckType switch
            {
                ButtonDeckType.Virtual => VirtualButtonDeckHasLamps(cabinetDetectionService),
                ButtonDeckType.PhysicalButtonDeck => true, // E.g. the Bartop mechanical button deck has lamps
                ButtonDeckType.SimulatedVirtual or ButtonDeckType.LCD or ButtonDeckType.SimulatedLCD
                    or ButtonDeckType.NoButtonDeck => false,
                _ => false,
            };
        }

        /// <summary>Method to determine if a virtual button deck is attached</summary>
        /// <remarks>
        ///     Logic moved from HardwareHelpers.  A different way should be found for determining whether a virtual button deck is attached,
        ///     since this may not work correctly with attached LCD toppers.
        /// </remarks>
        private static bool VirtualButtonDeckScreenMatch(this ICabinetDetectionService cabinetDetectionService)
        {
            // Check for the expected third monitor of the right size.
            return cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD) is not null;
        }

        private static bool VirtualButtonDeckHasLamps(ICabinetDetectionService cabinetDetectionService)
        {
            // Query the VBD type
            var externalVbdType = cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD)?.Name;
            if (string.IsNullOrEmpty(externalVbdType))
            {
                return false;
            }

            var vbdType = ExternalVbdTypeToInternal(externalVbdType);
            return vbdType switch
            {
                VbdType.Helix or VbdType.HelixXT or VbdType.Bartop or VbdType.Flame or VbdType.EdgeX => true,
                _ => false,
            };
        }

        /// <summary>Converts external VBD type to internal VBD type.</summary>
        /// <param name="externalType">External VBD type.</param>
        /// <returns>Internal VBD type</returns>
        private static VbdType ExternalVbdTypeToInternal(string externalType)
        {
            var internalTypes = Enum.GetValues(typeof(VbdType)).Cast<VbdType>();
            var externalToInternalMap = internalTypes.ToDictionary(
                keySelector: iType => iType.GetDescription(typeof(VbdType)),
                elementSelector: iType => iType);

            return externalToInternalMap.TryGetValue(externalType, out var internalType) ? internalType : default;
        }
    }
}