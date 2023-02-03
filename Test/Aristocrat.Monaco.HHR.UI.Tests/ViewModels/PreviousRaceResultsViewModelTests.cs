namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using Hhr.Services;
    using Client.Data;
    using Storage.Helpers;
    using Test.Common;
    using UI.Models;
    using UI.Menu;
    using UI.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PreviousRaceResultsViewModelTests
    {
        private PreviousRaceResultPageViewModel _target;

        private Mock<IPrizeInformationEntityHelper> _prizeInformationEntityHelper = new Mock<IPrizeInformationEntityHelper>(MockBehavior.Default);

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);

            if (Application.Current == null)
            {
                new Application();
            }

            _target = new PreviousRaceResultPageViewModel(_prizeInformationEntityHelper.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        public PrizeInformation CreatePrizeInformation()
        {
            return new PrizeInformation
            {
                RaceSet1Wager = 300,
                RaceSet2Wager = 600,
                RaceInfo = new CRaceInfo
                {
                    RaceData = new CRaceData[]
                    {
                            new CRaceData { TrackDescription="Venue 1", RaceDate="Date 1", HorseSelection="12345678", HorseActual="123456789ABC" },
                            new CRaceData { TrackDescription="Venue 2", RaceDate="Date 2", HorseSelection="CBA12345", HorseActual="ABC123456789" },
                            new CRaceData { TrackDescription="Venue 3", RaceDate="Date 3", HorseSelection="A12B3C45", HorseActual="1A2B3C456789" },
                            new CRaceData { TrackDescription="Venue 4", RaceDate="Date 4", HorseSelection="4A2C3A15", HorseActual="4A2B3C156789" },
                            new CRaceData { TrackDescription="Venue 5", RaceDate="Date 5", HorseSelection="15432687", HorseActual="15432678" },
                            new CRaceData { TrackDescription="Venue 6", RaceDate="Date 6", HorseSelection="78563412", HorseActual="87654321" },
                            new CRaceData { TrackDescription="Venue 7", RaceDate="Date 7", HorseSelection="18365473", HorseActual="123456789A" },
                            new CRaceData { TrackDescription="Venue 8", RaceDate="Date 8", HorseSelection="74325681", HorseActual="143256879" },
                            new CRaceData { TrackDescription="Venue 9", RaceDate="Date 9", HorseSelection="19283746", HorseActual="192837465" },
                            new CRaceData { TrackDescription="Venue 10", RaceDate="Date 10", HorseSelection="BA918253", HorseActual="BA918237465" }
                    }
                }
            };
        }

        [TestMethod]
        public void PreviousResults_FirstAccess()
        {
            _prizeInformationEntityHelper.Setup(p => p.PrizeInformation).Returns((PrizeInformation)null);

            _target.Init(Command.PreviousResults);

            for (int raceIdx = 0; raceIdx < HhrUiConstants.NumberOfRacesPerRaceSet; raceIdx++)
            {
                var previousRaceResultModel = _target.PreviousResultCollection1[raceIdx];

                Assert.AreEqual(previousRaceResultModel.Name, string.Empty);
                Assert.AreEqual(previousRaceResultModel.Date, string.Empty);

                var horseCollection = previousRaceResultModel.HorseCollection;

                for (int j = 0; j < 8; j++)
                {
                    // The first Race Info menu access after sram clear should show Previous Results page with each race set having
                    // its races enumerated from horse number one to eight.
                    Assert.AreEqual(horseCollection[j].Number, j + 1);
                }
            }
        }

        [TestMethod]
        public void PreviousResults_PicksAndWinnersMatch()
        {
            var prizeInfo = CreatePrizeInformation();

            _prizeInformationEntityHelper.Setup(m => m.PrizeInformation).Returns(prizeInfo);

            _target.Init(Command.PreviousResults);

            Assert.AreEqual(_target.PreviousResultCollection1.Count, HhrUiConstants.NumberOfRacesPerRaceSet);
            Assert.AreEqual(_target.PreviousResultCollection2.Count, HhrUiConstants.NumberOfRacesPerRaceSet);

            Assert.AreEqual("Wager: $3.00", _target.WagerLabel1);
            Assert.AreEqual("Wager: $6.00", _target.WagerLabel2);

            // Race set 1
            VerifyRaceSet(0, _target.PreviousResultCollection1);

            // Race set 2
            VerifyRaceSet(HhrUiConstants.NumberOfRacesPerRaceSet, _target.PreviousResultCollection2);
        }

        private void VerifyRaceSet(int testDataIdxOffset, ObservableCollection<PreviousRaceResultModel> collection)
        {
            var prizeInfo = CreatePrizeInformation();

            for (int raceIdx = 0; raceIdx < HhrUiConstants.NumberOfRacesPerRaceSet; raceIdx++)
            {
                var viewModelRaceResult = collection[raceIdx];
                var testDataRaceResult = prizeInfo.RaceInfo.RaceData[raceIdx + testDataIdxOffset];

                Assert.AreEqual(viewModelRaceResult.Name, testDataRaceResult.TrackDescription);
                Assert.AreEqual(viewModelRaceResult.Date, testDataRaceResult.RaceDate);

                var testHorseSelections = testDataRaceResult.HorseSelection
                    .ToCharArray()
                    .Select(ch => Convert.ToInt32(ch.ToString(), 16))
                    .ToList();

                var testHorseActuals = testDataRaceResult.HorseActual
                    .ToCharArray()
                    .Select(ch => Convert.ToInt32(ch.ToString(), 16))
                    .ToList();

                for (int horseIdx = 0; horseIdx < 8; horseIdx++)
                {
                    var horseModel = viewModelRaceResult.HorseCollection[horseIdx];
                    var testSelectedHorse = testHorseSelections[horseIdx];
                    var testHorseActual = testHorseActuals[horseIdx];

                    // Verify the (auto/quick/manual) picked horses appear in the order expected
                    Assert.AreEqual(horseModel.Number, testHorseActual);

                    // Verify the horse position is marked as a match if the horse actual and horse selection index matches
                    Assert.AreEqual(testSelectedHorse == testHorseActual, horseModel.IsCorrectPick);
                }
            }
        }
    }
}
