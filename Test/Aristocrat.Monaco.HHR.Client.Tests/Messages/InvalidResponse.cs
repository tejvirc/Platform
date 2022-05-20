namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages
{
    using Client.Messages;
    using Client.Messages.Converters;

    public class InvalidResponse : Response
    {
        public int MyProperty { get; set; }
        public InvalidResponse() : base(Command.CmdInvalidCommand)
        {
        }
    }

    public class InvalidResponseConverter : IResponseConverter<InvalidResponse>
    {
        public InvalidResponse Convert(byte[] data)
        {
            return new InvalidResponse()
            {
                MyProperty = data[0]
            };
        }
    }
}
