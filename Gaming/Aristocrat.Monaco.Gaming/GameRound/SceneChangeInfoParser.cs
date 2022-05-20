namespace Aristocrat.Monaco.Gaming.GameRound
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts.Events;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Parses the SceneChanged information sent from
    ///     GameRoundInfoParserFactory
    /// </summary>
    public class SceneChangeInfoParser : IGameRoundInfoParser
    {
        private const int MinimumTriggeredDataSize = 3;
        private const int SceneOffset = 2;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEventBus _eventBus;

        public SceneChangeInfoParser(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public string GameType => "Controllable Scene";

        public string Version => "1";

        public void UpdateGameRoundInfo(IList<string> sceneChangeInformation)
        {
            // Scene change notifications will be encoded as follows:
            // sceneChangeInformation[0] contains the gameType ("Controllable Scene")
            // sceneChangeInformation[1] contains the version  ("1")
            // sceneChangeInformation[2] will contain the scene name being changed to, for example "RSFS" or "Normal"
            if (sceneChangeInformation.Count < MinimumTriggeredDataSize)
            {
                return;
            }

            var sceneName = sceneChangeInformation[SceneOffset];

            Logger.Debug($"Sending event to change to scene '{sceneName}'");
            _eventBus.Publish(new SceneChangedEvent(sceneName));
        }
    }
}