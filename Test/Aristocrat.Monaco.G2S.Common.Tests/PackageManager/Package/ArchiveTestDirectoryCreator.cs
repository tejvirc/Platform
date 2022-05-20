namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.Package
{
    using System.IO;

    public static class ArchiveTestDirectoryCreator
    {
        public const string RootFileName = "RootFile.txt";
        public const string NestedFolderName = "NestedFolder";
        public const string NestedFileName = "NestedFolder/NestedFile.txt";

        public static void Create(string testDir)
        {
            Directory.CreateDirectory(testDir);
            Directory.CreateDirectory(Path.Combine(testDir, NestedFolderName));
            File.AppendAllText(Path.Combine(testDir, RootFileName), @"root file");
            File.AppendAllText(Path.Combine(testDir, NestedFileName), @"nested file");
        }
    }
}