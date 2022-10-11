namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.TiltLogger;
    using Aristocrat.Monaco.G2S.Handlers;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Display;
    using Aristocrat.Monaco.Hardware.Contracts.IdReader;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Aristocrat.Monaco.Hardware.Contracts.Printer;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Sas.Contracts.SASProperties;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using DataModel;
    using Gaming.Contracts;
    using Gaming.Contracts.Lobby;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Gds;
    using Hardware.Contracts.Gds.NoteAcceptor;
    using Hardware.Contracts.IO;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;
    using RobotController.Contracts;
    using Test.Automation;
    using Wait;
    using HardwareFaultClearEvent = Hardware.Contracts.NoteAcceptor.HardwareFaultClearEvent;
    using HardwareFaultEvent = Hardware.Contracts.NoteAcceptor.HardwareFaultEvent;
    using Aristocrat.Monaco.Gaming.UI.ViewModels;
    using CoreWCF;

    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        AddressFilterMode = AddressFilterMode.Any
        )]
    public partial class TestControllerEngine : ITestController
    {
        private const string ResponseTo = "response-to";

        private const string OverlayMessage = "OverlayMessage";

        private const string OverlayMessageUrl = "/Game/OverlayMessage/Get";

        /// <summary>
        ///     Amazing comment
        /// </summary>
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Handles mouse inputs
        /// </summary>
        private readonly MouseHelper _mouse = new MouseHelper();

        /// <summary>
        /// Automation for the platform
        /// </summary>
        private Automation _automata;

        /// <summary>
        /// Manager to encapsulate IO Manipulation
        /// </summary>
        private readonly IoManager _io = new IoManager();
        /// <summary>
        ///     Bank service
        /// </summary>
        private IBank _bank;

        /// <summary>
        ///     Mediocre comment
        /// </summary>
        private IEventBus _eventBus;

        /// <summary>
        ///     Whether TestController will dismiss RG dialogs
        /// </summary>
        private bool _handleRg;

        /// <summary>
        ///     Perceived platform state
        /// </summary>
        private PlatformStateEnum _platformState = PlatformStateEnum.Unknown;

        /// <summary>
        ///     Perceived runtime mode
        /// </summary>
        private RuntimeMode _runtimeMode = RuntimeMode.Regular;

        /// <summary>
        /// Monitoring light tower states, 5 are currently supported by driver.  Only 0 and 1 are currently used in the Helix+ configuration at this time.
        /// </summary>
        private readonly List<bool> _towerLightStates = new List<bool> { false, false, false, false, false };

        /// <summary>
        ///     Superior comment
        /// </summary>
        private IPropertiesManager _pm;

        private ITiltLogger _tiltLogger;

        private IWaitStrategy _waitStrategy;

        private readonly ConcurrentDictionary<Guid, string> _currentLockups = new ConcurrentDictionary<Guid, string>();

        private readonly HashSet<DisplayableMessage> _gameLineMessages = new HashSet<DisplayableMessage>(new DisplayableMessageComparer());

        private VoucherIssuedEvent _lastVoucherIssued;

        private List<VoucherIssuedEvent> _vouchersIssued = new List<VoucherIssuedEvent>();

        private ProcessMonitor _processMonitor;

        private PrinterWarningTypes _printerWarnings = PrinterWarningTypes.None;

        private PrinterFaultTypes _printerFaults = PrinterFaultTypes.None;

        private MeterManager _meterManager;

        private MessageOverlayData _messageOverlayData;

        public void Initialize()
        {
            _pm = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();

            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            _automata = new Automation(_pm, _eventBus);

            _processMonitor = new ProcessMonitor(Constants.PlatformProcessName);

            _meterManager = new MeterManager();

            _tiltLogger = ServiceManager.GetInstance().GetService<ITiltLogger>();


			//
			// Call the InitializeV2 in the new Partial Class object
			//
			InitializeV2();

        }

        public CommandResult ClosePlatform()
        {
            _eventBus.Publish(new ExitRequestedEvent(ExitAction.ShutDown));
            return new CommandResult()
            {
                data = new Dictionary<string, object> { { "response-to", "/Platform/Close" } },
                Result = true,
                Info = "Closing platform"
            };
        }

        public CommandResult GetEventLogs(string eventType)
        {
            var responseInfo = new Dictionary<string, object> { ["response-to"] = $"/Platform/Logs/{eventType}" };
            var events = new List<EventDescription>(_tiltLogger.GetEvents(eventType));
            int logCount = 0;

            foreach (var e in events)
            {
                var entryNumber = "LogDetail" + (++logCount).ToString();

                responseInfo.Add(entryNumber, JsonConvert.SerializeObject(e, new Newtonsoft.Json.Converters.StringEnumConverter()));
            }

            return new CommandResult()
            {
                data = responseInfo,
                Result = true,
                Info = $"Log details for event type {eventType}"
            };
        }

        public CommandResult AuditMenu(bool open)
        {
            if (open)
            {
                _automata.LoadAuditMenu();
            }
            else
            {
                _automata.ExitAuditMenu();
            }

            return new CommandResult()
            {
                data = new Dictionary<string, object> { { "response-to", "/Platform/AuditMenu" } },
                Result = true,
                Info = (open ? "Entering" : "Exiting") + " audit menu."
            };
        }

        public CommandResult SelectMenuTab(string name)
        {
            WindowHelper.GetAuditPage("Views", Test.Automation.Constants.OperatorWindowName, name);

            return new CommandResult()
            {
                data = new Dictionary<string, object> { { "response-to", "/Platform/AuditMenu" } },
                Result = true,
                Info = "Selecting audit menu tab: {name}."
            };
        }

        public CommandResult ToggleRobotMode()
        {
            _eventBus.Publish(new RobotControllerEnableEvent());
            return new CommandResult()
            {
                data = new Dictionary<string, object> { { "response-to", "/Platform/ToggleRobotMode" } },
                Result = true,
                Info = "Toggle robot mode"
            };
        }

        public CommandResult GetPlatformStatus()
        {
            return new CommandResult()
            {
                data = new Dictionary<string, object>
                {
                    { "response-to", "/Platform/State" },
                    { "machine_state", _platformState.ToString() }
                },
                Result = true,
                Info = _platformState.ToString()
            };
        }

        public CommandResult RequestGame(string GameName, long Denomination)
        {
            var gameFound = false;

            var games = _pm.GetValues<IGameDetail>(GamingConstants.Games).ToList();

            var gameInfo = games.FirstOrDefault(g => g.ThemeName == GameName && g.EgmEnabled && g.ActiveDenominations.Contains(Denomination));

            if (gameInfo != null)
            {
                Log($"Requesting game {gameInfo.ThemeName} with Denom {Denomination} be loaded.");
                _eventBus.Publish(new DenominationSelectedEvent(gameInfo.Id, Denomination));
                Task.Delay(1000).ContinueWith(_ => { _eventBus.Publish(new GameLoadRequestedEvent() { GameId = gameInfo.Id, Denomination = Denomination }); });       
                gameFound = true;
            }

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/GameLoad" },
                Result = gameFound,
                Info = gameFound
                    ? $"Test Controller requesting game {GameName} {Denomination}"
                    : $"Test Controller could not find {GameName}"
            };
        }

        public CommandResult RequestGameExit()
        {
            _eventBus.Publish(new GameRequestedLobbyEvent(false));
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/ExitGame" },
                Result = true,
                Info = "Test Controller requesting game exit"
            };
        }

        public CommandResult ForceGameExit()
        {
            var killProcess = false;

            var runtimes = Process.GetProcessesByName(Constants.GdkRuntimeHostName);

            foreach (var runtime in runtimes)
            {
                Log("Forcing runtime process close");
                runtime.Kill();
                killProcess = true;
            }

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/ForceGameExit", ["success"] = killProcess.ToString() },
                Result = killProcess,
                Info = $"Test Controller killing process: {killProcess.ToString()}"
            };
        }

        public CommandResult EnableCashOut(bool enable)
        {
            _pm.SetProperty("Automation.HandleCashOut", enable);
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/EnableCashout" },
                Result = true,
                Info = $"Test Controller allowing cash out: {enable.ToString()}"
            };
        }

        public CommandResult RequestCashOut()
        {
            EnableCashOut(true);
            _eventBus.Publish(new CashOutButtonPressedEvent());
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/Cashout/Request" },
                Result = true,
                Info = "Test Controller requesting cash out."
            };
        }

        public CommandResult HandleRG(bool enable)
        {
            _handleRg = enable;
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/HandleRg" },
                Result = true,
                Info = $"Test Controller will handle Responsible Gaming dialog: {enable.ToString()}"
            };
        }

        public CommandResult SetRgDialogOptions(string[] buttonNames)
        {
            _mouse.TimeLimitButtons = buttonNames.ToList();
            return new CommandResult();
        }

        public CommandResult RequestSpin()
        {
            _eventBus.Publish(new InputEvent(22, true));
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Runtime/RequestSpin" },
                Result = true,
                Info = "Request spin."
            };
        }

        public CommandResult SetBetLevel(int index)
        {
            _eventBus.Publish(new InputEvent(22 + index, true));
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Runtime/BetLevel/Set" },
                Result = true,
                Info = $"Request line index {index}."
            };
        }

        public CommandResult SetLineLevel(int index)
        {
            _eventBus.Publish(new InputEvent(29 + index, true));

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Runtime/LineLevel/Set" },
                Result = true,
                Info = $"Request line index {index}."
            };
        }

        public CommandResult SetBetMax()
        {
            _eventBus.Publish(new UpEvent((int)ButtonLogicalId.MaxBet));

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Runtime/BetMax/Set" },
                Result = true,
                Info = "Request bet max."
            };
        }

        public CommandResult GetMeter(string name, string category, string type, string game, string denom)
        {
            var response = new Dictionary<string, object>();

            var value = _meterManager.GetMeter(name, category, type, Convert.ToInt32(game), Convert.ToInt32(denom));

            response["metervalue"] = value.ToString();

            if (value < 0)
            {
                response["error"] = "Meter not found.";
            }

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Platform/Meters/{category}/{name}/{type}/Get", ["metervalue"] = response["metervalue"] },
                Result = true,
                Info = JsonConvert.SerializeObject(response)
            };
        }

        public CommandResult GetUpiMeters()
        {
            var response = new Dictionary<string, object>
            {
                { "response-to", "/Platform/Meters/Upi/Get" }
            };

            var values = _meterManager.GetUpiMeters();

            values.ToList().ForEach(x => response.Add(x.Key, x.Value));

            return new CommandResult
            {
                data = response,
                Result = true,
                Info = JsonConvert.SerializeObject(response)
            };
        }

        public CommandResult GetRuntimeState()
        {
            var response = new Dictionary<string, object> { { "response-to", "/Runtime/State" } };

            var state = _automata.GetRuntimeState() ?? "Unknown";

            response["game_state"] = state;

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Runtime/State", ["state"] = state },
                Result = true,
                Info = JsonConvert.SerializeObject(response)
            };
        }

        public CommandResult RuntimeLoaded()
        {
            var response = new Dictionary<string, object> { { "response-to", "/Runtime/Running" } };

            var running = _automata.GetRuntimeState();

            response["running"] = running;

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Runtime/Running", ["running"] = running },
                Result = true,
                Info = JsonConvert.SerializeObject(response)
            };
        }

        public CommandResult GetRuntimeMode()
        {
            var response = new Dictionary<string, object> { { "response-to", "/Runtime/Mode" } };

            response["mode"] = Enum.GetName(typeof(RuntimeMode), _runtimeMode);

            return new CommandResult()
            {
                data = response,
                Result = true,
                Info = JsonConvert.SerializeObject(response)
            };
        }

        public CommandResult SendInput(int input)
        {
            _eventBus.Publish(new InputEvent(input, false));

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/Platform/SendInput/{input}" },
                Result = true,
                Info = $"Sending input {input}"
            };
        }

        public CommandResult SendTouchGame(int x, int y)
        {
            _mouse.ClickGame(x, y);

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/TouchScreen/0/Touch" },
                Result = true,
                Info = $"Clicking main window at x: {x} y: {y} "
            };
        }

        public CommandResult SendTouchVBD(int x, int y)
        {
            _mouse.ClickVirtualButtonDeck(x, y);

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/TouchScreen/2/Touch" },
                Result = true,
                Info = $"Clicking VBD at x: {x} y: {y} "
            };
        }

        public CommandResult InsertCredits(int bill_value, string id)
        {
            if (!int.TryParse(id, out int deviceId))
            {
                return new CommandResult()
                {
                    data = new Dictionary<string, object>() { ["response-to"] = $"/BNA/{id}/Bill/Insert", ["Error"] = $"Could not cast {id} to a valid id." },
                    Result = false,
                    Info = $"Failed to insert {bill_value} for device {id}"
                };
            }
            CreateTable();

            try
            {
                _bnaNoteTransactionId++;
            }
            catch (OverflowException)
            {
                _bnaNoteTransactionId = 0;
            }


            _eventBus.Publish(new FakeDeviceMessageEvent
            {
                Message = new NoteValidated
                {
                    ReportId = GdsConstants.ReportId.NoteAcceptorAcceptNoteOrTicket,
                    TransactionId = _bnaNoteTransactionId,
                    NoteId = GetNoteId(bill_value)
                }
            });

            var result = new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/BNA/{id}/Bill/Insert" },
                Result = true,
                Info = $"Inserting {bill_value} dollars"
            };

            return result;
        }

		public CommandResult InsertTicket(string validation_id, string id)	 	 
		{	 	 
            try	 	 
		    {	 	 
		        _bnaTicketTransactionID++;	 	 
		    }	 	 
		    catch (OverflowException)	 	 
		    {	 	 
		        _bnaTicketTransactionID = 0;	 	 
		    }	 	 
		 
		    _eventBus.Publish(new FakeDeviceMessageEvent	 	 
		    {	 	 
		        Message = new TicketValidated	 	 
		        {	 	 
		                ReportId = GdsConstants.ReportId.NoteAcceptorAcceptNoteOrTicket,	 	 
		            TransactionId = _bnaTicketTransactionID,	 	 
		                Code = validation_id	 	 
		        }	 	 
		    });	 	 
		 
		    var result = new CommandResult()	 	 
		    {	 	 
		        data = new Dictionary<string, object> { ["response-to"] = $"/BNA/{id}/Ticket/Insert" },	 	 
		        Result = true,	 	 
		        Info = "Inserting ticket"	 	 
		    };	 	 
		 
		    return result;	 	 
		}
		
        public CommandResult PlayerCardEvent(bool inserted, string data)
        {
            _automata.CardEvent(inserted, data);

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/CardReaders/0/Event" },
                Result = true,
                Info = $"Card was requested {(inserted ? "inserted" : "removed")} with data: {data}"
            };
        }

        public CommandResult ServiceButton(bool pressed)
        {
            var attendantService = ServiceManager.GetInstance().GetService<IAttendantService>();
            attendantService.IsServiceRequested = pressed;
            //attendantService.OnServiceButtonPressed();

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/Service/Request" },
                Result = true,
                Info = $"Service button {(pressed ? "pressed" : "cleared")}"
            };
        }

        public CommandResult Lockup(LockupTypeEnum type, bool state)
        {
            BaseEvent evt = null;

            try
            {
                evt = MapLockup(type, state);

                if (evt != null)
                {
                    switch (evt)
                    {
                        case InputEvent i:
                        {
                            _eventBus.Publish(i);
                            break;
                        }
                        case HardwareFaultEvent hf:
                        {
                            _eventBus.Publish(hf);
                            break;
                        }
                        case HardwareFaultClearEvent hfc:
                        {
                            _eventBus.Publish(hfc);
                            break;
                        }
                        case LegitimacyLockUpEvent lle:
                        {
                            _eventBus.Publish(lle);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new CommandResult() { Result = false, Info = $"Error: {ex.ToString()}" };
            }

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/Lockup" },
                Result = evt != null,
                Info = $"Lockup of type {type} was requested {(state ? "entered" : "cleared")}"
            };
        }

        public CommandResult SetMaxWinLimitOverride(int maxWinLimitOverrideMillicents)
        {
            _automata.SetMaxWinLimit(maxWinLimitOverrideMillicents);

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/MaxWinLimitOverride" },
                Result = true,
                Info = $"Max Win Limit overridden to {maxWinLimitOverrideMillicents}"
            };
        }

        public CommandResult RequestHandPay(long amount, TransferOutType type, Account accountType)
        {
            //var playerBank = ServiceManager.GetInstance().TryGetService<IPlayerBank>();
            //playerBank.ForceHandpay(Guid.NewGuid(), amount, Accounting.Contracts.TransferOut.TransferOutReason.LargeWin, -1);

            var transferOutHandler = ServiceManager.GetInstance().GetService<ITransferOutHandler>();

            _eventBus.Publish(new CashOutStartedEvent(false, true));

            transferOutHandler.TransferOut<IHandpayProvider>(accountType.ToType(), amount, type.ToReason());

            Log($"Transfer out requested.  Amount: {amount}, Type: {type}, AccountType: {accountType}");

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/HandpayRequest" },
                Result = true,
                Info = $"Transfer out requested.  Amount: {amount}, Type: {type}, AccountType: {accountType}"
            };
        }

        public CommandResult RequestKeyOff()
        {
            _automata.JackpotKeyoff();

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/KeyOff" },
                Result = true,
                Info = "Jackpot key off requested."
            };
        }

        public CommandResult GetCurrentLockups()
        {
            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Lockups/Get", ["Lockups"] = string.Join("\n", _currentLockups.Values) },
                Result = true,
            };
        }

        public CommandResult GetGameMessages()
        {
            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Game/Messages/Lockup/GetGameScreenMessages", ["Value"] = string.Join("\n", _currentLockups.Values) },
                Result = true,
            };
        }

        public CommandResult GetNumberOfGameHistoryEntires()
        {
            var gameHistory = ServiceManager.GetInstance().TryGetService<IGameHistory>();

            var gameHistoryCount = 0;
            if (gameHistory != null)
            {

                gameHistoryCount = gameHistory.GetGameHistory().Count();

            }

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Game/History/NumberOfEntries", ["Value"] = gameHistoryCount.ToString() },
                Result = true,
            };
        }


        public CommandResult GetGameHistory(string count)
        {
            var responseInfo = new Dictionary<string, object> { ["response-to"] = $"/Game/History/{count}" };

            if (!Int32.TryParse(count, out int requestCount))
                requestCount = 1;


            var gameHistory = ServiceManager.GetInstance().TryGetService<IGameHistory>();

            var newHistory = gameHistory.GetGameHistory().ToList().TakeTransactions(gameHistory.LogSequence, requestCount);

            int collectedCount = 0;

            foreach (var transaction in newHistory)
            {
                // Creating a dictionary to capture the required data from each transaction object to be displayed as API response
                Dictionary<string, object> history = new Dictionary<string, object>()
                {
                    {"TransactionID", transaction.TransactionId},
                    {"GameID", transaction.GameId},
                    {"LogSequence", transaction.LogSequence},
                    {"DenomID", transaction.DenomId},
                    {"StartDateTime", transaction.StartDateTime},
                    {"EndDateTime", transaction.EndDateTime},
                    {"StartCredits", transaction.StartCredits},
                    {"EndCredits", transaction.EndCredits},
                    {"InitialWager", transaction.InitialWager},
                    {"FinalWager", transaction.FinalWager},
                    {"InitialWin", transaction.InitialWin },
                    {"FinalWin", transaction.FinalWin},
                    {"GameOutcome", transaction.Result.ToString()},
                    {"GamePlayState", transaction.PlayState.ToString()}
                };

                var entryNumber = "GameHistoryLogEntry" + (++collectedCount).ToString();
                responseInfo.Add(entryNumber, JsonConvert.SerializeObject(history));
            }

            return new CommandResult()
            {
                data = responseInfo,
                Result = true,
                Info = $"Test Controller returning {collectedCount} entries in game history log"
            };

        }

        public CommandResult GetHandPayLog(string count)
        {
            var responseInfo = new Dictionary<string, object> { ["response-to"] = $"Platform/Logs/HandPay/{count}" };

            if (!Int32.TryParse(count, out int requestCount))
                requestCount = 1;

            var transactionHistory = ServiceManager.GetInstance().TryGetService<ITransactionHistory>();
            var handPayTransactions = transactionHistory.RecallTransactions<HandpayTransaction>().OrderByDescending(o => o.TransactionId);
            var orderedTransactions = handPayTransactions.TakeTransactions(0, requestCount);

            int collectedCount = 0;
            foreach (var transaction in orderedTransactions)
            {
                var entryNumber = "HandPayEntry" + (++collectedCount).ToString();

                responseInfo.Add(entryNumber, JsonConvert.SerializeObject(transaction, new Newtonsoft.Json.Converters.StringEnumConverter()));
            }

            return new CommandResult()
            {
                data = responseInfo,
                Result = true,
                Info = $"Test Controller returning {collectedCount} entries in HandPay log"
            };

        }

        public CommandResult GetTransferOutLog(string count)
        {
            var responseInfo = new Dictionary<string, object> { ["response-to"] = $"Platform/Logs/TransferOut/{count}" };


            if (!Int32.TryParse(count, out int requestCount))
                requestCount = 1;

            var transactionHistory = ServiceManager.GetInstance().TryGetService<ITransactionHistory>();
            var transferOutLogs = transactionHistory.RecallTransactions<WatTransaction>().OrderByDescending(o => o.LogSequence);
            var orderedTransactions = transferOutLogs.TakeTransactions(0, requestCount);

            int collectedCount = 0;

            foreach (var transaction in orderedTransactions)
            {
                var entryNumber = "TransferOutLogEntry" + (++collectedCount).ToString();
                responseInfo.Add(entryNumber, JsonConvert.SerializeObject(transaction, new Newtonsoft.Json.Converters.StringEnumConverter()));
            }

            return new CommandResult()
            {
                data = responseInfo,
                Result = true,
                Info = $"Test Controller returning {collectedCount} entries in TransferOut log"
            };

        }

        public CommandResult GetTransferInLog(string count)
        {
            var responseInfo = new Dictionary<string, object> { ["response-to"] = $"Platform/Logs/TransferIn/{count}" };

            if (!Int32.TryParse(count, out int requestCount))
                requestCount = 1;

            var transactionHistory = ServiceManager.GetInstance().TryGetService<ITransactionHistory>();
            var transaferInLogs = transactionHistory.RecallTransactions<WatOnTransaction>().OrderByDescending(o => o.LogSequence);
            var orderedTransactions = transaferInLogs.TakeTransactions(0, requestCount);

            int collectedCount = 0;

            foreach (var transaction in orderedTransactions)
            {
                var entryNumber = "TransferInLogEntry" + (++collectedCount).ToString();
                responseInfo.Add(entryNumber, JsonConvert.SerializeObject(transaction, new Newtonsoft.Json.Converters.StringEnumConverter()));
            }

            return new CommandResult()
            {
                data = responseInfo,
                Result = true,
                Info = $"Test Controller returning {collectedCount} entries in TransferIn log"
            };

        }

        public CommandResult GetGameLineMessages()
        {
            string messages = "";            
            foreach (DisplayableMessage message in _gameLineMessages)
            {
                messages = messages + message.Message + "\n";
            }
            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Game/Messages/Get", ["Messages"] = messages},
                Result = true,
            };
        }

        public CommandResult GetGameOverlayMessage()
        {
            string message = string.Empty;

            if (_messageOverlayData != null)
            {
                message = $"{_messageOverlayData.Text} " +
                    $"{(_messageOverlayData.IsSubTextVisible ? _messageOverlayData.SubText : string.Empty)} " +
                    $"{(_messageOverlayData.IsSubText2Visible ? _messageOverlayData.SubText2 : string.Empty)}";
            }

            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    [ResponseTo] = OverlayMessageUrl,
                    [OverlayMessage] = message
                },
                Result = true,
            };
        }

        public CommandResult GetInfo(List<PlatformInfoEnum> info)
        {
            var desiredInfo = "";
            var desiredInfoCollective = "";
            var desiredInfoFound = false;

            var data = new StringBuilder();

            var dataMulti = new Dictionary<PlatformInfoEnum, string>();

            try
            {
                foreach (var type in info)
                {
                    switch (type)
                    {
                        case PlatformInfoEnum.ProcessMetrics:
                        {
                            var metrics = new Dictionary<string, string>();
                            _processMonitor.GetMetrics(metrics);
                            desiredInfo = JsonConvert.SerializeObject(metrics, Formatting.Indented);
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.GameInfo:
                        {
                            var games = _pm.GetValues<IGameProfile>(GamingConstants.Games).Where(g => g.Enabled).ToArray();

                            Array.ForEach(games, g => data.AppendLine(g.ThemeName));

                            desiredInfo = data.ToString();
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.PlayerBalance:
                        {
                            if (_bank == null) _bank = ServiceManager.GetInstance().TryGetService<IBank>();
                            desiredInfo = _bank.QueryBalance().ToString();
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.State:
                        {
                            desiredInfo = _platformState.ToString();
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.Printer:
                        {
                            var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
                            desiredInfo = printer?.ToAString();
                            desiredInfoFound = printer != null;
                            break;
                        }
                        case PlatformInfoEnum.NoteAcceptor:
                        {
                            var na = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                            desiredInfo = na?.ToAString();
                            desiredInfoFound = na != null;
                            break;
                        }
                        case PlatformInfoEnum.Os:
                        {
                            var os = ServiceManager.GetInstance().TryGetService<IOSService>();
                            desiredInfo = os?.ToAString();
                            desiredInfoFound = os != null;
                            break;
                        }
                        case PlatformInfoEnum.Io:
                        {
                            var io = ServiceManager.GetInstance().TryGetService<IIO>();
                            desiredInfo = io?.ToAString();
                            desiredInfoFound = io != null;
                            break;
                        }
                        case PlatformInfoEnum.Display:
                        {
                            var display = ServiceManager.GetInstance().TryGetService<IDisplayService>();
                            desiredInfo = display?.ToAString();
                            desiredInfoFound = display != null;
                            break;
                        }
                        case PlatformInfoEnum.Id:
                        {
                            var id = ServiceManager.GetInstance().TryGetService<IIdReader>();
                            desiredInfo = id?.ToAString();
                            desiredInfoFound = id != null;
                            break;
                        }
                        case PlatformInfoEnum.Network:
                        {
                            var net = ServiceManager.GetInstance().TryGetService<INetworkService>();
                            desiredInfo = net?.ToAString();
                            desiredInfoFound = net != null;
                            break;
                        }
                        case PlatformInfoEnum.Jurisdiction:
                        {
                            desiredInfo = _pm.GetProperty(ApplicationConstants.JurisdictionKey, "").ToString();
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.Protocol:
                        {
                            desiredInfo = _pm.GetProperty(ApplicationConstants.ActiveProtocol, "").ToString();
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.CurrentLockups:
                        {
                            desiredInfo = JsonConvert.SerializeObject(_currentLockups.Values, Formatting.Indented);
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.TowerLightState:
                        {
                            desiredInfo = JsonConvert.SerializeObject(_towerLightStates, Formatting.Indented);
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.Meters:
                        {
                            var meterManager = ServiceManager.GetInstance().TryGetService<IMeterManager>();
                            desiredInfo = meterManager?.ToAString();
                            desiredInfoFound = meterManager != null;
                            break;
                        }
                        case PlatformInfoEnum.IsRobotModeRunning:
                        {
                            desiredInfo = _automata.IsRobotModeRunning.ToString();
                            desiredInfoFound = true;
                            break;
                        }
                        case PlatformInfoEnum.Detailed:
                        {
                            var metrics = new Dictionary<string, string>();
                            _processMonitor.GetMetrics(metrics);
                            dataMulti.Add(PlatformInfoEnum.ProcessMetrics ,JsonConvert.SerializeObject(metrics, Formatting.Indented));

                            var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
                            dataMulti.Add(PlatformInfoEnum.Printer, printer.ToAString());

                            var na = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                            dataMulti.Add(PlatformInfoEnum.NoteAcceptor, na.ToAString());

                            var os = ServiceManager.GetInstance().TryGetService<IOSService>();
                            dataMulti.Add(PlatformInfoEnum.Os, os.ToAString());

                            var io = ServiceManager.GetInstance().TryGetService<IIO>();
                            dataMulti.Add(PlatformInfoEnum.Io, io.ToAString());

                            var display = ServiceManager.GetInstance().TryGetService<IDisplayService>();
                            dataMulti.Add(PlatformInfoEnum.Display, display.ToAString());

                            var id = ServiceManager.GetInstance().TryGetService<IIdReader>();
                            dataMulti.Add(PlatformInfoEnum.Id, id.ToAString());

                            var net = ServiceManager.GetInstance().TryGetService<INetworkService>();
                            dataMulti.Add(PlatformInfoEnum.Network, net.ToAString());

                            dataMulti.Add(PlatformInfoEnum.Jurisdiction, _pm.GetProperty(ApplicationConstants.JurisdictionKey, "").ToString());

                            data.AppendLine($"Protocol: {_pm.GetProperty(ApplicationConstants.ActiveProtocol, "").ToString()}");
                            dataMulti.Add(PlatformInfoEnum.Protocol, _pm.GetProperty(ApplicationConstants.ActiveProtocol, "").ToString());

                            var games = _pm.GetValues<IGameProfile>(GamingConstants.Games).ToList();
                            dataMulti.Add(PlatformInfoEnum.GameInfo, JsonConvert.SerializeObject(games, Formatting.Indented));

                            dataMulti.Add(PlatformInfoEnum.CurrentLockups, JsonConvert.SerializeObject(_currentLockups, Formatting.Indented));

                            dataMulti.Add(PlatformInfoEnum.IsRobotModeRunning, _automata.IsRobotModeRunning.ToString());

                            desiredInfo = data.ToString();
                            desiredInfoFound = true;
                            break;
                        }
                        default:
                        {
                            desiredInfoFound = false;
                            break;
                        }
                    }

                    dataMulti.Add(type, desiredInfo);
                }

                desiredInfoCollective = JsonConvert.SerializeObject(dataMulti);
            }
            catch (Exception ex)
            {
                desiredInfo = ex.ToString();
                desiredInfoFound = false;
                _logger.Error(desiredInfo);
            }

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/Info" },
                Result = desiredInfoFound,
                Info = desiredInfoCollective
            };
        }

        public CommandResult GetProgressives(string gameName)
        {
            var info = _automata.GetPools(gameName);
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Platform/Progressives/Get", ["progressives"] = info },
                Result = info != string.Empty,
                Info = info
            };
        }

        public CommandResult GetConfigOption(string option)
        {
            var desiredInfo = "";
            var desiredInfoFound = false;

            try
            {
                //foreach (string type in option)
                    ConfigOptionInfo optName = (ConfigOptionInfo)Enum.Parse(typeof(ConfigOptionInfo), option);

                    switch (optName)
                    {
                        case ConfigOptionInfo.CreditLimit:
                            {
                                var maxCreditLimit = _pm.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue) / 1000;

                                desiredInfo = maxCreditLimit.ToString();
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.EftAftTransferLimit:
                            {
                                //get AftTransferLimit
                                var features = _pm.GetValue(Sas.Contracts.SASProperties.SasProperties.SasFeatureSettings, new SasFeatures());

                                desiredInfo = features.TransferLimit.ToString();
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.EftAftTransferInMode:
                            {
                                //get AftInEnabled
                                desiredInfo = _pm.GetValue(Sas.Contracts.SASProperties.SasProperties.SasFeatureSettings, new SasFeatures()).TransferInAllowed.ToString();
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.EftAftTransferOutMode:
                            {
                                //get AftOutEnabled
                                desiredInfo = _pm.GetValue(Sas.Contracts.SASProperties.SasProperties.SasFeatureSettings, new SasFeatures()).TransferOutAllowed.ToString();
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.BillAcceptorDriver:
                            {
                                //get BillAcceptorDriver
                                desiredInfo = "Bill Acceptor Drover Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.PrinterDriver:
                            {
                                //get PrinterDriver
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.VoucherInLimit:
                            {
                                //get VoucherInLimit
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.VoucherOutLimit:
                            {
                                //get VoucherOutLimit
                                var voucherOutLimit = ((long)_pm.GetProperty(AccountingConstants.VoucherOutLimit, long.MaxValue)) / 1000;
                                desiredInfo = voucherOutLimit.ToString();                                    
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.PrintPromoTickets:
                            {
                                //get PrintPromoTickets
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.ValidationType:
                            {
                                //var validationType = Sas.Contracts.SASProperties.SasValidationType;
                                //desiredInfo = validationType.ToString();
                                //desiredInfoFound = true;
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.SerialNumber:
                            {
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.MachineId:
                            {
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.Protocol:
                            {
                                desiredInfo = _pm.GetProperty(ApplicationConstants.Protocol, "").ToString();
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.SasHost1Address:
                            {
                                var hosts = _pm.GetValue(Sas.Contracts.SASProperties.SasProperties.SasHosts, Enumerable.Empty<Host>());
                                desiredInfo = string.Join(" : ", hosts.Select(x => x.ComPort.ToString()));
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.SasHost2Address:
                            {
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.GameDenomValidation:
                            {
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.G2SHostUri:
                            {
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.ZoneId:
                            {
                                desiredInfo = "Not Implemented";
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.LargeWinLimit:
                            {
                                var largeWinLimit = ((long)_pm.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit)) / 1000;
                                desiredInfo = largeWinLimit.ToString();
                                desiredInfoFound = true;
                                break;
                            }
                        case ConfigOptionInfo.HandpayLimit:
                            {
                                var handpayLimit = ((long)_pm.GetProperty(AccountingConstants.HandpayLimit, AccountingConstants.DefaultHandpayLimit)) / 1000;
                                desiredInfo = handpayLimit.ToString();
                                desiredInfoFound = true;
                                break;
                            }
                    case ConfigOptionInfo.PrintHandpayReceipt:
                        {
                            var printHandpayReceipt = _pm.GetValue(AccountingConstants.EnableReceipts, false);
                            desiredInfo = printHandpayReceipt.ToString();
                            desiredInfoFound = true;
                            break;
                        }
                    default:
                            {
                                desiredInfoFound = false;
                                break;
                            }
                    }
            }


            catch (Exception ex)
            {
                desiredInfo = ex.ToString();
                desiredInfoFound = false;
                _logger.Error(desiredInfo);
            }

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Config/Read", ["Value"] = string.Join("\n", desiredInfo) },
                Result = desiredInfoFound
            };

        }

        public CommandResult SetConfigOption(string option, object value)
        {
            try
            {
                //foreach (string type in option)
                {
                    ConfigOptionInfo optName = (ConfigOptionInfo)Enum.Parse(typeof(ConfigOptionInfo), option);

                    switch (optName)
                    {
                        case ConfigOptionInfo.CreditLimit:
                            {
                                _pm.SetProperty(AccountingConstants.MaxCreditMeter, Convert.ToDecimal(value).DollarsToMillicents());
                                break;
                            }
                        case ConfigOptionInfo.EftAftTransferLimit:
                            {
                                var features = _pm.GetValue(Sas.Contracts.SASProperties.SasProperties.SasFeatureSettings, new SasFeatures());
                                features.TransferLimit = Convert.ToDecimal(value).DollarsToCents();
                                _pm.SetProperty(Sas.Contracts.SASProperties.SasProperties.SasFeatureSettings, features);
                                break;
                            }
                        case ConfigOptionInfo.EftAftTransferInMode:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.EftAftTransferOutMode:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.BillAcceptorDriver:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.PrinterDriver:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.VoucherInLimit:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.VoucherOutLimit:
                            {
                                //get VoucherOutLimit
                                _pm.SetProperty(AccountingConstants.VoucherOutLimit, Convert.ToDecimal(value).DollarsToMillicents());                                
                                break;                               
                            }
                        case ConfigOptionInfo.PrintPromoTickets:
                            {
                                bool allowVoucherOutNonCash = Equals(value,"true");
                                _pm.SetProperty(AccountingConstants.VoucherOutNonCash, allowVoucherOutNonCash);
                                break;
                            }
                        case ConfigOptionInfo.ValidationType:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.SerialNumber:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.MachineId:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.Protocol:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.SasHost1Address:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.SasHost2Address:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.GameDenomValidation:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.G2SHostUri:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.HostCashoutAction:
                            {
                                Enum.TryParse(value.ToString(), out CashableLockupStrategy strategy);
                                _pm.SetProperty(GamingConstants.LockupBehavior, strategy);
                                break;
                            }
                        case ConfigOptionInfo.ZoneId:
                            {
                                
                                break;
                            }
                        case ConfigOptionInfo.LargeWinLimit:
                            {
                               _pm.SetProperty(AccountingConstants.LargeWinLimit, Convert.ToDecimal(value).DollarsToMillicents());                                
                                break;
                            }
                        case ConfigOptionInfo.HandpayLimit:
                            {
                                _pm.SetProperty(AccountingConstants.HandpayLimit, Convert.ToDecimal(value).DollarsToMillicents());                                
                                break;
                            }
                        case ConfigOptionInfo.PrintHandpayReceipt:
                            {
                                bool allowPrintHandpay = Equals(value, "true");
                                _pm.SetProperty(AccountingConstants.EnableReceipts, allowPrintHandpay);
                                break;
                            }
                        default:
                            {
                                
                                break;
                            }
                    }

                }

            }
            catch (Exception ex)
            {                
                _logger.Error(ex.ToString());
            }

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Config/Write" },
                Result = true,
                Info = $"Set Property {option}"
            };
        }

        public CommandResult Wait(string evt, int timeout)
        {
            ClearWaits();

            if (Enum.TryParse<WaitEventEnum>(evt, true, out var wait))
            {
                _waitStrategy = new WaitSingle(wait, timeout);
                _waitStrategy.Start();
            }

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Event/Wait" },
                Result = true,
                Info = $"Waiting for {evt}"
            };
        }

        public CommandResult WaitAll(string[] events, int timeout)
        {
            var waits = events.Select(a => (WaitEventEnum)Enum.Parse(typeof(WaitEventEnum), a)).ToList();

            _waitStrategy = new WaitAll(waits, timeout);
            _waitStrategy.Start();

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Event/Wait/All" },
                Result = true,
                Info = $"Waiting for all of {string.Join(" ", events)}"
            };
        }

        public CommandResult WaitAny(string[] events, int timeout)
        {
            var waits = events.Select(a => (WaitEventEnum)Enum.Parse(typeof(WaitEventEnum), a)).ToList();

            _waitStrategy = new WaitAny(waits, timeout);
            _waitStrategy.Start();

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Event/Wait/Any" },
                Result = true,
                Info = $"Waiting for any of {string.Join(" ", events)}"
            };
        }

        public CommandResult CancelWait(string evtType)
        {
            Enum.TryParse(evtType, out WaitEventEnum wait);

            _waitStrategy?.CancelWait(wait);

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Event/Wait/Cancel" },
                Result = true,
                Info = $"Canceling wait for {evtType}"
            };
        }

        public CommandResult ClearWaits()
        {
            _waitStrategy?.ClearWaits();

            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Event/Wait/Clear" },
                Result = true,
                Info = "All waits are cleared."
            };
        }

        public CommandResult CheckWaitStatus()
        {
            var response = new Dictionary<string, object> { ["response-to"] = "/Event/Wait/Status" };

            var status = new Dictionary<WaitEventEnum, WaitStatus>();

            var debugOutput = "";

            var result = _waitStrategy?.CheckStatus(status, out debugOutput);

            foreach (var wait in status)
            {
                response[wait.Key.ToString()] = wait.Value.ToString();
            }

            if (!string.IsNullOrEmpty(result))
            {
                response["result"] = result;
            }

            return new CommandResult()
            {
                data = response,
                Result = true,
                Info = debugOutput
            };
        }

        private void CheckForWait(Type eventType)
        {
            var foundEnum = MapEventType(eventType);

            _waitStrategy?.EventPublished(foundEnum);
        }

        public object GetProperty(string property)
        {
            return _pm.GetProperty(property, null);
        }

        public void SetProperty(string property, object value, bool isConfig)
        {
            _pm.SetProperty(property, value, isConfig);
        }

        public CommandResult SetIo(string input, bool status)
        {
            _io.SetInput(input, status);

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Io/Set" },
                Result = true,
            };
        }

        public CommandResult GetIo()
        {
            var inputs = _io.GetInputs();

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = "/Io/Get", ["Map"] = JsonConvert.SerializeObject(inputs) },
                Result = true,
            };
        }

        private void HandleTimeLimitDialog()
        {
            if (!_handleRg)
            {
                return;
            }

            Log("Attempting to dismiss the RG Time Limit Dialog");
            _mouse.ClickRG(); //clicks the 60 minute option of the Responsible Gaming dialog.
        }

        private static WaitEventEnum MapEventType(Type eventType)
        {
            var enumFound = WaitEventEnum.InvalidEvent;

            if (EventWaitMap.ContainsKey(eventType))
            {
                enumFound = EventWaitMap[eventType];
            }

            return enumFound;
        }

        private BaseEvent MapLockup(LockupTypeEnum type, bool state)
        {
            BaseEvent evt = null;

            try
            {
                switch (type)
                {
                    case LockupTypeEnum.MainDoor:
                    {
                        evt = new InputEvent(49, state);
                        break;
                    }
                    case LockupTypeEnum.BellyDoor:
                    {
                        evt = new InputEvent(51, state);
                        break;
                    }
                    case LockupTypeEnum.CashDoor:
                    {
                        evt = new InputEvent(50, state);
                        break;
                    }
                    case LockupTypeEnum.Stacker:
                    {
                        if (state)
                        {
                            evt = new Hardware.Contracts.NoteAcceptor.HardwareFaultEvent(
                                NoteAcceptorFaultTypes.StackerDisconnected);
                        }
                        else
                        {
                            evt = new Hardware.Contracts.NoteAcceptor.HardwareFaultClearEvent(
                                NoteAcceptorFaultTypes.StackerDisconnected);
                        }

                        break;
                    }
                    case LockupTypeEnum.SecondaryCashDoor:
                    {
                        evt = new InputEvent(1024, state);
                        break;
                    }
                    case LockupTypeEnum.LogicDoor:
                    {
                        evt = new InputEvent(45, state);
                        break;
                    }
                    case LockupTypeEnum.TopBox:
                    {
                        evt = new InputEvent(46, state);
                        break;
                    }
                    case LockupTypeEnum.DropDoor:
                    {
                        evt = new InputEvent(1037, state);
                        break;
                    }
                    case LockupTypeEnum.Legitimacy:
                    {
                        evt = new LegitimacyLockUpEvent();
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error($"{type} is not a known lockup type for Test Controller.: {e}");
            }

            return evt;
        }

        private void Log(string msg)
        {
            _logger.Info(msg);
        }

        #region Hardware

        public CommandResult TouchScreenConnect(string id)
        {
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/TouchScreen/{id}/Connect" },
                Command = "TouchScreenConnect",
                Result = true
            };
        }

        public CommandResult TouchScreenDisconnect(string id)
        {
            return new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/TouchScreen/{id}/Disconnect" },
                Command = "TouchScreenDisconnect",
                Result = true
            };
        }
        #endregion

        private static readonly Dictionary<Type, WaitEventEnum> EventWaitMap = new Dictionary<Type, WaitEventEnum>
        {
            [typeof(LobbyInitializedEvent)] = WaitEventEnum.LobbyLoaded,
            [typeof(PrimaryGameStartedEvent)] = WaitEventEnum.SpinStart,
            [typeof(PrimaryGameEndedEvent)] = WaitEventEnum.SpinComplete,
            [typeof(TimeLimitDialogVisibleEvent)] = WaitEventEnum.ResponsibleGamingDialogVisible,
            [typeof(TimeLimitDialogHiddenEvent)] = WaitEventEnum.ResponsibleGamingDialogInvisible,
            [typeof(GameSelectedEvent)] = WaitEventEnum.GameSelected,
            [typeof(GameProcessExitedEvent)] = WaitEventEnum.GameExited,
            [typeof(GameInitializationCompletedEvent)] = WaitEventEnum.GameLoaded,
            [typeof(GameIdleEvent)] = WaitEventEnum.GameIdle,
            [typeof(OperatorMenuEnteredEvent)] = WaitEventEnum.OperatorMenuEntered,
            [typeof(GamePlayDisabledEvent)] = WaitEventEnum.GamePlayDisabled,
            [typeof(SystemDisableAddedEvent)] = WaitEventEnum.SystemDisabled,
            [typeof(RecoveryStartedEvent)] = WaitEventEnum.RecoveryStarted,
            [typeof(RecoveryCompletePlaceHolder)] = WaitEventEnum.RecoveryComplete,
            [typeof(IdPresentedEvent)] = WaitEventEnum.IdPresented,
            [typeof(IdClearedEvent)] = WaitEventEnum.IdCleared,
            [typeof(ReadErrorEvent)] = WaitEventEnum.IdReadError,
            [typeof(IdReaderTimeoutEvent)] = WaitEventEnum.IdReaderTimeout
        };

        private byte _bnaNoteTransactionId = 0;
		private byte _bnaTicketTransactionID = 0;
		
        private class RecoveryCompletePlaceHolder
        {

        }
    }

    class DisplayableMessageComparer : IEqualityComparer<DisplayableMessage>
    {
        public bool Equals(DisplayableMessage x, DisplayableMessage y)
        {
            return x.Message.Trim().ToLower().Equals(y.Message.Trim().ToLower());
        }

        public int GetHashCode(DisplayableMessage obj)
        {
            return obj.Message.GetHashCode();
        }
    }
}