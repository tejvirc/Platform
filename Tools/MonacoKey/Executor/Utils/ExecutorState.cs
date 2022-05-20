namespace Executor.Utils
{
    public enum ExecutorState
    {
        Started = 0,
        PassedCommandLineParametersCheck = 1,
        PassedCommandCheck = 2,
        PassedRSAKeyCheck = 3,
        PassedUSBCheck = 4,
        PassedLogicDoorCheck = 5,
        ExecutionSucceeded = 6,
        ExecutionFailed = 7,
        ExitImmediately = 8
    }
}
