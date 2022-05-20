namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using Contracts;
    using Contracts.Models;
    using Hardware.Contracts.Audio;
    using MVVM.Model;

    /// <summary>
    ///     Base class for game category settings.
    /// </summary>
    internal abstract class GameCategorySettings : BaseNotify
    {
        private bool _autoPlay;
        private bool _autoHold;
        private bool _showPlayerSpeedButton;
        private int _dealSpeed;
        private int _playerSpeed;
        private VolumeScalar _volumeScalar;
        private string _backgroundColor;

        /// <summary>
        ///     Gets the game type.
        /// </summary>
        public abstract GameType GameType { get; }

        /// <summary>
        ///     Gets or sets a value that indicates whether auto play is enabled.
        /// </summary>
        public bool AutoPlay
        {
            get => _autoPlay;

            set => SetProperty(ref _autoPlay, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether auto hold is enabled.
        /// </summary>
        public bool AutoHold
        {
            get => _autoHold;

            set => SetProperty(ref _autoHold, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether show player speed button is visible.
        /// </summary>
        public bool ShowPlayerSpeedButton
        {
            get => _showPlayerSpeedButton;

            set => SetProperty(ref _showPlayerSpeedButton, value);
        }

        /// <summary>
        ///     Gets or sets the deal speed.
        /// </summary>
        public int DealSpeed
        {
            get => _dealSpeed;

            set => SetProperty(ref _dealSpeed, value);
        }

        /// <summary>
        ///     Gets or sets the player speed.
        /// </summary>
        public int PlayerSpeed
        {
            get => _playerSpeed;

            set => SetProperty(ref _playerSpeed, value);
        }

        /// <summary>
        ///     Gets or sets the volume scalar.
        /// </summary>
        public VolumeScalar VolumeScalar
        {
            get => _volumeScalar;

            set => SetProperty(ref _volumeScalar, value);
        }

        /// <summary>
        ///     Gets or sets the background color.
        /// </summary>
        public string BackgroundColor
        {
            get => _backgroundColor;

            set => SetProperty(ref _backgroundColor, value);
        }

        /// <summary>
        ///     Performs conversion from <see cref="DenominationSettings"/> to <see cref="Denomination"/>.
        /// </summary>
        /// <param name="settings">The <see cref="DenominationSettings"/> setting.</param>
        public static explicit operator GameCategorySetting(GameCategorySettings settings) => new GameCategorySetting
        {
            AutoPlay = settings.AutoPlay,
            VolumeScalar = settings.VolumeScalar,
            PlayerSpeed = settings.PlayerSpeed,
            DealSpeed = settings.DealSpeed,
            ShowPlayerSpeedButton = settings.ShowPlayerSpeedButton,
            AutoHold = settings.AutoHold,
            BackgroundColor = settings.BackgroundColor
        };
    }
}
