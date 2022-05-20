// ReSharper disable once CheckNamespace
namespace Aristocrat.Monaco.Mgam.Common
{
    using System.Data.Entity;
    using Aristocrat.Monaco.Protocol.Common.Storage;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.Monaco.Protocol.Common.Storage.Repositories;
    using Data;
    using Data.Models;
    using SimpleInjector;
    using SimpleInjector.Diagnostics;

    /// <summary>
    ///     Container extension methods.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="container"></param>
        /// <returns><see cref="Container"/></returns>
        public static Container AddDbContext(this Container container)
        {
            container.RegisterSingleton<IConnectionStringResolver, DefaultConnectionStringResolver>();

            container.Register<DbContext, MgamContext>(Lifestyle.Scoped);

            container.RegisterConditional(typeof(IRepository<>), typeof(Repository<>), Lifestyle.Scoped, _ => true);

            var registration = Lifestyle.Transient.CreateRegistration<UnitOfWork>(container);
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "ignore");
            container.AddRegistration(typeof(IUnitOfWork), registration);

            container.RegisterSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();

            return container;
        }
    }
}
