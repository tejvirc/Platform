namespace Aristocrat.Monaco.PackageManifest.Ati
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Models;

    /// <summary>
    ///     Manifest reader/writer implementation.
    /// </summary>
    public class ImageManifest : IManifest<Image>
    {
        /// <inheritdoc />
        public Image Read(string file)
        {
            return ToManifest(File.ReadAllLines(file));
        }

        /// <inheritdoc />
        public Image Read(Func<Stream> streamProvider)
        {
            var lines = ReadLines(streamProvider);

            return ToManifest(lines.ToArray());
        }

        private static Image ToManifest(string[] contents)
        {
            return new Image(
                contents: contents,
                name: contents.SingleOrDefault(l => l.StartsWith("Image:"))?.Split(':')[1],
                type: contents.SingleOrDefault(l => l.StartsWith("Type:"))?.Split(':')[1],
                file: contents.SingleOrDefault(l => l.StartsWith("File:"))?.Split(':')[1],
                size: Convert.ToInt64(contents.SingleOrDefault(l => l.StartsWith("Size:"))?.Split(':')[1]),
                version: contents.SingleOrDefault(l => l.StartsWith("Version:"))?.Split(':')[1],
                jurisdiction: contents.SingleOrDefault(l => l.StartsWith("Jurisdiction:"))?.Split(':')[1],
                assemblyHash: contents.SingleOrDefault(l => l.StartsWith("AssemblyHash:"))?.Split(':')[1],
                fileHash: contents.SingleOrDefault(l => l.StartsWith("SHA1:"))?.Split(':')[1],
                manifestHash: contents.Skip(contents.Length - 2).Take(1).Single(),
                signature: contents.Last());
        }

        private static IEnumerable<string> ReadLines(Func<Stream> streamProvider)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
