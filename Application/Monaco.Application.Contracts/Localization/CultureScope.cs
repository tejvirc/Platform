namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Kernel;

    /// <summary>
    ///     Implements the <see cref="ICultureScope"/> interface.
    /// </summary>
    public class CultureScope : ICultureScope
    {
        private readonly ILocalizer _localizer;
        private CultureInfo _currentCulture;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CultureScope"/> class.
        /// </summary>
        /// <param name="name">The culture provider name.</param>
        public CultureScope(string name)
            : this(name, ServiceManager.GetInstance().GetService<ILocalizerFactory>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CultureScope"/> class.
        /// </summary>
        /// <param name="name">The culture provider name.</param>
        /// <param name="localizer"><see cref="ILocalizerFactory"/> interface instance.</param>
        private CultureScope(string name, ILocalizerFactory localizer)
        {
            _localizer = localizer?.For(name) ?? throw new ArgumentNullException(nameof(localizer));

            if (_localizer == null)
            {
                throw new ArgumentException($"No provider found for {name}");
            }

            BeginScope();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CultureScope"/> class.
        /// </summary>
        /// <param name="localizer"><see cref="ILocalizer"/> interface instance.</param>
        public CultureScope(ILocalizer localizer)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

            BeginScope();
        }

        /// <inheritdoc />
        ~CultureScope()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public TResource GetObject<TResource>(string key, LocalizeOptions options = LocalizeOptions.None)
        {
            CultureInfo culture;

            switch (options)
            {
                case LocalizeOptions.None:
                    culture = _localizer.CurrentCulture;
                    break;

                case LocalizeOptions.UseInvariant:
                    culture = CultureInfo.InvariantCulture;
                    break;

                case LocalizeOptions.UseCurrent:
                    culture = _currentCulture;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(options), options, null);
            }

            return _localizer.GetObject<TResource>(culture, key);
        }

        /// <inheritdoc />
        public string GetString(string key, LocalizeOptions options = LocalizeOptions.None)
        {
            return GetObject<string>(key);
        }

        /// <inheritdoc />
        public string FormatString(string key, params object[] args)
        {
            return FormatString(key, LocalizeOptions.None, args);
        }

        /// <inheritdoc />
        public string FormatString(string key, LocalizeOptions options = LocalizeOptions.None, params object[] args)
        {
            var format = GetObject<string>(key);
            return string.Format(format, args);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Thread.CurrentThread.CurrentCulture = _currentCulture;
            }
        }

        private void BeginScope()
        {
            _currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = _localizer.CurrentCulture;
        }
    }
}
