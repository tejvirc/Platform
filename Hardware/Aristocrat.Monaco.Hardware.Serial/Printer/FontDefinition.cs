namespace Aristocrat.Monaco.Hardware.Serial.Printer
{
    /// <summary>
    ///     Font details
    /// </summary>
    public class FontDefinition
    {
        /// <summary>
        ///     Unique index
        /// </summary>
        public int Index;

        /// <summary>
        ///     Height in points (where 1" = 72 pts)
        /// </summary>
        public float HeightPts;

        /// <summary>
        ///     Width in characters/inch (CPI)
        /// </summary>
        public float PitchCpi;
    }
}
