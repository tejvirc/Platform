namespace Aristocrat.Monaco.Bingo.Common.CompositionRoot
{
    using Microsoft.EntityFrameworkCore;
    using Protocol.Common.Storage;
    using Protocol.Common.Storage.Entity;
    using Protocol.Common.Storage.Repositories;
    using SimpleInjector;
    using SimpleInjector.Diagnostics;
    using Storage;
    using Storage.Model;

    public static class ContainerExtensions
    {
        public static Container AddPersistenceStorage(this Container container)
        {
            container.RegisterSingleton<IConnectionStringResolver, DefaultConnectionStringResolver>();
            container.Register<DbContext, BingoContext>(Lifestyle.Scoped);
            container.Register(typeof(IRepository<>), typeof(Repository<>), Lifestyle.Scoped);

            var registration = Lifestyle.Transient.CreateRegistration<UnitOfWork>(container);
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "ignore");
            container.AddRegistration(typeof(IUnitOfWork), registration);
            container.RegisterSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
            return container;
        }
    }
}
