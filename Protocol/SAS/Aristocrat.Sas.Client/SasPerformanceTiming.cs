namespace Aristocrat.Sas.Client
{
    using System.Diagnostics;
    using log4net;

    /// <summary>
    ///     A class to hold the state when receiving and processing the data
    /// </summary>
    public class SasPerformanceTiming
    {
        private readonly decimal _millisecondsPerTick = (1_000M) / Stopwatch.Frequency;
        private readonly SasClient _sasClient;
        private const string NewLine = "\r\n\t\t\t\t\t";
        private const long ChirpIntervalTolerance = 12;

        /// <summary>
        ///     Sets or gets a value indicating if the package is ignored when the
        ///     inter-byte delay is bigger than 5 ms.
        /// </summary>
        public bool SupportInterBytesDelay { set; get; } = false;

        /// <summary> Number of milliseconds for inter-bytes delay threshold regulated in the SAS spec (must be 5 ms)  </summary>
        public decimal InterBytesDelayThreshold => 5.00M;
        
        /// <summary> Number of milliseconds GM has to reply a poll after the entire message is received </summary>
        public decimal ReplyTimeThreshold => 20.00M;

        /// <summary> Chirps are started to be sent after the number of milliseconds. </summary>
        public long ChirpTimeout { set; get; } = 5000;

        /// <summary> A chirp must be sent once for every interval </summary>
        public long ChirpInterval { set;  get; } = 200 - ChirpIntervalTolerance;

        /// <summary> A log message about chirping will be logged for every 1 minute </summary>
        public long LogChirpTimeout => 60000;

        /// <summary> A watch to track the elapsed time for receiving a poll. </summary>
        public Stopwatch PollWatch { get; } = new Stopwatch();

        /// <summary> A watch to track the elapsed time for chirping </summary>
        public Stopwatch ChirpWatch { get; } = new Stopwatch();

        /// <summary> A watch to track the elapsed time since last logging </summary>
        public Stopwatch LogChirpWatch { get; } = new Stopwatch();

        /// <summary> A watch to track the elapsed time between two byte reads </summary>
        public Stopwatch InterByteWatch { get; } = new Stopwatch();

        /// <summary> Records how many ticks are used to get a poll. </summary>
        public long TicksToGetPoll { set; get; }

        /// <summary> Records how many ticks are used before processing a long poll. </summary>
        internal long TicksUsedBeforeProcessingLongPoll { set; get; }

        /// <summary> Records how many ticks are used to peek at the top most exception </summary>
        internal long TicksToPeekAtTopException { set; get; }

        /// <summary> Records how many ticks are used to get the next exception </summary>
        internal long TicksToGetNextException { set; get; }

        /// <summary> Records how many ticks are used to send bytes out </summary>
        internal long TicksToSendBytes { set; get; }

        /// <summary> Records how many ticks are used to process a long poll specifically </summary>
        internal long TicksToProcessLongPoll { set; get; }

        /// <summary> Records how many ticks are used to handle a poll  </summary>
        internal long TicksToHandlePoll { set; get; }

        /// <summary> Records how many ticks are used totally  </summary>
        public long TicksUsedTotally { set; get; }

        /// <summary> Records how many times the inter-bytes delay exceeds the threshold </summary>
        public long InterBytesDelayCount { private set; get; }

        /// <summary> The class constructor </summary>
        /// <param name="sasClient">The sasClient instance </param>
        internal SasPerformanceTiming(SasClient sasClient)
        {
            _sasClient = sasClient;
        }

        internal bool IgnorePackageDueToInterBytesDelay(ILog logger, byte last, byte current, bool first = false)
        {
            var result = false;
            var delay = InterByteWatch.ElapsedTicks * _millisecondsPerTick;
            if (delay > InterBytesDelayThreshold)
            {
                var log = string.Format($"[SAS] Inter-Bytes Delay {0}: {delay:0.000} ms between {last:X2} and {current:X2}. Total Times: {++InterBytesDelayCount}",
                    first ? "to read Long Poll's command" : $"for Long Poll {_sasClient.Command:X2}");
                logger.Error(log);

                if (SupportInterBytesDelay)
                {
                    result = true;
                }
            }

            InterByteWatch.Restart();
            return result;
        }

        /// <summary> Resets all variables </summary>
        /// <param name="restartPollWatch"> Indicates whether to restart the PollWatch </param>
        internal void Reset(bool restartPollWatch = true)
        {
            TicksToGetPoll = 0;
            TicksUsedBeforeProcessingLongPoll = 0;
            TicksToPeekAtTopException = 0;
            TicksToGetNextException = 0;
            TicksToSendBytes = 0;
            TicksToProcessLongPoll = 0;
            TicksToHandlePoll = 0;
            TicksUsedTotally = 0;

            InterByteWatch.Restart();

            if (restartPollWatch)
            {
                PollWatch.Restart();
            }
        }

        /// <summary> Prints the audit information </summary>
        /// <param name="logger">Indicates where to print </param>
        internal void PrintAudit(ILog logger)
        {
            decimal millisecondsToHandlePoll = TicksToHandlePoll * _millisecondsPerTick;
            if (millisecondsToHandlePoll > ReplyTimeThreshold)
            {
                var log = NewLine + $"[SAS] Total Milliseconds used in processing a poll: {TicksUsedTotally * _millisecondsPerTick:0.00} ms" +
                          NewLine + $"[SAS] Milliseconds to get the poll: {TicksToGetPoll * _millisecondsPerTick:0.00} ms" +
                          NewLine;
                if (_sasClient.IsLongPoll)
                {
                    log += $"[SAS] Milliseconds to prepare for the long poll: {(TicksUsedBeforeProcessingLongPoll - TicksToGetPoll) * _millisecondsPerTick:0.00} ms"
                        + NewLine + $"[SAS] Milliseconds to process the long poll: {TicksToProcessLongPoll * _millisecondsPerTick:0.00} ms"
                        + NewLine;
                }
                else if (_sasClient.IsGeneralPoll)
                {
                    log += $"[SAS] Milliseconds to peek at top exception: {TicksToPeekAtTopException * _millisecondsPerTick:0.00} ms"
                        + NewLine + $"[SAS] Milliseconds to get next exception: {TicksToGetNextException * _millisecondsPerTick:0.00} ms"
                        + NewLine + $"[SAS] Milliseconds to send bytes: {TicksToSendBytes * _millisecondsPerTick:0.00} ms"
                        + NewLine;
                }

                log += "[SAS] TOTAL MILLISECONDS TO HANDLE " +
                     (_sasClient.IsGeneralPoll ? "GENERAL POLL "
                                  : (_sasClient.IsLongPoll ? $"LONG POLL({_sasClient.Command:X2})"
                                                           : "GLOBAL OR OTHER ADDRESS POLL")) +
                                                              $"===> {millisecondsToHandlePoll:0.00} ms";
                logger.Error(log);
            }
        }
    }
}
