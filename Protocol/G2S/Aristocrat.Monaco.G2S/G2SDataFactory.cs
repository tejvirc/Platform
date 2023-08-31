namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using CompositionRoot;
    using G2S.Common.Data;
    using Protocol.Common.Storage;
    using Data.Hosts;
    using Kernel;

    /// <summary>
    ///     An implementation of an <see cref="IG2SDataFactory" />
    /// </summary>
    /// <remarks>
    ///     This is only here so we can get access to portions of the G2S layer prior to the layer actually being loaded and
    ///     composed.
    /// </remarks>
    public class G2SDataFactory : IG2SDataFactory, IService
    {
        /// <inheritdoc />
        public IHostService GetHostService()
        {
            return new HostService(
                new DbContextFactory(new DefaultConnectionStringResolver(ServiceManager.GetInstance().GetService<IPathMapper>())),
                new HostRepository());
        }

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