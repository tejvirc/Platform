namespace Aristocrat.Monaco.Gaming.UI.PlayerInfoDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts.PlayerInfoDisplay;
    using log4net;
    using SimpleInjector;

    /// <inheritdoc cref="IPlayerInfoDisplayManagerFactory" />
    public sealed class PlayerInfoDisplayManagerFactory : IPlayerInfoDisplayManagerFactory
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string DefaultManager = "Default";
        private const string NotSupportedManager = "NotSupported";
        private readonly IDictionary<string, Lazy<InstanceProducer<IPlayerInfoDisplayManager>>> _producers;
        private readonly IPlayerInfoDisplayFeatureProvider _playerInfoDisplayFeatureProvider;

        public PlayerInfoDisplayManagerFactory(Container container, IPlayerInfoDisplayFeatureProvider playerInfoDisplayFeatureProvider)
        {
            _playerInfoDisplayFeatureProvider = playerInfoDisplayFeatureProvider;
            _producers = new Dictionary<string, Lazy<InstanceProducer<IPlayerInfoDisplayManager>>>()
            {
                { DefaultManager, new Lazy<InstanceProducer<IPlayerInfoDisplayManager>>(() => Lifestyle.Singleton.CreateProducer<IPlayerInfoDisplayManager, DefaultPlayerInfoDisplayManager>(container))},
                { NotSupportedManager, new Lazy<InstanceProducer<IPlayerInfoDisplayManager>>(() => Lifestyle.Singleton.CreateProducer<IPlayerInfoDisplayManager, PlayerInfoDisplayNotSupportedManager>(container))}
            };
        }

        /// <inheritdoc />
        public IPlayerInfoDisplayManager Create(IPlayerInfoDisplayScreensContainer screensProvider)
        {
            Logger.Debug("Create Player Info Display Manager");
            var manager = _playerInfoDisplayFeatureProvider.IsPlayerInfoDisplaySupported
                ? _producers[DefaultManager].Value.GetInstance()
                : _producers[NotSupportedManager].Value.GetInstance();
            manager.AddPages(screensProvider.AvailablePages);
            return manager;
        }
    }
}