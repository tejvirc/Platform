namespace Aristocrat.Mgam.Client.Options
{
    using System;

    /// <summary>
    ///     Options event args.
    /// </summary>
    public class OptionChangedEventArgs<TOptions> : EventArgs
        where TOptions : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionChangedEventArgs{TOptions}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="name">Name of the option that changed.</param>
        public OptionChangedEventArgs(TOptions options, string name)
        {
            Options = options;
            Name = name;
        }

        /// <summary>
        ///     Gets the options.
        /// </summary>
        public TOptions Options { get; }

        /// <summary>
        ///     Gets the name of the option that changed.
        /// </summary>
        public string Name { get; }
    }
}
