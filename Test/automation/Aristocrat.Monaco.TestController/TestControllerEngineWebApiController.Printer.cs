namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using TestController.Models.Request;

    public partial class TestControllerEngineWebApiController
    {
        [HttpPost]
        [Route("Printer/{id}/Connect")]
        public ActionResult PrinterConnect([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterConnect(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Disconnect")]
        public ActionResult PrinterDisconnect([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterDisconnect(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Jam")]
        public ActionResult PrinterJam([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterJam(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/Out")]
        public ActionResult PrinterEmpty([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterEmpty(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/PaperInChute")]
        public ActionResult PrinterPaperInChute([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterPaperInChute(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Status")]
        public ActionResult PrinterStatus([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterStatus(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/Status")]
        public ActionResult PrinterPaperStatus([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterPaperStatus(id));
        }

        [HttpPost]
        [Route("Printer/{id}/ChassisOpen")]
        public ActionResult PrinterChassisOpen([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterChassisOpen(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/Low")]
        public ActionResult PrinterPaperLow([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterPaperLow(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Paper/Fill")]
        public ActionResult PrinterPaperFill([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterPaperFill(id));
        }

        [HttpGet]
        [Route("Printer/{id}/Ticket/Read/{count=1}")]
        public ActionResult PrinterGetTicketList([FromRoute] string id, [FromRoute] string count = "1")
        {
            return Ok(_engineInstance.PrinterGetTicketList(id, count));
        }

        [HttpPost]
        [Route("Printer/{id}/Ticket/Remove")]
        public ActionResult PrinterTicketRemove([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterTicketRemove(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Head/Lift")]
        public ActionResult PrinterHeadLift([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterHeadLift(id));
        }

        [HttpPost]
        [Route("Printer/{id}/Head/Lower")]
        public ActionResult PrinterHeadLower([FromRoute] string id)
        {
            return Ok(_engineInstance.PrinterHeadLower(id));
        }
    }
}