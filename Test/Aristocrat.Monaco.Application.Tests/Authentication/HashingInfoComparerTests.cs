namespace Aristocrat.Monaco.Application.Tests.Authentication;

using System.Collections.Generic;
using System.Linq;
using Application.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class HashingInfoComparerTests
{
    private static IEnumerable<object[]> TestData => new List<object[]>
    {
        new object[]
        {
            new[]
            {
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_nsw_nocsap_forced_min.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_nsw_nocsap.xml"
            },
            new[]
            {
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_nsw_nocsap.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_nsw_nocsap_forced_min.xml"
            }
        },
        new object[]
        {
            new[]
            {
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld_mars.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld_antebet.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld_nocsap_forced_min.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld_nocsap.xml"
            },
            new[]
            {
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld_antebet.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld_mars.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld_nocsap.xml",
                "ATI_WW_BigFortune_0.99.0.144\\bin\\configuration_qld_nocsap_forced_min.xml"
            }
        }
    };

    private readonly HashingInfoComparer _target = new();

    [DynamicData(nameof(TestData))]
    [DataTestMethod]
    public void SortingTest(string[] input, string[] expected)
    {
        var actual = input.OrderBy(x => x, _target).ToArray();
        CollectionAssert.AreEqual(expected, actual);
    }
}