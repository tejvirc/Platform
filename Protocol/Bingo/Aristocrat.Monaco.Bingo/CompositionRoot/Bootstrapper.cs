namespace Aristocrat.Monaco.Bingo.CompositionRoot
{
    using System.Reflection;
    using Aristocrat.Bingo.Client.CompositionRoot;
    using Common;
    using Common.CompositionRoot;
    using Kernel;
    using Kernel.Contracts.Events;
    using Monaco.Common;
    using Monaco.Common.Container;
    using Services.GamePlay;
    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary>
        ///     Initializes the container.
        /// </summary>
        /// <returns>the container</returns>
        public static Container InitializeContainer(this Container container, IPropertiesManager propertiesManager, bool isBingoProgressiveEnabled)
        {
            return container.ConfigureContainer(propertiesManager, isBingoProgressiveEnabled);
        }

        private static Container ConfigureContainer(this Container container, IPropertiesManager propertiesManager, bool isBingoProgressiveEnabled)
        {
            var loggingEnabled =
                propertiesManager.GetValue(BingoConstants.EnableGrpcLogging, Constants.False).ToUpper();
            return container.AddExternalServices()
                .AddPersistenceStorage()
                .RegisterClient(isBingoProgressiveEnabled, Assembly.GetExecutingAssembly())
                .AddInternalServices(isBingoProgressiveEnabled)
                .WithGrpcLogging(loggingEnabled == Constants.True)
                .ConfigureServices(isBingoProgressiveEnabled)
                .ConfigureConsumers();
        }

        private static Container ConfigureServices(this Container container, bool isBingoProgressiveEnabled)
        {
            if (isBingoProgressiveEnabled)
            {
                container.RegisterSingleton<IProgressiveController, ProgressiveController>();
            }

            return container;
        }

        private static Container ConfigureConsumers(this Container container)
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
