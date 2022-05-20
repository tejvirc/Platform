//
// Version: 1.0.0.0
// Copyright(c) 2021 by Aristocrat Technologies Australia Pty Ltd.
//
// DESIGNED, DEVELOPED and OWNED BY:
// Aristocrat Technologies Australia Pty Ltd.
// Building A, Pinnacle Office Park
// 85 Epping Rd.
// North Ryde, NSW 2113
// Australia
// +61 2 9013 6000
// www.aristocrat.com
//
//
// Copyright (c) 2021 by Aristocrat Technologies Australia Pty Ltd.
// ALL RIGHTS RESERVED.
//
// THIS SOFTWARE IS CONFIDENTIAL AND PROPRIETARY
// TO ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD. AND MAY NOT BE REPRODUCED,
// PUBLISHED, OR DISCLOSED TO OTHERS WITHOUT WRITTEN AUTHORIZATION
// BY ARISTOCRAT TECHNOLOGIES PTY LTD.
//
// COPYRIGHT NOTICE: This copyright notice may NOT be removed,
//                   obscured or modified without the written
//                   consent of the copyright owner:
//                   Aristocrat Technologies Australia Pty Ltd.
//
namespace Aristocrat.Monaco.TestController
{
    using Accounting.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Aristocrat.Monaco.Hardware.Contracts.Printer;
    using DataModel;
    using Gaming.Contracts;
    using Hardware.Contracts.Gds;
    using Hardware.Contracts.Gds.NoteAcceptor;
    using Hardware.Contracts.IO;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using HardwareFaultEvent = Hardware.Contracts.NoteAcceptor.HardwareFaultEvent;
    using NoteAcceptorDisconnectedEvent = Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor.DisconnectedEvent;
    using PrinterDisconnectedEvent = Aristocrat.Monaco.Hardware.Contracts.Printer.DisconnectedEvent;
    using VirtualDeviceType = Hardware.Contracts.SharedDevice.DeviceType;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using System.Linq;
    using Newtonsoft.Json;
    using RobotController.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;

    public partial class TestControllerEngine : ITestController
    {
        private static ILobbyStateManager _lobbyStateManager = null;
        private static ManualResetEvent   _returnedToLobbyEvent = new ManualResetEvent(false);
        private static bool               _logIt = true;
        private static ILog               _noteAcceptorLogger = LogManager.GetLogger("NoteAcceptorV2");
        private static ManualResetEvent   _printCompleted = new ManualResetEvent(false);
        private static ManualResetEvent   _gameLoaded = new ManualResetEvent(false);
        private static SemaphoreSlim      _oneAPICallAtATime = new SemaphoreSlim(1, 1);
        private static ManualResetEvent[] _insertCreditsEvents = new ManualResetEvent[3];
        private static ManualResetEvent   _insertCreditsCompletedAfterInsertCredits = new ManualResetEvent(false);
        private static ManualResetEvent   _bankBalanceChangedAfterInsertCredits = new ManualResetEvent(false);
        private static ManualResetEvent   _bankBalanceChangedAfterCashOut = new ManualResetEvent(false);

        private static ManualResetEvent   _currencyReturnedAfterInsertCredits = new ManualResetEvent(false);
        private static ManualResetEvent   _currencyStackedAfterInsertCredits = new ManualResetEvent(false);
        private static INoteAcceptor      _noteAcceptor = null;
        private static Stopwatch          _stopWatch = new Stopwatch();
        private static ManualResetEvent[] _cashOutEvents = new ManualResetEvent[2];
        private static ManualResetEvent[] _fakePrinterErrorEvents      = new ManualResetEvent[6];
        private static ManualResetEvent   _fakePrinterErrorChassisOpen   = new ManualResetEvent(false);
        private static ManualResetEvent   _fakePrinterErrorPaperEmpty    = new ManualResetEvent(false);
        private static ManualResetEvent   _fakePrinterErrorPaperInChute  = new ManualResetEvent(false);
        private static ManualResetEvent   _fakePrinterErrorPaperJam      = new ManualResetEvent(false);
        private static ManualResetEvent   _fakePrinterErrorPaperLow      = new ManualResetEvent(false);
        private static ManualResetEvent   _fakePrinterErrorPrintHeadOpen = new ManualResetEvent(false);
        private static ManualResetEvent   _fakePrinterDisconnected = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorDisconnected = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorMechanicalFault = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorOpticalFault = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorComponentFault = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorNvmFault = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorOtherFault = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorStackerDisconnected = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorNone = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorNoteJammed = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorFirmwareFault = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorCheatDetected = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorStackerFault = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorStackerFull = new ManualResetEvent(false);
        private static ManualResetEvent   _fakeNoteAcceptorStackerJammed = new ManualResetEvent(false);
        private static ManualResetEvent   _cashOutAborted = new ManualResetEvent(false);

        private static ManualResetEvent   _gamePlayInitiated = new ManualResetEvent(false);
        private static ManualResetEvent   _gameEnded = new ManualResetEvent(false);
        private static Stopwatch          _gameRunningStopWatch = new Stopwatch();
        private static ManualResetEvent[] _gamePlayEvents = new ManualResetEvent[2];
        private static Stopwatch          _gameLoadingStopwatch = new Stopwatch();


        //
        // NOTE***  We included the 250 value to allow negative testing
        //          We would expect the 250 to get returned not processed
        //          
        private static List<int>          _validBills = new List<int> { 1, 2, 5, 10, 20, 50, 100, 250 };
        private static int                _milliSecondsToWaitForSystemToReact = 3000;
        private static Action             _waitForSystemToReact = () => Thread.Sleep(_milliSecondsToWaitForSystemToReact);
        private static bool               _initialized = false;


        private static Func<NoteAcceptorStackerState, NoteAcceptorStackerState, string> _noteAcceptorStackerStatePresent = (stackerStateToTest, stackerState) =>
        {
            return ((stackerStateToTest & stackerState) == stackerState) ? "True" : "False";
        };

        private static Func<NoteAcceptorLogicalState, NoteAcceptorLogicalState, string> _noteAcceptorLogicalStatePresent = (stateToTest, state) =>
        {
            return ((stateToTest & state) == state) ? "True" : "False";
        };

        private static Func<NoteAcceptorFaultTypes, NoteAcceptorFaultTypes, string> _noteAcceptorFaultPresent = (faultToTest, faultType) =>
        {
            return ((faultToTest & faultType) == faultType) ? "True" : "False";
        };

        /// <summary>
        /// This Action<> just displays debugging information on the Bootstrap Console.
        /// if you want to see the debug message that this action will produce
        /// you need to define SHOW_DEBUG_MESSAGES_FORV2 in
        /// Solution Monaco -> Test -> Automation -> Aristocrat.Monaco.TestController
        /// else { it does nothing }
        /// </summary>
        private static Action<string,bool> _displayDebugText = (message,logItFlag) =>
        {
            if (!string.IsNullOrEmpty(message))
            {

#if SHOW_DEBUG_MESSAGES_FORV2
                Console.WriteLine(message);
#endif
                if (logItFlag)
                {

                    if (message.Contains("ERRORMESSAGE:"))
                        _noteAcceptorLogger.Error(message);
                    else if (message.Contains("SUCCESS:"))
                        _noteAcceptorLogger.Info(message);
                    else
                        _noteAcceptorLogger.Debug(message);
                }
            }

        };

        /// <summary>
        /// This method initializes the objects needed by provided V2 methods
        /// NOTE****  This must be called in the Initialize() method
        /// in the TestControllerEngine.cs file 
        /// </summary>
        public void InitializeV2()
        {
            if (!_initialized)
            {
                //
                // setup the _insertCreditsEvents[] array
                // the code uses each of them separately by name
                // and the code waits on them both e.g. WaitHandle.WaitAll(_insertCreditsEvents)
                //
                _insertCreditsEvents[0] = _insertCreditsCompletedAfterInsertCredits;
                _insertCreditsEvents[1] = _bankBalanceChangedAfterInsertCredits;
                _insertCreditsEvents[2] = _currencyStackedAfterInsertCredits;
                _cashOutEvents[0] = _bankBalanceChangedAfterCashOut;
                _cashOutEvents[1] = _printCompleted;

                //
                // setup the errors that can happen during cash out
                //
                _fakePrinterErrorEvents[0] = _fakePrinterErrorChassisOpen;
                _fakePrinterErrorEvents[1] = _fakePrinterErrorPaperEmpty;
                _fakePrinterErrorEvents[2] = _fakePrinterErrorPaperInChute;
                _fakePrinterErrorEvents[3] = _fakePrinterErrorPaperJam;
                _fakePrinterErrorEvents[4] = _fakePrinterErrorPaperLow;
                _fakePrinterErrorEvents[5] = _fakePrinterErrorPrintHeadOpen;

                //
                // cash out events
                //
                _eventBus.Subscribe<PrintStartedEvent>(this, HandleEvent);
                _eventBus.Subscribe<PrintCompletedEvent>(this, HandleEvent);
                _eventBus.Subscribe<PrintFakeTicketEvent>(this, HandleEvent);
                _eventBus.Subscribe<FakePrinterEvent>(this, HandleEvent);
                _eventBus.Subscribe<ErrorWhilePrintingEvent>(this, HandleEvent);
                _eventBus.Subscribe<HardwareFaultEvent>(this, HandleEvent);
                _eventBus.Subscribe<CashOutStartedEvent>(this, HandleEvent);
                _eventBus.Subscribe<CashOutAbortedEvent>(this, HandleEvent);
                _eventBus.Subscribe<CashoutNotificationEvent>(this, HandleEvent);
                _eventBus.Subscribe<VoucherOutStartedEvent>(this, HandleEvent);
                _eventBus.Subscribe<VoucherIssuedEvent>(this, HandleEvent);

                //
                // cash in events
                //
                _eventBus.Subscribe<CurrencyInStartedEvent>(this, HandleEvent);
                _eventBus.Subscribe<CurrencyEscrowedEvent>(this, HandleEvent);
                _eventBus.Subscribe<CurrencyReturnedEvent>(this, HandleEvent);
                _eventBus.Subscribe<CurrencyInCompletedEvent>(this, HandleEvent);
                _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
                _eventBus.Subscribe<NoteAcceptorDisconnectedEvent>(this, HandleEvent);
                _eventBus.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);
                _eventBus.Subscribe<CurrencyStackedEvent>(this, HandleEvent);
                _initialized = true;

                _eventBus.Subscribe<GameLoadedEvent>(this, HandleEvent);
                _eventBus.Subscribe<GameStatusChangedEvent>(this, HandleEvent);
                _eventBus.Subscribe<LobbyInitializedEvent>(this, HandleEvent);
                _eventBus.Subscribe<GameRequestedLobbyEvent>(this, HandleEvent);
                _eventBus.Subscribe<GamePlayInitiatedEvent>(this, HandleEvent);
                _eventBus.Subscribe<GameEndedEvent>(this, HandleEvent);
                _gamePlayEvents[0] = _gamePlayInitiated;
                _gamePlayEvents[1] = _gameEnded;

            }

        }

        /// <summary>
        /// This method resets all the local (private) ManualResetEvents
        /// This action will reset any of the events that are currently set
        /// thus, allowing for a "known" starting state for the methods that consume them.
        /// </summary>
        private void ResetLocalManualResetEvents()
        {
            if (_currencyStackedAfterInsertCredits.WaitOne(0))
            {
                _currencyStackedAfterInsertCredits.Reset();
            }

            if (_currencyReturnedAfterInsertCredits.WaitOne(0))
            {
                _currencyReturnedAfterInsertCredits.Reset();
            }

            if (_insertCreditsCompletedAfterInsertCredits.WaitOne(0))
            {
                _insertCreditsCompletedAfterInsertCredits.Reset();
            }

            if (_bankBalanceChangedAfterInsertCredits.WaitOne(0))
            {
                _bankBalanceChangedAfterInsertCredits.Reset();
            }

            if (_bankBalanceChangedAfterCashOut.WaitOne(0))
            {
                _bankBalanceChangedAfterCashOut.Reset();
            }

            if (_printCompleted.WaitOne(0))
            {
                _printCompleted.Reset();
            }

            if (_cashOutAborted.WaitOne(0))
            {
                _cashOutAborted.Reset();
            }

            if (_gamePlayInitiated.WaitOne(0))
            {
                _gamePlayInitiated.Reset();
            }

            if (_gameEnded.WaitOne(0))
            {
                _gameEnded.Reset();
            }

            if (_gameLoaded.WaitOne(0))
            {
                _gameLoaded.Reset();
            }

        }

        private void HandleEvent(IEvent evt)
        {
            //this method is required to support unhandled events.
        }

        private void HandleEvent(GameRequestedLobbyEvent evt)
        {
            _returnedToLobbyEvent.Set();
        }

        private void HandleEvent(LobbyInitializedEvent evt)
        {
            var guid = evt.GloballyUniqueId;
         
        }

        /// <summary>
        /// This function sets all the local events based on the flags in the FakePrinterEvent
        /// </summary>
        /// <param name="evt"></param>
        private void HandleEvent(FakePrinterEvent evt)
        {
            _displayDebugText($"FakePrinterEvent(1) - Chassis Open    {evt.ChassisOpen}", !_logIt);
            _displayDebugText($"FakePrinterEvent(2) - Paper Empty     {evt.PaperEmpty}", !_logIt);
            _displayDebugText($"FakePrinterEvent(3) - Paper In Chute  {evt.PaperInChute}", !_logIt);
            _displayDebugText($"FakePrinterEvent(4) - Paper Jam       {evt.PaperJam}", !_logIt);
            _displayDebugText($"FakePrinterEvent(5) - Paper Low       {evt.PaperLow}", !_logIt);
            _displayDebugText($"FakePrinterEvent(6) - Print Head Open {evt.PrintHeadOpen}", !_logIt);
            _displayDebugText($"FakePrinterEvent(7) - Top Of Form     {evt.TopOfForm}", !_logIt);

            //
            // toggle the events based on their bool value
            //
            if (evt.ChassisOpen)
            {
                _fakePrinterErrorChassisOpen.Set();
            }
            else
            {
                _fakePrinterErrorChassisOpen.Reset();
            }

            if (evt.PaperEmpty)
            {
                _fakePrinterErrorPaperEmpty.Set();
            }
            else
            {
                _fakePrinterErrorPaperEmpty.Reset();
            }

            if (evt.PaperInChute)
            {
                _fakePrinterErrorPaperInChute.Set();
            }
            else
            {
                _fakePrinterErrorPaperInChute.Reset();
            }

            if (evt.PaperJam)
            {
                _fakePrinterErrorPaperJam.Set();
            }
            else
            {
                _fakePrinterErrorPaperJam.Reset();
            }

            if (evt.PaperLow)
            {
                _fakePrinterErrorPaperLow.Set();
            }
            else
            {
                _fakePrinterErrorPaperLow.Reset();
            }

            if (evt.PrintHeadOpen)  {
                _fakePrinterErrorPrintHeadOpen.Set();
            }
            else
            {
                _fakePrinterErrorPrintHeadOpen.Reset();
            }

        }

        private void HandleEvent(PrintFakeTicketEvent evt)
        {
            _displayDebugText($"PrintFakeTicketEvent(1) - Ticket Text     {evt.TicketText}", !_logIt);
            _displayDebugText($"PrintFakeTicketEvent(1) - Time Stamp      {evt.Timestamp}", !_logIt);
        }

        private void HandleEvent(GameLoadedEvent evt)
        {
            _gameLoaded.Set();
        }

        private void HandleEvent(GameStatusChangedEvent evt)
        {
            // DONT CARE ABOUT THIS - SO FAR
        }

        private void HandleEvent(GamePlayInitiatedEvent evt)
        {
            _gameRunningStopWatch.Restart();
            _gamePlayInitiated.Set();
        }

        private void HandleEvent(GameEndedEvent evt)
        {
            _gameRunningStopWatch.Stop();
            _gameEnded.Set();
        }

        private void HandleEvent(HardwareFaultEvent evt)
        {
            //
            // toggle the events based on their bool value
            //
            // bool _ means Don't Care about the return value 
            //
            bool _ = ((evt.Fault & NoteAcceptorFaultTypes.MechanicalFault) == NoteAcceptorFaultTypes.MechanicalFault)
            ? _fakeNoteAcceptorMechanicalFault.Set()
            : _fakeNoteAcceptorMechanicalFault.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.MechanicalFault) == NoteAcceptorFaultTypes.MechanicalFault)
            ? _fakeNoteAcceptorMechanicalFault.Set()
            : _fakeNoteAcceptorMechanicalFault.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.OpticalFault) == NoteAcceptorFaultTypes.OpticalFault )
            ? _fakeNoteAcceptorOpticalFault.Set()
            : _fakeNoteAcceptorOpticalFault.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.ComponentFault) == NoteAcceptorFaultTypes.ComponentFault)
            ? _fakeNoteAcceptorComponentFault.Set()
            : _fakeNoteAcceptorComponentFault.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.NvmFault) == NoteAcceptorFaultTypes.NvmFault)
            ? _fakeNoteAcceptorNvmFault.Set()
            : _fakeNoteAcceptorNvmFault.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.OtherFault) == NoteAcceptorFaultTypes.OtherFault)
            ? _fakeNoteAcceptorOtherFault.Set()
            : _fakeNoteAcceptorOtherFault.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.StackerDisconnected) == NoteAcceptorFaultTypes.StackerDisconnected)
            ? _fakeNoteAcceptorStackerDisconnected.Set()
            : _fakeNoteAcceptorStackerDisconnected.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.None) == NoteAcceptorFaultTypes.None)
            ? _fakeNoteAcceptorNone.Set()
            : _fakeNoteAcceptorNone.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.NoteJammed) == NoteAcceptorFaultTypes.NoteJammed)
            ? _fakeNoteAcceptorNoteJammed.Set()
            : _fakeNoteAcceptorNoteJammed.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.FirmwareFault) == NoteAcceptorFaultTypes.FirmwareFault)
            ? _fakeNoteAcceptorFirmwareFault.Set()
            : _fakeNoteAcceptorFirmwareFault.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.CheatDetected) == NoteAcceptorFaultTypes.CheatDetected)
            ? _fakeNoteAcceptorCheatDetected.Set()
            : _fakeNoteAcceptorCheatDetected.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.StackerFault) == NoteAcceptorFaultTypes.StackerFault)
            ? _fakeNoteAcceptorStackerFault.Set()
            : _fakeNoteAcceptorStackerFault.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.StackerFull) == NoteAcceptorFaultTypes.StackerFull)
            ? _fakeNoteAcceptorStackerFull.Set()
            : _fakeNoteAcceptorStackerFull.Reset();

            _ = ((evt.Fault & NoteAcceptorFaultTypes.StackerJammed) == NoteAcceptorFaultTypes.StackerJammed)
            ? _fakeNoteAcceptorStackerJammed.Set()
            : _fakeNoteAcceptorStackerJammed.Reset();

        }

        private void HandleEvent(ErrorWhilePrintingEvent evt)
        {
            _displayDebugText($"ErrorWhilePrintingEvent(1) - PrinterId {evt.PrinterId} ", !_logIt);
            // DONT CARE ABOUT THIS - SO FAR
        }

        private void HandleEvent(PrinterDisconnectedEvent evt)
        {
            // DONT CARE ABOUT THIS - SO FAR
        }

        /// <summary>
        /// This event handler will set the local _currencyReturnedAfterInsertCredits event.
        /// This local event is used by the InsertCreditsV2 method and is reset there.
        /// </summary>
        /// <param name="evt"></param>
        private void HandleEvent(CurrencyReturnedEvent evt)
        {
            _displayDebugText($"CurrencyReturnedEvent(1) - Note = {evt.Note}", !_logIt);
            _currencyReturnedAfterInsertCredits.Set();
        }

        private void HandleEvent(CurrencyInCompletedEvent evt)
        {
            _displayDebugText($"CurrencyInCompletedEvent(1) - Note = {evt.Note}", !_logIt);
            _insertCreditsCompletedAfterInsertCredits.Set();
        }

        private void HandleEvent(CurrencyStackedEvent evt)
        {
            _displayDebugText($"CurrencyStackedEvent(1) - Note = {evt.Note}", !_logIt);
            _currencyStackedAfterInsertCredits.Set();
        }


        private void HandleEvent(CurrencyInStartedEvent evt)
        {
            _displayDebugText($"CurrencyInStartedEvent(1) - Note = {evt.Note}", !_logIt);
            // DONT CARE ABOUT THIS - SO FAR
        }

        private void HandleEvent(BankBalanceChangedEvent evt)
        {
            _displayDebugText($"BankBalanceChangedEvent(1) - Old Balance = {evt.OldBalance} New Balance = {evt.NewBalance}", !_logIt);
            _bankBalanceChangedAfterInsertCredits.Set();
            _bankBalanceChangedAfterCashOut.Set();
        }

        private void HandleEvent(CurrencyEscrowedEvent evt)
        {
            _displayDebugText($"CurrencyEscrowedEvent(1) - Note = {evt.Note.Value}", !_logIt);
            // DONT CARE So Far

        }

        private void HandleEvent(PrintStartedEvent evt)
        {
            _displayDebugText($"PrintStartedEvent(1) - Time Stamp = {evt.Timestamp}", !_logIt);
            // DONT CARE ABOUT THIS - SO FAR
        }

        private void HandleEvent(PrintCompletedEvent evt)
        {
            _displayDebugText($"PrintCompletedEvent(1) - TimeStamp = {evt.Timestamp}", !_logIt);
            _printCompleted.Set();
        }

        private void HandleEvent(CashOutStartedEvent evt)
        {
            _displayDebugText($"CashOutStartedEvent(1) - Time Stamp = {evt.Timestamp}", !_logIt);
        }

        private void HandleEvent(CashOutAbortedEvent evt)
        {
            _displayDebugText($"CashOutAbortedEvent(1) - Time Stamp = {evt.Timestamp}", !_logIt);
            _cashOutAborted.Set();
        }

        private void HandleEvent(CashoutNotificationEvent evt)
        {
            _displayDebugText($"CashoutNotificationEvent(1) - Time Stamp = {evt.Timestamp} Paper In Chute =  {evt.PaperIsInChute}  ", !_logIt);
            // DONT CARE ABOUT THIS - SO FAR
        }


        private void HandleEvent(VoucherOutStartedEvent evt)
        {
            _displayDebugText($"VoucherOutStartedEvent(1) - Time Stamp = {evt.Timestamp} Amount = {evt.Amount}", !_logIt);
            // DONT CARE ABOUT THIS - SO FAR
        }

        private void HandleEvent(VoucherIssuedEvent evt)
        {
            _displayDebugText($"VoucherIssuedEvent(1) - Time Stamp = {evt.Timestamp} ", !_logIt);
        }

        private void HandleEvent(VoucherOutCanceledEvent evt)
        {
            _displayDebugText($"VoucherOutCanceledEvent(1) - Time Stamp = {evt.Timestamp}", !_logIt);
        }


        private void HandleEvent(NoteAcceptorDisconnectedEvent evt)
        {
            // DONT CARE ABOUT THIS - SO FAR
        }

        private void HandleEvent(FakeDeviceConnectedEvent evt)
        {

            switch ( evt.Type )
            {
                case VirtualDeviceType.Printer:
                    if (evt.Connected)
                    {
                        _fakePrinterDisconnected.Reset();
                        _displayDebugText($"Fake Printer Connected {evt.Timestamp}", !_logIt);
                    }
                    else
                    {
                        _fakePrinterDisconnected.Set();
                        _displayDebugText($"Fake Printer Disconnected {evt.Timestamp}", !_logIt);
                    }
                    break;

                case VirtualDeviceType.NoteAcceptor:
                    if (evt.Connected)
                    {
                        _fakeNoteAcceptorDisconnected.Reset();
                        _displayDebugText($"Fake Note Acceptor Connected {evt.Timestamp}", !_logIt);
                    }
                    else
                    {
                        _fakeNoteAcceptorDisconnected.Set();
                        _displayDebugText($"Fake Note Acceptor Disconnected {evt.Timestamp}", !_logIt);
                    }
                    break;

                default:
                    _displayDebugText($"ERRORMESSAGE: Unknown Virtual Device Type {evt.Timestamp}", !_logIt);
                    break;

            }
     
        }

        public CommandResult GetAvailableGamesV2(string test_name)
        {
            CommandResult commandResult = null;

            if (_oneAPICallAtATime.WaitAsync(TimeSpan.FromSeconds(15)).Result)
            {

                var games = _pm.GetValues<IGameDetail>(GamingConstants.Games).ToList();

                commandResult = new CommandResult()
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = "/Platform/GetAvailableGamesV2",
                        ["status"] = "SUCCESS:",
                        ["location-in-code"] = "GetAvailableGamesV2(1)",
                        ["test-name"] = $"{test_name}",
                        ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                        ["number_of_games"] = games.Count
                    },
                    Result = true,
                    Info = $"Returning {games.Count} game records"
                };


                //
                // This purpose of this code is a little OBSCURE and maybe TRICKY!
                //
                // The code is using this int as part of the KEY for the dictionary values
                // this allows the calling code to generate the keys in the returned dictionary
                // the calling code knows the keys will be composed by the code indicted below
                // and will always look like "game_0" thru 'game_N"
                // so, if the EGM has 10 games the dictionary keys will be "game_0" thru "game_9"
                // see this line of code below...
                //  commandResult.data.Add($"game_{gameIndex++}", JsonConvert.SerializeObject(game));
                int gameIndex = 0;

                foreach (IGameProfile gameProfile in games)
                {

                    var game = new
                    {
                        theme_name = gameProfile.ThemeName,
                        paytable_name = gameProfile.PaytableName,
                        variation_id = gameProfile.VariationId,
                        product_code = gameProfile.ProductCode,
                        enabled = gameProfile.Enabled,
                        egm_enabled = gameProfile.EgmEnabled,
                        reference_id = gameProfile.ReferenceId,
                        maximum_wager_credits = gameProfile.MaximumWagerCredits,
                        minimum_wager_credits = gameProfile.MinimumWagerCredits,
                        game_type = (int)gameProfile.GameType,
                        game_status = (int)gameProfile.Status,
                        id = gameProfile.Id,
                        theme_id = gameProfile.ThemeId,
                        paytable_id = gameProfile.PaytableId,
                        active = gameProfile.Active,
                        version = gameProfile.Version
                    };

                    commandResult.data.Add($"game_{gameIndex++}", JsonConvert.SerializeObject(game));
                }
            }
            else
            {
                commandResult = new CommandResult()
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = "/Platform/GetAvailableGamesV2",
                        ["status"] = "ERRORMESSAGE:",
                        ["location-in-code"] = "GetAvailableGamesV2(1)",
                        ["test-name"] = $"{test_name}",
                        ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                        ["error-message"] = $"Could not get access to the method (The wait timed out).  Please try again. "
                    },
                    Result = false,
                    Info = $"ERRORMESSAGE: Request to RequestSpinV2 Failed. The synchronization semaphore wait just timed out?"
                };

                _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

            }

            ResetLocalManualResetEvents();
            _oneAPICallAtATime.Release();

            return commandResult;
        }

        public CommandResult TryLoadGameV2(string gameName, string denomination, string test_name)
        {

            CommandResult commandResult = null;

            //
            // This should never be a problem - but who knows what the test developers might try to do?
            // 
            if (_oneAPICallAtATime.WaitAsync(TimeSpan.FromSeconds(15)).Result)
            {

                _stopWatch.Restart();
                _gameLoadingStopwatch.Reset();

                //
                // go back to lobby
                //
                _eventBus.Publish(new GameRequestedLobbyEvent(true));

                //
                // wait for the lobby event to fire
                //
                if (_returnedToLobbyEvent.WaitOne(TimeSpan.FromSeconds(30)))
                {

                    //
                    // reset the lobby event
                    //
                    _returnedToLobbyEvent.Reset();

                    //
                    // get the lobby state manager
                    // 
                    if (null == _lobbyStateManager)
                    {
                        _lobbyStateManager = ServiceManager.GetInstance().GetService<IContainerService>().Container.GetInstance<ILobbyStateManager>();
                    }

                    //
                    // now we have to wait for the lobby to
                    // go back to the "Chooser" State or the game will never load
                    int  triesToGetChooserLobbyState = 0;
                    bool lobbyStateIsChooser = false;

                    //
                    // wait for the stat to change
                    // 20 iterations of 2 second sleeps
                    // NOTE - if it is going to work at all it takes just 2-3 tries...
                    do
                    {
                        if (_lobbyStateManager.CurrentState == LobbyState.Chooser)
                        {
                            lobbyStateIsChooser = true;
                        }
                        else
                        {
                            Thread.Sleep(2000);
                        }

                        triesToGetChooserLobbyState++;

                    } while (triesToGetChooserLobbyState < 20 && !lobbyStateIsChooser);

                    //
                    // OK - We can proceed now
                    //
                    if (lobbyStateIsChooser)
                    {

                        ResetLocalManualResetEvents();

                        try
                        {

                            //
                            // the service is expecting the denomination to be in the form of 0.01 and so forth
                            // this is consistent with the older version
                            //
                            if (decimal.TryParse(denomination, out decimal denom))
                            {
                                // E.g.  (0.01 * 100000M) gets it into 1000 millipennies
                                denom *= 100000M;

                                var games = _pm.GetValues<IGameDetail>(GamingConstants.Games).ToList();

                                if (null != games)
                                {

                                    var gameInfo = games.FirstOrDefault(g => g.ThemeName == gameName && g.EgmEnabled && g.ActiveDenominations.Contains(Convert.ToInt64(denom)));

                                    if (null != gameInfo)
                                    {

                                        _gameLoadingStopwatch.Start();

                                        _eventBus.Publish(new GameLoadRequestedEvent() { GameId = gameInfo.Id });

                                        if (_gameLoaded.WaitOne(TimeSpan.FromSeconds(30)))
                                        {
                                            _stopWatch.Stop();
                                            _gameLoadingStopwatch.Stop();

                                            commandResult = new CommandResult()
                                            {
                                                data = new Dictionary<string, object>
                                                {
                                                    ["response-to"] = "/Platform/LoadGameV2",
                                                    ["status"] = "SUCCESS:",
                                                    ["location-in-code"] = "TryLoadGameV2(1)",
                                                    ["test-name"] = $"{test_name}",
                                                    ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                                    ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                                    ["load-time"] = _gameLoadingStopwatch.ElapsedMilliseconds.ToString() + " ms."
                                                },
                                                Result = true,
                                                Info = $"SUCCESS: TryLoadGameV2() Game ({gameName}) Loaded Successfully."
                                            };

                                            _displayDebugText($"{commandResult.Info}", _logIt);



                                        }
                                        else
                                        {
                                            _stopWatch.Stop();
                                            _gameLoadingStopwatch.Stop();

                                            commandResult = new CommandResult()
                                            {
                                                data = new Dictionary<string, object>
                                                {
                                                    ["response-to"] = "/Platform/LoadGameV2",
                                                    ["status"] = "ERRORMESSAGE:",
                                                    ["location-in-code"] = "TryLoadGameV2(2)",
                                                    ["test-name"] = $"{test_name}",
                                                    ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                                    ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                                    ["load-time"] = "0 ms."
                                                },
                                                Result = false,
                                                Info = $"ERRORMESSAGE: TryLoadGameV2() failed. Wait for Game Loaded Event Timed-Out. Game state is unknown."
                                            };

                                            _displayDebugText($"{commandResult.Info}", _logIt);

                                        }



                                    }  // if (null != gameInfo)
                                    else
                                    {
                                        _stopWatch.Stop();

                                        commandResult = new CommandResult()
                                        {
                                            data = new Dictionary<string, object>
                                            {
                                                ["response-to"] = "/Platform/LoadGameV2",
                                                ["status"] = "ERRORMESSAGE:",
                                                ["location-in-code"] = "TryLoadGameV2(3)",
                                                ["test-name"] = $"{test_name}",
                                                ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                                ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                                ["load-time"] = "0 ms."
                                            },
                                            Result = false,
                                            Info = $"ERRORMESSAGE: TryLoadGameV2() failed. Could not find a game to load using the parameters provided."
                                        };

                                        _displayDebugText($"{commandResult.Info}", _logIt);


                                    }

                                } // if (null != games)
                                else
                                {
                                    _stopWatch.Stop();

                                    commandResult = new CommandResult()
                                    {
                                        data = new Dictionary<string, object>
                                        {
                                            ["response-to"] = "/Platform/LoadGameV2",
                                            ["status"] = "ERRORMESSAGE:",
                                            ["location-in-code"] = "TryLoadGameV2(4)",
                                            ["test-name"] = $"{test_name}",
                                            ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                            ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                            ["load-time"] = "0 ms."
                                        },
                                        Result = false,
                                        Info = $"ERRORMESSAGE: TryLoadGameV2() failed. Internal Error - No Games Retrieved."
                                    };

                                    _displayDebugText($"{commandResult.Info}", _logIt);

                                }

                            }  // if (decimal.TryParse(denomination, out decimal denom))
                            else
                            {
                                _stopWatch.Stop();

                                commandResult = new CommandResult()
                                {
                                    data = new Dictionary<string, object>
                                    {
                                        ["response-to"] = "/Platform/LoadGameV2",
                                        ["status"] = "ERRORMESSAGE:",
                                        ["location-in-code"] = "TryLoadGameV2(5)",
                                        ["test-name"] = $"{test_name}",
                                        ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                        ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                        ["load-time"] = "0 ms."
                                    },
                                    Result = false,
                                    Info = $"ERRORMESSAGE: TryLoadGameV2() failed. The denomination ({denomination}) would not parse into a decimal value."
                                };

                                _displayDebugText($"{commandResult.Info}", _logIt);

                            }
                        }
                        catch (Exception ex)
                        {
                            _stopWatch.Stop();

                            commandResult = new CommandResult()
                            {
                                data = new Dictionary<string, object>
                                {
                                    ["response-to"] = "/Platform/LoadGameV2",
                                    ["status"] = "ERRORMESSAGE:",
                                    ["location-in-code"] = "TryLoadGameV2(6)",
                                    ["test-name"] = $"{test_name}",
                                    ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                    ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                    ["load-time"] = "0 ms."
                                },
                                Result = false,
                                Info = $"ERRORMESSAGE: TryLoadGameV2() failed with this Exception - ({ex.Message})"
                            };

                            _displayDebugText($"{commandResult.Info}", _logIt);

                        }

                    } // if (lobbyStateIsChooser)
                    else
                    {
                        _stopWatch.Stop();
                   
                        commandResult = new CommandResult()
                        {
                            data = new Dictionary<string, object>
                            {
                                ["response-to"] = "/Platform/LoadGameV2",
                                ["status"] = "ERRORMESSAGE:",
                                ["location-in-code"] = "TryLoadGameV2(1)",
                                ["test-name"] = $"{test_name}",
                                ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                ["load-time"] = "0 ms."
                            },
                            Result = false,
                            Info = $"ERRORMESSAGE: TryLoadGameV2() Failed with Internal Error - Lobby State did not allow the game to be loaded.."
                        };

                        _displayDebugText($"{commandResult.Info}", _logIt);

                    }


                } // if (_returnedToLobbyEvent.WaitOne(TimeSpan.FromSeconds(30)))
                else
                {
                    _stopWatch.Stop();

                    commandResult = new CommandResult()
                    {
                        data = new Dictionary<string, object>
                        {
                            ["response-to"] = "/Platform/LoadGameV2",
                            ["status"] = "ERRORMESSAGE:",
                            ["location-in-code"] = "TryLoadGameV2(1)",
                            ["test-name"] = $"{test_name}",
                            ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                            ["load-time"] = "0 ms."
                        },
                        Result = false,
                        Info = $"ERRORMESSAGE: TryLoadGameV2() Failed with Internal Error - Return to Lobby Event never fired."
                    };

                    _displayDebugText($"({test_name}) reports {commandResult.Info}", _logIt);

                }

            } // if (_oneAPICallAtATime.WaitAsync(TimeSpan.FromSeconds(15)).Result)
            else
            {
                _stopWatch.Stop();

                commandResult = new CommandResult()
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = "/Platform/LoadGameV2",
                        ["status"] = "ERRORMESSAGE:",
                        ["location-in-code"] = "TryLoadGameV2(7)",
                        ["test-name"] = $"{test_name}",
                        ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                        ["elapsed-time"] = "0 ms.",
                        ["load-time"] = "0 ms."
                    },
                    Result = false,
                    Info = $"ERRORMESSAGE: TryLoadGameV2() failed. The synchronization semaphore wait just timed out? "
                };

                _displayDebugText($"{commandResult.Info}", _logIt);

            }

            //
            // just make sure
            //
            _stopWatch.Stop();
            _gameLoadingStopwatch.Stop();
            ResetLocalManualResetEvents();
            _oneAPICallAtATime.Release();

            return commandResult;


        }

        public CommandResult RequestSpinV2(string test_name)
        {

            CommandResult commandResult = null;

            //
            // This should never be a problem - but who knows what the test developers might try to do?
            // 
            if (_oneAPICallAtATime.WaitAsync(TimeSpan.FromSeconds(15)).Result)
            {

                _stopWatch.Restart();

                ResetLocalManualResetEvents();

                //
                // tell everyone that cares about the Play Button Press...
                //
                // The number 22 used in the code below is from legacy code
                // This is all I could find elsewhere in the code
                // private const int Gen8PlayButtonPhysicalId = 22;
                //
                _eventBus.Publish(new InputEvent(22, true));


                //
                // wait up to 25 seconds for the required events to fire...
                // 
                //
                if ( WaitHandle.WaitAll(_gamePlayEvents, TimeSpan.FromSeconds(25)))
                {

                    
                    //
                    // happy path - return SUCCESS:
                    //
                    _stopWatch.Stop();

                    commandResult = new CommandResult()
                    {
                        data = new Dictionary<string, object>
                        {
                            ["response-to"]         = "/Runtime/RequestSpinV2",
                            ["status"]              = "SUCCESS:",
                            ["location-in-code"]    = "RequestSpinV2(1)",
                            ["test-name"]           = $"{test_name}",
                            ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                            ["game_play_initiated"] = "true",
                            ["game_play_ended"]     = "true",
                            ["game-time"]           = _gameRunningStopWatch.ElapsedMilliseconds.ToString() + " ms."
                        },
                        Result = true,
                        Info = $"Request to RequestSpinV2 completed successfully."
                    };

                    _displayDebugText($"SUCCESS: {commandResult.Info}.", _logIt);

                }
                else
                {
                    //
                    //  If we got here then the requested spin did not start or end
                    //  identify all the events that did not happen (not set)
                    //  the "error-message" will carry these 2 boolean values
                    //  that can be used for debugging in the caller's code.
                    //
                    bool a2 = _gamePlayInitiated.WaitOne(0);
                    bool b2 = _gameEnded.WaitOne(0);

                    commandResult = new CommandResult()
                    {
                        data = new Dictionary<string, object>
                        {
                            ["response-to"]         = "/Runtime/RequestSpinV2",
                            ["status"]              = "ERRORMESSAGE:",
                            ["test_name"]           = $"{test_name}",
                            ["location-in-code"]    = "RequestSpinV2(2)",
                            ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                            ["game-time"]           = "0 ms.",
                            ["game_play_initiated"] = $"{a2}",
                            ["game_play_ended"]     = $"{b2}",
                            ["error-message"]       = $"All expected events did not happen. Game Play Initiated = ({a2}) or Game Play Ended = ({b2}) did not get handled.  Current state of Game Play is unknown."
                        },
                        Result = false,
                        Info = $"ERRORMESSAGE: Request to RequestSpinV2 Failed. Expected Events did not happen"
                    };

                    _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                }

            }
            else
            {

                //
                // This is not really likely to ever happen!
                // If we got here then the thread synchronization semaphore was not acquired?
                //
                _stopWatch.Stop();

                commandResult = new CommandResult()
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"]         = "/Runtime/RequestSpinV2",
                        ["status"]              = "ERRORMESSAGE:",
                        ["test_name"]           = $"{test_name}",
                        ["location-in-code"]    = "RequestSpinV2(3)",
                        ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                        ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                        ["game-time"]           = "0 ms.",
                        ["error-message"]       = $"Could not get access to the method (The wait timed out).  Please try again. "
                    },
                    Result = false,
                    Info = $"ERRORMESSAGE: Request to RequestSpinV2 Failed. The synchronization semaphore wait just timed out?"
                };

                _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

            }

            //
            // just in case ...
            //
            _gameRunningStopWatch.Stop();

            ResetLocalManualResetEvents();
            _stopWatch.Stop();
            _oneAPICallAtATime.Release();

            return commandResult;

        }

        /// <summary>
        /// This method replaces the old BNA status that just returned "enabled" or "disabled"
        /// However the BNA being just "enabled" doe not mean it is going to work.  This new method
        /// returns a great deal of information that the calling Python test can log, analyze and display
        /// </summary>
        /// <param name="test_name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public CommandResult RequestBNAStatusV2(string test_name, string id)
        {
            CommandResult commandResult = null;

            //
            // This should never be a problem - but who knows what the test developers might try to do?
            // 
            if (_oneAPICallAtATime.WaitAsync(TimeSpan.FromSeconds(15)).Result)
            {

                bool isFaulted = false;
                int numFaults = 0;

                _stopWatch.Restart();
               
                if (null == _noteAcceptor)
                {
                    _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.CheatDetected) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.ComponentFault) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.FirmwareFault) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.MechanicalFault) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NoteJammed) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NvmFault) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OpticalFault) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OtherFault) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerDisconnected) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFull) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFault) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerJammed) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disabled) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disconnected) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.InEscrow) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Inspecting) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Returning) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Stacking) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Fault) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Full) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Jammed) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Removed) == "True")
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (!_noteAcceptor.Enabled)
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (!_noteAcceptor.CanValidate)
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (!_noteAcceptor.Connected)
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (_noteAcceptor.DisabledByError)
                {
                    isFaulted = true;
                    numFaults++;
                }

                if (!_noteAcceptor.Initialized)
                {
                    isFaulted = true;
                    numFaults++;
                }

                //
                // see if the Note Acceptor is disconnected now or in 3 seconds
                //
                if (_fakeNoteAcceptorDisconnected.WaitOne(TimeSpan.FromSeconds(3)))
                {

                    _stopWatch.Stop();

                    commandResult = new CommandResult()
                    {
                        data = new Dictionary<string, object>
                        {
                            ["response-to"] = $"/BNA/{id}/StatusV2",
                            ["status"] = "ERRORMESSAGE:",
                            ["is-faulted"] = isFaulted.ToString(),
                            ["number-of-faults"] = numFaults.ToString(),
                            ["test_name"] = $"{test_name}",
                            ["location-in-code"] = "RequestBNAStatusV2(1)",
                            ["error-message"] = $"Fake Note Acceptor is not connected or enabled",
                            ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",

                            ["faults-cheat-detected"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.CheatDetected),
                            ["faults-component-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.ComponentFault),
                            ["faults-firmware-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.FirmwareFault),
                            ["faults-mechanical-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.MechanicalFault),
                            ["faults-note-jammed"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NoteJammed),
                            ["faults-nvm-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NvmFault),
                            ["faults-optical-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OpticalFault),
                            ["faults-other-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OtherFault),
                            ["faults-stacker-disconnected"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerDisconnected),
                            ["faults-stacker-full"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFull),
                            ["faults-stacker-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFault),
                            ["faults-stacker-jammed"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerJammed),

                            ["logical-states-disabled"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disabled),
                            ["logical-states-disconnected"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disconnected),
                            ["logical-states-idle"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Idle),
                            ["logical-states-in-escrow"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.InEscrow),
                            ["logical-states-inspecting"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Inspecting),
                            ["logical-states-returning"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Returning),
                            ["logical-states-stacking"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Stacking),
                            ["logical-states-uninitialized"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Uninitialized),

                            ["stacker-states-fault"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Fault),
                            ["stacker-states-full"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Full),
                            ["stacker-states-inserted"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Inserted),
                            ["stacker-states-jammed"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Jammed),
                            ["stacker-states-removed"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Removed),

                            ["enabled"] = _noteAcceptor.Enabled ? "Enabled" : "Disabled",
                            ["can-validate"] = _noteAcceptor.CanValidate ? "True" : "False",
                            ["connected"] = _noteAcceptor.Connected ? "True" : "False",
                            ["disabled-by-error"] = _noteAcceptor.DisabledByError ? "True" : "False",
                            ["initialized"] = _noteAcceptor.Initialized ? "True" : "False",
                            ["stacker-state"] = _noteAcceptor.StackerState.ToString(),
                            ["last-error"] = _noteAcceptor.LastError,
                            ["reason-disabled"] = _noteAcceptor.DisabledByError ? _noteAcceptor.ReasonDisabled.ToString() : string.Empty

                        },
                        Result = false,
                        Info = "Test Controller NoteAcceptorStatusV2() Failed!"
                    };

                    _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                }
                else if (_noteAcceptor.Faults != NoteAcceptorFaultTypes.None)
                {
                    _stopWatch.Stop();

                    commandResult = new CommandResult()
                    {
                        data = new Dictionary<string, object>
                        {
                            ["response-to"] = $"/BNA/{id}/StatusV2",
                            ["status"] = "ERRORMESSAGE:",
                            ["is-faulted"] = isFaulted.ToString(),
                            ["number-of-faults"] = numFaults.ToString(),
                            ["test_name"] = $"{test_name}",
                            ["location-in-code"] = "RequestBNAStatusV2(2)",
                            ["error-message"] = $"Fake Note Acceptor is not connected or enabled",
                            ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",

                            ["faults-cheat-detected"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.CheatDetected),
                            ["faults-component-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.ComponentFault),
                            ["faults-firmware-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.FirmwareFault),
                            ["faults-mechanical-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.MechanicalFault),
                            ["faults-note-jammed"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NoteJammed),
                            ["faults-nvm-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NvmFault),
                            ["faults-optical-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OpticalFault),
                            ["faults-other-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OtherFault),
                            ["faults-stacker-disconnected"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerDisconnected),
                            ["faults-stacker-full"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFull),
                            ["faults-stacker-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFault),
                            ["faults-stacker-jammed"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerJammed),

                            ["logical-states-disabled"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disabled),
                            ["logical-states-disconnected"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disconnected),
                            ["logical-states-idle"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Idle),
                            ["logical-states-in-escrow"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.InEscrow),
                            ["logical-states-inspecting"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Inspecting),
                            ["logical-states-returning"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Returning),
                            ["logical-states-stacking"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Stacking),
                            ["logical-states-uninitialized"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Uninitialized),

                            ["stacker-states-fault"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Fault),
                            ["stacker-states-full"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Full),
                            ["stacker-states-inserted"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Inserted),
                            ["stacker-states-jammed"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Jammed),
                            ["stacker-states-removed"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Removed),

                            ["enabled"] = _noteAcceptor.Enabled ? "Enabled" : "Disabled",
                            ["can-validate"] = _noteAcceptor.CanValidate ? "True" : "False",
                            ["connected"] = _noteAcceptor.Connected ? "True" : "False",
                            ["disabled-by-error"] = _noteAcceptor.DisabledByError ? "True" : "False",
                            ["initialized"] = _noteAcceptor.Initialized ? "True" : "False",
                            ["stacker-state"] = _noteAcceptor.StackerState.ToString(),
                            ["last-error"] = _noteAcceptor.LastError,
                            ["reason-disabled"] = _noteAcceptor.DisabledByError ? _noteAcceptor.ReasonDisabled.ToString() : string.Empty

                        },
                        Result = false,
                        Info = "Test Controller NoteAcceptorStatusV2() Failed!"
                    };

                    _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                }
                else
                {
                    _stopWatch.Stop();

                    commandResult = new CommandResult
                    {

                        data = new Dictionary<string, object>
                        {
                            ["response-to"] = $"/BNA/{id}/StatusV2",
                            ["test-name"] = $"{test_name}",
                            ["status"] = isFaulted ? "ERRORMESSAGE:" : "SUCCESS:",
                            ["error-message"] = isFaulted ? $"There are {numFaults} active faults" : string.Empty,
                            ["is-faulted"] = isFaulted.ToString(),
                            ["number-of-faults"] = numFaults.ToString(),
                            ["location-in-code"] = "RequestBNAStatusV2(3)",
                            ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",

                            ["faults-cheat-detected"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.CheatDetected),
                            ["faults-component-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.ComponentFault),
                            ["faults-firmware-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.FirmwareFault),
                            ["faults-mechanical-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.MechanicalFault),
                            ["faults-note-jammed"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NoteJammed),
                            ["faults-nvm-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NvmFault),
                            ["faults-optical-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OpticalFault),
                            ["faults-other-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OtherFault),
                            ["faults-stacker-disconnected"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerDisconnected),
                            ["faults-stacker-full"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFull),
                            ["faults-stacker-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFault),
                            ["faults-stacker-jammed"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerJammed),

                            ["logical-states-disabled"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disabled),
                            ["logical-states-disconnected"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disconnected),
                            ["logical-states-idle"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Idle),
                            ["logical-states-in-escrow"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.InEscrow),
                            ["logical-states-inspecting"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Inspecting),
                            ["logical-states-returning"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Returning),
                            ["logical-states-stacking"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Stacking),
                            ["logical-states-uninitialized"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Uninitialized),

                            ["stacker-states-fault"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Fault),
                            ["stacker-states-full"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Full),
                            ["stacker-states-inserted"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Inserted),
                            ["stacker-states-jammed"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Jammed),
                            ["stacker-states-removed"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Removed),

                            ["enabled"] = _noteAcceptor.Enabled ? "Enabled" : "Disabled",
                            ["can-validate"] = _noteAcceptor.CanValidate ? "True" : "False",
                            ["connected"] = _noteAcceptor.Connected ? "True" : "False",
                            ["disabled-by-error"] = _noteAcceptor.DisabledByError ? "True" : "False",
                            ["initialized"] = _noteAcceptor.Initialized ? "True" : "False",
                            ["stacker-state"] = _noteAcceptor.StackerState.ToString(),
                            ["last-error"] = _noteAcceptor.LastError,
                            ["reason-disabled"] = _noteAcceptor.DisabledByError ? _noteAcceptor.ReasonDisabled.ToString() : string.Empty

                        },

                        Result = isFaulted ? false : true,
                        Info = isFaulted ? "RequestBNAStatusV2() Failed!" : "RequestBNAStatusV2() Returned requested status."

                    };

                    _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                }

            }
            else
            {
                _stopWatch.Stop();

                commandResult = new CommandResult
                {

                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = $"/BNA/{id}/StatusV2",
                        ["test-name"] = $"{test_name}",
                        ["status"] = "ERRORMESSAGE:",
                        ["error-message"] = "",
                        ["is-faulted"] = "False",
                        ["number-of-faults"] = "0",
                        ["location-in-code"] = "RequestBNAStatusV2(3)",
                        ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                        ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",

                        ["faults-cheat-detected"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.CheatDetected),
                        ["faults-component-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.ComponentFault),
                        ["faults-firmware-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.FirmwareFault),
                        ["faults-mechanical-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.MechanicalFault),
                        ["faults-note-jammed"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NoteJammed),
                        ["faults-nvm-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.NvmFault),
                        ["faults-optical-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OpticalFault),
                        ["faults-other-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.OtherFault),
                        ["faults-stacker-disconnected"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerDisconnected),
                        ["faults-stacker-full"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFull),
                        ["faults-stacker-fault"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerFault),
                        ["faults-stacker-jammed"] = _noteAcceptorFaultPresent(_noteAcceptor.Faults, NoteAcceptorFaultTypes.StackerJammed),

                        ["logical-states-disabled"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disabled),
                        ["logical-states-disconnected"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Disconnected),
                        ["logical-states-idle"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Idle),
                        ["logical-states-in-escrow"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.InEscrow),
                        ["logical-states-inspecting"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Inspecting),
                        ["logical-states-returning"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Returning),
                        ["logical-states-stacking"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Stacking),
                        ["logical-states-uninitialized"] = _noteAcceptorLogicalStatePresent(_noteAcceptor.LogicalState, NoteAcceptorLogicalState.Uninitialized),

                        ["stacker-states-fault"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Fault),
                        ["stacker-states-full"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Full),
                        ["stacker-states-inserted"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Inserted),
                        ["stacker-states-jammed"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Jammed),
                        ["stacker-states-removed"] = _noteAcceptorStackerStatePresent(_noteAcceptor.StackerState, NoteAcceptorStackerState.Removed),

                        ["enabled"] = _noteAcceptor.Enabled ? "Enabled" : "Disabled",
                        ["can-validate"] = _noteAcceptor.CanValidate ? "True" : "False",
                        ["connected"] = _noteAcceptor.Connected ? "True" : "False",
                        ["disabled-by-error"] = _noteAcceptor.DisabledByError ? "True" : "False",
                        ["initialized"] = _noteAcceptor.Initialized ? "True" : "False",
                        ["stacker-state"] = _noteAcceptor.StackerState.ToString(),
                        ["last-error"] = _noteAcceptor.LastError,
                        ["reason-disabled"] = _noteAcceptor.DisabledByError ? _noteAcceptor.ReasonDisabled.ToString() : string.Empty

                    },

                    Result = false,
                    Info = $"Attempt to get BNA Status failed! The synchronization semaphore wait timed out?"
                };

                _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

            }

            _stopWatch.Stop();
            _oneAPICallAtATime.Release();

            return commandResult;

        }



        /// <summary>
        /// This method will return a populated JSON object
        /// The calling Python method can still assert on this object like this:
        ///
        ///     button_deck = ButtonDeck()
        ///     button_deck_response: ButtonDeckResponse = button_deck.try_cash_out()
        ///     assert button_deck_response.result
        ///
        /// However a better approach could be much more helpful
        ///
        ///     button_deck = ButtonDeck()
        ///     button_deck_response: ButtonDeckResponse = button_deck.try_cash_out()
        ///     if not button_deck.Response.result:
        ///         ui_automation.display_error_message(f'ERRORMESSAGE: Cash Out Failed with {button_deck_response.info}')
        ///         ui_automation.display_error_message(f'Paper In Chute = {button_deck_response.printer_paprer_in_chute}')
        ///         # you get the idea - there are 20 status points that can be logged and handled
        ///
        ///             self.response_to:               str = None
        ///             self.status:                    str = None
        ///             self.test_name:                 str = test_name
        ///             self.time_stamp:                str = None
        ///             self.elapsed_time:              str = None
        ///             self.location_in_code:          str = None
        ///             self.error_message:             str = None
        ///             self.starting_balance:          str = None
        ///             self.ending_balance:            str = None
        ///             self.bank_balance_changed:      str = None
        ///             self.print_completed:           str = None
        ///             self.printer_chassis_open:      str = None
        ///             self.printer_paper_empty:       str = None
        ///             self.printer_paper_in_chute:    str = None
        ///             self.printer_paper_jam:         str = None
        ///             self.printer_paper_low:         str = None
        ///             self.printer_print_head_open:   str = None
        ///             self.raw_json_text:             str = None
        ///             self.result:                    bool = False
        ///             self.info:                      str = None
        ///
        ///             as you can see there are many possible issues that can be handled and logged
        ///             in the Python tests - including timing information
        /// 
        ///         
        /// 
        /// </summary>
        /// <param name="test_name">The name of the calling test</param>
        /// <returns>A populated JSON object that is returned to the calling REST client</returns>
        public CommandResult RequestCashOutV2(string test_name)
        {
            CommandResult commandResult = null;

            //
            // wait for access to the semaphore
            //
            if (_oneAPICallAtATime.WaitAsync(TimeSpan.FromSeconds(15)).Result)
            {
                _stopWatch.Restart();

                //
                // get the current balance
                //
                long creditMeterValue = _meterManager.GetMeter("Credit", "Panel", "Lifetime", 0, 0);

                //
                // we have to have credits to cash out!
                //
                if (0 != creditMeterValue)
                {
                    //
                    // get the beginning known event state setup
                    //
                    ResetLocalManualResetEvents();
                    EnableCashOut(true);
                    _eventBus.Publish(new CashOutButtonPressedEvent());

                    //
                    // give the system a few seconds before waiting on the events
                    //
                    _waitForSystemToReact();

                    if (WaitHandle.WaitAll(_cashOutEvents, TimeSpan.FromSeconds(25)))
                    {

                        //   
                        // check to see if there are any _fakePrinterErrorEvents 
                        //
                        int errorIndex = WaitHandle.WaitAny(_fakePrinterErrorEvents, TimeSpan.FromSeconds(5));

                        switch (errorIndex)
                        {
                            //
                            // Wait Timed out - E.g. no error event
                            //
                            case 258:
                                //
                                // happy path
                                //
                                _stopWatch.Stop();

                                commandResult = new CommandResult()
                                {
                                    data = new Dictionary<string, object>
                                    {
                                        ["response-to"] = "/Platform/CashoutV2/Request",
                                        ["status"] = "SUCCESS:",
                                        ["location-in-code"] = "RequestCashOutV2(1)",
                                        ["test-name"] = $"{test_name}",
                                        ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                        ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                        ["starting-balance"] = creditMeterValue.ToString(),
                                        ["ending-balance"] = _meterManager.GetMeter("Credit", "Panel", "Lifetime", 0, 0).ToString()
                                    },
                                    Result = true,
                                    Info = $"Attempt to Cash Out ({creditMeterValue}) credits completed successfully."

                                };

                                _displayDebugText($"SUCCESS: {commandResult.Info}.", _logIt);
                                break;

                            //
                            // Unknown condition
                            //
                            default:
                                _stopWatch.Stop();

                                commandResult = new CommandResult()
                                {
                                    data = new Dictionary<string, object>
                                    {
                                        ["response-to"] = "/Platform/CashoutV2/Request",
                                        ["status"] = "ERRORMESSAGE:",
                                        ["test-name"] = $"{test_name}",
                                        ["error-message"] = $"[Unknown Internal Error].",
                                        ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                        ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                        ["location-in-code"] = "RequestCashOutV2(2)",
                                        ["starting-balance"] = creditMeterValue.ToString(),
                                        ["ending-balance"] = _meterManager.GetMeter("Credit", "Panel", "Lifetime", 0, 0).ToString()
                                    },
                                    Result = false,
                                    Info = "Test Controller RequestCashOutV2() Failed!"
                                };

                                _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                                break;
                        }

                    }
                    else
                    {

                        _stopWatch.Stop();

                        //
                        // one of the necessary events did not happen
                        //
                        bool a0 = _bankBalanceChangedAfterCashOut.WaitOne(0);
                        bool b0 = _printCompleted.WaitOne(0);
                        bool c0 = _fakePrinterErrorChassisOpen.WaitOne(0);
                        bool d0 = _fakePrinterErrorPaperEmpty.WaitOne(0);
                        bool e0 = _fakePrinterErrorPaperInChute.WaitOne(0);
                        bool f0 = _fakePrinterErrorPaperJam.WaitOne(0);
                        bool g0 = _fakePrinterErrorPaperLow.WaitOne(0);
                        bool h0 = _fakePrinterErrorPrintHeadOpen.WaitOne(0);

                        commandResult = new CommandResult()
                        {
                            data = new Dictionary<string, object>
                            {
                                ["response-to"]             = "/Platform/CashoutV2/Request",
                                ["status"]                  = "ERRORMESSAGE:",
                                ["test-name"]               = $"{test_name}",
                                ["time-stamp"]              = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                ["elapsed-time"]            = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                                ["location-in-code"]        = "RequestCashOutV2(3)",
                                ["error-message"]           = $"All required events:  Print Completed({b0}), BankBalanceChanged({a0}) did not get handled. Current state of the Bank is now unknown",
                                ["starting-balance"]        = creditMeterValue.ToString(),
                                ["ending-balance"]          = _meterManager.GetMeter("Credit", "Panel", "Lifetime", 0, 0).ToString(),
                                ["bank-balance-changed"]    = a0.ToString(),
                                ["print-completed"]         = b0.ToString(),
                                ["printer-chassis-open"]    = c0.ToString(),
                                ["printer-paper-empty"]     = d0.ToString(),
                                ["printer-paper-in-chute"]  = e0.ToString(),
                                ["printer-paper-jam"]       = f0.ToString(),
                                ["printer-paper-low"]       = g0.ToString(),
                                ["printer-print-head-open"] = h0.ToString(),
                            },

                            Result = false,
                            Info = $"Attempt to insert cash out failed! The state of the Bank is now unknown."
                        };

                        _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                    }
                } // if (0 != creditMeterValue)
                else
                {
                    _stopWatch.Stop();

                    // error nothing to cash out
                    commandResult = new CommandResult()
                    {
                        data = new Dictionary<string, object>
                        {
                            ["response-to"] = "/Platform/CashoutV2/Request",
                            ["status"] = "SUCCESS:",
                            ["test-name"] = $"{test_name}",
                            ["time-stamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"] = _stopWatch.ElapsedMilliseconds.ToString() + " ms.",
                            ["location-in-code"] = "RequestCashOutV2(4)",
                            ["starting-balance"] = creditMeterValue.ToString(),
                            ["ending-balance"] = _meterManager.GetMeter("Credit", "Panel", "Lifetime", 0, 0).ToString()
                        },
                        Result = true,
                        Info = $"Cash Out (ZERO) credits"

                    };

                    _displayDebugText($"SUCCESS: {commandResult.Info}", _logIt);

                }

            }  // if (_oneAPICallAtATime.WaitAsync(TimeSpan.FromSeconds(15)).Result)
            else
            {
                long currentBalance = _meterManager.GetMeter("Credit", "Panel", "Lifetime", 0, 0);

                commandResult = new CommandResult()
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"]         = "/Platform/CashoutV2/Request",
                        ["status"]              = "ERRORMESSAGE:",
                        ["test-name"]           = $"{test_name}",
                        ["error-message"]       = $"Could not get access to the method [The wait timed out].  Please try again. ",
                        ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                        ["elapsed-time"]        = "0 ms.",
                        ["location-in-code"]    = "RequestCashOutV2(5)",
                        ["starting-balance"]    = currentBalance.ToString(),
                        ["ending-balance"]      = currentBalance.ToString()
                    },
                    Result = false,
                    Info = $"Attempt to Cash Out failed! The synchronization semaphore wait timed out?"

                };

                _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

            }

            _stopWatch.Stop();
            ResetLocalManualResetEvents();
            _oneAPICallAtATime.Release();

            return commandResult;

        }

        /// <summary>
        /// This method will process the request to insert a valid Dollar Bill
        /// Look for the "Result", "status", "Info", and "error-message" values in the return
        /// 
        /// The use of _displayDebugText() requires:
        /// SHOW_DEBUG_MESSAGES_FORV2 to be defined of nothing will be displayed
        /// See the definition of the Action<> at the top of this file for details
        /// 
        /// </summary>
        /// <param name="bill_value">Valid Bill Value (1,2,5,10,20,50,100)</param>
        /// <param name="id">The ID of the BNA (currently we only use 0)</param>
        /// <returns>
        /// A populated CommandResult Object
        /// 
        ///     The "Results" value will be "true" or "false" as a string
        ///
        ///     The "status" value will be SUCCESS: or ERRORMESSAGE:
        ///     All of the consuming tests expect one of these values to be in the returned JSON
        ///
        ///     If there was an error the "error-message" member of the data dictionary will be present
        ///     in the response JSON and will have the details of the failure
        ///
        ///     The "Info" member will always have information about the request and activity
        ///     
        /// </returns>
        public CommandResult InsertCreditsV2(int bill_value, string test_name, string id)
        {
            string testName = test_name;

            CommandResult commandResult = null;

            //
            // This should never be a problem - but who knows what the test developers might try to do?
            // 
            if (_oneAPICallAtATime.Wait(TimeSpan.FromSeconds(35)))
            {
                _stopWatch.Restart();

                //
                // get the beginning known event state setup
                //
                ResetLocalManualResetEvents();

                //
                // make sure it is a bill we recognize
                //
                if (!_validBills.Contains(bill_value))
                {

                    _stopWatch.Stop();

                    commandResult = new CommandResult()
                    {
                        data = new Dictionary<string, object>()
                        {
                            ["response-to"]         = $"/BNA/{id}/Bill/InsertV2",
                            ["status"]              = "ERRORMESSAGE:",
                            ["test_name"]           = $"{testName}",
                            ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms",          
                            ["error-message"]       = $"Bill Value ({bill_value}) is invalid.",
                            ["location-in-code"]    = "InsertCreditsV2(1)"
                        },
                        Result = false,
                        Info = $"Attempt to insert {bill_value} dollar bill failed!",

                    };

                    _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                } // if (_validBills.Contains(bill_value))

                //
                // make sure the BNA id is at least a number
                //
                else if (!int.TryParse(id, out int deviceId))
                {
                    _stopWatch.Stop();

                    commandResult = new CommandResult()
                    {
                        data = new Dictionary<string, object>()
                        {
                            ["response-to"]         = $"/BNA/{id}/Bill/InsertV2",
                            ["status"]              = "ERRORMESSAGE:",
                            ["test_name"]           = $"{testName}",
                            ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms",
                            ["error-message"]       = $"Could not parse the BNA/ID where ID is {id}",
                            ["location-in-code"]    = "InsertCreditsV2(2)"
                        },
                        Result = false,
                        Info = $"Attempt to insert {bill_value} dollar bill failed!",

                    };

                    _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);


                } // else if (!int.TryParse(id, out int deviceId))
                else
                {
                    //
                    // This is the HAPPY PATH.
                    // e.g. we validated the parameters and found no problems
                    //
                    CreateTable();

                    try
                    {
                        _bnaNoteTransactionId++;
                    }
                    catch (OverflowException)
                    {
                        _bnaNoteTransactionId = 0;
                    }

                    //
                    // tell the rest of the system that we are processing a valid bill value
                    //
                    _eventBus.Publish(new FakeDeviceMessageEvent
                    {
                        Message = new NoteValidated
                        {
                            ReportId        = GdsConstants.ReportId.NoteAcceptorAcceptNoteOrTicket,
                            TransactionId   = _bnaNoteTransactionId,
                            NoteId          = GetNoteId(bill_value)
                        }
                    });

                    //
                    // give the system a few seconds before waiting on the events
                    //
                    _waitForSystemToReact();

                    //
                    // _insertCreditsEvents is an array of events that we consider
                    // to be necessary for the insert to be completed
                    //
                    if (WaitHandle.WaitAll(_insertCreditsEvents, TimeSpan.FromSeconds(25)))
                    {

                        _stopWatch.Stop();

                        //
                        // catch the possibility that the bill was returned
                        // and report the error to the caller
                        //
                        if (_currencyReturnedAfterInsertCredits.WaitOne(0))
                        {
                            commandResult = new CommandResult()
                            {
                                data = new Dictionary<string, object>
                                {
                                    ["response-to"]         = $"/BNA/{id}/Bill/InsertV2",
                                    ["status"]              = "ERRORMESSAGE:",
                                    ["test_name"]           = $"{testName}",
                                    ["location-in-code"]    = "InsertCreditsV2(3)",
                                    ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                    ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms",
                                    ["error-message"]       = $"{bill_value} dollars was returned"
                                },
                                Result = false,
                                Info = $"Failed to insert {bill_value} dollars. The bill was returned"
                            };

                            _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                        }
                        else
                        {
                            _stopWatch.Stop();

                            //
                            // if we got here then all the dependent events have been set
                            // so we can proceed with the response
                            //
                            commandResult = new CommandResult()
                            {
                                data = new Dictionary<string, object>
                                {
                                    ["response-to"]         = $"/BNA/{id}/Bill/InsertV2",
                                    ["location-in-code"]    = "InsertCreditsV2(4)",
                                    ["test_name"]           = $"{testName}",
                                    ["status"]              = "SUCCESS:",
                                    ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                    ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms",
                                },
                                Result = true,
                                Info = $"Inserted {bill_value} dollars."
                            };

                            _displayDebugText($"SUCCESS: At-{commandResult.data["location-in-code"]} {commandResult.Info}", _logIt);


                        }
                    } //if (WaitHandle.WaitAll(_insertCreditsEvents, TimeSpan.FromSeconds(25)))
                    else
                    {

                        //
                        //  if we got here then the required events did not happen
                        //  before the wait timed out.  So, we report the error
                        //  the next three lines
                        //  identify all the events that did not happen (not set)
                        //  the "error-message" will carry these 3 boolean values
                        //  that can be used for debugging in the caller's code.
                        //
                        bool a1 = _insertCreditsCompletedAfterInsertCredits.WaitOne(0);
                        bool b1 = _bankBalanceChangedAfterInsertCredits.WaitOne(0);
                        bool c1 = _currencyStackedAfterInsertCredits.WaitOne(0);

                        //
                        //  The first possibility is that the bill was returned
                        //  So we return the error
                        //
                        if (_currencyReturnedAfterInsertCredits.WaitOne(0))
                        {
                            _stopWatch.Stop();

                            commandResult = new CommandResult()
                            {
                                data = new Dictionary<string, object>
                                {
                                    ["response-to"]         = $"/BNA/{id}/Bill/InsertV2",
                                    ["status"]              = "ERRORMESSAGE:",
                                    ["test_name"]           = $"{testName}",
                                    ["location-in-code"]    = "InsertCreditsV2(5)",
                                    ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                    ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms",
                                    ["error-message"]       = $"{bill_value} dollars was returned",

                                    //
                                    // a,b,c are declared just above - see that code
                                    //
                                    ["insert-completed"]        = a1.ToString(),
                                    ["bank-balance-changed"]    = b1.ToString(),
                                    ["bill-stacked"]            = c1.ToString()

                                },
                                Result = false,
                                Info = $"Failed to insert {bill_value} dollars. The bill was returned Check Limit Settings."
                            };

                            _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);


                        } // if (_currencyReturnedAfterInsertCredits.WaitOne(0))
                        else
                        {

                            //
                            //  If we got here then the bill was not returned and one of the
                            //  required events did not happen - the next three lines
                            //  identify all the events that did not happen (not set)
                            //  the "error-message" will carry these 3 boolean values
                            //  that can be used for debugging in the caller's code.
                            //
                            bool a2 = _insertCreditsCompletedAfterInsertCredits.WaitOne(0);
                            bool b2 = _bankBalanceChangedAfterInsertCredits.WaitOne(0);
                            bool c2 = _currencyStackedAfterInsertCredits.WaitOne(0);

                            _stopWatch.Stop();

                            commandResult = new CommandResult()
                            {
                                data = new Dictionary<string, object>
                                {
                                    ["response-to"]         = $"/BNA/{id}/Bill/InsertV2",
                                    ["status"]              = "ERRORMESSAGE:",
                                    ["test_name"]           = $"{testName}",
                                    ["location-in-code"]    = "InsertCreditsV2(6)",
                                    ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                                    ["elapsed-time"]        = _stopWatch.ElapsedMilliseconds.ToString() + " ms",
                                    ["error-message"]       = $"All required events:  Insert Completed({a2}), BankBalanceChanged({b2}) BillStacked({c2}) did not get handled. Current state of the Bank is now unknown",

                                    //
                                    // a,b,c are declared just above - see that code
                                    //
                                    ["insert-completed"]        = a2.ToString(),
                                    ["bank-balance-changed"]    = b2.ToString(),
                                    ["bill-stacked"]            = c2.ToString()

                                },
                                Result = false,
                                Info = $"Attempt to insert {bill_value} dollar bill failed! The state of the Bank is now unknown."
                            };

                            _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

                        }

                    }

                }

            }  // if (_oneInsertCreditsAtATime.Wait(TimeSpan.FromSeconds(35)))
            else
            {

                _stopWatch.Stop();
                
                commandResult = new CommandResult()
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"]         = $"/BNA/{id}/Bill/InsertV2",
                        ["status"]              = "ERRORMESSAGE:",
                        ["test_name"]           = $"{testName}",
                        ["location-in-code"]    = "InsertCreditsV2(7)",
                        ["time-stamp"]          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                        ["elapsed-time"]        = "0",
                        ["error-message"]       = $"Could not get access to the method (The wait timed out).  Please try again. "
                    },
                    Result = false,
                    Info = $"Attempt to insert {bill_value} dollar bill failed! The synchronization semaphore wait just timed out?"
                };

                _displayDebugText($"ERRORMESSAGE: At-{commandResult.data["location-in-code"]} {commandResult.data["error-message"]}", _logIt);

            }

            _stopWatch.Stop();

            ResetLocalManualResetEvents();

            _oneAPICallAtATime.Release();

            return commandResult;

        }

    }

}