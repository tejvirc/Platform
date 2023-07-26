namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using G2S.Data.Model;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Script states
    /// </summary>
    public enum ScriptState
    {
        /// <summary>
        ///     PendingDateTime
        /// </summary>
        PendingDateTime = 0,

        /// <summary>
        ///     PendingDisable
        /// </summary>
        PendingDisable = 1,

        /// <summary>
        ///     PendingAuthorization
        /// </summary>
        PendingAuthorization = 2,

        /// <summary>
        ///     PendingOperatorAction
        /// </summary>
        PendingOperatorAction = 3,

        /// <summary>
        ///     PendingPackage
        /// </summary>
        PendingPackage = 4,

        /// <summary>
        ///     InProgress
        /// </summary>
        InProgress = 5,

        /// <summary>
        ///     Completed
        /// </summary>
        Completed = 6,

        /// <summary>
        ///     Canceled
        /// </summary>
        Canceled = 7,

        /// <summary>
        ///     Error
        /// </summary>
        Error = 8
    }

    /// <summary>
    ///     Implementation of Script entity.
    /// </summary>
    public class Script : BaseEntity, ILogSequence
    {
        /// <summary>
        ///     Gets or sets Script Id
        /// </summary>
        public int ScriptId { get; set; }

        /// <summary>
        ///     Gets or sets Device Id
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets script state.
        /// </summary>
        public ScriptState State { get; set; }

        /// <summary>
        ///     Gets or sets the apply condition.
        /// </summary>
        public ApplyCondition ApplyCondition { get; set; }

        /// <summary>
        ///     Gets or sets the disable condition.
        /// </summary>
        public DisableCondition DisableCondition { get; set; }

        /// <summary>
        ///     Gets or sets authorized date time.
        /// </summary>
        public DateTime? AuthorizeDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the script exception value.
        /// </summary>
        public int ScriptException { get; set; }

        /// <summary>
        ///     Gets or sets completed date time.
        /// </summary>
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        ///     Gets or sets start date time.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        ///     Gets or sets end date time.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        ///     Gets or sets transaction id.
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether completed acknowledged by the host.
        /// </summary>
        public bool ScriptCompleteHostAcknowledged { get; set; }

        /// <summary>
        ///     Gets or sets reason code.
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        ///     Gets or sets the command data.
        /// </summary>
        public string CommandData { get; set; }

        /// <summary>
        ///     Gets or sets the option change authorize items.
        /// </summary>
        public virtual ICollection<ConfigChangeAuthorizeItem> AuthorizeItems { get; set; }

        /// <summary>
        ///     Returns a human-readable representation of a Script.
        /// </summary>
        /// <returns>A human-readable string.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [ScriptId={1}, DeviceId={2}, State={3}, ApplyCondition={4}, DisableCondition={5}, AuthorizeDateTime={6}, ScriptException={7}, CompletedDateTime={8}, StartDateTime={9}, EndDateTime={10}, TransactionId={11}, ScriptCompletedHostAcknowledged={12}, ReasonCode={13}, CommandData={14}",
                GetType(),
                ScriptId,
                DeviceId,
                State,
                ApplyCondition,
                DisableCondition,
                AuthorizeDateTime?.ToString(CultureInfo.InvariantCulture) ??
                DateTime.MinValue.ToString(CultureInfo.InvariantCulture),
                ScriptException,
                CompletedDateTime?.ToString(CultureInfo.InvariantCulture) ??
                DateTime.MinValue.ToString(CultureInfo.InvariantCulture),
                StartDateTime?.ToString(CultureInfo.InvariantCulture) ??
                DateTime.MinValue.ToString(CultureInfo.InvariantCulture),
                EndDateTime?.ToString(CultureInfo.InvariantCulture) ??
                DateTime.MinValue.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                ScriptCompleteHostAcknowledged,
                ReasonCode,
                CommandData);

            return builder.ToString();
        }
    }
}