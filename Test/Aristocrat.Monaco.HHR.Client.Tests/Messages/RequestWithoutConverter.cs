namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages
{
    using Client.Messages;

    public class RequestWithoutConverter : Request
    {
        public RequestWithoutConverter() : base(Command.CmdInactive1) { }
    }
}
