

namespace Aristocrat.Monaco.Application.Tests.Helper
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Aristocrat.Monaco.Application.Helpers;
    using log4net;
    using Aristocrat.Monaco.Application.Localization;
    

    [TestClass]
    public class CurrencyCultureHelperTest
    {
        private readonly Collection<int>  _supportedNotes = new Collection<int>() { 1, 2, 10 };
        private readonly List<string> _BNASupportedCodes = new List<string>()
        {
            "USD",
            "EUR"
        };

        private const string _euroMajorSymbol = "€";
        private const string _euroMinorSymbol = "¢";
        private const string _separatorComma = ",";
        private const string _separatorDot = ".";
        private const int _symbolPostPlacement = 0;
        private const int _symbolPrePlacement = 1;

        private const string _currencyCodeEuro = "EUR";

        private const int _numberOfSupportedCurrencies = 150;

        private Dictionary<string, CurrencyDefaultsCurrencyInfo> _currencyDefaults = new Dictionary<string, CurrencyDefaultsCurrencyInfo>();
        private List<string> _euroCurrencyDescription;
        private Dictionary<string, CurrencyDefaultsCurrencyInfo> _emptyCurrencyDefaults = new Dictionary<string, CurrencyDefaultsCurrencyInfo>();

        private Mock<ILog> _loggerMock = new Mock<ILog>();

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock.Setup(l => l.Info(It.IsAny<string>()));

            _currencyDefaults = new Dictionary<string, CurrencyDefaultsCurrencyInfo>()
            {
                { _currencyCodeEuro, new CurrencyDefaultsCurrencyInfo()
                        {
                            CurrencyCode = _currencyCodeEuro,
                            
                                Formats = new CurrencyDefaultsCurrencyInfoFormat[]
                                {
                                    new CurrencyDefaultsCurrencyInfoFormat()
                                    {
                                        idSpecified = true,
                                        id = 1,
                                        Symbol = _euroMajorSymbol,
                                        MinorUnitSymbol = _euroMinorSymbol,
                                        PositivePatternSpecified = true,
                                        PositivePattern = _symbolPostPlacement,
                                        GroupSeparator = _separatorComma,
                                        DecimalSeparator = _separatorDot
                                    },
                                    new CurrencyDefaultsCurrencyInfoFormat()
                                    {
                                        idSpecified = true,
                                        id = 2,
                                        Symbol = _euroMajorSymbol,
                                        MinorUnitSymbol = _euroMinorSymbol,
                                        PositivePatternSpecified = true,
                                        PositivePattern = _symbolPrePlacement,
                                        GroupSeparator = _separatorComma,
                                        DecimalSeparator = _separatorDot
                                    }
                                }
                            
                        }
                }
            };
            _euroCurrencyDescription = new List<string>()
            {
                "Euro EUR €1,000.00",
                "Euro EUR 1,000.00€"
            };
        }

        [TestMethod]
        public void GetSupportedCurrencies_WhenNoBNA_Success()
        {
            string currencyCode = "USD";

            var supportedCurrencies = CurrencyCultureHelper.GetSupportedCurrencies(
                currencyCode, _emptyCurrencyDefaults, _loggerMock.Object, null, true);

            Assert.IsNotNull(supportedCurrencies);
            Assert.AreEqual(supportedCurrencies.Count(), _numberOfSupportedCurrencies);
        }

        [TestMethod]
        public void GetSupportedCurrencies_WhenBNA_Success()
        {
            string currencyCode = "USD";
            var noteAcceptorMock = new Mock<INoteAcceptor>();
            noteAcceptorMock.Setup(n => n.GetSupportedNotes(It.IsAny<string>())).Returns( (string code) =>
            {
                if (_BNASupportedCodes.Contains(code))
                { 
                    return _supportedNotes;
                }
                return new Collection<int>();
            });

            var supportedCurrencies = CurrencyCultureHelper.GetSupportedCurrencies(
                currencyCode, _emptyCurrencyDefaults, _loggerMock.Object, noteAcceptorMock.Object, true);

            Assert.IsNotNull(supportedCurrencies);
            Assert.AreEqual(2, supportedCurrencies.Count());
        }

        [TestMethod]
        public void GetSupportedCurrencies_WhenBNA_FormatOverride_Success()
        {
            string currencyCode = "USD";
            var noteAcceptorMock = new Mock<INoteAcceptor>();
            noteAcceptorMock.Setup(n => n.GetSupportedNotes(It.IsAny<string>())).Returns((string code) =>
            {
                if (_BNASupportedCodes.Contains(code))
                {
                    return _supportedNotes;
                }
                return new Collection<int>();
            });

            var supportedCurrencies = CurrencyCultureHelper.GetSupportedCurrencies(
                currencyCode, _currencyDefaults, _loggerMock.Object, noteAcceptorMock.Object, true);

            Assert.IsNotNull(supportedCurrencies);
            Assert.AreEqual(3, supportedCurrencies.Count());

            // verify the EURO currency format
            var euroCurrencies = supportedCurrencies.Where(s => s.IsoCode.Equals(_currencyCodeEuro, StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.IsNotNull(euroCurrencies);
            // Should have two formats
            Assert.AreEqual(_currencyDefaults[_currencyCodeEuro]?.Formats.Length, euroCurrencies.Count());

            // Check if the formats are correct
            Assert.IsTrue(_euroCurrencyDescription.Any(d => d.Equals(euroCurrencies[0].Description)));
            Assert.IsTrue(_euroCurrencyDescription.Any(d => d.Equals(euroCurrencies[1].Description)));
        }

        [TestMethod]
        public void GetSupportedCurrencies_WhenBNAHaveUnsupportedCurrency_Success()
        {
            string currencyCode = "USD";
            var noteAcceptorMock = new Mock<INoteAcceptor>();

            List<string> BNASupportedCodes = new List<string>()
            {
                "USD",
                "EUR",
                "UNK"
            };

            noteAcceptorMock.Setup(n => n.GetSupportedNotes(It.IsAny<string>())).Returns((string code) =>
            {
                if (BNASupportedCodes.Contains(code))
                {
                    return _supportedNotes;
                }
                return new Collection<int>();
            });

            var supportedCurrencies = CurrencyCultureHelper.GetSupportedCurrencies(
                currencyCode, _emptyCurrencyDefaults, _loggerMock.Object, noteAcceptorMock.Object, true);

            Assert.IsNotNull(supportedCurrencies);
            Assert.AreEqual(2, supportedCurrencies.Count());
        }
    }
}
