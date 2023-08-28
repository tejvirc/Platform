namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Windows;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using Menu;
    using Models;
    using Storage.Helpers;
    using Aristocrat.Monaco.Hhr.Services;
    using Client.Data;
    using Aristocrat.Extensions.CommunityToolkit;

    public class PreviousRaceResultPageViewModel : HhrMenuPageViewModelBase
    {
        private readonly IPrizeInformationEntityHelper _prizeInformationEntityHelper;

        public PreviousRaceResultPageViewModel(IPrizeInformationEntityHelper prizeInformationEntityHelper)
        {
            _prizeInformationEntityHelper = prizeInformationEntityHelper ?? throw new ArgumentNullException(nameof(prizeInformationEntityHelper));

            Util.LoadImageResources();
        }

        public string WagerLabel1 { get; set; }

        public string WagerLabel2 { get; set; }

        public ObservableCollection<PreviousRaceResultModel> PreviousResultCollection1 { get; set; } =
            new ObservableCollection<PreviousRaceResultModel>();

        public ObservableCollection<PreviousRaceResultModel> PreviousResultCollection2 { get; set; } =
            new ObservableCollection<PreviousRaceResultModel>();

        public override Task Init(Command command)
        {
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.ManualHandicap));
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Help));
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.ReturnToGame));

            SetupData();

            return Task.CompletedTask;
        }

        public override void Reset()
        {
            base.Reset();
            Execute.OnUIThread(
                () =>
                {
                    PreviousResultCollection1.Clear();
                    PreviousResultCollection2.Clear();
                });
        }

        private void PageCommandHandler(object command)
        {
            switch ((Command)command)
            {
                case Command.ManualHandicap:
                    OnHhrButtonClicked(Command.ManualHandicapHelp);
                    break;
                case Command.Help:
                    OnHhrButtonClicked(Command.Help);
                    break;
                case Command.ReturnToGame:
                    OnHhrButtonClicked(Command.ReturnToGame);
                    break;
            }
        }

        private string GetWagerString(uint wager)
        {
            var wagerText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WagerCategoryTicketText);

            return $"{wagerText}: {((long)wager).CentsToDollars().FormattedCurrencyString()}";
        }

        void ConfigureResultsForDisplay(
            PrizeInformation prizeInformation,
            int startIdx,
            int endIdx,
            ObservableCollection<PreviousRaceResultModel> collection)
        {
            for (var i = startIdx; i < endIdx; i++)
            {
                var horseSelection = prizeInformation.RaceInfo.RaceData[i].HorseSelection;
                var horseActual = prizeInformation.RaceInfo.RaceData[i].HorseActual;

                var resultList = new ObservableCollection<HorseModel>();

                // The order of the horses is the manual picks (Horse selection)
                for (int j = 0; j < HhrUiConstants.NumberOfHorsePickPositions; j++)
                {
                    var selected = Convert.ToInt32(horseSelection[j].ToString(), 16);
                    var actual = Convert.ToInt32(horseActual[j].ToString(), 16);

                    resultList.Add(
                        new HorseModel(
                            actual,
                            j,
                            selected == actual));
                }

                var previousRaceResult = new PreviousRaceResultModel(
                    prizeInformation.RaceInfo.RaceData[i].TrackDescription,
                    prizeInformation.RaceInfo.RaceData[i].RaceDate,
                    resultList
                );

                Application.Current.Dispatcher.Invoke(
                    () => { collection.Add(previousRaceResult); });
            }
        }

        private void SetupData()
        {
            PrizeInformation prizeInformation = _prizeInformationEntityHelper.PrizeInformation;

            // The first Race Info menu access after sram clear should show Previous Results page with each race set having
            // its races enumerated from horse number one to eight.
            if (prizeInformation != null)
            {
                WagerLabel1 = GetWagerString(prizeInformation.RaceSet1Wager);
                WagerLabel2 = GetWagerString(prizeInformation.RaceSet2Wager);
            }
            else
            {
                prizeInformation = new PrizeInformation
                {
                    RaceInfo = new CRaceInfo
                    {
                        RaceData = new CRaceData[HhrUiConstants.NumberOfRacesPerRaceSet * 2]
                    }
                };

                for (int i = 0; i < HhrUiConstants.NumberOfRacesPerRaceSet * 2; i++)
                {
                    prizeInformation.RaceInfo.RaceData[i] = new CRaceData
                    {
                        TrackDescription = "",
                        RaceDate = "",
                        HorseActual = "12345678",
                        HorseSelection = "00000000"
                    };
                }
            }

            ConfigureResultsForDisplay(
                prizeInformation,
                0,
                HhrUiConstants.NumberOfRacesPerRaceSet,
                PreviousResultCollection1);

            ConfigureResultsForDisplay(
                prizeInformation,
                HhrUiConstants.NumberOfRacesPerRaceSet,
                HhrUiConstants.NumberOfRacesPerRaceSet * 2,
                PreviousResultCollection2);
            OnPropertyChanged(nameof(WagerLabel1));
            OnPropertyChanged(nameof(WagerLabel2));
        }
    }
}
