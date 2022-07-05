namespace Aristocrat.Monaco.Gaming.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using Application.Contracts.Extensions;
    using Contracts;
    using log4net;

    public class ReplayContext : IDiagnosticContext<IGameHistoryLog>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public ReplayContext(IGameHistoryLog log, int gameIndex)
        {
            Arguments = log ?? throw new ArgumentNullException(nameof(log));

            GameIndex = gameIndex;
        }

        public int GameIndex { get; }

        public IReadOnlyDictionary<string, string> GetParameters()
        {
            var parameters = new Dictionary<string, string>
            {
                { "/Runtime/Localization/Language", Arguments.LocaleCode },
                { "/Runtime/ReplayMode", "true" },
                { "/Runtime/ReplayMode&realtime", "true" },
                { "/Runtime/ReplayMode&replaypause", "true" },
                { "/Runtime/Account&balance", Arguments.StartCredits.MillicentsToCents().ToString() }
            };

            if (GameIndex != -1)
            {
                parameters.Add("/Runtime/ReplayGameIndex", GameIndex.ToString());
            }

            if (!string.IsNullOrEmpty(Arguments.GameConfiguration))
            {
                parameters.Add("/Runtime/Multigame&ActivePack", Arguments.GameConfiguration);
            }

            if (Arguments.RecoveryBlob is { Length: > 0 })
            {
                parameters.Add("/Runtime/Recovery/BinaryData", Encoding.ASCII.GetString(Arguments.RecoveryBlob));

                var encoded = Encoding.ASCII.GetString(Arguments.RecoveryBlob);

                Logger.Debug($"[REPLAY DATA] -> {encoded.Length} bytes : {encoded}");
            }

            if (Arguments.DenomConfiguration != null)
            {
                if (!string.IsNullOrEmpty(Arguments.DenomConfiguration.BetOption))
                {
                    parameters.Add("/Runtime/BetOption", Arguments.DenomConfiguration.BetOption);
                }

                if (!string.IsNullOrEmpty(Arguments.DenomConfiguration.LineOption))
                {
                    parameters.Add("/Runtime/LineOption", Arguments.DenomConfiguration.LineOption);
                }

                if (Arguments.DenomConfiguration.BonusBet != 0)
                {
                    parameters.Add("/Runtime/BonusAwardMultiplier", Arguments.DenomConfiguration.BonusBet.ToString());
                }

                parameters.Add(
                    "/Runtime/MinimumWagerCredits",
                    Arguments.DenomConfiguration.MinimumWagerCredits.ToString());
                parameters.Add(
                    "/Runtime/MaximumWagerCredits",
                    Arguments.DenomConfiguration.MaximumWagerCredits.ToString());
                parameters.Add(
                    "/Runtime/MaximumWagerInsideCredits",
                    Arguments.DenomConfiguration.MaximumWagerCredits.ToString());
                parameters.Add(
                    "/Runtime/MaximumWagerOutsideCredits",
                    Arguments.DenomConfiguration.MaximumWagerOutsideCredits.ToString());
            }

            return parameters;
        }

        public IGameHistoryLog Arguments { get; }
    }
}