namespace Aristocrat.Monaco.Protocol.Common.Storage.Entity
{
    using System;
    using System.Threading.Tasks;
    using SimpleInjector;

    /// <summary>
    ///     Creates <see cref="IUnitOfWork"/> instances.
    /// </summary>
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly Container _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnitOfWorkFactory"/> class.
        /// </summary>
        /// <param name="container">Dependency injection container.</param>
        public UnitOfWorkFactory(Container container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public IUnitOfWork Create()
        {
            return _container.GetInstance<IUnitOfWork>();
        }

        /// <inheritdoc />
        public T Invoke<T>(Func<IUnitOfWork, T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var unitOfWork = _container.GetInstance<IUnitOfWork>())
            {
                return action.Invoke(unitOfWork);
            }
        }

        /// <inheritdoc />
        public void Invoke(Action<IUnitOfWork> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var unitOfWork = _container.GetInstance<IUnitOfWork>())
            {
                action.Invoke(unitOfWork);
            }
        }

        /// <inheritdoc />
        public Task<T> Invoke<T>(Func<IUnitOfWork, Task<T>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var unitOfWork = _container.GetInstance<IUnitOfWork>())
            {
                return action.Invoke(unitOfWork);
            }
        }

        /// <inheritdoc />
        public Task Invoke(Func<IUnitOfWork, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var unitOfWork = _container.GetInstance<IUnitOfWork>())
            {
                return action.Invoke(unitOfWork);
            }
        }
    }
}
