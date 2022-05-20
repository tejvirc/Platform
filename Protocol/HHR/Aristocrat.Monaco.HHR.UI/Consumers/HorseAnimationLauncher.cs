namespace Aristocrat.Monaco.Hhr.UI.Consumers
{
    using System;
    using System.Reflection;
    using System.Windows.Media;
    using Controls;
    using Gaming.Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using log4net;
    using MVVM;
    using Storage.Helpers;
    using ViewModels;
    using Views;

    /// <summary>
    ///     Handler responsible for launching Horse Animation.
    /// </summary>
    public class HorseAnimationLauncher
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public HorseAnimationLauncher(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetectionService,
            IPrizeInformationEntityHelper entityHelper,
            ISystemDisableManager systemDisableManager,
            IGamePlayEntityHelper gamePlayEntityHelper,
            IPropertiesManager properties)
        {
            var topmostDisplay = cabinetDetectionService?.GetTopmostDisplay() ??
                              throw new ArgumentNullException(nameof(cabinetDetectionService));

            if (eventBus == null)
            {
                throw new ArgumentNullException(nameof(eventBus));
            }

            var venueRaceCollectionViewModel = new VenueRaceCollectionViewModel(
                eventBus,
                entityHelper,
                systemDisableManager,
                gamePlayEntityHelper,
                properties);

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Logger.Debug($"Rendering capability tier: {RenderCapability.Tier >> 16}");

                    RaceTrackEntry.SetupHorseImages();

                    eventBus.Publish(
                        new ViewInjectionEvent(
                            new VenueRaceCollection(venueRaceCollectionViewModel),
                            topmostDisplay,
                            ViewInjectionEvent.ViewAction.Add));
                });
        }
    }
}