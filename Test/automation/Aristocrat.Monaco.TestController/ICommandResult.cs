namespace Aristocrat.Monaco.TestController
{
    using System;
    using DataModel;

    public interface ICommandResult
    {
        object Info { get; set; }
        string Message { get; set; }
        bool Result { get; set; }
        PlatformStateEnum State { get; set; }
        string Command { get; set; }
    }
}