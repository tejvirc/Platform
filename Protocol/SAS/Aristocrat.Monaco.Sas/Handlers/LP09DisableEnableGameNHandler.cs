namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Gaming.Contracts.Configuration;
    using Kernel;
    using log4net;
    using Application.Contracts.Extensions;

    /// <summary>
    ///     Handles the enable/disable game command
    /// </summary>
    public class LP09DisableEnableGameNHandler : ISasLongPollHandler<EnableDisableResponse, EnableDisableData>, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGameProvider _gameProvider;
        private readonly IBank _bank;
        private readonly IGamePlayState _gamePlayState;
        private readonly IEventBus _eventBus;
        private readonly IConfigurationProvider _gameConfigurationProvider;
        private bool _disposed;

        /// <summary>
        ///     Creates a new instance of the LP09DisableEnableGameNHandler
        /// </summary>
        /// <param name="gameProvider">a reference to the game provider</param>
        /// <param name="bank">a reference to the bank</param>
        /// <param name="gamePlayState">a reference to the GamePlayState object</param>
        /// <param name="eventBus">a reference to the EventBus object</param>
        /// <param name="gameConfigurationProvider">a reference to the ConfigurationProvider object</param>
        public LP09DisableEnableGameNHandler(IGameProvider gameProvider,
                                             IBank bank,
                                             IGamePlayState gamePlayState,
                                             IEventBus eventBus,
                                             IConfigurationProvider gameConfigurationProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameConfigurationProvider = gameConfigurationProvider ?? throw new ArgumentNullException(nameof(gameConfigurationProvider));
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, _ => InOperatorMenu = true);
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, _ => InOperatorMenu = false);
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.EnableDisableGameN };

        /// <summary>
        ///     Gets or sets a value indicating whether we are in the operator menu or not
        /// </summary>
        public bool InOperatorMenu { get; set; }

        /// <inheritdoc/>
        public EnableDisableResponse Handle(EnableDisableData data)
        {
            // Selected quotes from SAS 6.03 Specifications Appendix E
            //
            // The response to long poll 09 is either a simple ACK/NACK response or an optional BUSY response.
            // There is no mechanism available to allow a gaming machine to explain in detail why the poll
            // succeeds or fails. Therefore it is important to use the available responses consistently.
            // - ACK means the gaming machine expects to be able to perform the requested enable or disable.
            // - NACK means there is an error in the command and the gaming machine cannot perform the requested enable or disable
            // - BUSY means the command is valid, but the EGM cannot perform the requested enable or disable at this time.
            //
            // Enable/Disable Game 0000
            // It is strongly recommended that a gaming machine allow long poll 09 with game number = 0000 and
            // enable/disable = 00 to disable all games on the gaming machine.
            // Long poll 09 with game number = 0000 and enable/disable = 01 is always an invalid poll.
            //
            // A gaming machine should not be able to be remotely configured when it is currently being played.
            // This includes enabling and disabling games using long poll 09. Also, remote configuration should not
            // be allowed while an operator is accessing the menu system at the gaming machine.
            //
            // Example responses
            // The gaming machine should ignore long poll 09 in the following conditions:
            // - the command is not supported
            // - The CRC is not present or incorrect
            // - Game number is not valid BCD or not in range (NACK is also permitted)
            //
            // The gaming machine should reply with NACK in the following conditions:
            // - The host attempts to enable game 0000
            // - The host attempts to disable game 0000 and this behavior is not supported
            // - The host attempts to enable a tournament game
            // - The game can not be enabled under any circumstances (for example due to jurisdictional control)
            // - The game cannot be enabled for the requested denom (either denom not supported for the selected
            //     game, or operator has not enabled the requested denom for that game)
            // - The host attempts to enable game for all denoms when no denom is enabled for the selected game
            //     at the gaming machine
            //
            // The gaming machine should reply with BUSY in the following conditions:
            // - Gaming machine is in operator menu
            // - Game play is active (credits in credit meter or game in progress)
            // - Game is in tournament mode
            //
            // The gaming machine should reply with ACK in the following conditions:
            // - The gaming machine expects to be able to enable or disable the selected game, or the game is
            //     already in the requested state. The actual change of game enable status may occur after the
            //     gaming machine responds. The gaming machine must issue exception A2, enabled games/denoms changed
            //     once the change is complete. If the game was already enabled or disabled for the requested denom
            //     or denoms and no change is necessary, exception A2 is not issued.
            // - The host requested the gaming machine to disable a game that cannot be enabled. Exception A2 is
            //     not issued
            //
            // NOTE: Exception A2 will be issued by another component which listens for the GameEnabledEvent and
            //       GameDisabledEvent events.

            var id = data.Id;
            var enable = data.Enable;
            var denom = data.TargetDenomination.CentsToMillicents();
            var multiDenomAware = data.MultiDenomPoll;

            var (game, denomInfo) = _gameProvider.GetGameDetail(id);
            var response = new EnableDisableResponse();

            var allGames = _gameProvider.GetAllGames();

            // requested denom isn't valid player denom
            if (denom != 0 && !allGames.Any(g => g.SupportedDenominations.Contains(denom)))
            {
                Logger.Debug($"Denom {denom} isn't a valid player denom");
                response.ErrorCode = MultiDenomAwareErrorCode.NotValidPlayerDenom;
                return response;
            }

            // host trying to enable game 0000 (not allowed)
            if (enable && id == 0)
            {
                Logger.Debug("can't enable all games");
                return response;
            }

            // game has credits on the credit meter
            if (_bank.QueryBalance() > 0)
            {
                Logger.Debug("can't enable/disable when there is money on the machine");
                response.Busy = true;
                return response;
            }

            // currently playing a game
            if (_gamePlayState.InGameRound)
            {
                Logger.Debug("can't enable/disable when a game is playing");
                response.Busy = true;
                return response;
            }

            // in the operator menu
            if (InOperatorMenu)
            {
                Logger.Debug("can't enable/disable when in the audit menu");
                response.Busy = true;
                return response;
            }

            // host trying to disable game 0000 (all games)
            if (!enable && id == 0)
            {
                // if there are any packs, we can't allow this to happen
                foreach (IGameDetail aGame in allGames)
                {
                    if (_gameConfigurationProvider.GetByThemeId(aGame.ThemeId).Any())
                    {
                        Logger.Debug($"Game {aGame.ThemeId} has packs, ignoring disable all");
                        response.ErrorCode = MultiDenomAwareErrorCode.LongPollNotSupported;
                        return response;
                    }
                }

                Logger.Debug("Disabling all games");

                // disabling all the games may take a while, so do it asynchronously
                if (multiDenomAware)
                {
                    DisableDenomAllGamesAsync(allGames, denom);
                }
                else
                {
                    DisableAllGamesAsync(allGames);
                }
                response.Succeeded = true;
                return response;
            }

            // no game with that id
            if (game is null)
            {
                Logger.Debug("invalid game id");
                return response;
            }

            // requested game has packs and can't enable/disable individual denoms
            if (_gameConfigurationProvider.GetByThemeId(game.ThemeId).Any())
            {
                Logger.Debug($"Game {game.ThemeId} has packs, ignoring disable this game");
                response.ErrorCode = MultiDenomAwareErrorCode.LongPollNotSupported;
                return response;
            }

            // game doesn't have target denom
            if (denom != 0 && denomInfo.Value != denom)
            {
                Logger.Debug($"Denom {denom} is not supported for game {game.ThemeName} ({game.Id})");
                response.ErrorCode = MultiDenomAwareErrorCode.SpecificDenomNotSupported;
                return response;
            }

            // the game enable/disable calls take a long time so they need to be called asynchronously or the ACK takes to long to occur
            EnableDisableDenomAsync(enable, game, denomInfo.Value);

            response.Succeeded = true;
            return response;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void DisableAllGamesAsync(IEnumerable<IGameProfile> games)
        {
            Task.Run(
                () =>
                {
                    foreach (var game in games)
                    {
                        _gameProvider.SetActiveDenominations(game.Id, new List<long>());
                    }
                });
        }

        private void DisableDenomAllGamesAsync(IEnumerable<IGameDetail> games, long denom)
        {
            Task.Run(
                () =>
                {
                    if (denom > 0)
                    {
                        foreach (var game in games.Where(g => g.ActiveDenominations.Contains(denom)))
                        {
                            var newDenomList = game.ActiveDenominations.Where(d => d != denom).ToList();
                            if (_gameProvider.ValidateConfiguration(game, newDenomList))
                            {
                                _gameProvider.SetActiveDenominations(game.Id, newDenomList);
                            }
                        }
                    }
                    else
                    {
                        foreach (var game in games)
                        {
                            if (_gameProvider.ValidateConfiguration(game, new List<long>()))
                            {
                                _gameProvider.SetActiveDenominations(game.Id, new List<long>());
                            }
                        }
                    }
                });
        }

        private void EnableDisableDenomAsync(bool enable, IGameDetail game, long denom)
        {
            Task.Run(
                () =>
                {
                    if (denom > 0)
                    {
                        if (enable && !game.ActiveDenominations.Contains(denom))
                        {
                            var newDenomList = game.ActiveDenominations.Append(denom).ToList();
                            if (_gameProvider.ValidateConfiguration(game, newDenomList))
                            {
                                _gameProvider.SetActiveDenominations(game.Id, newDenomList);
                            }
                        }
                        else if (!enable)
                        {
                            var newDenomList = game.ActiveDenominations.Where(d => d != denom).ToList();
                            if (_gameProvider.ValidateConfiguration(game, newDenomList))
                            {
                                _gameProvider.SetActiveDenominations(game.Id, newDenomList);
                            }
                        }
                    }
                    else
                    {
                        // if we're enabling all denoms, try to find the list of denoms that are valid,
                        // and enable those.
                        if (enable)
                        {
                            var newActiveDenoms = game.SupportedDenominations.Where(d => game.ActiveDenominations.Contains(d)
                                || _gameProvider.ValidateConfiguration(game, new List<long> { d }));
                            _gameProvider.SetActiveDenominations(game.Id, newActiveDenoms);
                        }
                        else
                        {
                            _gameProvider.SetActiveDenominations(game.Id, new List<long>());
                        }
                    }
                });
        }

        /// IDispose interface
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _eventBus.UnsubscribeAll(this);
                }

                _disposed = true;
            }
        }
    }
}
