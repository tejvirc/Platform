namespace Aristocrat.Monaco.Gaming.Contracts.ButtonDeck
{
    using System.Collections.Generic;
    using System.Linq;
    using Cabinet.Contracts;
    using Common;
    using Hardware.Contracts.Cabinet;
    using Kernel;

    /// <summary>
    ///     Helper class for button deck template selection
    /// </summary>
    public static class VirtualButtonDeckHelper
    {
        private const VbdType DefaultType = VbdType.Helix;
        private const string DefaultName = "Helix";

        private static readonly Dictionary<VbdType, string> CabinetTypeMap = new Dictionary<VbdType, string>
        {
            { VbdType.Helix, "Helix" }, {VbdType.HelixXT, "HelixXt" }, { VbdType.MarsX, "MarsX" }, { VbdType.Bartop, "Bartop" }
        };

        private static ICabinetDetectionService CabinetDetectionService =>
            ServiceManager.GetInstance().GetService<ICabinetDetectionService>();

        /// <summary>
        ///     Gets the vbd prefix name for the template based on the cabinet type.
        /// </summary>
        /// <returns>The vbd template name prefix</returns>
        public static string GetVbdPrefixNameByCabinetType()
        {
            // Create a mapping of external VBD name to internal VBD type
            var vbdTypes = CabinetTypeMap.ToDictionary(
                keySelector: x => x.Key.GetDescription(typeof(VbdType)),
                elementSelector: x => x.Key);

            // Get the external VBD name for the currently connected VBD
            var deviceName = CabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD)?.Name;

            // Resolve internal VBD type from external VBD name
            if (deviceName == null || !vbdTypes.TryGetValue(deviceName, out var vbdType))
            {
                vbdType = DefaultType;
            }

            // Resolve cabinet name from internal VBD type
            if (!CabinetTypeMap.TryGetValue(vbdType, out var name))
            {
                name = DefaultName;
            }

            return name;
        }
    }
}