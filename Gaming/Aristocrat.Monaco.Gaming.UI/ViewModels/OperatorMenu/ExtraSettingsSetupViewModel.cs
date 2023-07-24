namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Application.UI.OperatorMenu;
    using Contracts.GameSpecificOptions;
    using Models;

    /// <summary>
    ///     View model for ExtraSettingsSetupView
    /// </summary>
    public class ExtraSettingsSetupViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly string _themeId;
        private readonly IGameSpecificOptionProvider _gameSpecificOptionProvider;
        private ObservableCollection<GameSpecificOptionModel> _gameSpecificOptions;

        public ExtraSettingsSetupViewModel(string themeId, IGameSpecificOptionProvider gameSpecificOptionProvider)
        {
            _themeId = themeId;
            _gameSpecificOptionProvider = gameSpecificOptionProvider ?? throw new ArgumentNullException(nameof(gameSpecificOptionProvider));
        }

        public ObservableCollection<GameSpecificOptionModel> GameSpecificOptions
        {
            get => _gameSpecificOptions;
            set
            {
                SetProperty(ref _gameSpecificOptions, value, nameof(GameSpecificOptions));
                RaisePropertyChanged(nameof(GameSpecificOptions));
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs e) => UpdateCanSave = HasError;

        /// <summary>
        ///     Override base CanSave so that it is disable when TextBox validation failed
        /// </summary>
        public override bool CanSave => !HasError && base.CanSave;

        public bool HasError
        {
            get
            {
                if (GameSpecificOptions == null)
                    return false;

                return GameSpecificOptions.Any(x => x.HasErrors);
            }
        }

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            LoadData();
            base.OnLoaded();
        }

        protected override void DisposeInternal()
        {
            if (_gameSpecificOptions != null)
            {
                foreach (var item in _gameSpecificOptions)
                {
                    item.PropertyChanged -= ItemOnPropertyChanged;
                }
                _gameSpecificOptions.Clear();
            }

            base.DisposeInternal();
        }

        private void LoadData()
        {
            var options = _gameSpecificOptionProvider.GetGameSpecificOptions(_themeId)
                .Select(x => new GameSpecificOptionModel
                {
                    Name = x.Name,
                    Value = x.Value,
                    ValueSet = x.ValueSet,
                    OptionType = x.OptionType,
                    MinValue = x.MinValue,
                    MaxValue = x.MaxValue,
                    IsDropDownField = x.OptionType == OptionType.Toggle || x.OptionType == OptionType.List,
                    IsTextBoxField = x.OptionType == OptionType.Number
                });

            GameSpecificOptions = new ObservableCollection<GameSpecificOptionModel>(options);
            if (_gameSpecificOptions != null)
            {
                foreach (var item in _gameSpecificOptions)
                {
                    item.PropertyChanged += ItemOnPropertyChanged;
                }
            }
        }

        public override void Save() // Save button pressed
        {
            var options = GameSpecificOptions.Select(x => new GameSpecificOption
            {
                Name = x.Name,
                Value = x.Value,
                ValueSet = x.ValueSet,
                OptionType = x.OptionType,
                MinValue = x.MinValue,
                MaxValue = x.MaxValue,
            }).ToList();

            _gameSpecificOptionProvider.UpdateGameSpecificOptionsCache(_themeId, options);
        }
    }
}
