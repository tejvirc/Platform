namespace Aristocrat.Monaco.PackageManifest.Gsa
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Aristocrat.PackageManifest.Extension.v100;

    /// <summary>
    ///     GSA Manifest Base class
    /// </summary>
    public abstract class GsaManifest
    {
        /// <summary>
        ///     Checks for a locale match based on the current culture
        /// </summary>
        /// <param name="localeCode">The locale code from the manifest</param>
        /// <returns>Returns true if the value matches the current locale</returns>
        protected static bool IsLocaleMatch(string localeCode)
        {
            return string.Equals(
                localeCode.Replace(@"_", "-"),
                CultureInfo.CurrentCulture.Name,
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///     Parses the file to a GSA manifest
        /// </summary>
        /// <param name="file">The file to parse</param>
        /// <returns>The manifest if successful</returns>
        /// <exception cref="InvalidManifestException">Thrown on error</exception>
        protected virtual manifest Parse(string file)
        {
            manifest manifest;

            try
            {
                manifest = ManifestUtilities.Parse<manifest>(file);
            }
            catch (Exception ex)
            {
                throw new InvalidManifestException("Failed to parse the manifest.", ex);
            }

            return manifest;
        }

        /// <summary>
        ///     Validates a manifest
        /// </summary>
        /// <param name="manifest">The manifest</param>
        /// <exception cref="ArgumentNullException">Thrown when the manifest is null</exception>
        /// <exception cref="InvalidManifestException">Thrown when the manifest fails validation</exception>
        protected virtual void Validate(c_manifest manifest)
        {
            if (manifest == null)
            {
                throw new InvalidManifestException("Failed to parse the manifest.");
            }

            if (manifest.productList == null || !manifest.productList.product.Any())
            {
                throw new InvalidManifestException("The manifest does not contain any products");
            }

            if (manifest.packageList == null || !manifest.packageList.package.Any())
            {
                throw new InvalidManifestException("The package list is empty");
            }

            var product = manifest.productList.product.FirstOrDefault();
            if (product == null)
            {
                throw new InvalidManifestException("The manifest does not contain a product.");
            }
        }
    }
}