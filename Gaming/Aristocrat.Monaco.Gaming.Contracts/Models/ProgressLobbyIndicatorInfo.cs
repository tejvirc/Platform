namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Describes how the lobby renders progressive information for selectable progressives
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ProgressLobbyIndicatorInfo
    {
        /// <summary>
        ///     Creates an instance of ProgressLobbyIndicatorInfo
        /// </summary>
        /// <param name="value">The value to use</param>
        /// <param name="description">The description for the value</param>
        public ProgressLobbyIndicatorInfo(ProgressiveLobbyIndicator value, string description)
        {
            Value = value;
            Description = description;
        }

        /// <summary>
        ///     Gets the value for this indicator
        /// </summary>
        public ProgressiveLobbyIndicator Value { get; }

        /// <summary>
        ///     Gets the description for this indicator
        /// </summary>
        public string Description { get; }
    }
}