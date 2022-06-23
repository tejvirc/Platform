namespace Aristocrat.Monaco.RobotController
{
    using System;
    using Contracts;
    using Kernel;

    public sealed partial class RobotController : BaseRunnable, IRobotController
    {
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override void OnInitialize()
        {
            throw new NotImplementedException();
        }

        protected override void OnRun()
        {
            throw new NotImplementedException();
        }

        protected override void OnStop()
        {
            throw new NotImplementedException();
        }
    }
}
