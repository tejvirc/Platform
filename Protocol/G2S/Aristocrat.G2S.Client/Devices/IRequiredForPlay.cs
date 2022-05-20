namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Defines a contract for classes that support the setClassState command, an enterClassMode command appears within the
    ///     commConfig and optionConfig classes.
    /// </summary>
    public interface IRequiredForPlay
    {
        /// <summary>
        ///     Gets a value indicating whether indicates whether the device MUST be functioning and enabled before the EGM can be
        ///     played.
        ///     (true = enabled, false = disabled)
        /// </summary>
        bool RequiredForPlay { get; }

        /// <summary>
        ///     Gets or sets the text message to display while the device is disabled.
        /// </summary>
        string DisableText { get; set; }
    }
}