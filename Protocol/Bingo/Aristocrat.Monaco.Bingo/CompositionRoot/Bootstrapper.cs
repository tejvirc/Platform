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
    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary>
        ///     Initializes the container.
        /// </summary>
        /// <returns>the container</returns>
        public static Container InitializeContainer(this Container container, IPropertiesManager propertiesManager)
        {
            return container.ConfigureContainer(propertiesManager);
        }

        private static Container ConfigureContainer(this Container container, IPropertiesManager propertiesManager)
        {
            var loggingEnabled =
                propertiesManager.GetValue(BingoConstants.EnableGrpcLogging, Constants.False).ToUpper();
            return container.AddExternalServices()
                .AddPersistenceStorage()
                .RegisterClient(Assembly.GetExecutingAssembly())
                .AddInternalServices()
                .WithGrpcLogging(loggingEnabled == Constants.True)
                .ConfigureConsumers();
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
