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
        [Route("CardReaders/0/InsertCard")]
        public ActionResult InsertCard([FromBody] InsertCardRequest request)
        {
            return Ok(_engineInstance.InsertCard(request));
        }

        [HttpPost]
        [Route("CardReaders/0/RemoveCard")]
        public ActionResult RemoveCard()
        {
            return Ok(_engineInstance.RemoveCard());
        }

        [HttpPost]
        [Route("CardReaders/0/Disconnect")]
        public ActionResult CardReaderDisconnect()
        {
            return Ok(_engineInstance.CardReaderDisconnect());
        }

        [HttpPost]
        [Route("CardReaders/0/Connect")]
        public ActionResult CardReaderConnect()
        {
            return Ok(_engineInstance.CardReaderConnect());
        }
    }
}