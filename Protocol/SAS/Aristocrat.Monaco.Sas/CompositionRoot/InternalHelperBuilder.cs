namespace Aristocrat.Monaco.Sas.CompositionRoot
{
    using System;
    using SimpleInjector;
    using AftTransferProvider;
    using Common.Storage;
    using Consumers;

    internal static class InternalHelperBuilder
    {
        /// <summary>
        ///     Registers helpers used internally.
        /// </summary>
        /// <param name="container">The container.</param>
        internal static Container RegisterInternalHelpers(this Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.Register<IHardCashOutLock, HardCashOutLock>(Lifestyle.Singleton);
            container.Register<ISystemEventHandler, SystemEventHandler>(Lifestyle.Singleton);
            container.Register<IFileSystemProvider, FileSystemProvider>(Lifestyle.Singleton);
            return container;
        }
    }
}
