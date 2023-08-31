namespace Aristocrat.Monaco.G2S.Common
{
    using Data;
    using Protocol.Common.Storage;
    using Protocol.Common.Storage.Entity;
    using Protocol.Common.Storage.Repositories;
    using Microsoft.EntityFrameworkCore;
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
            container.RegisterConditional(typeof(IRepository<>), typeof(Repository<>), Lifestyle.Scoped, _ => true);

            var registration = Lifestyle.Transient.CreateRegistration<UnitOfWork>(container);
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "ignore");
            container.AddRegistration(typeof(IUnitOfWork), registration);
            container.RegisterSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
            return container;
        }
    }
}
