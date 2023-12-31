﻿namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using MVVM.ViewModel;

    /// <summary>
    ///     Level model class
    /// </summary>
    public class LevelModel : BaseEntityViewModel
    {
        private decimal _maxValue;
        private decimal _initialValue;
        private string _currentValue;
        private decimal _resetValue;
        private string _overflowValue;
        private decimal _incrementRate;
        private decimal _minimumRequiredValue;
        private ProgressiveErrors _levelErrors;
        private IReadOnlyCollection<IViewableLinkedProgressiveLevel> _linkedLevels;
        private IReadOnlyCollection<IViewableSharedSapLevel> _sharedSapLevels;
        private ObservableCollection<string> _selectableLevelTypes = new ObservableCollection<string>();
        private ObservableCollection<LevelDefinition> _selectableLevelNames = new ObservableCollection<LevelDefinition>();
        private string _selectableLevelType;
        private LevelDefinition _selectableLevel;
        private bool _selectableLevelNameTooLong;
        private readonly bool _canEdit;

        public LevelModel(
            IViewableProgressiveLevel level,
            IReadOnlyCollection<IViewableSharedSapLevel> customSapLevels,
            IReadOnlyCollection<IViewableLinkedProgressiveLevel> linkedLevels,
            int gameCount,
            IViewableSharedSapLevel sharedSapLevel)
        {
            GameCount = gameCount;

            AssociatedProgressiveLevel = level;
            AssignedProgressiveInfo = new AssignableProgressiveId(
                level.AssignedProgressiveId.AssignedProgressiveType,
                level.AssignedProgressiveId.AssignedProgressiveKey);

            if (LevelType == ProgressiveLevelType.LP)
            {
                var linkLevel = linkedLevels.FirstOrDefault(x => x.LevelName == LevelName);
                if (linkLevel != null)
                {
                    CurrentValue = linkLevel.Amount.CentsToDollars().FormattedCurrencyString(true);
                }
            }
            else
            {
                CurrentValue = sharedSapLevel?.CurrentValue.MillicentsToDollars().FormattedCurrencyString(true) ??
                               level.CurrentValue.MillicentsToDollars().FormattedCurrencyString(true);
            }

            if (LevelType == ProgressiveLevelType.Selectable)
            {
                MinimumRequiredValue = level.ResetValue.MillicentsToDollars();
            }

            InitialValue = level.InitialValue.MillicentsToDollars();
            ResetValue = level.ResetValue.MillicentsToDollars();
            IncrementRate = level.IncrementRate.ToPercentage();
            MaxValue = level.MaximumValue.MillicentsToDollars();
            OverflowValue = sharedSapLevel?.Overflow.MillicentsToDollars().FormattedCurrencyString(true) ??
                            level.Overflow.MillicentsToDollars().FormattedCurrencyString(true);

            _sharedSapLevels = customSapLevels;
            _linkedLevels = linkedLevels;
            LevelErrors = level.Errors;
            _selectableLevelType = DetermineSelectableLevelType();
            _canEdit = level.CanEdit;
            _selectableLevel = new LevelDefinition(LevelName, AssignedProgressiveInfo.AssignedProgressiveKey);

            LoadSelectableTypes();
            LoadSelectableNames();
        }

        /// <summary>
        ///     Gets whether or not this level can be saved
        /// </summary>
        public bool CanSave => !HasErrors && (LevelType != ProgressiveLevelType.Selectable ||
                                              !LevelSelectionEnabled ||
                                              SelectableLevel != null);

        /// <summary>
        ///     Gets or sets the viewable progressive level associated with the level model.
        /// </summary>
        public IViewableProgressiveLevel AssociatedProgressiveLevel { get; set; }

        /// <summary>
        ///     Gets or sets the progressive id
        /// </summary>
        public int ProgressiveId => AssociatedProgressiveLevel.ProgressiveId;

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string LevelName => AssociatedProgressiveLevel.LevelName;

        /// <summary>
        ///     Gets or sets the level id
        /// </summary>
        public int LevelId => AssociatedProgressiveLevel.LevelId;

        /// <summary>
        ///     Gets or sets the game defined level type
        /// </summary>
        public ProgressiveLevelType LevelType => AssociatedProgressiveLevel.LevelType;

        public void SetCSAPLevels(IReadOnlyCollection<IViewableSharedSapLevel> levels)
        {
            _sharedSapLevels = levels;
            LoadSelectableNames();
        }

        public void SetLinkedLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> levels)
        {
            _linkedLevels = levels;
            LoadSelectableNames();
        }

        /// <summary>
        ///     Gets or sets the level names that can be selected or configured
        /// </summary>
        public ObservableCollection<LevelDefinition> SelectableLevels
        {
            get => _selectableLevelNames;
            set
            {
                _selectableLevelNames = value;
                RaisePropertyChanged(nameof(SelectableLevels));
            }
        }

        /// <summary>
        ///     Gets or sets the name of the level that was selected for configuration
        /// </summary>
        public LevelDefinition SelectableLevel
        {
            get => LevelType == ProgressiveLevelType.Sap ? new LevelDefinition(LevelName) : _selectableLevel;
            set
            {
                if (LevelType == ProgressiveLevelType.Sap)
                {
                    return;
                }

                var tempValue = value;
                var assignableType = AssignableProgressiveType.None;
                var selectedCustomSapLevel = _sharedSapLevels.FirstOrDefault(x => x.LevelAssignmentKey == tempValue?.AssignmentKey);

                if (selectedCustomSapLevel != null)
                {
                    // Validation is being done in the view model when the list of names
                    // is generated. We will not allow an invalid name to be populated
                    // for an option to the user. 
                    CurrentValue = selectedCustomSapLevel.CurrentValue.MillicentsToDollars().FormattedCurrencyString(true);
                    InitialValue = selectedCustomSapLevel.ResetValue.MillicentsToDollars();
                    ResetValue = selectedCustomSapLevel.ResetValue.MillicentsToDollars();
                    assignableType = AssignableProgressiveType.CustomSap;
                }
                else if (ResetValue != AssociatedProgressiveLevel.ResetValue)
                {
                    ResetValue = AssociatedProgressiveLevel.ResetValue.MillicentsToDollars();
                }

                var selectedLinkLevel = _linkedLevels.FirstOrDefault(x => x.LevelName == tempValue?.AssignmentKey);

                if (selectedLinkLevel != null)
                {
                    // Validation is being done in the view model when the list
                    // of valid levels is being populated for selection
                    CurrentValue = selectedLinkLevel.Amount.CentsToDollars().FormattedCurrencyString(true);
                    assignableType = AssignableProgressiveType.Linked;
                }

                AssignedProgressiveInfo = new AssignableProgressiveId(assignableType, value?.AssignmentKey);
                _selectableLevel = value;
                RaisePropertyChanged(nameof(SelectableLevel), nameof(CanSave));
            }
        }

        /// <summary>
        ///     Gets or sets if the selectable level name is too long.
        /// </summary>
        public bool SelectableLevelNameTooLong
        {
            get => _selectableLevelNameTooLong;
            set
            {
                _selectableLevelNameTooLong = value;
                RaisePropertyChanged(nameof(SelectableLevelNameTooLong));
            }
        }

        /// <summary>
        ///     Gets or sets the selected progressive
        /// </summary>
        public AssignableProgressiveId AssignedProgressiveInfo { get; set; }

        public ProgressiveErrors LevelErrors
        {
            get => _levelErrors;
            set
            {
                _levelErrors = value;
                RaisePropertyChanged(nameof(LevelErrors));
            }
        }

        /// <summary>
        ///     Gets or sets the increment rate for the level
        /// </summary>
        public decimal IncrementRate
        {
            get => _incrementRate;
            set
            {
                _incrementRate = value;
                RaisePropertyChanged(nameof(IncrementRate));
            }
        }

        /// <summary>
        ///     Gets or sets the max value for the level
        /// </summary>
        public decimal MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                RaisePropertyChanged(nameof(MaxValue));
            }
        }

        /// <summary>
        ///     Gets or sets the initial value.
        /// </summary>
        public decimal InitialValue
        {
            get => _initialValue;
            set
            {
                // Initial value should be >= reset value
                _initialValue = value;
                var errors = _initialValue.Validate(
                    ResetValue <= 0M,
                    MaxValue.DollarsToMillicents(),
                    ResetValue.DollarsToMillicents());
                ClearOrSetError(errors, nameof(InitialValue));

                RaisePropertyChanged(nameof(InitialValue));
            }
        }

        public decimal ResetValue
        {
            get => _resetValue;
            set
            {
                _resetValue = value;
                RaisePropertyChanged(nameof(ResetValue));
            }
        }

        /// <summary>
        ///     Gets or sets the current value.
        /// </summary>
        public string CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                RaisePropertyChanged(nameof(CurrentValue));
            }
        }

        /// <summary>
        ///     Gets or sets the overflow value for the level
        /// </summary>
        public string OverflowValue
        {
            get => _overflowValue;
            set
            {
                _overflowValue = value;
                RaisePropertyChanged(nameof(OverflowValue));
            }
        }

        /// <summary>
        ///     Gets or sets the minimum required value (for selectable levels) as initially supplied by the game
        /// </summary>
        public decimal MinimumRequiredValue
        {
            get => _minimumRequiredValue;
            set
            {
                _minimumRequiredValue = value;
                RaisePropertyChanged(nameof(MinimumRequiredValue));
            }
        }

        public bool CanSetInitialValue => LevelType == ProgressiveLevelType.Sap && _canEdit;

        public bool LevelSelectionEnabled => LevelType != ProgressiveLevelType.Sap &&
                                                !SelectableLevelType.Equals(
                                                    Resources.NoProgressive,
                                                    StringComparison.InvariantCulture);
      
        public ObservableCollection<string> SelectableLevelTypes
        {
            get => _selectableLevelTypes;
            set
            {
                _selectableLevelTypes = value;
                RaisePropertyChanged(nameof(SelectableLevelTypes));
            }
        }

        public string SelectableLevelType
        {
            get => _selectableLevelType;
            set
            {
                if (LevelType != ProgressiveLevelType.Selectable || _selectableLevelType == value)
                {
                    return;
                }

                _selectableLevelType = value;
                LoadSelectableNames();
                RaisePropertyChanged(nameof(SelectableLevelType), nameof(LevelSelectionEnabled), nameof(CanSave));
            }
        }

        public int GameCount { get; }
        
        private void ClearOrSetError(string errors, string propertyName)
        {
            if (string.IsNullOrEmpty(errors))
            {
                ClearErrors(propertyName);
                RaisePropertyChanged(nameof(CanSave));
            }
            else
            {
                SetError(propertyName, errors);
                RaisePropertyChanged(nameof(CanSave));
            }
        }

        private string DetermineSelectableLevelType()
        {
            if (AssociatedProgressiveLevel.AssignedProgressiveId.AssignedProgressiveType ==
                AssignableProgressiveType.AssociativeSap)
            {
                return Resources.SharedStandaloneProgressive;
            }

            if (LevelType == ProgressiveLevelType.Sap ||
                AssignedProgressiveInfo.AssignedProgressiveType == AssignableProgressiveType.CustomSap)
                return Resources.StandaloneProgressive;

            if (LevelType == ProgressiveLevelType.LP ||
                AssignedProgressiveInfo.AssignedProgressiveType == AssignableProgressiveType.Linked)
                return Resources.LinkedProgressive;

            return Resources.NoProgressive;
        }

        private void LoadSelectableTypes()
        {
            var currentSelectedType = SelectableLevelType;
            SelectableLevelTypes.Clear();
            if (AssociatedProgressiveLevel.AssignedProgressiveId.AssignedProgressiveType ==
                AssignableProgressiveType.AssociativeSap)
            {
                SelectableLevelTypes.Add(Resources.SharedStandaloneProgressive);
            }
            else if (LevelType == ProgressiveLevelType.Sap)
            {
                SelectableLevelTypes.Add(Resources.StandaloneProgressive);
            }
            else if (LevelType == ProgressiveLevelType.Selectable)
            {
                SelectableLevelTypes.Add(Resources.NoProgressive);
                SelectableLevelTypes.Add(Resources.StandaloneProgressive);
                SelectableLevelTypes.Add(Resources.LinkedProgressive);
            }
            else if (LevelType == ProgressiveLevelType.LP)
            {
                SelectableLevelTypes.Add(Resources.LinkedProgressive);
            }

            if (SelectableLevelTypes.Contains(currentSelectedType))
            {
                SelectableLevelType = currentSelectedType;
            }
        }

        private void LoadSelectableNames()
        {
            var selectableLevels = new ObservableCollection<LevelDefinition>();

            if (LevelType == ProgressiveLevelType.Sap)
            {
                selectableLevels.Add(new LevelDefinition(LevelName));
                SelectableLevel = selectableLevels.First();
            }
            else if (_selectableLevelType.Equals(Resources.LinkedProgressive, StringComparison.InvariantCulture))
            {
                selectableLevels.AddRange(_linkedLevels.Select(l => new LevelDefinition(l.LevelName)));
            }
            else if (_selectableLevelType.Equals(Resources.StandaloneProgressive, StringComparison.InvariantCulture))
            {
                selectableLevels.AddRange(_sharedSapLevels.Select(l => new LevelDefinition(l.Name, l.LevelAssignmentKey)));
            }

            SelectableLevels = selectableLevels;
            if (LevelType == ProgressiveLevelType.Sap)
            {
                SelectableLevel = selectableLevels.FirstOrDefault();
            }
            else
            {
                SelectableLevel = selectableLevels.FirstOrDefault(
                    x => x.AssignmentKey == AssignedProgressiveInfo.AssignedProgressiveKey);
            }
        }

        public class LevelDefinition
        {
            private bool Equals(LevelDefinition other)
            {
                return string.Equals(Name, other.Name, StringComparison.InvariantCulture) && string.Equals(
                    AssignmentKey,
                    other.AssignmentKey,
                    StringComparison.InvariantCulture);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((LevelDefinition)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Name != null ? StringComparer.InvariantCulture.GetHashCode(Name) : 0) * 397) ^ (AssignmentKey != null ? StringComparer.InvariantCulture.GetHashCode(AssignmentKey) : 0);
                }
            }

            public static bool operator ==(LevelDefinition left, LevelDefinition right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(LevelDefinition left, LevelDefinition right)
            {
                return !Equals(left, right);
            }

            public string Name { get; }

            public string AssignmentKey { get; }

            public LevelDefinition(string name)
            {
                Name = name;
                AssignmentKey = name;
            }

            public LevelDefinition(string name, string assignmentKey)
            {
                Name = name;
                AssignmentKey = assignmentKey;
            }
        }
    }
}