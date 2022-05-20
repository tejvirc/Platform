namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Cabinet.Contracts;

    /// <summary>
    ///     Configure Client Constants
    /// </summary>
    public static class ConfigureClientConstants
    {
        /// <summary> Display role to display name mapping </summary>
        private static readonly Dictionary<DisplayRole, string> DisplayNames = new Dictionary<DisplayRole, string>
        {
            { DisplayRole.Main, "Main" },
            { DisplayRole.Top, "Top" },
            { DisplayRole.Topper, "Topper" },
            { DisplayRole.VBD, "VBD" }
        };

        /// <summary> Client command flag for the physical display size </summary>
        /// <param name="role">The display role</param>
        /// <returns>The physical display size command flag</returns>
        public static string DiagonalDisplayFlag(DisplayRole role)
        {
            return DisplayNames.TryGetValue(role, out var name)
                ? $"/Runtime/Hardware/Display/{name}&PhysicalDiagonal"
                : null;
        }
    }
}