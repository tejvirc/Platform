namespace Aristocrat.Monaco.RobotController.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RobotService
    {
        private readonly RobotController _robotController;

        public RobotService(RobotController robotController) => _robotController = robotController;

        public bool IsRegularRobots()
        {
            return _robotController.InProgressRequests.Contains(RobotStateAndOperations.RegularMode);
        }
    }
}
