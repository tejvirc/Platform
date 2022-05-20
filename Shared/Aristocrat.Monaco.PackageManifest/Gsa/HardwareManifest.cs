namespace Aristocrat.Monaco.PackageManifest.Gsa
{
    using System;
    using System.IO;
    using System.Linq;
    using Models;

    /// <summary>
    ///     Defines a hardware manifest
    /// </summary>
    public class HardwareManifest : GsaManifest, IManifest<Product>
    {
        /// <inheritdoc />
        public Product Read(string file)
        {
            var manifest = Parse(file);

            // throws on error
            Validate(manifest);

            var product = manifest.productList.product.FirstOrDefault();
            if (product == null)
            {
                throw new InvalidManifestException("The manifest does not contain a product.");
            }

            // Get the localized text if present, otherwise get the first one
            var localizedInfo = product.localization.FirstOrDefault(l => IsLocaleMatch(l.localeCode)) ??
                                product.localization.First();

            return new Product
            {
                Name = localizedInfo.productName,
                ProductId = product.productId,
                ManifestId = manifest.manifestId,
                ReleaseNumber = product.releaseNum,
                ReleaseDate = product.releaseDateTime,
                Description = localizedInfo.shortDesc,
                DetailedDescription = localizedInfo.longDesc,
                InstallSequence = "<installSeq />",
                UninstallSequence = "<uninstallSeq />",
                MechanicalReels = product.mechanicalReels,
                MechanicalReelHomeStops = product.mechanicalReelHomeStops.Split(' ').Select(int.Parse).ToArray()
            };
        }

        /// <inheritdoc />
        public Product Read(Func<Stream> streamProvider)
        {
            throw new NotImplementedException();
        }
    }
}
