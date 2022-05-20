namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using Application.Contracts;
    using JetBrains.Annotations;

    /// <summary>
    ///     Class that describes a location where the volume control can be placed
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class VolumeControlLocationInfo
    {
        /// <summary>
        ///     Creates an instance of VolumeControlLocationInfo
        /// </summary>
        /// <param name="value">The value to use</param>
        /// <param name="description">The description for the value</param>
        public VolumeControlLocationInfo(VolumeControlLocation value, string description)
        {
            Value = value;
            Description = description;
        }

        /// <summary>
        ///     Gets the value for this location
        /// </summary>
        public VolumeControlLocation Value { get; }

        /// <summary>
        ///     Gets the description for this location
        /// </summary>
        public string Description { get; }
    }
}