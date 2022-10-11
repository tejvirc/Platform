namespace Aristocrat.Monaco.WebApiServer.Controller
{
    using System.Collections.Generic;
    using System.Web.Http;

    /// <summary>
    /// Controller to get the list of all the loaded APIs.
    /// </summary>
    public class LoadedApiController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return AssembliesResolver.AssemblyList;
        }
    }
}
