

namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Application.Contracts.Extensions;
    using Contracts.Central;
    using System;
    using System.Collections.Generic;

    public class CentralTransactionData
    {
        private readonly CentralTransaction _centralTransaction;

        public CentralTransactionData(CentralTransaction transaction)
        {
            _centralTransaction = transaction;
            CreateOutcomes();
        }

        public DateTime TransactionDateTime => _centralTransaction?.TransactionDateTime ?? DateTime.MinValue;

        public int GameId => _centralTransaction?.GameId ?? -1;

        public string Denomination => (_centralTransaction?.Denomination ?? 0).MillicentsToDollars().FormattedCurrencyString();

        public string WagerCategory => _centralTransaction?.WagerCategory ?? string.Empty;

        public string WagerAmount => (_centralTransaction?.WagerAmount ?? 0).MillicentsToDollars().FormattedCurrencyString();

        public int OutcomesRequested => _centralTransaction?.OutcomesRequested ?? 0;

        public string OutcomeState => (_centralTransaction?.OutcomeState ?? Contracts.Central.OutcomeState.Failed).ToString();

        public string OutcomeException => (_centralTransaction?.Exception ?? Contracts.Central.OutcomeException.None).ToString();

        public List<OutcomeData> Outcomes { get; } = new List<OutcomeData>();

        private void CreateOutcomes()
        {
            if (_centralTransaction != null && _centralTransaction.Outcomes != null)
            {
                foreach (var outcome in _centralTransaction.Outcomes)
                {
                    Outcomes.Add(new OutcomeData(outcome));
                }
            }
        }
    }

    public class OutcomeData
    {
        private readonly Outcome _outcome;

        public OutcomeData(Outcome outcome)
        {
            _outcome = outcome;
        }

        public long GameSetId => _outcome?.GameSetId ?? -1;

        public string Reference => (_outcome?.Reference ?? OutcomeReference.Direct).ToString();

        public string Type => (_outcome?.Type ?? OutcomeType.Standard).ToString();

        public string Value => (_outcome?.Value ?? 0).MillicentsToDollars().FormattedCurrencyString();

        public int WinLevelIndex => _outcome?.WinLevelIndex ?? -1;

        public string LookupData => _outcome?.LookupData ?? string.Empty;
    }
}
