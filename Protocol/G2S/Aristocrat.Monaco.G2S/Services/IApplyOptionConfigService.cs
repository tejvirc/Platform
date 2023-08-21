namespace Aristocrat.Monaco.G2S.Services
{
    using Data.OptionConfig.ChangeOptionConfig;

    /// <summary>
    ///     Apply optionConfig values to device profiles on changes
    /// </summary>
    public interface IApplyOptionConfigService
    {
        /// <summary>
        ///     Updates the device profiles.
        /// </summary>
        /// <param name="changeOptionConfigRequest">The change option configuration request.</param>
        void UpdateDeviceProfiles(ChangeOptionConfigRequest changeOptionConfigRequest);
    }
}