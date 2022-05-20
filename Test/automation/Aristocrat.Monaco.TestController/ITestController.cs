namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using DataModel;

    [ServiceContract]
    public interface ITestController
    {

        /// <summary>
        /// This will enable the Test Automation team to add a new set of game performance tests
        /// and have a better idea about what games are available for loading and testing
        /// </summary>
        /// <param name="test_name">This is the name of the calling Automated Test</param>
        /// <returns>A list of the current games on the EGM and a subset of the settings</returns>
        [OperationContract]
        [WebInvoke(
           UriTemplate = "/Platform/GetAvailableGamesV2",
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           ResponseFormat = WebMessageFormat.Json,
           RequestFormat = WebMessageFormat.Json)]
        CommandResult GetAvailableGamesV2(string test_name);

        /// <summary>
        /// This is the definition of the entry point for the TryLoadGameV2 API
        /// This implementation will wait until the operation finishes and only then reports the status.
        /// </summary>
        /// <param name="gameName">The name of the game to be loaded.  E.g. "LepreCoins" or "SunAndMoon"</param>
        /// <param name="denomination">The value to be used selecting the game to load</param>
        /// <param name="test_name">The name of the calling test - used for failure analysis and automation statistics</param>
        /// <returns>A populated CommandResult with a Dictionary of status and statistical data</returns>
        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/LoadGameV2",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult TryLoadGameV2(string gameName, string denomination, string test_name);


        /// <summary>
        /// This is the definition of the entry point for the RequestSpinV2 API
        /// This implementation will wait until the operation finishes and only then reports the status.
        /// </summary>
        /// <param name="test_name">The name of the calling test - used for failure analysis and automation statistics</param>
        /// <returns>A populated CommandResult with a Dictionary of status and statistical data</returns>
        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Runtime/RequestSpinV2",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestSpinV2(string test_name);

        /// <summary>
        /// This is the definition of the entry point for the InsertCreditsV2 API
        /// This implementation will wait until the operation finishes and only then reports the status.
        /// </summary>
        /// <param name="bill_value">This is a value retrieved from a list of legitimate values</param>
        /// <param name="test_name">The name of the calling test - used for failure analysis and automation statistics</param>
        /// <param name="id">This is the ID of the Note Acceptor (currently always 0).  It is in the {id} field on the route.</param>
        /// <returns>A populated CommandResult with a Dictionary of status and statistical data</returns>
        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Bill/InsertV2",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult InsertCreditsV2(int bill_value, string test_name, string id);

        /// <summary>
        /// This is the definition of the entry point for the RequestCashOutV2 API
        /// This implementation will wait until the operation finishes and only then reports the status.
        /// </summary>
        /// <param name="test_name">The name of the calling test - used for failure analysis and automation statistics</param>
        /// <returns>A populated CommandResult with a Dictionary of status and statistical data</returns>
        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/CashoutV2/Request",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestCashOutV2(string test_name);

        /// <summary>
        /// This is the definition of the entry point for the RequestCashOutV2 API
        /// This implementation will wait until the operation finishes and only then reports the status.
        /// </summary>
        /// <param name="test_name">The name of the calling test - used for failure analysis and automation statistics</param>
        /// <param name="id">This is the ID of the Note Acceptor (currently always 0).  It is in the {id} field on the route.</param>
        /// <returns>A populated CommandResult with a Dictionary of status and statistical data</returns>
        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/StatusV2",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestBNAStatusV2(string test_name, string id);




        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/Close",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult ClosePlatform();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Platform/Logs/{eventType=null}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetEventLogs(string eventType);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/ToggleRobotMode",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult ToggleRobotMode();

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/OperatorMenu",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult AuditMenu(bool open);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/OperatorMenu/Tab",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SelectMenuTab(string name);

        [OperationContract]
        [WebGet(
            UriTemplate = "/Platform/State",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetPlatformStatus();

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/LoadGame",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestGame(string GameName, long Denomination);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/ExitGame",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestGameExit();

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/ForceGameExit",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult ForceGameExit();

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/EnableCashout",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult EnableCashOut(bool enable);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/Cashout/Request",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestCashOut();

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/HandleRg",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult HandleRG(bool enable);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/ResponsibleGaming/Dialog/Options/Set",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SetRgDialogOptions(string[] buttonNames);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Runtime/RequestSpin",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestSpin();

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Runtime/BetLevel/Set",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SetBetLevel(int index);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Runtime/LineLevel/Set",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SetLineLevel(int index);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Runtime/BetMax/Set",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SetBetMax();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Platform/Meters/{category}/{name}/{type}/Get/{game=0}/{denom=0}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetMeter(string name, string category, string type, string game, string denom);

        [OperationContract]
        [WebGet(
            UriTemplate = "/Platform/Meters/Upi/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetUpiMeters();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Runtime/State",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetRuntimeState();

        [OperationContract]
        [WebInvoke( 
            UriTemplate = "/Runtime/Running",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RuntimeLoaded();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Runtime/Mode",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetRuntimeMode();

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/SendInput",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SendInput(int input);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/TouchScreen/0/Touch",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SendTouchGame(int x, int y);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/TouchScreen/2/Touch",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SendTouchVBD(int x, int y);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Bill/Insert",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult InsertCredits(int bill_value, string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/Service/Request",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult ServiceButton(bool pressed);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/Lockup",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult Lockup(LockupTypeEnum type, bool clear);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/MaxWinLimitOverride",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SetMaxWinLimitOverride(int maxWinLimitOverrideMillicents);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/HandpayRequest",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestHandPay(long amount, TransferOutType type, Account accountType);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/KeyOff",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult RequestKeyOff();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Lockups/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetCurrentLockups();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Game/Messages/Lockup/GetGameScreenMessages",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetGameMessages();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Game/Messages/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetGameLineMessages();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Game/OverlayMessage/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
             RequestFormat = WebMessageFormat.Json)]
        CommandResult GetGameOverlayMessage();

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/Info",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetInfo(List<PlatformInfoEnum> info);

        [OperationContract]
        [WebGet(
            UriTemplate = "/Game/History/NumberOfEntries",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetNumberOfGameHistoryEntires();



        [OperationContract]
        [WebGet(
            UriTemplate = "/Game/History/{count=1}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetGameHistory(string count);

        [OperationContract]
        [WebGet(
        UriTemplate = "/Platform/Logs/HandPay/{count=1}",
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        CommandResult GetHandPayLog(string count);

        [OperationContract]
        [WebGet(
            UriTemplate = "/Platform/Logs/TransferOut/{count=1}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetTransferOutLog(string count);

        [OperationContract]
        [WebGet(
            UriTemplate = "/Platform/Logs/TransferIn/{count=1}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetTransferInLog(string count);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/Progressives/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetProgressives(string gameName);

        #region Config

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Config/Read",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetConfigOption(string option);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Config/Write",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SetConfigOption(string option, object value);

        #endregion Config


        #region Waits

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Event/Wait",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult Wait(string evtType, int timeout);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Event/Wait/Cancel",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult CancelWait(string evtType);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Event/Wait/All",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult WaitAll(string[] evtType, int timeout);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Event/Wait/Any",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult WaitAny(string[] evtType, int timeout);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Event/Wait/Clear",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult ClearWaits();

        [OperationContract]
        [WebGet(
            UriTemplate = "/Event/Wait/Status",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult CheckWaitStatus();

        #endregion Waits

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/Property",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        object GetProperty(string property);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Platform/Property/Set",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        void SetProperty(string property, object value, bool isConfig);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/IO/Set",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult SetIo(string index, bool status);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/IO/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult GetIo();


        #region Hardware

        #region Note Acceptor

        [OperationContract]
        [WebGet(
            UriTemplate = "/BNA/{id}/Status",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorStatus(string id);

        [OperationContract]
        [WebGet(
            UriTemplate = "/BNA/{id}/Bill/Mask/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorGetMask(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Bill/Mask/Set",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorSetMask(string id, string mask);

        [OperationContract]
        [WebGet(
            UriTemplate = "/BNA/{id}/Bill/Notes/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorGetNotes(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Connect",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorConnect(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Disconnect",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorDisconnect(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Cheat",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorCheat(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Firmware/Get",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorGetFirmware(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Firmware/Set",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorSetFirmware(string contents, string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Stacker/Attach",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorAttachStacker(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Stacker/Full",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorStackerFull(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Stacker/Remove",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorStackerRemove(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/BNA/{id}/Ticket/Insert",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult InsertTicket(string validation_id, string id);

        [OperationContract]
        [WebGet(
            UriTemplate = "/BNA/{id}/Escrow/Status",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult NoteAcceptorEscrowStatus(string id);
        #endregion

        #region Printer

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Connect",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterConnect(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Disconnect",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterDisconnect(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Jam",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterJam(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Paper/Out",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterEmpty(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Paper/PaperInChute",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterPaperInChute(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Status",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterStatus(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Paper/Status",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterPaperStatus(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/ChassisOpen",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterChassisOpen(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Paper/Low",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterPaperLow(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Paper/Fill",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterPaperFill(string id);

        [OperationContract]
        [WebGet(
            UriTemplate = "/Printer/{id}/Ticket/Read/{count=1}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterGetTicketList(string id, string count);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Ticket/Remove",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterTicketRemove(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Head/Lift",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterHeadLift(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/Printer/{id}/Head/Lower",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult PrinterHeadLower(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/TouchScreen/{id}/Connect",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult TouchScreenConnect(string id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/TouchScreen/{id}/Disconnect",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json)]
        CommandResult TouchScreenDisconnect(string id);

        #endregion Printer

        #region CardReader

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/CardReaders/0/Event",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult PlayerCardEvent(bool inserted, string data);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "/CardReaders/0/InsertCard/",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        CommandResult InsertCard(string CardName);

        [OperationContract]
        [WebInvoke(
                   UriTemplate = "/CardReaders/0/RemoveCard",
                   BodyStyle = WebMessageBodyStyle.WrappedRequest,
                   ResponseFormat = WebMessageFormat.Json,
                   RequestFormat = WebMessageFormat.Json)]
        CommandResult RemoveCard();

        [OperationContract]
        [WebInvoke(
                   UriTemplate = "/CardReaders/0/Disconnect",
                   BodyStyle = WebMessageBodyStyle.WrappedRequest,
                   ResponseFormat = WebMessageFormat.Json,
                   RequestFormat = WebMessageFormat.Json)]
        CommandResult CardReaderDisconnect();

        [OperationContract]
        [WebInvoke(
                   UriTemplate = "/CardReaders/0/Connect",
                   BodyStyle = WebMessageBodyStyle.WrappedRequest,
                   ResponseFormat = WebMessageFormat.Json,
                   RequestFormat = WebMessageFormat.Json)]
        CommandResult CardReaderConnect();


        #endregion CardReader

        #endregion Hardware
    }
}