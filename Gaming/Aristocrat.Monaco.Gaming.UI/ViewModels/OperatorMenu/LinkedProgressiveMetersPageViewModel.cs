namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Application.Contracts.MeterPage;
    using Application.UI.OperatorMenu;
    using Aristocrat.Extensions.CommunityToolkit;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.UI.Events;
    using Aristocrat.Monaco.Application.UI.MeterPage;
    using Aristocrat.Monaco.Gaming.Contracts.Meters;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Kernel;
    using Contracts;
    using Contracts.Progressives.Linked;
    using Factories;
    using Localization.Properties;
    using Progressives;

    public class LinkedProgressiveMetersPageViewModel : MetersPageViewModelBase
    {
        private readonly ILinkedProgressiveProvider _linkedProgressiveProvider;
        private readonly IProgressiveMeterManager _progressiveMeterManager;
        private bool _hasEnabledLinkedProgressives;
        private bool _hasLinkedProgressives;

        // Not implemented
        public LinkedProgressiveMetersPageViewModel()
            : base(MeterNodePage.LinkedProgressives)
        {
            var serviceManager = ServiceManager.GetInstance();
            _linkedProgressiveProvider = serviceManager.GetService<IContainerService>()
                .Container.GetInstance<ILinkedProgressiveProvider>();

            _progressiveMeterManager = serviceManager.GetService<IProgressiveMeterManager>();
        }

        public bool HasLinkedProgressivesButNoneEnabled => !HasEnabledLinkedProgressives && HasLinkedProgressives;

        public bool HasEnabledLinkedProgressives
        {
            get => _hasEnabledLinkedProgressives;
            private set
            {
                SetProperty(ref _hasEnabledLinkedProgressives, value, nameof(HasEnabledLinkedProgressives));
                OnPropertyChanged(nameof(HasLinkedProgressivesButNoneEnabled));
            }
        }

        public bool HasLinkedProgressives
        {
            get => _hasLinkedProgressives;
            private set
            {
                SetProperty(ref _hasLinkedProgressives, value, nameof(HasLinkedProgressives));
                OnPropertyChanged(nameof(HasLinkedProgressivesButNoneEnabled));
            }
        }

        public ObservableCollection<ProgressiveDisplayMeter> LinkedProgressiveDetailMeters { get; } =
            new ObservableCollection<ProgressiveDisplayMeter>();

        // Not implemented
        protected override void OnLoaded()
        {
            base.OnLoaded();

            // Refresh each load in case game configurations have changed
            HasEnabledLinkedProgressives = IsEnabledLinkedProgressives();

            // If we don't check on the initial load, then no meters will be shown
            // until the user changes pages.
            if (HasEnabledLinkedProgressives)
            {
                InitializeMeters();
            }
            else
            {
                EventBus.Publish(
                    new OperatorMenuWarningMessageEvent(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLinkedProgressiveLevelsAdded)));
            }
        }

        protected override void InitializeMeters()
        {
            ClearMeters();

            if (!HasEnabledLinkedProgressives)
            {
                return;
            }

            LoadLinkedProgressiveMeters(GetLinkedProgressiveLevels());

        }

        // Not implemented
        protected void LoadLinkedProgressiveMeters(List<LinkedProgressiveLevel> linkedProgressiveLevels)
        {
            HasLinkedProgressives = linkedProgressiveLevels.Any();

            if (!HasLinkedProgressives)
            {
                return;
            }

            Execute.OnUIThread(
                () =>
                {
                    foreach (var level in linkedProgressiveLevels)
                    {
                        var collectionOfMeters = new ObservableCollection<DisplayMeter>();
                        foreach (var meterNode in MeterNodes)
                        {
                            collectionOfMeters.Add(_progressiveMeterManager.Build(level, meterNode, ShowLifetime));
                        }

                        LinkedProgressiveDetailMeters.Add(new ProgressiveDisplayMeter(level.LevelName,ShowLifetime, collectionOfMeters));
                    }
                });
        }

        private bool IsEnabledLinkedProgressives()
        {
            return GetLinkedProgressiveLevels().Any();
        }

        private List<LinkedProgressiveLevel> GetLinkedProgressiveLevels()
        {
            return HasEnabledLinkedProgressives ? _linkedProgressiveProvider.GetLinkedProgressiveLevels().ToList() : new List<LinkedProgressiveLevel>();
        }

        private void ClearMeters()
        {
            foreach (var meter in LinkedProgressiveDetailMeters)
            {
                meter.Dispose();
            }

            LinkedProgressiveDetailMeters.Clear();
        }
    }
}