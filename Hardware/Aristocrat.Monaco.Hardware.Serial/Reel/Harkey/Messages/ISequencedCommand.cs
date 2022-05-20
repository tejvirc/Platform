namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    /// <summary>
    ///     The Sequenced Command interface
    /// </summary>
    public interface ISequencedCommand
    {
        /// <summary>
        ///     Gets or sets the SequenceId
        /// </summary>
        byte SequenceId { get; set; }
    }
}