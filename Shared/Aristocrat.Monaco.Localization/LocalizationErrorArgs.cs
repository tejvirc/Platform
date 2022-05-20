namespace Aristocrat.Monaco.Localization
{
    using System;

    /// <summary>
    ///     Error event args.
    /// </summary>
    public class LocalizationErrorArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationErrorArgs"/> class.
        /// </summary>
        /// <param name="key">The key used to retrieve the resource.</param>
        /// <param name="message">The error message.</param>
        public LocalizationErrorArgs(string key, string message)
        {
            Key = key;
            Message = message;
        }

        /// <summary>
        ///     Gets the key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        ///     Gets the error message.
        /// </summary>
        public string Message { get; }
    }
}
