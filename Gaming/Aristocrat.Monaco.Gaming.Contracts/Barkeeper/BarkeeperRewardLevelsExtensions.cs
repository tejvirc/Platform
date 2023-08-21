namespace Aristocrat.Monaco.Gaming.Contracts.Barkeeper
{
    using System;
    using System.Linq;

    partial class BarkeeperRewardLevels : IEquatable<BarkeeperRewardLevels>
    {
        /// <summary>
        ///     Parameterized constructor
        /// </summary>
        /// <param name="rewardLevels"></param>
        public BarkeeperRewardLevels(BarkeeperRewardLevels rewardLevels)
        {
            CashInStrategy = rewardLevels.CashInStrategy;
            CoinInStrategy = new CoinInStrategy { CoinInRate = new CoinInRate(rewardLevels.CoinInStrategy.CoinInRate) };
            Enabled = rewardLevels.Enabled;
            RewardLevels = rewardLevels.RewardLevels.Select(x => new RewardLevel(x)).ToArray();
        }

        /// <inheritdoc />
        public bool Equals(BarkeeperRewardLevels other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return rewardLevelsField.SequenceEqual(other.rewardLevelsField) &&
                   Equals(coinInStrategyField.CoinInRate, other.coinInStrategyField.CoinInRate) &&
                   enabledField == other.enabledField;
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(BarkeeperRewardLevels left, BarkeeperRewardLevels right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(BarkeeperRewardLevels left, BarkeeperRewardLevels right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((BarkeeperRewardLevels)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = rewardLevelsField != null ? rewardLevelsField.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (coinInStrategyField != null ? coinInStrategyField.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    partial class RewardLevel : IEquatable<RewardLevel>
    {
        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        public RewardLevel(RewardLevel other)
        {
            ThresholdInCents = other.ThresholdInCents;
            Enabled = other.Enabled;
            Name = other.Name;
            Alert = other.Alert;
            TriggerStrategy = other.TriggerStrategy;
            Color = other.Color;
            Led = other.Led;
        }

        /// <inheritdoc />
        public bool Equals(RewardLevel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return triggerStrategyField == other.triggerStrategyField && nameField == other.nameField &&
                   thresholdInCentsField == other.thresholdInCentsField && ledField == other.ledField &&
                   colorField == other.colorField && alertField == other.alertField &&
                   enabledField == other.enabledField;
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(RewardLevel left, RewardLevel right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(RewardLevel left, RewardLevel right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RewardLevel)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)triggerStrategyField;
                hashCode = (hashCode * 397) ^ (nameField != null ? nameField.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    partial class CoinInRate : IEquatable<CoinInRate>
    {
        /// <summary>
        /// </summary>
        /// <param name="rate"></param>
        public CoinInRate(CoinInRate rate)
        {
            Enabled = rate.Enabled;
            Amount = rate.Amount;
            SessionRateInMs = rate.SessionRateInMs;
        }

        /// <inheritdoc />
        public bool Equals(CoinInRate other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return sessionRateInMsField == other.sessionRateInMsField && amountField == other.amountField &&
                   enabledField == other.enabledField;
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(CoinInRate left, CoinInRate right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(CoinInRate left, CoinInRate right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((CoinInRate)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return sessionRateInMsField.GetHashCode();
        }
    }
}