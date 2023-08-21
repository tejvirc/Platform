namespace Aristocrat.Monaco.Hhr.UI.Extensions
{
    using System.Reflection;
    using Kernel;
    using SimpleInjector;
    using Common.Container;
    using CompositionRoot;
    using Consumers;
    using Services;
    using SimpleInjector.Lifestyles;
    using ViewModels;

    /// <summary>
    ///     Extensions function for SimpleInjector container.
    /// </summary>
    public static class ContainerExtensions
    {
        public static void ConfigureConsumers(this Container @this)
        {
            @this.RegisterManyForOpenGeneric(
                typeof(IConsumer<>),
                true,
                Assembly.GetExecutingAssembly());
        }

        public static void Initialize(this Container @this)
        {
            @this.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            Bootstrapper.InitializeBase(@this);

            @this.Register<IMenuAccessService, HostPageViewModelManager>(Lifestyle.Singleton);
            @this.Register<ManualHandicapPageViewModel>(Lifestyle.Singleton);
            @this.Register<ManualHandicapHelpPageViewModel>(Lifestyle.Singleton);
            @this.Register<RaceStatsPageViewModel>(Lifestyle.Singleton);
            @this.Register<WinningCombinationPageViewModel>(Lifestyle.Singleton);
            @this.Register<PreviousRaceResultPageViewModel>(Lifestyle.Singleton);
            @this.Register<CurrentProgressivePageViewModel>(Lifestyle.Singleton);
            @this.Register<HelpPageViewModel>(Lifestyle.Singleton);
            @this.Register<BetHelpPageViewModel>(Lifestyle.Singleton);
            @this.RegisterSingleton<HorseAnimationLauncher>();
            @this.RegisterSingleton<CurrentProgressivesLauncher>();
        }
    }
}