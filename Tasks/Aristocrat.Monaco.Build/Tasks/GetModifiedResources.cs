namespace Aristocrat.Monaco.Build.Tasks
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    ///     Checks if resources have been modified since the resources assembly was built.
    /// </summary>
    public class GetModifiedResources : Task
    {
        /// <inheritdoc />
        public override bool Execute()
        {
            var modifiedResources =
            (
                from item in Resources ?? new ITaskItem[] { }
                let directory = item.GetMetadata("RelativeDir")
                let assemblyName = item.GetMetadata("AssemblyName")
                let modified = DateTime.Parse(item.GetMetadata("ModifiedTime"))
                let assemblyPath = $"{directory}{assemblyName}.dll"
                where !File.Exists(assemblyPath) || modified > File.GetLastWriteTime(assemblyPath)
                select new TaskItem(item.ItemSpec)
            ).Cast<ITaskItem>().ToList();

            ModifiedResources = modifiedResources.Any() ? modifiedResources.ToArray() : null;

            return true;
        }

        /// <summary>
        ///     Gets or sets the resource files (*.resx).
        /// </summary>
        [Required]
        public ITaskItem[] Resources { get; set; }

        /// <summary>
        ///     Gets or sets the the resources that have been modified.
        /// </summary>
        [Output]
        public ITaskItem[] ModifiedResources { get; private set; }
    }
}
