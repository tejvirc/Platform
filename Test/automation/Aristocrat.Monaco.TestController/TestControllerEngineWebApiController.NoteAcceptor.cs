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
        [HttpGet]
        [Route("BNA/{id}/Status")]
        public ActionResult NoteAcceptorStatus([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorStatus(id));
        }

        [HttpGet]
        [Route("BNA/{id}/Bill/Mask/Get")]
        public ActionResult NoteAcceptorGetMask([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorGetMask(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Bill/Mask/Set")]
        public ActionResult NoteAcceptorSetMask([FromRoute] string id, [FromBody] NoteAcceptorSetMaskRequest request)
        {
            return Ok(_engineInstance.NoteAcceptorSetMask(id, request));
        }

        [HttpGet]
        [Route("BNA/{id}/Bill/Notes/Get")]
        public ActionResult NoteAcceptorGetNotes([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorGetNotes(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Cheat")]
        public ActionResult NoteAcceptorCheat([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorCheat(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Connect")]
        public ActionResult NoteAcceptorConnect([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorConnect(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Disconnect")]
        public ActionResult NoteAcceptorDisconnect([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorDisconnect(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Firmware/Get")]
        public ActionResult NoteAcceptorGetFirmware([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorGetFirmware(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Firmware/Set")]
        public ActionResult NoteAcceptorSetFirmware(
            [FromRoute] string id,
            [FromBody] NoteAcceptorSetFirmwareRequest request)
        {
            return Ok(_engineInstance.NoteAcceptorSetFirmware(id, request));
        }

        [HttpPost]
        [Route("BNA/{id}/Stacker/Attach")]
        public ActionResult NoteAcceptorAttachStacker([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorAttachStacker(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Stacker/Full")]
        public ActionResult NoteAcceptorStackerFull([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorStackerFull(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Stacker/Jam")]
        public ActionResult NoteAcceptorStackerJam(string id)
        {
            return Ok(_engineInstance.NoteAcceptorStackerJam(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Stacker/Remove")]
        public ActionResult NoteAcceptorStackerRemove([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorStackerRemove(id));
        }

        [HttpPost]
        [Route("BNA/{id}/Ticket/Insert")]
        public ActionResult NoteAcceptorInsertTicket([FromRoute] string id, [FromBody] InsertTicketRequest request)
        {
            return Ok(_engineInstance.NoteAcceptorInsertTicket(request.ValidationId, id));
        }

        [HttpPost]
        [Route("BNA/{id}/Escrow/Status")]
        public ActionResult NoteAcceptorEscrowStatus([FromRoute] string id)
        {
            return Ok(_engineInstance.NoteAcceptorEscrowStatus(id));
        }
    }
}