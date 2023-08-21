namespace Aristocrat.Monaco.Gaming.Commands
{
    using Contracts;

    public class PrimaryGameEscrow
    {
        public PrimaryGameEscrow(long initialWager, byte[] data, IOutcomeRequest request)
        {
            InitialWager = initialWager;
            Data = data;
            Request = request;
        }

        public long InitialWager { get; }

        public byte[] Data { get; }

        public IOutcomeRequest Request { get; }

        public bool Result { get; set; }
    }
}