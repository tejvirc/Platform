namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    public class UpdateBetOptions
    {
        public UpdateBetOptions(IEnumerable<IBetDetails> betDetails)
        {
            BetDetails = betDetails;
        }

        public IEnumerable<IBetDetails> BetDetails { get; }
    }
}