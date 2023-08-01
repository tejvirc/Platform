namespace Aristocrat.Monaco.RobotApiServer.Controllers
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.RobotController.Contracts;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/{controller}")]
    internal class AutomationController : ControllerBase
    {
        private IEventBus _eventBus;

        public AutomationController()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        }

        [HttpPost("robot/enable")]
        public ActionResult EnableRobot()
        {
            _eventBus.Publish(new RobotControllerEnableEvent());

            return Ok(true);
        }
    }
}
