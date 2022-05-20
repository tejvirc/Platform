namespace Aristocrat.Monaco.Gaming.UI.CompositionRoot
{
    using Contracts;
    using SimpleInjector;
    using ViewModels;

    public static class ContainerExtensions
    {
        public static Container AddOverlayMessageStrategies(this Container container)
        {
            var overlayMessageStrategyFactory = new OverlayMessageStrategyFactory(container);
            overlayMessageStrategyFactory.Register<BasicOverlayMessageStrategy>(OverlayMessageStrategyOptions.Basic);
            overlayMessageStrategyFactory.Register<EnhancedOverlayMessageStrategy>(OverlayMessageStrategyOptions.Enhanced);
            overlayMessageStrategyFactory.Register<GameControlledOverlayMessageStrategy>(OverlayMessageStrategyOptions.GameDriven);

            container.RegisterInstance<IOverlayMessageStrategyFactory>(overlayMessageStrategyFactory);
            container.RegisterSingleton<IMessageOverlayData, MessageOverlayData>();
            container.RegisterSingleton<IOverlayMessageStrategyController, OverlayMessageStrategyController>();
            return container;
        }
    }
}
