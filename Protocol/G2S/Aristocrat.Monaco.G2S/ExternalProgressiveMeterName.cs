namespace Aristocrat.Monaco.G2S
{
    using System.ComponentModel.DataAnnotations;

    public static class ExternalProgressiveMeterName
    {
        /// <summary>
        ///     The current value of the linked progressive.
        /// </summary>
        private const string CurrentValueMeterName = @"ATI_EXT_CURRENT_VALUE";

        /// <summary>
        ///     The pattern to use for sub meters reported under ATI_EXT_CURRENT_VALUE.
        /// </summary>
        private const string CurrentValueSubMeterFormat = @"ATI_CV_{0}_{1}";

        /// <summary>
        ///     The meter and sub-meter name combination for CurrentValue
        /// </summary>
        public static readonly (string meterName, string subMeterNameFormat) CurrentValue = (CurrentValueMeterName,
            CurrentValueSubMeterFormat);

        /// <summary>
        ///     The accumulation of hit amount for the linked progressive.
        /// </summary>
        private const string WinAccumulationMeterName = @"ATI_EXT_ACCUMULATED";

        /// <summary>
        ///     The pattern to use for sub meters reported under ATI_EXT_ACCUMULATED.
        /// </summary>
        private const string WinAccumulationSubMeterFormat = @"ATI_ACC_{0}_{1}";

        /// <summary>
        ///     The meter and sub-meter name combination for WinAccumulation
        /// </summary>
        public static readonly (string meterName, string subMeterNameFormat) WinAccumulation =
            (WinAccumulationMeterName, WinAccumulationSubMeterFormat);

        /// <summary>
        ///     The number of hit occurrences for the linked progressive. 
        /// </summary>
        private const string WinOccurrenceMeterName = @"ATI_EXT_OCCURRENCE";

        /// <summary>
        ///     The pattern to use for sub meters reported under ATI_EXT_OCCURRENCE.
        /// </summary>
        private const string WinOccurrenceSubMeterFormat = @"ATI_OCC_{0}_{1}";

        /// <summary>
        ///     The meter and sub-meter name combination for WinOccurrence
        /// </summary>
        public static readonly (string meterName, string subMeterNameFormat) WinOccurrence =
            (WinOccurrenceMeterName, WinOccurrenceSubMeterFormat);
    }
}
