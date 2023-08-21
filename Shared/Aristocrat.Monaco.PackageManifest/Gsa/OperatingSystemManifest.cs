namespace Aristocrat.Monaco.PackageManifest.Gsa
{
    using System;
    using System.IO;
    using System.Linq;
    using Models;

    /// <summary>
    ///     Defines an operating system manifest
    /// </summary>
    public class OperatingSystemManifest : GsaManifest, IManifest<Product>
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

            var localizedInfo = GetLocalization(product);

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
                UninstallSequence = "<uninstallSeq />"
            };
        }

        /// <inheritdoc />
        public Product Read(Func<Stream> streamProvider)
        {
            throw new NotImplementedException();
        }
    }
}
