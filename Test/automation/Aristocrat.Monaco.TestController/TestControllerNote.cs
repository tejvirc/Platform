namespace Aristocrat.Monaco.TestController
{
    using Application.Contracts;
    using Common.Currency;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using System;
    using System.Collections.Generic;

    public partial class TestControllerEngine
    {
        private readonly Dictionary<string, List<int>> Currencies = new Dictionary<string, List<int>>();
        private readonly List<int> _standardDenominations = new List<int>
        {
            1,
            2,
            5,
            10,
            20,
            50,
            100
        };
        private readonly Dictionary<int, Note> _noteTable = new Dictionary<int, Note>();

        private void CreateTable()
        {
            _noteTable.Clear();
            var noteId = 1;

            GetSupportedCurrencies();

            foreach (var currency in Currencies)
            {
                foreach (var value in currency.Value)
                {
                    var note = new Note
                    {
                        NoteId = noteId++,
                        Value = value,
                        ISOCurrencySymbol = currency.Key,
                        Version = 1
                    };

                    _noteTable[note.NoteId] = note;
                }
            }
        }

        private int GetNoteId(int denom)
        {
            int noteId = 0;
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            string currency = propertiesManager.GetValue(ApplicationConstants.CurrencyId, string.Empty);
            foreach (var note in _noteTable.Values)
            {
                if ((note.Value == denom) && (note.ISOCurrencySymbol == currency))
                {
                    noteId = note.NoteId;
                    break;
                }
            }
            return noteId;
        }

        private void GetSupportedCurrencies()
        {
            var currencies = CurrencyLoader.GetCurrenciesFromWindows(_logger);

            foreach (var currency in currencies.Keys)
            {
                if (Enum.IsDefined(typeof(ISOCurrencyCode), currency.ToUpper()) &&
                    !Currencies.ContainsKey(currency))
                {
                    Currencies[currency] = _standardDenominations;
                }
            }
        }
    }
}
