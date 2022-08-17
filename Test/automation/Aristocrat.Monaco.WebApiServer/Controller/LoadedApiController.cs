namespace Aristocrat.Monaco.WebApiServer.Controller
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Web.Http;

    /// <summary>
    /// Controller to get the list of all the loaded APIs.
    /// </summary>
    [ComVisible(false)]
    public class LoadedApiController : ApiController
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return AssembliesResolver.AssemblyList;
        }
    }
}
