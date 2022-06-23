namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;

    internal interface IRobotService : IService
    {
        void Execute();
        void Halt();
    }
}
