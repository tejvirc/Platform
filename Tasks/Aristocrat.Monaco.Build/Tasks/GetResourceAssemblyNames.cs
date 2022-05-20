namespace Aristocrat.Monaco.Build.Tasks
{
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    ///     Generate resource assembly names.
    /// </summary>
    public class GetResourceAssemblyNames : Task
    {
        /// <inheritdoc />
        public override bool Execute()
        {
            if (string.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory))
            {
                return false;
            }

            var rootDirectory = new DirectoryInfo(RootDirectory);

            var regex = new Regex($@"{Regex.Escape(rootDirectory.FullName)}([\w|\s|\-|_|\\]+)", RegexOptions.IgnoreCase);

            foreach (var r in Resources ?? new ITaskItem[] { })
            {
                var relativeDir = new DirectoryInfo(r.GetMetadata("RelativeDir"));

                var m = regex.Match(relativeDir.FullName);

                var val = m.Groups[1].Captures[0].Value;
                var assemblyName =
                    $"Aristocrat.Monaco.{val.Replace("\\resources", "").Replace("\\", ".").Replace("-", ".").Replace("_", ".").TrimEnd('.')}";

                r.SetMetadata("AssemblyName", assemblyName);
            }

            return true;
        }

        /// <summary>
        ///     Gets or sets the resource files (*.resx).
        /// </summary>
        [Required]
        public string RootDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the resource files (*.resx).
        /// </summary>
        [Required]
        [Output]
        public ITaskItem[] Resources { get; set; }
    }
}
