using Aristocrat.Monaco.Kernel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System;
using log4net;

namespace Aristocrat.Monaco.RobotApiServer
{
    public class RobotApiServerService : IService
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public string Name => GetType().ToString();
        public ICollection<Type> ServiceTypes => new[] { typeof(RobotApiServerService) };

        public void Initialize()
        {
            try
            {
               Task.Run(() => { RobotApiServerCore.StartApiServer(); }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void Log(string message)
        {
            _logger.Info(message);
        }
    }
}