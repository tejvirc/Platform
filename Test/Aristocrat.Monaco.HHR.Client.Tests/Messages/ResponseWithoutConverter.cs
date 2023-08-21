namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages
{
    using Client.Messages;

    public class ResponseWithoutConverter : Response
    {
        public ResponseWithoutConverter() : base(Command.CmdInactive1) { }
    }
}
