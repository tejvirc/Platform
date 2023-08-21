namespace Aristocrat.Monaco.Gaming.Consumers
{
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;

    /// <summary>
    ///     Extension methods for an IAudio.
    /// </summary>
    public static class AudioServiceExtensions
    {
        /// <summary>
        ///     Get max volume
        ///     Purposely to provide the maximum volume output to Runtime
        /// </summary>
        /// <param name = "audio"></param>the audio service
        /// <param name = "propertiesManager"></param>the properties manager provider
        /// <param name = "gameCategoryService"></param>the game category service
        /// <param name = "showVolumeControlInLobbyOnly"></param>true if lobby control only, fault otherwise</param>
        /// <returns>Returns the max volume value (0-100.0)</returns>
        public static float GetMaxVolume(this IAudio audio, IPropertiesManager propertiesManager, IGameCategoryService gameCategoryService, bool showVolumeControlInLobbyOnly)
        {
            var volumeLevel = (VolumeLevel)propertiesManager.GetProperty(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            var masterVolume = audio.GetVolume(volumeLevel);

            var useGameTypeVolume = propertiesManager.GetValue(ApplicationConstants.UseGameTypeVolumeKey, ApplicationConstants.UseGameTypeVolume);
            var gameTypeVolumeScalar = useGameTypeVolume ? audio.GetVolumeScalar(gameCategoryService.SelectedGameCategorySetting.VolumeScalar) : 1.0f;

            var playerVolumeScalar = audio.GetVolumeScalar((VolumeScalar)propertiesManager.GetValue(ApplicationConstants.PlayerVolumeScalarKey, ApplicationConstants.PlayerVolumeScalar));

            return masterVolume * gameTypeVolumeScalar * (!showVolumeControlInLobbyOnly ? playerVolumeScalar : 1.0f);
        }
    }
}
