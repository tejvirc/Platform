namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using Common.PackageManager;
    using Common.PackageManager.Storage;
    using Common.Transfer;
    using SimpleInjector;

    /// <summary>
    ///     Handles configuring the Package Manager.
    /// </summary>
    internal static class PackageManagerBuilder
    {
        /// <summary>
        ///     Registers the package manager with the container.
        /// </summary>
        /// <param name="this">The container.</param>
        /// <param name="connectionString">The connection string.</param>
        internal static void RegisterPackageManager(this Container @this, string connectionString)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            @this.Register<IPackageRepository, PackageRepository>(Lifestyle.Singleton);
            @this.Register<ITransferRepository, TransferRepository>(Lifestyle.Singleton);
            @this.Register<IPackageErrorRepository, PackageErrorRepository>(Lifestyle.Singleton);
            @this.Register<IScriptRepository, ScriptRepository>(Lifestyle.Singleton);
            @this.Register<IModuleRepository, ModuleRepository>(Lifestyle.Singleton);
            @this.Register<ITransferService, TransferService>(Lifestyle.Singleton);
            @this.Register<ITransferFactory, TransferFactory>(Lifestyle.Singleton);
            @this.Register<IPackageManager, PackageManager>(Lifestyle.Singleton);
        }
    }
}
