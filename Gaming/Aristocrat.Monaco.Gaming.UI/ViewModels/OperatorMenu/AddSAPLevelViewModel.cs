namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Localization.Properties;

    /// <summary>
    ///     Defines the AddSAPLevelViewModel class
    /// </summary>
    [CLSCompliant(false)]
    public class AddSAPLevelViewModel : OperatorMenuSaveViewModelBase
    {
        private const string Poker = "Poker";
        private const string Keno = "Keno";

        private readonly GameType _gameType;
        private readonly ISharedSapProvider _sharedSapProvider;
        private readonly SharedSapLevel _sharedSapLevel;

        private decimal _resetValue;
        private decimal _initialValue;
        private decimal _maxValue;
        private string _levelName;
        private decimal _incrementRate;

        public AddSAPLevelViewModel(
            GameType type,
            ISharedSapProvider sharedSapProvider,
            IViewableSharedSapLevel level = null,
            bool assigned = false)
        {
            _gameType = type;
            _sharedSapProvider = sharedSapProvider;
            IsLevelUnassigned = !assigned;

            if (level != null)
            {
                _sharedSapLevel = new SharedSapLevel
                {
                    Id = level.Id,
                    SupportedGameTypes = level.SupportedGameTypes.ToArray(),
                    CurrentErrorStatus = level.CurrentErrorStatus,
                    CurrentValue = level.CurrentValue,
                    IncrementRate = level.IncrementRate,
                    InitialValue = level.InitialValue,
                    LevelId = level.LevelId,
                    MaximumValue = level.MaximumValue,
                    Name = level.Name,
                    ResetValue = level.ResetValue,
                    CanEdit = level.CanEdit
                };
            }
        }

        public override bool CanSave => base.CanSave
                                        && !string.IsNullOrWhiteSpace(LevelName)
                                        && _maxValue >= 0
                                        && _maxValue.Validate(true) is null
                                        && _resetValue.Validate() is null
                                        && _initialValue.Validate() is null;

        public bool IsLevelUnassigned { get; }

        public string GameTypeLabel => (_gameType == GameType.Poker) ? Poker : Keno;

        public string LevelName
        {
            get => _levelName;

            set
            {
                _levelName = value;
                var trimmedName = value?.Trim();
                if (string.IsNullOrWhiteSpace(_levelName))
                {
                    SetError(
                        nameof(LevelName),
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyStringNotAllowErrorMessage));
                }
                else if (_sharedSapLevel?.Name != trimmedName && _sharedSapProvider.ViewSharedSapLevels().Any(x => x.Name == trimmedName))
                {
                    SetError(nameof(LevelName), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UniqueLevelNameError));
                }
                else
                {
                    ClearErrors(nameof(LevelName));
                }

                RaisePropertyChanged(nameof(LevelName), nameof(CanSave));
            }
        }

        public decimal ResetValue
        {
            get => _resetValue;
            set
            {
                if (SetProperty(ref _resetValue, value, nameof(ResetValue)))
                {
                    ValidValueFields();
                }
            }
        }

        public decimal InitialValue
        {
            get => _initialValue;
            set
            {
                if (SetProperty(ref _initialValue, value, nameof(InitialValue)))
                {
                    ValidValueFields();
                }
            }
        }

        public decimal MaxValue
        {
            get => _maxValue;
            set
            {
                if (SetProperty(ref _maxValue, value, nameof(MaxValue)))
                {
                    ValidValueFields();
                }
            }
        }

        public decimal MaximumIncrementRate =>
            GamingConstants.MaxSharedLevelDetailIncrementRate;

        public decimal IncrementRate
        {
            get => _incrementRate;
            set => SetProperty(ref _incrementRate, value, nameof(IncrementRate));
        }

        public override void Save()
        {
            var sharedSapList = new List<SharedSapLevel>();

            if (_sharedSapLevel == null)
            {
                sharedSapList.Add(
                    new SharedSapLevel
                    {
                        Id = Guid.NewGuid(),
                        Name = LevelName.Trim(),
                        SupportedGameTypes = new[] { _gameType },
                        InitialValue = InitialValue.DollarsToMillicents(),
                        CurrentValue = InitialValue.DollarsToMillicents(),
                        ResetValue = ResetValue.DollarsToMillicents(),
                        IncrementRate = _incrementRate.ToPercentage(),
                        MaximumValue = MaxValue.DollarsToMillicents(),
                        CanEdit = true
                    });

                // TODO: Check the returned levels for errors
                _sharedSapProvider.AddSharedSapLevel(sharedSapList);
            }
            else
            {
                _sharedSapLevel.Name = LevelName.Trim();
                _sharedSapLevel.SupportedGameTypes = new[] { _gameType };
                _sharedSapLevel.InitialValue = InitialValue.DollarsToMillicents();
                _sharedSapLevel.CurrentValue = _sharedSapLevel.InitialValue;
                _sharedSapLevel.ResetValue = ResetValue.DollarsToMillicents();
                _sharedSapLevel.IncrementRate = _incrementRate.ToPercentage();
                _sharedSapLevel.MaximumValue = MaxValue.DollarsToMillicents();
                _sharedSapLevel.CanEdit = true;
                sharedSapList.Add(_sharedSapLevel);
                _sharedSapProvider.UpdateSharedSapLevel(sharedSapList);
            }

            base.Save();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (_sharedSapLevel == null)
            {
                return;
            }


            ResetValue = _sharedSapLevel.ResetValue.MillicentsToDollars();
            InitialValue = _sharedSapLevel.InitialValue.MillicentsToDollars();
            MaxValue = _sharedSapLevel.MaximumValue.MillicentsToDollars();
            LevelName = _sharedSapLevel.Name;
            IncrementRate = _sharedSapLevel.IncrementRate.ToPercentage();
        }

        protected override void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(propertyName);
            }
            else
            {
                base.SetError(propertyName, error);
            }
        }

        private void ValidValueFields()
        {
            SetError(nameof(ResetValue), ResetValue.Validate(false, MaxValue.DollarsToMillicents()));
            SetError(nameof(MaxValue), MaxValue == 0M ? string.Empty : MaxValue.Validate(false, 0, ResetValue.DollarsToMillicents()));
            SetError(nameof(InitialValue), InitialValue.Validate(false, MaxValue.DollarsToMillicents(), ResetValue.DollarsToMillicents()));

            RaisePropertyChanged(nameof(ResetValue), nameof(MaxValue), nameof(InitialValue), nameof(CanSave));
        }
    }
}
