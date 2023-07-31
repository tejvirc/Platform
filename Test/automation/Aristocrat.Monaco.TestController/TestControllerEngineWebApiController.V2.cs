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
        [Route("Platform/GetAvailableGamesV2")]
        public ActionResult GetAvailableGamesV2([FromBody] GetAvailableGamesV2Request request)
        {
            return Ok(_engineInstance.GetAvailableGamesV2(request));
        }

        [HttpPost]
        [Route("Platform/LoadGameV2")]
        public ActionResult TryLoadGameV2([FromBody] TryLoadGameV2Request request)
        {
            return Ok(_engineInstance.TryLoadGameV2(request));
        }

        [HttpPost]
        [Route("Runtime/RequestSpinV2")]
        public ActionResult RequestSpinV2([FromBody] RequestSpinV2Request request)
        {
            return Ok(_engineInstance.RequestSpinV2(request));
        }

        [HttpPost]
        [Route("BNA/{id}/StatusV2")]
        public ActionResult RequestBNAStatusV2([FromRoute] int id, [FromBody] RequestBNAStatusV2 request)
        {
            return Ok(_engineInstance.RequestBNAStatusV2(id, request));
        }

        [HttpPost]
        [Route("Platform/CashoutV2/Request")]
        public ActionResult RequestCashOutV2([FromBody] RequestCashOutV2 request)
        {
            return Ok(_engineInstance.RequestCashOutV2(request));
        }

        [HttpPost]
        [Route("BNA/{id}/Bill/InsertV2")]
        public ActionResult InsertCreditsV2([FromRoute] string id, [FromBody] InsertCreditsV2Request request)
        {
            return Ok(_engineInstance.InsertCreditsV2(id, request));
        }
    }
}