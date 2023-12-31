﻿namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using ConfigWizard;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM.Command;

    [CLSCompliant(false)]
    public class BeagleBonePageViewModel : InspectionWizardViewModelBase
    {
        private readonly IBeagleBoneController _beagleBoneController;

        private readonly Dictionary<string, LightShows> _shows = new
            Dictionary<string, LightShows>
        {
            { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DefaultBeagleBoneLightShowText), LightShows.Default },
            { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StartBeagleBoneLightShowText), LightShows.Start },
            { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AttractBeagleBoneLightShowText), LightShows.Attract },
            { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.REDSBeagleBoneLightShowText), LightShows.Reds },
            { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NormalWinBeagleBoneLightShowText), LightShows.Win1 },
            { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GoodWinBeagleBoneLightShowText), LightShows.Win2 },
            { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BigWinBeagleBoneLightShowText), LightShows.Win4 }
        };

        private string _showOverrideSelection;

        public static Dictionary<string, string> LightShowChoices =>
            new Dictionary<string, string>
            {
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DefaultBeagleBoneLightShowText),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DefaultBeagleBoneLightShowText)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AttractBeagleBoneLightShowText),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AttractBeagleBoneLightShowText)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StartBeagleBoneLightShowText),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StartBeagleBoneLightShowText)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.REDSBeagleBoneLightShowText),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.REDSBeagleBoneLightShowText)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NormalWinBeagleBoneLightShowText),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NormalWinBeagleBoneLightShowText)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GoodWinBeagleBoneLightShowText),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GoodWinBeagleBoneLightShowText)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BigWinBeagleBoneLightShowText),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BigWinBeagleBoneLightShowText)
                }
            };

        public BeagleBonePageViewModel(bool isWizard) : base(isWizard)
        {
            _beagleBoneController = ServiceManager.GetInstance().GetService<IBeagleBoneController>();

            _showOverrideSelection = PropertiesManager.GetValue(
                ApplicationConstants.ShowOverrideSelectionKey,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DefaultBeagleBoneLightShowText));

            SendShowCommand = new ActionCommand<object>(_ => SendShow());
        }

        public IActionCommand SendShowCommand { get; }

        public string ShowOverrideSelection
        {
            get => _showOverrideSelection;
            set
            {
                _ = SetProperty(ref _showOverrideSelection, value, nameof(ShowOverrideSelection));
                PropertiesManager.SetProperty(
                    ApplicationConstants.ShowOverrideSelectionKey,
                    _showOverrideSelection);
            }
        }

        protected override void SetupNavigation()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = true;
            }
        }

        protected override void SaveChanges()
        {
        }

        private void SendShow()
        {
            var show = _shows[_showOverrideSelection];

            Inspection?.SetTestName($"Light show '{_showOverrideSelection}'");

            _beagleBoneController.SendShow(show);
        }
    }
}
