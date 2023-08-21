namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.UI.Events;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Kernel;
    using Localization.Properties;

    [CLSCompliant(false)]
    public class CustomSAPMetersPageViewModel : MetersPageViewModelBase
    {
        private readonly ISharedSapProvider _sharedSapProvider;
        private readonly IProgressiveLevelProvider _progressiveLevelProvider;
        private readonly IProgressiveMeterManager _metersManager;

        private IReadOnlyList<CountDisplayMeter> _sharedSAPMeters = new List<CountDisplayMeter>();
        private bool _hasEnabledCustomSAPLevels;

        public CustomSAPMetersPageViewModel()
            : base(null, true)
        {
            _sharedSapProvider = ServiceManager
                .GetInstance()
                .GetService<IContainerService>()
                .Container
                .GetInstance<ISharedSapProvider>();

            _progressiveLevelProvider = ServiceManager
                .GetInstance()
                .GetService<IContainerService>()
                .Container
                .GetInstance<IProgressiveLevelProvider>();

            _metersManager = ServiceManager
                .GetInstance()
                .GetService<IProgressiveMeterManager>();
        }

        public IReadOnlyList<CountDisplayMeter> SharedSAPMeters
        {
            get => _sharedSAPMeters;
            private set => SetProperty(ref _sharedSAPMeters, value);
        }

        public bool HasEnabledCustomSAPLevels
        {
            get => _hasEnabledCustomSAPLevels;
            private set => SetProperty(ref _hasEnabledCustomSAPLevels, value);
        }

        protected override void UpdateMeters()
        {
            foreach (var meter in SharedSAPMeters)
            {
                meter.ShowLifetime = ShowLifetime;
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            SetupMeters();
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            UpdateStatusText();
            base.OnOperatorCultureChanged(evt);
        }

        protected override void InitializeMeters()
        {
        }

        protected override void UpdateStatusText()
        {
            if (!HasEnabledCustomSAPLevels)
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoCustomSapLevelsAdded)));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        private void SetupMeters()
        {
            var associativeSapKeys = _progressiveLevelProvider.GetProgressiveLevels().Select(l => l.AssignedProgressiveId)
                .Where(a => a.AssignedProgressiveType == AssignableProgressiveType.AssociativeSap).ToArray();
            var levels = _sharedSapProvider.ViewSharedSapLevels().Where(
                l =>
                    !associativeSapKeys.Any(k => k.AssignedProgressiveKey.Equals(l.LevelAssignmentKey))).ToArray();

            HasEnabledCustomSAPLevels = levels.Any();
            if (!HasEnabledCustomSAPLevels)
            {
                SharedSAPMeters = new List<CountDisplayMeter>();
                return;
            }

            SharedSAPMeters = levels.Where(
                level => IsMeterProvided(level.Id, ProgressiveMeters.SharedLevelWinOccurrence) &&
                         IsMeterProvided(level.Id, ProgressiveMeters.SharedLevelWinAccumulation)).Select(
                level => new CountDisplayMeter(
                    level.Name,
                    GetMeter(level.Id, ProgressiveMeters.SharedLevelWinOccurrence),
                    GetMeter(level.Id, ProgressiveMeters.SharedLevelWinAccumulation),
                    ShowLifetime)).ToList();
        }

        private bool IsMeterProvided(Guid sharedLevelId, string meterName) =>
            _metersManager.IsMeterProvided(sharedLevelId, meterName);

        private IMeter GetMeter(Guid sharedLevelId, string meterName) =>
            _metersManager.GetMeter(sharedLevelId, meterName);
    }
}
