namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.PackageManifest.Extension.v100;

    /// <summary>
    ///     A list of <see cref="BetOption" />.
    /// </summary>
    public class BetOptionList : IEnumerable<BetOption>
    {
        private readonly IEnumerable<BetOption> _options;

        /// <summary>
        ///     Creates a BetOptionList from a corresponding manifest object
        /// </summary>
        public BetOptionList(IEnumerable<c_betOption> options)
        {
            if (options == null)
            {
                _options = Enumerable.Empty<BetOption>();
                return;
            }

            _options =
                from o in options
                select new BetOption
                {
                    Name = o.name,
                    Description = o.description,
                    MaxInitialBet = o.maxInitialBetSpecified ? new int?(o.maxInitialBet) : null,
                    MaxTotalBet = o.maxTotalBetSpecified ? new int?(o.maxTotalBet) : null,
                    MaxWin = o.maxWinSpecified ? o.maxWin : null,
                    Bets =
                        from b in o.bet
                        select new Bet { Button = b.button, ButtonName = b.buttonName, Multiplier = b.multiplier },
                    BonusBets = new List<int>((o.bonusBets
                                               ?? o.bet.Select(x => x.multiplier)
                                                   .Where(x => x % BetOption.PokerBonusBetMultiple == 0))
                                               ?? Array.Empty<int>())
                };
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<BetOption> GetEnumerator()
        {
            return _options.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}