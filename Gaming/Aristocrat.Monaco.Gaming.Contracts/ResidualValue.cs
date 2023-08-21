namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Numerics;
    using Newtonsoft.Json;
    using static System.FormattableString;

    /// <summary>
    ///     A value with residual.
    /// </summary>
    /// <seealso cref="T:System.IEquatable{Aristocrat.Monaco.Gaming.Contracts.ResidualValue}"/>
    [Serializable]
    public class ResidualValue : IEquatable<ResidualValue>
    {
        /// <summary>
        ///     The residual units.
        /// </summary>
        public const long ResidualUnits = GamingConstants.PercentageConversion * 100L; // use 100 percent

        /// <summary>
        ///     The residual factor.
        /// </summary>
        public static readonly BigInteger ResidualFactor = ResidualUnits;

        private static long _payableUnit = GamingConstants.Millicents;

        /// <summary>
        ///     Implicit cast that converts the given BigInteger to a <see cref="ResidualValue"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the operation.</returns>
        public static implicit operator ResidualValue(BigInteger value)
        {
            return new ResidualValue(value);
        }

        /// <summary>
        ///     Implicit cast that converts the given long to a ResidualValue.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the operation.</returns>
        public static implicit operator ResidualValue(long value)
        {
            return new ResidualValue(value);
        }

        /// <summary>
        ///     Implicit cast that converts the given <see cref="ResidualValue"/> to a long.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the operation.</returns>
        public static implicit operator long(ResidualValue value)
        {
            return value.Amount;
        }

        /// <summary>
        ///     Explicit cast that converts the given <see cref="ResidualValue"/> to a BigInteger.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the operation.</returns>
        public static explicit operator BigInteger(ResidualValue value)
        {
            return value.InternalValue;
        }

        /// <summary>
        ///     Multiplication operator.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator *(ResidualValue left, long right)
        {
            return new ResidualValue(left.InternalValue * right);
        }

        /// <summary>
        ///     Multiplication operator.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator *(long left, ResidualValue right)
        {
            return new ResidualValue(left * right.InternalValue);
        }

        /// <summary>
        ///     Addition operator.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">A value to add to it.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator +(ResidualValue left, BigInteger right)
        {
            return new ResidualValue(left.InternalValue + right);
        }

        /// <summary>
        ///     Addition operator.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">A value to add to it.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator +(BigInteger left, ResidualValue right)
        {
            return new ResidualValue(left + right.InternalValue);
        }

        /// <summary>
        ///     Addition operator.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">A value to add to it.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator +(ResidualValue left, long right)
        {
            return left.InternalValue + right * ResidualFactor;
        }

        /// <summary>
        ///     Addition operator.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">A value to add to it.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator +(long left, ResidualValue right)
        {
            return left * ResidualFactor + right.InternalValue;
        }

        /// <summary>
        ///     Subtraction operator.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">A value to subtract from it.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator -(ResidualValue left, BigInteger right)
        {
            return new ResidualValue(left.InternalValue - right);
        }

        /// <summary>
        ///     Subtraction operator.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">A value to subtract from it.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator -(BigInteger left, ResidualValue right)
        {
            return new ResidualValue(left - right.InternalValue);
        }

        /// <summary>
        ///     Subtraction operator.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">A value to subtract from it.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator -(ResidualValue left, long right)
        {
            return left.InternalValue - right * ResidualFactor;
        }

        /// <summary>
        ///     Subtraction operator.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">A value to subtract from it.</param>
        /// <returns>The result of the operation.</returns>
        public static ResidualValue operator -(long left, ResidualValue right)
        {
            return left * ResidualFactor - right.InternalValue;
        }

        /// <summary>
        ///     Equality operator.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator ==(ResidualValue left, ResidualValue right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        ///     Inequality operator.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator !=(ResidualValue left, ResidualValue right)
        {
            return !(left == right);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResidualValue"/> class.
        /// </summary>
        public ResidualValue()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResidualValue"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="residual">(Optional) The residual.</param>
        public ResidualValue(long value, long residual = 0)
            : this(value * ResidualFactor + residual)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResidualValue"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ResidualValue(BigInteger value)
        {
            InternalValue = value;
        }

        /// <summary>
        ///     Gets or sets the amount. Amount is presented in millicents rounded to smallest payable unit.
        /// </summary>
        /// <value>The amount.</value>
        public virtual long Amount
        {
            get => InternalValue.ToPayable();
            set => InternalValue = value * ResidualFactor + Residual;
        }

        /// <summary>
        ///     Gets the residual.
        /// </summary>
        /// <value>The residual.</value>
        public virtual long Residual
        {
            get => (long)(InternalValue % TruncationFactor);
            set => InternalValue = Amount * ResidualFactor + value;
        }

        /// <summary>
        ///     Gets the smallest payable unit. Payable unit is expressed in millicents and defaults to 1 cent.
        /// </summary>
        /// <value>The smallest payable unit.</value>
        public static long PayableUnit
        {
            get => _payableUnit;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _payableUnit = value;
            }
        }

        /// <summary>
        ///     Gets the truncation factor.
        /// </summary>
        /// <value>The truncation factor.</value>
        public static BigInteger TruncationFactor => ResidualFactor * PayableUnit;

        /// <summary>
        ///     Gets or sets the internal value.
        /// </summary>
        /// <value>The internal value.</value>
        [JsonProperty]
        protected BigInteger InternalValue { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ResidualValue);
        }

        /// <inheritdoc/>
        public bool Equals(ResidualValue other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return InternalValue.Equals(other.InternalValue);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new { InternalValue }.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"ResidualValue [{Amount} {Residual}]");
        }
    }
}