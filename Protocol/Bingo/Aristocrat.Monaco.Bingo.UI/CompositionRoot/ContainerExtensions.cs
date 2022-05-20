namespace Aristocrat.Monaco.Bingo.UI.CompositionRoot
{
    using System.Reflection;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts.Events;
    using Loaders;
    using Monaco.Common.Container;
    using OverlayServer;
    using Services;
    using SimpleInjector;

    public static class ContainerExtensions
    {
        public static Container AddBingoOverlay(this Container container)
        {
            container.RegisterSingleton<IDispatcher, DispatcherWrapper>();
            container.RegisterSingleton<IServer, Server>();
            container.RegisterSingleton<IBingoDisplayConfigurationProvider, BingoDisplayConfigurationProvider>();
            container.RegisterSingleton<ILegacyAttractProvider, LegacyAttractProvider>();
            container.RegisterManyAsCollection(typeof(IBingoPresentationLoader), Assembly.GetExecutingAssembly());
            return container;
        }

        public static Container AddGameRoundHistoryDetails(this Container container)
        {
            container.RegisterSingleton<IGameRoundDetailsDisplayProvider, BingoGameRoundDetailsDisplayProvider>();
            return container;
        }

        public static Container AddEventConsumers(this Container container)
        {
            container.RegisterManyForOpenGeneric(
                typeof(IConsumer<>),
                true,
                Assembly.GetExecutingAssembly());
            container.RegisterManyForOpenGeneric(
                typeof(IAsyncConsumer<>),
                true,
                Assembly.GetExecutingAssembly());
            return container;
        }
    }
}