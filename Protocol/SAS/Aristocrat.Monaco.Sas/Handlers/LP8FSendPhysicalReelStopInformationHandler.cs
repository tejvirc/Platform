namespace Aristocrat.Monaco.Sas.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Hardware.Contracts.Reel;
    using Kernel;

    /// <summary>
    ///     The handler for LP 8F Send Physical Reel Stop Information
    /// </summary>
    public class LP8FSendPhysicalReelStopInformationHandler : ISasLongPollHandler<SendPhysicalReelStopInformationResponse, LongPollData>
    {
        private const byte MinimumStopValue = 0x00;
        private const byte MaximumStopValue = 0xFF;
        private const byte DefaultStopValue = 0xFF;

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendPhysicalReelStopInformation
        };

        /// <inheritdoc />
        public SendPhysicalReelStopInformationResponse Handle(LongPollData data)
        {
            return new SendPhysicalReelStopInformationResponse
            { 
                Reel1 = GetReelStop(1),
                Reel2 = GetReelStop(2),
                Reel3 = GetReelStop(3),
                Reel4 = GetReelStop(4),
                Reel5 = GetReelStop(5),
                Reel6 = GetReelStop(6),
                Reel7 = GetReelStop(7),
                Reel8 = GetReelStop(8),
                Reel9 = GetReelStop(9),
            };
        }

        private static IReelController ReelController => ServiceManager.GetInstance().TryGetService<IReelController>();

        private byte GetReelStop(int reel)
        {
            if (ReelController?.ConnectedReels.Contains(reel) ?? false)
            {
                var state = ReelController?.ReelStates[reel];
                if (state == ReelLogicalState.IdleAtStop)
                {
                    var step = ReelController?.Steps[reel];
                    if (step is >= MinimumStopValue and <= MaximumStopValue)
                    {
                        return (byte)step;
                    }
                }
            }

            return DefaultStopValue;
        }
    }
}