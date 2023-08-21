namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class SasPollData
    {
        public enum PacketType
        {
            Tx,
            Rx
        }

        public enum PollType
        {
            GeneralPoll,
            LongPoll,
            SyncPoll,
            NoActivity
        }

        private readonly decimal _millisecondsPerTick = 1_000M / Stopwatch.Frequency;

        private decimal _elapsedTime;

        public SasPollData(IEnumerable<byte> response)
        {
            PollData = new List<byte>(response);
        }

        public PacketType Type { get; set; }

        public PollType SasPollType { get; set; }

        public string TypeDescription { get; set; }

        public IReadOnlyCollection<byte> PollData { get; set; }

        public string PollDataString => BitConverter.ToString(PollData.ToArray()).Replace("-", " ");

        public DateTime Time { get; } = DateTime.Now;

        public string Description { get; set; }

        public byte PollName
        {
            get
            {
                switch (SasPollType)
                {
                    case PollType.GeneralPoll:
                        return PollData.Count > 2 ? PollData.ElementAt(2) : PollData.ElementAt(0);
                    case PollType.LongPoll:
                        return PollData.ElementAt(1);
                    case PollType.SyncPoll:
                        return PollData.ElementAt(0);
                    case PollType.NoActivity:
                        return PollData.ElementAt(0);
                    default:
                        return 0;
                }
            }
        }

        public decimal ElapsedTime
        {
            get => _elapsedTime;
            set => _elapsedTime = value * _millisecondsPerTick;
        }
    }
}