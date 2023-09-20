namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using Data.Hosts;
    using Kernel;
    using Monaco.Common.Storage;

    /// <summary>
    ///     An implementation of an <see cref="IG2SDataFactory" />
    /// </summary>
    /// <remarks>
    ///     This is only here so we can get access to portions of the G2S layer prior to the layer actually being loaded and
    ///     composed.
    /// </remarks>
    public class G2SDataFactory : IG2SDataFactory, IService
    {
        private readonly IMonacoContextFactory _contextFactory;

        public G2SDataFactory()
            : this(ServiceManager.GetInstance().GetService<IMonacoContextFactory>())
        {
        }

        public G2SDataFactory(IMonacoContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public IHostService GetHostService() => new HostService(_contextFactory, new HostRepository());

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IG2SDataFactory) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}