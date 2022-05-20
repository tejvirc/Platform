namespace Aristocrat.Monaco.G2S.Handlers.Central
{
    using System;
    using System.Linq;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;

    public static class CentralExtensions
    {
        public static centralLog ToCentralLog(this CentralTransaction @this, IGameProvider games)
        {
            var game = games.GetGame(@this.GameId);

            return new centralLog
            {
                transactionId = @this.TransactionId,
                deviceId = @this.DeviceId,
                logSequence = @this.LogSequence,
                outcomeState = ToOutcomeState(@this.OutcomeState),
                gamePlayId = @this.GameId,
                themeId = game?.ThemeId,
                paytableId = game?.PaytableId,
                denomId = @this.Denomination,
                wagerCategory = @this.WagerCategory,
                wagerAmt = @this.WagerAmount,
                outcomeQty = @this.OutcomesRequested,
                outcomeDateTime = @this.TransactionDateTime,
                outcomeException = (int)@this.Exception,
                outcome = @this.Outcomes.Select(o => o.ToOutcome()).ToArray()
            };
        }

        public static OutcomeReference ToOutcomeReference(this t_outcomeRefs @this)
        {
            switch (@this)
            {
                case t_outcomeRefs.G2S_direct:
                    return OutcomeReference.Direct;
                case t_outcomeRefs.G2S_indirect:
                    return OutcomeReference.Indirect;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static OutcomeType ToOutcomeType(this t_outcomeTypes @this)
        {
            switch (@this)
            {
                case t_outcomeTypes.G2S_standard:
                    return OutcomeType.Standard;
                case t_outcomeTypes.G2S_progressive:
                    return OutcomeType.Progressive;
                case t_outcomeTypes.G2S_fractional:
                    return OutcomeType.Fractional;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static Outcome ToOutcome(this outcome @this)
        {
            return new Outcome(
                @this.outcomeId,
                @this.gameSetId,
                @this.subSetId,
                @this.outcomeReference.ToOutcomeReference(),
                @this.outcomeType.ToOutcomeType(),
                @this.outcomeValue,
                @this.winLevelIndex,
                @this.lookupReference);
        }

        private static outcome ToOutcome(this Outcome @this)
        {
            return new outcome
            {
                outcomeId = @this.Id,
                gameSetId = @this.GameSetId,
                subSetId = @this.SubsetId,
                outcomeReference = ToOutcomeRef(@this.Reference),
                outcomeType = ToOutcomeType(@this.Type),
                outcomeValue = @this.Value,
                winLevelIndex = @this.WinLevelIndex,
                lookupReference = @this.LookupData
            };
        }

        private static t_outcomeTypes ToOutcomeType(OutcomeType type)
        {
            switch (type)
            {
                case OutcomeType.Standard:
                    return t_outcomeTypes.G2S_standard;
                case OutcomeType.Progressive:
                    return t_outcomeTypes.G2S_progressive;
                case OutcomeType.Fractional:
                    return t_outcomeTypes.G2S_fractional;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static t_outcomeRefs ToOutcomeRef(OutcomeReference reference)
        {
            switch (reference)
            {
                case OutcomeReference.Direct:
                    return t_outcomeRefs.G2S_direct;
                case OutcomeReference.Indirect:
                    return t_outcomeRefs.G2S_indirect;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reference), reference, null);
            }
        }

        private static t_outcomeStates ToOutcomeState(this OutcomeState state)
        {
            switch (state)
            {
                case OutcomeState.Requested:
                    return t_outcomeStates.G2S_outcomeRequest;
                case OutcomeState.Failed:
                    return t_outcomeStates.G2S_outcomePend;
                case OutcomeState.Committed:
                    return t_outcomeStates.G2S_outcomeCommit;
                case OutcomeState.Acknowledged:
                    return t_outcomeStates.G2S_outcomeAck;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}