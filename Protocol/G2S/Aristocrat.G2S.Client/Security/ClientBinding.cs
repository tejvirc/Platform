namespace Aristocrat.G2S.Client.Security
{
    using System;
    using System.Net;

    /// <summary>
    ///     Utility methods for managing client bindings
    /// </summary>
    public static class ClientBinding
    {
        /// <summary>
        ///     Creates or updates a client binding
        /// </summary>
        /// <param name="endPoint">The endpoint.</param>
        /// <param name="thumbprint">The expected thumbprint.</param>
        /// <param name="bindingFactory">The function used to generate a new binding.</param>
        public static void AddOrUpdate(IPEndPoint endPoint, string thumbprint, Func<CertificateBinding> bindingFactory)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }

            if (string.IsNullOrEmpty(thumbprint))
            {
                throw new ArgumentNullException(nameof(thumbprint));
            }

            if (bindingFactory == null)
            {
                throw new ArgumentNullException(nameof(bindingFactory));
            }

            var bindingConfiguration = new CertificateBindingConfiguration();

            var currentBinding = bindingConfiguration.Get(endPoint);

            var newBinding = bindingFactory();

            if (currentBinding != null &&
                string.Compare(
                    currentBinding.Thumbprint,
                    thumbprint,
                    StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                bindingConfiguration.Delete(endPoint);

                if (newBinding != null)
                {
                    bindingConfiguration.Bind(newBinding);
                }
            }
            else if (currentBinding == null && newBinding != null)
            {
                bindingConfiguration.Bind(newBinding);
            }
        }
    }
}