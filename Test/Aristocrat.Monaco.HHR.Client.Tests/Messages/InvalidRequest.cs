namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages
{
    using System.Text;
    using Client.Messages;
    using Client.Messages.Converters;

    public class InvalidRequest : Request
    {
        public InvalidRequest() : base(Command.CmdInvalidCommand) { }

        public string MyRequest { get; set; }
    }

    public class InvalidRequestConverter : IRequestConverter<InvalidRequest>
    {
        public byte[] Convert(InvalidRequest message)
        {
            return Encoding.UTF8.GetBytes(message.MyRequest);
        }
    }
}
