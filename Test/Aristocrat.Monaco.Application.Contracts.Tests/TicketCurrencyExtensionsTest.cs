namespace Aristocrat.Monaco.Application.Contracts.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Extensions;
    using Localization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class TicketCurrencyExtensionsTest
    {
        /// <summary>
        ///     Setups for each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            TicketCurrencyExtensions.PlayerTicketLocale = CultureInfo.CurrentCulture.Name;
            TicketCurrencyExtensions.SetCultureInfo(
                CultureInfo.CurrentCulture.Name,
                CultureInfo.CurrentCulture,
                CultureInfo.CurrentCulture,
                null,
                null,
                true,
                true,
                "c"
            );
        }

        /// <summary>
        ///     Cleanups for each test
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConvertCurrencyToWordsNegativeAmountExpectException()
        {
            TicketCurrencyExtensions.ConvertCurrencyToWords(-1.00m);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConvertCurrencyToWordsOverLimitExpectException()
        {
            TicketCurrencyExtensions.ConvertCurrencyToWords(1.00m * 1000m * 1000m * 1000m * 1000m);

            Assert.Fail();
        }

        [TestMethod]
        public void ConvertCurrencyToWordsConversions()
        {
            // Base case
            Assert.AreEqual("ONE DOLLAR AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1.00m));

            // Plurals
            Assert.AreEqual("ONE DOLLAR AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1));
            Assert.AreEqual("ZERO DOLLARS AND ONE CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(0.01m));
            Assert.AreEqual("ZERO DOLLARS AND TWO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(0.02m));
            Assert.AreEqual("TWO DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(2));

            // Various amounts of cents
            Assert.AreEqual("ZERO DOLLARS AND FIFTEEN CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(0.15m));
            Assert.AreEqual("ONE DOLLAR AND FORTY-THREE CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1.43m));

            // Round numbers
            Assert.AreEqual("TEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(10));
            Assert.AreEqual("ONE HUNDRED DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(100));
            Assert.AreEqual("ONE THOUSAND DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1000));
            Assert.AreEqual("ONE MILLION DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1000000));
            Assert.AreEqual("ONE BILLION DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1000000000));

            // Big nasty numbers
            Assert.AreEqual(
                "EIGHTY-ONE MILLION TWO HUNDRED SIXTY-EIGHT THOUSAND SEVEN HUNDRED NINETY-THREE DOLLARS AND THIRTY-SIX CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(81268793.36m));
            Assert.AreEqual(
                "NINE HUNDRED NINETY-NINE BILLION NINE HUNDRED NINETY-NINE MILLION NINE HUNDRED NINETY-NINE THOUSAND NINE HUNDRED NINETY-NINE DOLLARS AND NINETY-NINE CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(999999999999.99m));

            // Longest string
            Assert.AreEqual(
                "THREE HUNDRED SEVENTY-SEVEN BILLION EIGHT HUNDRED SEVENTY-THREE MILLION SEVEN HUNDRED SEVENTY-EIGHT THOUSAND THREE HUNDRED SEVENTY-SEVEN DOLLARS AND SEVENTY-EIGHT CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(377873778377.78m));
        }

        [TestMethod]
        public void ConvertCurrencyToWordsConversionsFrench()
        {
            // Convert language to French
            var frenchLocale = "fr-CA";
            CultureInfo frenchCulture = new CultureInfo(frenchLocale);
            TicketCurrencyExtensions.PlayerTicketLocale = frenchLocale;
            TicketCurrencyExtensions.SetCultureInfo(
                frenchLocale,
                frenchCulture,
                frenchCulture,
                null,
                null,
                false,
                false,
                "c"
            );

            // Base case
            Assert.AreEqual("UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1.00m));

            // Plurals
            Assert.AreEqual("ZÉRO DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(0));
            Assert.AreEqual("UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1));
            Assert.AreEqual("ZÉRO DOLLAR ET UN CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(0.01m));
            Assert.AreEqual("ZÉRO DOLLAR ET DEUX CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(0.02m));
            Assert.AreEqual("DEUX DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(2));

            // Various amounts of cents
            Assert.AreEqual("ZÉRO DOLLAR ET QUINZE CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(0.15m));
            Assert.AreEqual("UN DOLLAR ET QUARANTE-TROIS CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1.43m));

            // Round numbers
            Assert.AreEqual("DIX DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(10));
            Assert.AreEqual("CENT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(100));
            Assert.AreEqual("MILLE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1000));
            Assert.AreEqual("UN MILLION DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1000000));
            Assert.AreEqual("UN MILLIARD DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1000000000));

            // Big nasty numbers
            Assert.AreEqual(
                "QUATRE-VINGT-UN MILLIONS DEUX CENT SOIXANTE-HUIT MILLE SEPT CENT QUATRE-VINGT-TREIZE DOLLAR ET TRENTE-SIX CENT",
                TicketCurrencyExtensions.ConvertCurrencyToWords(81268793.36m));
           /* Assert.AreEqual(
                "NEUF CENT QUATRE-VINGT-DIX-NEUF MILLIARDS NEUF CENT QUATRE-VINGT-DIX-NEUF MILLIONS NEUF CENT QUATRE-VINGT-DIX-NEUF MILLE NEUF CENT QUATRE-VINGT-DIX-NEUF DOLLAR ET QUATRE-VINGT-DIX-NEUF CENT",
                TicketCurrencyExtensions.ConvertCurrencyToWords(999999999999.99m));

            // Longest string
            Assert.AreEqual(
                "QUATRE CENT QUATRE-VINGT-QUATORZE MILLIARDS QUATRE CENT QUATRE-VINGT-QUATORZE MILLIONS QUATRE CENT QUATRE-VINGT-QUATORZE MILLE QUATRE CENT QUATRE-VINGT-QUATORZE DOLLAR ET QUATRE-VINGT-QUATORZE CENT",
                TicketCurrencyExtensions.ConvertCurrencyToWords(494494494494.94m));*/
        }

        [TestMethod]
        public void ConvertCurrencyToWordsMoreCases()
        {
            Assert.AreEqual("ZERO DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(0));
            Assert.AreEqual("ONE DOLLAR AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1));
            Assert.AreEqual("TWO DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(2));
            Assert.AreEqual("THREE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(3));
            Assert.AreEqual("FOUR DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(4));
            Assert.AreEqual("FIVE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(5));
            Assert.AreEqual("SIX DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(6));
            Assert.AreEqual("SEVEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(7));
            Assert.AreEqual("EIGHT DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(8));
            Assert.AreEqual("NINE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(9));
            Assert.AreEqual("TEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(10));
            Assert.AreEqual("ELEVEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(11));
            Assert.AreEqual("TWELVE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(12));
            Assert.AreEqual("THIRTEEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(13));
            Assert.AreEqual("FOURTEEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(14));
            Assert.AreEqual("FIFTEEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(15));
            Assert.AreEqual("SIXTEEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(16));
            Assert.AreEqual("SEVENTEEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(17));
            Assert.AreEqual("EIGHTEEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(18));
            Assert.AreEqual("NINETEEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(19));
            Assert.AreEqual("TWENTY DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(20));
            Assert.AreEqual("TWENTY-ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(21));
            Assert.AreEqual("THIRTY DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(30));
            Assert.AreEqual("THIRTY-ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(31));
            Assert.AreEqual("FORTY DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(40));
            Assert.AreEqual("FORTY-ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(41));
            Assert.AreEqual("FIFTY DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(50));
            Assert.AreEqual("FIFTY-ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(51));
            Assert.AreEqual("SIXTY DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(60));
            Assert.AreEqual("SIXTY-ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(61));
            Assert.AreEqual("SEVENTY DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(70));
            Assert.AreEqual("SEVENTY-ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(71));
            Assert.AreEqual("EIGHTY DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(80));
            Assert.AreEqual("EIGHTY-ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(81));
            Assert.AreEqual("NINETY DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(90));
            Assert.AreEqual("NINETY-ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(91));
            Assert.AreEqual("ONE HUNDRED DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(100));
            Assert.AreEqual("ONE HUNDRED ONE DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(101));
            Assert.AreEqual("ONE HUNDRED TEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(110));
            Assert.AreEqual("ONE HUNDRED ELEVEN DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(111));
            Assert.AreEqual(
                "ONE HUNDRED SEVENTY-FIVE DOLLARS AND ZERO CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(175));
            Assert.AreEqual("TWO HUNDRED DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(200));
            Assert.AreEqual(
                "TWO HUNDRED TWENTY-FIVE DOLLARS AND ZERO CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(225));
            Assert.AreEqual("ONE THOUSAND DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1000));
            Assert.AreEqual(
                "ONE THOUSAND ONE HUNDRED DOLLARS AND ZERO CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(1100));
            Assert.AreEqual(
                "TWO THOUSAND ONE HUNDRED DOLLARS AND ZERO CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(2100));
            Assert.AreEqual("TEN THOUSAND DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(10000));
            Assert.AreEqual(
                "ONE HUNDRED THOUSAND DOLLARS AND ZERO CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(100000));
            Assert.AreEqual(
                "ONE HUNDRED THOUSAND ONE HUNDRED DOLLARS AND ZERO CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(100100));
            Assert.AreEqual(
                "ONE HUNDRED ONE THOUSAND ONE HUNDRED DOLLARS AND ZERO CENTS",
                TicketCurrencyExtensions.ConvertCurrencyToWords(101100));
            Assert.AreEqual("ONE MILLION DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1000000));
            Assert.AreEqual("TWO MILLION DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(2000000));
            Assert.AreEqual("ONE BILLION DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(1000000000));
            Assert.AreEqual("TWO BILLION DOLLARS AND ZERO CENTS", TicketCurrencyExtensions.ConvertCurrencyToWords(2000000000));
        }

        [TestMethod]
        public void ConvertCurrencyToWordsMoreCasesFrench()
        {
            // Convert language to French
            var frenchLocale = "fr-CA";
            CultureInfo frenchCulture = new CultureInfo(frenchLocale);
            TicketCurrencyExtensions.PlayerTicketLocale = frenchLocale;
            TicketCurrencyExtensions.SetCultureInfo(
                frenchLocale,
                frenchCulture,
                frenchCulture,
                null,
                null,
                false,
                false,
                "c"
            );

            Assert.AreEqual("ZÉRO DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(0));
            Assert.AreEqual("UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1));
            Assert.AreEqual("DEUX DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(2));
            Assert.AreEqual("TROIS DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(3));
            Assert.AreEqual("QUATRE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(4));
            Assert.AreEqual("CINQ DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(5));
            Assert.AreEqual("SIX DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(6));
            Assert.AreEqual("SEPT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(7));
            Assert.AreEqual("HUIT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(8));
            Assert.AreEqual("NEUF DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(9));
            Assert.AreEqual("DIX DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(10));
            Assert.AreEqual("ONZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(11));
            Assert.AreEqual("DOUZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(12));
            Assert.AreEqual("TREIZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(13));
            Assert.AreEqual("QUATORZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(14));
            Assert.AreEqual("QUINZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(15));
            Assert.AreEqual("SEIZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(16));
            Assert.AreEqual("DIX-SEPT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(17));
            Assert.AreEqual("DIX-HUIT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(18));
            Assert.AreEqual("DIX-NEUF DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(19));
            Assert.AreEqual("VINGT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(20));
            Assert.AreEqual("VINGT ET UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(21));
            Assert.AreEqual("TRENTE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(30));
            Assert.AreEqual("TRENTE ET UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(31));
            Assert.AreEqual("QUARANTE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(40));
            Assert.AreEqual("QUARANTE ET UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(41));
            Assert.AreEqual("CINQUANTE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(50));
            Assert.AreEqual("CINQUANTE ET UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(51));
            Assert.AreEqual("SOIXANTE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(60));
            Assert.AreEqual("SOIXANTE ET UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(61));
            Assert.AreEqual("SOIXANTE-DIX DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(70));
            Assert.AreEqual("SOIXANTE ET ONZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(71));
            Assert.AreEqual("QUATRE-VINGTS DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(80));
            Assert.AreEqual("QUATRE-VINGT-UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(81));
            Assert.AreEqual("QUATRE-VINGT-DIX DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(90));
            Assert.AreEqual("QUATRE-VINGT-ONZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(91));
            Assert.AreEqual("CENT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(100));
            Assert.AreEqual("CENT UN DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(101));
            Assert.AreEqual("CENT DIX DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(110));
            Assert.AreEqual("CENT ONZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(111));
            Assert.AreEqual("CENT SOIXANTE-QUINZE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(175));
            Assert.AreEqual("DEUX CENTS DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(200));
            Assert.AreEqual("DEUX CENT VINGT-CINQ DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(225));
            Assert.AreEqual("MILLE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1000));
            Assert.AreEqual("MILLE CENT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1100));
            Assert.AreEqual("DEUX MILLE CENT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(2100));
            Assert.AreEqual("DIX MILLE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(10000));
            Assert.AreEqual("CENT MILLE DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(100000));
            Assert.AreEqual("CENT MILLE CENT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(100100));
            Assert.AreEqual("CENT UN MILLE CENT DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(101100));
            Assert.AreEqual("UN MILLION DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1000000));
            Assert.AreEqual("DEUX MILLIONS DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(2000000));
            Assert.AreEqual("UN MILLIARD DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(1000000000));
            Assert.AreEqual("DEUX MILLIARDS DOLLAR ET ZÉRO CENT", TicketCurrencyExtensions.ConvertCurrencyToWords(2000000000));
        }

        [TestMethod]
        public void ConvertCurrencyToWrappedWordsTest()
        {
            List<string> strings = TicketCurrencyExtensions.ConvertCurrencyToWrappedWords(2222, 10).ToList();
            Assert.AreEqual(8, strings.Count);
        }
    }
}