namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class ExtensionsTestHelper
    {
        public static void AssertEnumToG2SData<T1, T2>(Func<T1, T2> convert, T1[] ignoreEnums = null)
        {
            foreach (var inputEnum in Enum.GetValues(typeof(T1)))
            {
                if (ignoreEnums != null)
                {
                    if (ignoreEnums.Contains((T1)inputEnum))
                    {
                        continue;
                    }
                }

                var inputEnumName = inputEnum.ToString().ToLower();

                var convertedValue = convert((T1)inputEnum)?.ToString()
                    .Replace("G2S_", "")
                    .Replace("GTK_", "")
                    .Replace("IGT_", "")
                    .ToLower();

                Assert.AreEqual(inputEnumName, convertedValue);
            }
        }

        public static void AssertG2SStringsToEnum<T>(Func<string, T> convert, string[] g2sStrings)
        {
            foreach (var g2sString in g2sStrings)
            {
                var resultEnum = convert(g2sString);
                Assert.AreEqual(
                    g2sString
                        .Replace("G2S_", "")
                        .Replace("GTK_", "")
                        .Replace("IGT_", "")
                        .ToLower(),
                    resultEnum.ToString().ToLower());
            }
        }
    }
}