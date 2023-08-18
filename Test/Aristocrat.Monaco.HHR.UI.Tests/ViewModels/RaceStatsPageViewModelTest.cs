namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using System.Windows;
    using Hhr.Services;
    using Client.Data;
    using Kernel;
    using Test.Common;
    using UI.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RaceStatsPageViewModelTest
    {
        private RaceStatsPageViewModel _target;

        private Mock<IPropertiesManager> _propertiesManagerMock;
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IEventBus> _eventBusMock;
        private Mock<IPrizeDeterminationService> _prizeDeterminationService;

        [TestInitialize]
        public void TestInitialization()
        {
            _serviceManagerMock = MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManagerMock = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _eventBusMock = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _prizeDeterminationService = new Mock<IPrizeDeterminationService>(MockBehavior.Default);

            UiProperties.ManualHandicapRemainingTime = 200;

            if (Application.Current == null)
            {
                new Application();
            }
            _target = new RaceStatsPageViewModel(_propertiesManagerMock.Object, _prizeDeterminationService.Object, _eventBusMock.Object);
            ClientProperties.RaceStatTimeOut = 10;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public async void RaceStaPageLoaded_Init_VerifyPageTimeout()
        {
            await _target.Init(Menu.Command.RaceStats);

            Assert.AreEqual(ClientProperties.RaceStatTimeOut, _target.TimerInfo.Timeout);
        }

        [TestMethod]
        public void HorseNumberWinningOddsStatInfo_SetTest()
        {
            var myRaceInfo = GetRaceInfoSample();

            _target.SetData(myRaceInfo);

            for (var raceIndex = 0; raceIndex < myRaceInfo.RaceData.Length; raceIndex++)
            {
                UiProperties.CurrentRaceIndex = raceIndex;

                for (int statChartIndex = 0; statChartIndex < myRaceInfo.RaceData[raceIndex].Racestat.Length; statChartIndex++)
                {
                    for (int horseIndex = 0; horseIndex < myRaceInfo.RaceData[raceIndex].Runners; horseIndex++)
                    {
                        //Test If Correct horse no. is going to display in RaceStatPageView
                        Assert.AreEqual(horseIndex + 1, _target.RaceStatsCharts[statChartIndex].WinningOddsList[horseIndex].HorseNo);

                        //Test If Correct WinningOdds number is assigned to StatChart
                        Assert.AreEqual((int)myRaceInfo.RaceData[raceIndex].Racestat[statChartIndex].StatNum[horseIndex], _target.RaceStatsCharts[statChartIndex].WinningOddsList[horseIndex].WinningOdds);
                    }

                    //Test If Information of all charts is set correctly as per race info for all the races
                    Assert.AreEqual(myRaceInfo.RaceData[raceIndex].Racestat[statChartIndex].StatStr, _target.RaceStatsCharts[statChartIndex].ChartInfo);
                }
            }
        }

        private CRaceInfo GetRaceInfoSample()
        {
            return new CRaceInfo
            {
                /* First Race */
                RaceData = new[]
               {
                    new CRaceData
                    {
                        Runners = 8 ,
                        Racestat = new[]
                        {
                            new CRaceStat
                            {
                                StatStr="Trainer Earnings",
                                StatNum = new uint[] {4, 5, 4, 0, 1, 8, 7, 3}
                            },
                            new CRaceStat
                            {
                                StatStr="HORSE SECOND PLACES(S)",
                                StatNum = new uint[] {3, 5, 4, 0, 8, 1, 7, 4}
                            },
                            new CRaceStat
                            {
                                StatStr="TRAINER THIRD PLACE(S)",
                                StatNum = new uint[] {5, 3, 0, 4, 8, 1, 7, 4}
                            }
                        }
                    },

                    /*2nd Race */
                   new CRaceData
                    {
                        Runners = 9 ,
                        Racestat = new[]
                        {
                            new CRaceStat
                            {
                                StatStr="HORSE SECOND PLACES(S)",
                                StatNum = new uint[] {5, 3, 0, 4, 8, 1, 7, 4,9}
                            },
                            new CRaceStat
                            {
                                StatStr="TRAINER THIRD PLACE(S)",
                                StatNum = new uint[] {5, 3,9, 0, 4, 8, 1, 7, 4}
                            },
                            new CRaceStat
                            {
                                StatStr="TRAINER SECOND PLACE(S)",
                                StatNum = new uint[] {5, 3,9, 0, 4, 8, 1, 9, 4}
                            }
                        }
                    },
               }
            };
        }
    }
}
