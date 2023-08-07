namespace Aristocrat.Monaco.Test.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Gaming.Contracts;
    using Gaming.Contracts.Lobby;
    using Gaming.Contracts.Models;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using log4net;
    using RobotController.Contracts;
    using Vgt.Client12.Testing.Tools;

    public class Automation
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly MouseHelper _mouseHelper = new MouseHelper();
        private readonly IPropertiesManager _pm;

        private long? _originalWinLimit;

        public Automation(IPropertiesManager pm, IEventBus eb)
        {
            _pm = pm;

            _eventBus = eb;

            _mouseHelper.Logger = Log;
        }

        public bool IsRobotModeRunning { get; set; }

        public void SetOverlayText(string text, bool clear, Guid guid, InfoLocation location)
        {
            _eventBus.Publish(
                new InfoOverlayTextEvent
                {
                    Text = text, TextGuid = guid, Location = InfoLocation.TopLeft, Clear = clear
                }
            );
        }

        public void SpinRequest()
        {
            _eventBus.Publish(new InputEvent(22, true));
            _eventBus.Publish(new InputEvent(22, false));
        }

        public void SetBetLevel(int index)
        {
            _eventBus.Publish(new InputEvent(22 + index, true));
            _eventBus.Publish(new InputEvent(22 + index, false));
        }

        public void SetBetMax()
        {
            _eventBus.Publish(new DownEvent((int)ButtonLogicalId.MaxBet));
            _eventBus.Publish(new UpEvent((int)ButtonLogicalId.MaxBet));
        }

        public void SetLineLevel(int index)
        {
            _eventBus.Publish(new InputEvent(29 + index, true));
            _eventBus.Publish(new InputEvent(29 + index, false));
        }

        public string GetRuntimeState()
        {
            var state = "NotLoaded";

            try
            {
                var runtime = ServiceManager.GetInstance().TryGetService<IGamePlayState>();

                if (runtime != null)
                {
                    state = runtime.CurrentState.ToString();
                }
            }
            catch (Exception)
            {
                Log("Could not find the runtime service.");
            }

            return state;
        }

        public void InsertDollars(int amount)
        {
            _eventBus.Publish(new DebugNoteEvent(amount));
        }

        public void RequestGameLoad(int gameId, long denom)
        {
            _eventBus.Publish(new GameLoadRequestedEvent { GameId = gameId, Denomination = denom });
        }

        public void ForceGameExit(string aGdkRuntimeHostName)
        {
            try
            {
                var runtimes = Process.GetProcessesByName(aGdkRuntimeHostName);

                foreach (var runtime in runtimes)
                {
                    Log("Forcing runtime process close");
                    runtime.Kill();
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        public void RequestGameExit()
        {
            Log("Requesting game exit");
            EnableExitToLobby(true);
            _eventBus.Publish(new GameRequestExitEvent());
        }

        public void DismissTimeLimitDialog(bool isTimeLimitDialogVisible)
        {
            if (isTimeLimitDialogVisible)
            {
                Log("Attempting to dismiss the RG Time Limit Dialog");
                _mouseHelper.ClickRG(); //clicks the 60 minute option of the Responsible Gaming dialog.
            }
        }

        public void SetResponsibleGamingTimeElapsed(int timeElapsed)
        {
            Log($"Setting responsible gaming time elapsed to {timeElapsed}");
            _pm.SetProperty(LobbyConstants.LobbyPlayTimeElapsedInSecondsOverride, timeElapsed);
        }

        public void SetRgSessionCountOverride(int sessionCount)
        {
            Log($"Setting responsible gaming session count to {sessionCount}");
            _pm.SetProperty(LobbyConstants.LobbyPlayTimeSessionCountOverride, sessionCount);
        }

        public void LoadAuditMenu()
        {
            _eventBus.Publish(new InputEvent(2, true));
        }

        public void EnterLockup()
        {
            _eventBus.Publish(new InputEvent(49, true));
        }

        public void ExitLockup()
        {
            _eventBus.Publish(new InputEvent(49, false));
        }

        public void CardEvent(bool inserted, string data)
        {
            _eventBus.Publish(new FakeCardReaderEvent(0, data, inserted));
        }

        public void ServiceButton(bool switchOn)
        {
            if (switchOn)
            {
                _eventBus.Publish(new CallAttendantButtonOnEvent());
            }
            else
            {
                _eventBus.Publish(new CallAttendantButtonOffEvent());
            }
        }

        public string GetPools(string name)
        {
            var result = string.Empty;

            var gameDetails = _pm.GetValues<IGameDetail>(GamingConstants.Games);
            var game = gameDetails?.FirstOrDefault(g => g.ThemeName == name);
            if (game == null)
            {
                return result;
            }

            // Progressive data is moving to the denom on the game. This call will require a denomination to get a valid result
            //if (game.HasPools)
            //{
            //    result = JsonConvert.SerializeObject(game.Pools, Formatting.None);
            //}

            return result;
        }

        public void EnableExitToLobby(bool enable)
        {
            Log((enable ? "Enabling" : "Disabling") + " Exit To Lobby");
            _pm.SetProperty("Automation.HandleExitToLobby", enable);
        }

        public void EnableCashOut(bool enable)
        {
            Log((enable ? "Enabling" : "Disabling") + " Cash Out");
            _pm.SetProperty("Automation.HandleCashOut", enable);
        }

        public void TouchMainScreen(int x, int y)
        {
            _mouseHelper.ClickGame(x, y);
        }

        public void TouchVBD(int x, int y)
        {
            _mouseHelper.ClickVirtualButtonDeck(x, y);
        }

        public void ExitAuditMenu()
        {
            _eventBus.Publish(new InputEvent(2, false));
        }

        public void SetTimeLimitButtons(List<string> buttonList)
        {
            _mouseHelper.TimeLimitButtons = buttonList;
        }

        public void JackpotKeyoff()
        {
            _eventBus.Publish(new UpEvent((int)ButtonLogicalId.Button30));
        }

        public void InsertVoucher(string barcode)
        {
            _eventBus.Publish(new VoucherEscrowedEvent(barcode));
        }

        public void SetMaxWinLimit(long winLimitMillicents)
        {
            _originalWinLimit = _originalWinLimit ?? (long)_pm.GetProperty("Cabinet.LargeWinLimit", 10000000);

            _pm.SetProperty("Cabinet.LargeWinLimit", winLimitMillicents);
        }

        private void Log(string msg)
        {
            _logger.Info(msg);
        }

        public void SetSpeed(string speed)
        {
            _pm.SetProperty(GamingConstants.RuntimeArgs, $"--plugin=SpeedPlugin.dll --parg=SpeedPlugin:\"{speed}\"");
        }

        public void ResetSpeed()
        {
            _pm.SetProperty(GamingConstants.RuntimeArgs, string.Empty);
        }
    }
}