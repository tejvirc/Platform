namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>Class to handle fake card reader events.</summary>
    [Serializable]
    public class FakeCardReaderEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeCardReaderEvent" /> class.
        /// </summary>
        public FakeCardReaderEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeCardReaderEvent" /> class.
        /// </summary>
        /// <param name="id">Physical IO that was handled.</param>
        /// ///
        /// <param name="cardValue">card number</param>
        /// <param name="action">Action that was performed</param>
        public FakeCardReaderEvent(int id, string cardValue, bool action)
        {
            Id = id;
            CardValue = cardValue;
            Action = action;
        }

        /// <summary>Gets a value indicating whether Id is set.</summary>
        public int Id { get; }

        /// <summary>Gets a value of card number</summary>
        public string CardValue { get; }

        /// <summary>Gets a value indicating whether Action is set.</summary>
        public bool Action { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Id={1},  Action={2}, Action={3}]",
                GetType().Name,
                Id,
                CardValue,
                Action);
        }
    }
}