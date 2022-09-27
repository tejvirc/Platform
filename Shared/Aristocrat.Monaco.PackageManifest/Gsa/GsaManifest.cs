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

        /// <summary>
        ///     Gets the localized text, if present.
        /// </summary>
        /// <param name="product">The related product.</param>
        /// <returns>The matching localization.</returns>
        protected localization GetLocalization(product product)
        {
            return product.localization.FirstOrDefault(IsLocaleMatch) ??
                   product.localization.First();

            static bool IsLocaleMatch(localization locale)
            {
                return string.Equals(
                    locale.localeCode.Replace(@"_", "-"),
                    CultureInfo.CurrentCulture.Name,
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        ///     Gets the mechanical reel home steps.
        /// </summary>
        /// <param name="product">The related product.</param>
        /// <returns>The mechnical reel home steps.</returns>
        protected int[] GetMechanicalReelHomeSteps(product product)
        {
            var steps = string.IsNullOrWhiteSpace(product.mechanicalReelHomeStops)
                ? product.mechanicalReelHomeSteps
                : product.mechanicalReelHomeStops;

            return steps?.Split(' ').Select(int.Parse).ToArray();
        }
    }
}