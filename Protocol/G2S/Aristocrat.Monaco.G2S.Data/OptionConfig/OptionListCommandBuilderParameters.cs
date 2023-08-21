namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    /// <summary>
    ///     Contains parameters for option list command builder.
    /// </summary>
    public class OptionListCommandBuilderParameters
    {
        /// <summary>
        ///     Gets or sets a value indicating whether [include all device classes].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [include all device classes]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAllDeviceClasses { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [include all devices].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [include all devices]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAllDevices { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether /[include all groups].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [include all groups]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAllGroups { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether /[include all options].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [include all options]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAllOptions { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether /[include details].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [include details]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeDetails { get; set; }

        /// <summary>
        ///     Gets or sets the device class.
        /// </summary>
        /// <value>
        ///     The device class.
        /// </value>
        public string DeviceClass { get; set; }

        /// <summary>
        ///     Gets or sets the device identifier.
        /// </summary>
        /// <value>
        ///     The device identifier.
        /// </value>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the option group identifier.
        /// </summary>
        /// <value>
        ///     The option group identifier.
        /// </value>
        public string OptionGroupId { get; set; }

        /// <summary>
        ///     Gets or sets the option identifier.
        /// </summary>
        /// <value>
        ///     The option identifier.
        /// </value>
        public string OptionId { get; set; }
    }
}