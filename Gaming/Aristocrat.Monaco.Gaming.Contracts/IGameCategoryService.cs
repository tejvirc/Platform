namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Hardware.Contracts.Audio;
    using Models;

    /// <summary>
    ///     GameCategorySetting struct
    /// </summary>
    public struct GameCategorySetting
    {
        /// <summary>
        ///     Gets a value indicating whether the game auto play is set
        /// </summary>
        public bool AutoPlay { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the game auto hold is set
        /// </summary>
        public bool AutoHold { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the game show player speed is set
        /// </summary>
        public bool ShowPlayerSpeedButton { get; set; }

        /// <summary>
        ///     Gets the game type volume scalar
        ///     This is not the same as a VolumeLevel (as used for DefaultVolume ... the operator-controlled MasterVolume for the
        ///     cabinet)
        ///     This is a value between 1-5 which scales the volume by 20%, 40%, 60%, 80%, or 100%
        /// </summary>
        public VolumeScalar VolumeScalar { get; set; }

        /// <summary>
        ///     Gets the player-selectable game speed within a set of 1-3
        /// </summary>
        public int PlayerSpeed { get; set; }

        /// <summary>
        ///     Gets the operator-selectable game speed range (set of three) within range of 1-9
        /// </summary>
        public int DealSpeed { get; set; }

        /// <summary>
        ///     Gets, sets the background color for game.
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        ///     ToString override
        /// </summary>
        /// <returns>Description of object</returns>
        public override string ToString()
        {
            return $"{typeof(GameCategorySetting)} AutoPlay:{AutoPlay} AutoHold:{AutoHold} ShowPlayerSpeedButton:{ShowPlayerSpeedButton} VolumeScalar:{VolumeScalar} PlayerSpeed:{PlayerSpeed} DealSpeed:{DealSpeed} BackgroundColor:{BackgroundColor}";
        }
    }

    /// <summary>
    ///     Contract for game category service instance.
    /// </summary>
    public interface IGameCategoryService
    {
        /// <summary>
        ///     Gets selected store setting for the game category.
        /// </summary>
        /// <returns>GameCategorySetting</returns>
        GameCategorySetting SelectedGameCategorySetting { get; }

        /// <summary>
        ///     Gets GameCategorySetting by game type.
        /// </summary>
        /// <param name="gameType">GameType</param>
        /// <returns>GameCategorySetting</returns>
        GameCategorySetting this[GameType gameType] { get; }

        /// <summary>
        ///     Updates game category setting
        /// </summary>
        /// <param name="gameType">Game type.</param>
        /// <param name="settings">Game type settings.</param>
        void UpdateGameCategory(GameType gameType, GameCategorySetting settings);

        /// <summary>
        ///     Gets store setting for the game category.
        /// </summary>
        /// <param name="gameType">Game type.</param>
        /// <returns>GameCategorySetting</returns>
        GameCategorySetting GetGameCategorySetting(GameType gameType);
    }
}