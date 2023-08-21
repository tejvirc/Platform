namespace Aristocrat.Mgam.Client.Options
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     Base options class.
    /// </summary>
    public abstract class Options<TOptions> : IOptions<TOptions>, IOptionsMonitor<TOptions>
        where TOptions : Options<TOptions>
    {
        private readonly IObservable<EventPattern<OptionChangedEventArgs<TOptions>>> _options;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Options{TOptions}"/> class.
        /// </summary>
        protected Options()
        {
            _options = Observable.FromEventPattern<OptionChangedEventArgs<TOptions>>(
                h => OptionChanged += h,
                h => OptionChanged -= h);
        }

        private event EventHandler<OptionChangedEventArgs<TOptions>> OptionChanged;

        /// <inheritdoc />
        public TOptions Value => (TOptions)this;

        /// <inheritdoc />
        public TOptions CurrentValue { get; protected set; }

        /// <inheritdoc />
        public IDisposable OnChange(Action<TOptions, string> listener, Predicate<string> filter)
        {
            return (CurrentValue ?? this)._options.Subscribe(e =>
            {
                if (filter?.Invoke(e.EventArgs.Name) ?? true)
                {
                    listener(e.EventArgs.Options, e.EventArgs.Name);
                }
            });
        }

        /// <summary>
        ///     Sets the options value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="backingField">Reference to the backing field.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The name of the property.</param>
        protected void SetOption<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            // Only current value properties can be changed
            if (CurrentValue != null)
            {
                return;
            }

            backingField = value;

            RaiseOptionChanged(propertyName);
        }

        private void RaiseOptionChanged(string propertyName)
        {
            OptionChanged?.Invoke(this, new OptionChangedEventArgs<TOptions>(Value, propertyName));
        }
    }
}
