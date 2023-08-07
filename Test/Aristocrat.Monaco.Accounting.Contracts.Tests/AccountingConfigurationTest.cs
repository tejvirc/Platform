namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for AccountingConfigurationTest
    /// </summary>
    [TestClass]
    public class AccountingConfigurationTest
    {
        //     [TestMethod]
        //     public void AccountingConfigurationConstructorTest()
        //     {
        //         var tenderIn = new AccountingConfigurationTenderIn();
        //         var creditLimits = new AccountingConfigurationCreditLimits();
        //
        //         var target = new AccountingConfiguration
        //         {
        //             CreditLimits = creditLimits,
        //             TenderIn = tenderIn
        //         };
        //
        //         Assert.IsNotNull(target);
        //         Assert.AreEqual(tenderIn, target.TenderIn);
        //         Assert.AreEqual(creditLimits, target.CreditLimits);
        //     }
        //
        //     [TestMethod]
        //     public void AccountingConfigurationTenderInConstructorTest()
        //     {
        //         long maxTenderInLimit = 100;
        //         long maxCreditsIn = 1234;
        //         long creditLimit = 12345;
        //
        //         var target = new AccountingConfigurationTenderIn
        //         {
        //             CheckLaundryLimit = true,
        //             MaxTenderInLimit = maxTenderInLimit,
        //             CheckCreditsIn = CheckCreditsStrategy.Balance,
        //             AllowCreditUnderLimit = false,
        //             CreditLimit = creditLimit,
        //             CreditLimitSpecified = true,
        //             MaxCreditsIn = new AccountingConfigurationTenderInMaxCreditsIn { Default = maxCreditsIn }
        //         };
        //
        //         Assert.IsNotNull(target);
        //         Assert.IsTrue(target.CheckLaundryLimit);
        //         Assert.AreEqual(target.CheckCreditsIn, CheckCreditsStrategy.Balance);
        //         Assert.IsFalse(target.AllowCreditUnderLimit);
        //         Assert.IsTrue(target.CreditLimitSpecified);
        //         Assert.AreEqual(maxTenderInLimit, target.MaxTenderInLimit);
        //         Assert.AreEqual(maxCreditsIn, target.MaxCreditsIn.Default);
        //         Assert.AreEqual(creditLimit, target.CreditLimit);
        //     }
        // }
    }
}