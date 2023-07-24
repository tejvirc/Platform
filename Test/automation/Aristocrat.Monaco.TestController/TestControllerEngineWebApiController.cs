namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using TestController.Models.Request;

    [ApiController]
    [Route("PlatformTestController")]
    [Route("VLTTestController")]
    public partial class TestControllerEngineWebApiController : ControllerBase
    {        
        private readonly TestControllerEngine _engineInstance;

        public TestControllerEngineWebApiController(TestControllerEngine testControllerEngine)
        {
            _engineInstance = testControllerEngine;
        }

        [HttpPost]
        [Route("Platform/Close")]
        public ActionResult ClosePlatform()
        {
            return Ok(_engineInstance.ClosePlatform());
        }

        [HttpGet]
        [Route("Platform/Logs/{eventType}")]
        public ActionResult GetEventLogs([FromRoute] string eventType = null)
        {
            return Ok(_engineInstance.GetEventLogs(eventType));
        }

        [HttpPost]
        [Route("Platform/OperatorMenu")]
        public ActionResult AuditMenu([FromBody] AuditMenuRequest request)
        {
            return Ok(_engineInstance.AuditMenu(request));
        }

        [HttpPost]
        [Route("Platform/OperatorMenu/Tab")]
        public ActionResult SelectMenuTab([FromBody] SelectMenuTabRequest request)
        {
            return Ok(_engineInstance.SelectMenuTab(request));
        }

        [HttpPost]
        [Route("Platform/ToggleRobotMode")]
        public ActionResult ToggleRobotMode()
        {
            return Ok(_engineInstance.ToggleRobotMode());            
        }

        [HttpGet]
        [Route("Platform/State")]
        public ActionResult GetPlatformStatus()
        {
            return Ok(_engineInstance.GetPlatformStatus());
        }

        [HttpPost]
        [Route("Platform/LoadGame")]
        public ActionResult RequestGame([FromBody] RequestGameRequest request)
        {
            return Ok(_engineInstance.RequestGame(request));
        }

        [HttpPost]
        [Route("Platform/ExitGame")]
        public ActionResult RequestGameExit()
        {
            return Ok(_engineInstance.RequestGameExit());
        }

        [HttpPost]
        [Route("Platform/ForceGameExit")]
        public ActionResult ForceGameExit()
        {
            return Ok(_engineInstance.ForceGameExit());
        }

        [HttpPost]
        [Route("Platform/EnableCashout")]
        public ActionResult EnableCashOut([FromBody] EnableCashOutRequest request)
        {
            return Ok(_engineInstance.EnableCashOut(request));
        }

        [HttpPost]
        [Route("Platform/Cashout/Request")]

        public ActionResult RequestCashOut()
        {
            return Ok(_engineInstance.RequestCashOut());
        }

        [HttpPost]
        [Route("Platform/HandleRg")]
        public ActionResult HandleRG([FromBody] HandleRGRequest request)
        {
            return Ok(_engineInstance.HandleRG(request));
        }

        [HttpPost]
        [Route("Platform/ResponsibleGaming/Dialog/Options/Set")]
        public ActionResult SetRgDialogOptions([FromBody] SetRgDialogOptionsRequest request)
        {
            return Ok(_engineInstance.SetRgDialogOptions(request));
        }

        [HttpPost]
        [Route("Runtime/RequestSpin")]
        public ActionResult RequestSpin()
        {
            return Ok(_engineInstance.RequestSpin());
        }

        [HttpPost]
        [Route("Runtime/BetLevel/Set")]
        public ActionResult SetBetLevel([FromBody] SetBetLevelRequest request)
        {
            return Ok(_engineInstance.SetBetLevel(request));
        }

        [HttpPost]
        [Route("Runtime/LineLevel/Set")]
        public ActionResult SetLineLevel([FromBody] SetLineLevelRequest request)
        {
            return Ok(_engineInstance.SetLineLevel(request));
        }

        [HttpPost]
        [Route("Runtime/BetMax/Set")]
        public ActionResult SetBetMax()
        {
            return Ok(_engineInstance.SetBetMax());
        }

        [HttpPost]
        [Route("Platform/Meters/{category}/{name}/{type}/Get/{game}/{denom}")]
        public ActionResult GetMeter(
            [FromRoute] string name,
            [FromRoute] string category,
            [FromRoute] string type,
            [FromRoute] string game = "0",
            [FromRoute] string denom = "0")
        {
            return Ok(_engineInstance.GetMeter(name, category, type, game, denom));
        }

        [HttpGet]
        [Route("Platform/Meters/Upi/Get")]
        public ActionResult GetUpiMeters()
        {
            return Ok(_engineInstance.GetUpiMeters());
        }

        [HttpGet]
        [Route("Runtime/State")]
        public ActionResult GetRuntimeState()
        {
            return Ok(_engineInstance.GetRuntimeState());
        }

        [HttpPost]
        [Route("Runtime/Running")]
        public ActionResult RuntimeLoaded()
        {
            return Ok(_engineInstance.RuntimeLoaded());
        }

        [HttpGet]
        [Route("Runtime/Mode")]
        public ActionResult GetRuntimeMode()
        {
            return Ok(_engineInstance.GetRuntimeMode());
        }

        [HttpPost]
        [Route("Platform/SendInput")]
        public ActionResult SendInput([FromBody] SendInputRequest request)
        {
            return Ok(_engineInstance.SendInput(request));
        }

        [HttpPost]
        [Route("TouchScreen/0/Touch")]
        public ActionResult SendTouchGame([FromBody] SendTouchGameRequest request)
        {
            return Ok(_engineInstance.SendTouchGame(request));
        }

        [HttpPost]
        [Route("TouchScreen/2/Touch")]
        public ActionResult SendTouchVBD([FromBody] SendTouchVBDRequest request)
        {
            return Ok(_engineInstance.SendTouchVBD(request));
        }

        [HttpPost]
        [Route("BNA/{id}/Bill/Insert")]
        public ActionResult InsertCredits([FromRoute] string id, [FromBody] InsertCreditsRequest request)
        {
            return Ok(_engineInstance.InsertCredits(id, request));
        }

        [HttpPost]
        [Route("BNA/{id}/Ticket/Insert")]
        public ActionResult InsertTicket([FromRoute] string id, [FromBody] InsertTicketRequest request)
        {
            return Ok(_engineInstance.InsertTicket(id, request));
        }

        [HttpPost]
        [Route("CardReaders/0/Event")]
        public ActionResult PlayerCardEvent([FromBody] PlayerCardEventRequest request)
        {
            return Ok(_engineInstance.PlayerCardEvent(request));
        }

        [HttpPost]
        [Route("Platform/Service/Request")]
        public ActionResult ServiceButton([FromBody] ServiceButtonRequest request)
        {
            return Ok(_engineInstance.ServiceButton(request));
        }

        [HttpPost]
        [Route("Platform/Lockup")]
        public ActionResult Lockup([FromBody] LockupRequest request)
        {
            return Ok(_engineInstance.Lockup(request));
        }

        [HttpPost]
        [Route("Platform/MaxWinLimitOverride")]
        public ActionResult SetMaxWinLimitOverride([FromBody] SetMaxWinLimitOverrideRequest request)
        {
            return Ok(_engineInstance.SetMaxWinLimitOverride(request));
        }

        [HttpPost]
        [Route("Platform/HandpayRequest")]
        public ActionResult RequestHandPay([FromBody] RequestHandPayRequest request)
        {
            return Ok(_engineInstance.RequestHandPay(request));
        }

        [HttpPost]
        [Route("Platform/KeyOff")]
        public ActionResult RequestKeyOff()
        {
            return Ok(_engineInstance.RequestKeyOff());
        }

        [HttpGet]
        [Route("Lockups/Get")]
        public ActionResult GetCurrentLockups()
        {
            return Ok(_engineInstance.GetCurrentLockups());
        }

        [HttpGet]
        [Route("Game/Messages/Lockup/GetGameScreenMessages")]
        public ActionResult GetGameMessages()
        {
            return Ok(_engineInstance.GetGameMessages());
        }

        [HttpGet]
        [Route("Game/History/NumberOfEntries")]
        public ActionResult GetNumberOfGameHistoryEntires()
        {
            return Ok(_engineInstance.GetNumberOfGameHistoryEntires());
        }

        [HttpGet]
        [Route("Game/History/{count}")]

        public ActionResult GetGameHistory([FromRoute] string count = "1")
        {
            return Ok(_engineInstance.GetGameHistory(count));
        }

        [HttpGet]
        [Route("Platform/Logs/HandPay/{count}")]
        public ActionResult GetHandPayLog([FromRoute] string count = "1")
        {
            return Ok(_engineInstance.GetHandPayLog(count));
        }

        [HttpGet]
        [Route("Platform/Logs/TransferOut/{count}")]
        public ActionResult GetTransferOutLog([FromRoute] string count = "1")
        {
            return Ok(_engineInstance.GetTransferOutLog(count));
        }

        [HttpGet]
        [Route("Platform/Logs/TransferIn/{count}")]
        public ActionResult GetTransferInLog([FromRoute] string count = "1")
        {
            return Ok(_engineInstance.GetTransferInLog(count));
        }

        [HttpGet]
        [Route("Game/Messages/Get")]
        public ActionResult GetGameLineMessages()
        {
            return Ok(_engineInstance.GetGameLineMessages());
        }

        [HttpGet]
        [Route("Game/OverlayMessage/Get")]
        public ActionResult GetGameOverlayMessage()
        {
            return Ok(_engineInstance.GetGameOverlayMessage());
        }

        [HttpPost]
        [Route("Platform/Info")]
        public ActionResult GetInfo([FromBody] GetInfoRequest request)
        {
            return Ok(_engineInstance.GetInfo(request));
        }

        [HttpPost]
        [Route("Config/Read")]
        public ActionResult GetConfigOption([FromBody] GetConfigOptionRequest request)
        {
            return Ok(_engineInstance.GetConfigOption(request));
        }

        [HttpPost]
        [Route("Config/Write")]
        public ActionResult SetConfigOption([FromBody] SetConfigOptionRequest request)
        {
            return Ok(_engineInstance.SetConfigOption(request));
        }

        [HttpPost]
        [Route("Event/Wait")]
        public ActionResult Wait([FromBody] WaitRequest request)
        {
            return Ok(_engineInstance.Wait(request));
        }

        [HttpPost]
        [Route("Event/Wait/All")]
        public ActionResult WaitAll([FromBody] WaitAllRequest request)
        {
            return Ok(_engineInstance.WaitAll(request));
        }

        [HttpPost]
        [Route("Event/Wait/Any")]
        public ActionResult WaitAny([FromBody] WaitAnyRequest request)
        {
            return Ok(_engineInstance.WaitAny(request));
        }

        [HttpPost]
        [Route("Event/Wait/Cancel")]
        public ActionResult CancelWait([FromBody] CancelWaitRequest request)
        {
            return Ok(_engineInstance.CancelWait(request));
        }

        [HttpPost]
        [Route("Event/Wait/Clear")]
        public ActionResult ClearWaits()
        {
            return Ok(_engineInstance.ClearWaits());
        }

        [HttpGet]
        [Route("Event/Wait/Status")]
        public ActionResult CheckWaitStatus()
        {
            return Ok(_engineInstance.CheckWaitStatus());
        }

        [HttpPost]
        [Route("Platform/Property")]
        public ActionResult GetProperty([FromBody] GetPropertyRequest request)
        {
            return Ok(_engineInstance.GetProperty(request));
        }

        [HttpPost]
        [Route("Platform/Property/Set")]
        public ActionResult SetProperty([FromBody] SetPropertyRequest request)
        {
            return Ok(_engineInstance.SetProperty(request));
        }

        [HttpPost]
        [Route("IO/Set")]
        public ActionResult SetIo([FromBody] SetIoRequest request)
        {
            return Ok(_engineInstance.SetIo(request));
        }

        [HttpPost]
        [Route("IO/Get")]
        public ActionResult GetIo()
        {
            return Ok(_engineInstance.GetIo());
        }

        [HttpPost]
        [Route("TouchScreen/{id}/Connect")]
        public ActionResult TouchScreenConnect([FromRoute] string id)
        {
            return Ok(_engineInstance.TouchScreenConnect(id));
        }

        [HttpPost]
        [Route("TouchScreen/{id}/Disconnect")]
        public ActionResult TouchScreenDisconnect([FromRoute] string id)
        {
            return Ok(_engineInstance.TouchScreenDisconnect(id));
        }
    }
}