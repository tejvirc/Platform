namespace Aristocrat.Monaco.Gaming.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GameAssetTagsTests
    {
        [TestMethod]
        public void AllTagsAreUnique()
        {
            var tags = typeof(GameAssetTags)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => ((string)x.GetRawConstantValue()).ToLower())
                .ToArray();
            var checkBuffer = new HashSet<string>();
            var duplicates = new HashSet<string>();
            foreach (var t in tags)
            {
                if (!checkBuffer.Add(t))
                {
                    duplicates.Add(t);
                }
            }
            Assert.AreEqual(0, duplicates.Count, $"Duplicated tags '{string.Join(",", duplicates)}'");
        }
    }
}