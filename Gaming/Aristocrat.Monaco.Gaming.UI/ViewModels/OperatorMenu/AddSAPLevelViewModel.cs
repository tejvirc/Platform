namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.Remoting.Contexts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Aristocrat.Monaco.Gaming.UI.Models;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using FluentValidation;
    using Localization.Properties;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

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

        [CustomValidation(typeof(AddSAPLevelViewModel), nameof(LevelNameValidate))]
        public string LevelName
        {
            get => _levelName;

            set
            {
                SetProperty(ref _levelName, value);
                OnPropertyChanged(nameof(CanSave));
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

        private void ValidValueFields()
        {
            ValidateProperty(ResetValue, nameof(ResetValue));
            ValidateProperty(MaxValue, nameof(MaxValue));
            ValidateProperty(InitialValue, nameof(InitialValue));

            OnPropertyChanged(nameof(ResetValue));
            OnPropertyChanged(nameof(MaxValue));
            OnPropertyChanged(nameof(InitialValue));
            OnPropertyChanged(nameof(CanSave));
        }

        public static ValidationResult LevelNameValidate(string name, ValidationContext context)
        {
            AddSAPLevelViewModel instance = (AddSAPLevelViewModel)context.ObjectInstance;
            var errors = "";
            var trimmedName = name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyStringNotAllowErrorMessage);
            }
            else if (instance._sharedSapLevel?.Name != trimmedName && instance._sharedSapProvider.ViewSharedSapLevels().Any(x => x.Name == trimmedName))
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UniqueLevelNameError);
            }

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }
    }
}
