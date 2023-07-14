namespace Aristocrat.Monaco.RobotController.ApiHooks
{
    using Aristocrat.Monaco.Application.Contracts.Robot;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.TestController.DataModel;
    using System;
    using System.Collections.Generic;

    public class RobotWebInvoker : IRobotWebInvoker
    {
        private IEventBus _eventBus;

        public RobotWebInvoker(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public CommandResult ToggleRobotMode()
        {
            _eventBus.Publish(new RobotControllerEnableEvent());
            return new CommandResult()
            {
                data = new Dictionary<string, object> { { "response-to", "/Platform/ToggleRobotMode" } },
                Result = true,
                Info = "Toggle robot mode"
            };
        }
    }
}
