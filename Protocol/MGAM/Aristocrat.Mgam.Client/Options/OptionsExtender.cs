namespace Aristocrat.Mgam.Client.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Extends the set options.
    /// </summary>
    public class OptionsExtender
    {
        private readonly IReadOnlyDictionary<Type, IOptionsExtension> _extensions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionsExtender"/> class.
        /// </summary>
        public OptionsExtender()
        {
            _extensions = new Dictionary<Type, IOptionsExtension>();
        }

        private OptionsExtender(IReadOnlyDictionary<Type, IOptionsExtension> extensions)
        {
            _extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
        }

        /// <summary>
        ///     Gets the client options extension.
        /// </summary>
        public IEnumerable<IOptionsExtension> Extensions => _extensions.Values;

        /// <summary>
        ///     Searches for extenstion of the <typeparamref name="TExtension"/> type.
        /// </summary>
        /// <typeparam name="TExtension">Options extension type.</typeparam>
        /// <returns><typeparamref name="TExtension"/> instance or NULL if not found.</returns>
        public TExtension FindExtension<TExtension>()
            where TExtension : class, IOptionsExtension
        {
            return _extensions.Values.OfType<TExtension>().FirstOrDefault();
        }

        /// <summary>
        ///     Searches for extenstion of the given type.
        /// </summary>
        /// <returns>Extension instance of given type or NULL if not found.</returns>
        public IOptionsExtension FindExtension(Type extensionType)
        {
            return _extensions.Values.FirstOrDefault(e => e.GetType() == extensionType);
        }

        /// <summary>
        ///     Adds an options extension.
        /// </summary>
        /// <typeparam name="TExtension">Extension type.</typeparam>
        /// <param name="extension">Options extension.</param>
        /// <returns></returns>
        public OptionsExtender WithExtension<TExtension>(TExtension extension)
            where TExtension : class, IOptionsExtension
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            var extensions = Extensions.ToDictionary(e => e.GetType(), e => e);
            extensions[extension.GetType()] = extension;

            return new OptionsExtender(extensions);
        }
    }
}
