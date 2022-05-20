namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Client.Data;
    using UI.Models;
    using UI.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Aristocrat.Monaco.Kernel;
    using Moq;
    using Aristocrat.Monaco.Test.Common;
    using System.Windows;
    using Aristocrat.Monaco.Hhr.Storage.Helpers;

    [TestClass]
    public class VenueRaceCollectionViewModelTests
    {
        private VenueRaceCollectionViewModel _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IPrizeInformationEntityHelper> _prizeInformationEntityHelper;
        private Mock<IEventBus> _eventBus;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IGamePlayEntityHelper> _gameEntityHelper;

        [TestInitialize]
        public void TestInitialization()
        {
           // MoqServiceManager.CreateInstance(MockBehavior.Strict);
            //MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _serviceManagerMock = MoqServiceManager.CreateInstance(MockBehavior.Default);
            _prizeInformationEntityHelper = new Mock<IPrizeInformationEntityHelper>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _disableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _gameEntityHelper = new Mock<IGamePlayEntityHelper>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(p => p.GetProperty(HHRPropertyNames.HorseResultsRunTime, It.IsAny<int>())).Returns(2400);
            _propertiesManager.Setup(p => p.GetProperty("horseAnimationOff", It.IsAny<string>())).Returns("");

            _target = new VenueRaceCollectionViewModel(
                _eventBus.Object,
                _prizeInformationEntityHelper.Object,
                _disableManager.Object,
                _gameEntityHelper.Object,
                _propertiesManager.Object);
            _target.SetupRaces(GetRaceInfo());
        }

        public CRaceInfo GetRaceInfo()
        {
            return new CRaceInfo
            {
                // Must be 10 races to match with VenueRaceCollectionViewModel.VenuesPerRow
                RaceData = new CRaceData[]
                {
                    new CRaceData { TrackDescription="Venue 1", HorseActual = "123456789ABC" },
                    new CRaceData { TrackDescription="Venue 2", HorseActual = "ABC123456789" },
                    new CRaceData { TrackDescription="Venue 3", HorseActual = "1A2B3C456789" },
                    new CRaceData { TrackDescription="Venue 4", HorseActual = "4A2B3C156789" },
                    new CRaceData { TrackDescription="Venue 5", HorseActual = "15432678" },
                    new CRaceData { TrackDescription="Venue 6", HorseActual = "87654321" },
                    new CRaceData { TrackDescription="Venue 7", HorseActual = "123456789A" },
                    new CRaceData { TrackDescription="Venue 8", HorseActual = "143256879" },
                    new CRaceData { TrackDescription="Venue 9", HorseActual = "192837465" },
                    new CRaceData { TrackDescription="Venue 10", HorseActual = "BA918237465" }
                }
            };
        }

        private void AssertTrackPositionsMatchData(VenueRaceTracksModel venueRaceTrackModel, int[] dataHorseActuals)
        {
            for (int j = 0; j < HhrUiConstants.MaxNumberOfHorses; j++)
            {
                var raceTrackEntryModel = venueRaceTrackModel.RaceTrackModels[j];

                Assert.AreEqual(raceTrackEntryModel.Position, j + 1);

                if (j < dataHorseActuals.Length)
                {
                    Assert.AreEqual(raceTrackEntryModel.FinishPosition, Array.IndexOf(dataHorseActuals, j + 1) + 1);
                    Assert.AreEqual(raceTrackEntryModel.Visibility, Visibility.Visible);
                }
                else
                {
                    Assert.AreEqual(raceTrackEntryModel.FinishPosition, 0);
                    Assert.AreEqual(raceTrackEntryModel.Visibility, Visibility.Hidden);
                }
            }
        }

        private void VerifyRaceSet(ObservableCollection<VenueRaceTracksModel> raceSet, int dataIdxStart)
        {
            var raceInfoData = GetRaceInfo().RaceData;

            for (int i = 0; i < _target.VenuesPerRow; i++)
            {
                var venueRaceTrackModel = raceSet.ToArray()[i];

                var dataHorseActuals = _target.GetHorseActualArray(raceInfoData[i+dataIdxStart].HorseActual);

                AssertTrackPositionsMatchData(venueRaceTrackModel, dataHorseActuals);

                Assert.AreEqual(raceInfoData[i+dataIdxStart].TrackDescription, venueRaceTrackModel.VenueName);
            }
        }

        [TestMethod]
        public void RecievedRaceInfo_MatchesAfterLoading()
        {
            Assert.AreEqual(_target.RaceSet1Models.Count, _target.VenuesPerRow);
            VerifyRaceSet(_target.RaceSet1Models, 0);

            Assert.AreEqual(_target.RaceSet2Models.Count, _target.VenuesPerRow);
            VerifyRaceSet(_target.RaceSet2Models, _target.VenuesPerRow);
        }
    }
}
