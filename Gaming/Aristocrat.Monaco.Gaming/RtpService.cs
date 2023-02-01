namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Rtp;
    using Kernel;

    public class RtpService : IService, IRtpService
    {
        private readonly IGameProvider _gameProvider;

        public RtpService(IGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IRtpService) };

        public RtpReportForGameTheme GenerateRtpReportForGame(string gameThemeId) 
        {
            var gamesForTheme = _gameProvider.GetAllGames()
                .Where(game => game.ThemeId.Equals(gameThemeId));

            var report = new RtpReportForGameTheme(gamesForTheme);

            // Validate - filter out or mark invalid RTP ranges
            // TODO: Implement this

            return report;
        }

        public void Initialize()
        {
            // Load and cache Jurisdictional RTP rules


            throw new NotImplementedException();
        }
    }
}