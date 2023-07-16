namespace Aristocrat.Monaco.TestController
{
    using System.Collections.Generic;
    using System.Linq;
    using DataModel;
    using Hardware.Contracts;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.CardReader;
    using Hardware.Contracts.IO;
    using Kernel;
    using Microsoft.AspNetCore.Mvc;
    using TestController.Models.Request;

    public partial class TestControllerEngine
    {
        private TrackData _selectedMagneticCard;
        private string _track1Data;
        private string _cardStatusText = "Card Removed";
        private readonly Dictionary<string, TrackData> _magneticCards = new Dictionary<string, TrackData>
        {
            { "NYL Employee 123", new TrackData {Track1 = "EMP123"} },
            { "NYL Employee 321", new TrackData {Track1 = "EMP321"} },
            { "NYL Player 456", new TrackData {Track1 = "PLY456"} },
            { "NYL Player 654", new TrackData {Track1 = "PLY654"} }
        };

        public Dictionary<string, TrackData> MagneticCards => _magneticCards;

        public TrackData SelectedMagneticCard
        {
            get => _selectedMagneticCard;

            set
            {
                if (_selectedMagneticCard == value)
                {
                    return;
                }

                _selectedMagneticCard = value;
                //RaisePropertyChanged(nameof(SelectedMagneticCard));
                Track1Data = SelectedMagneticCard.Track1;
            }
        }

        public string Track1Data
        {
            get => _track1Data;

            set
            {
                if (_track1Data == value)
                {
                    return;
                }

                _track1Data = value;
                //RaisePropertyChanged(nameof(Track1Data));
            }
        }

        public string CardStatusText
        {
            get => _cardStatusText;
            set
            {
                _cardStatusText = value;
                //RaisePropertyChanged(nameof(CardStatusText));
            }
        }

        //PMC::private FakeCardReaderEvent _cardReaderEvents = new List<Card

        [HttpPost]
        [Route("CardReaders/0/InsertCard")]
        public ActionResult InsertCard([FromBody] InsertCardRequest request)
        {
            string idName = request.CardName;
            //check for null or invalid string
            if (idName == null)
            {
                return Ok(new Dictionary<string, object>
                {
                    { "response-to", $"/CardReaders/0/InsertCard/{idName}" },
                    { "Command", "Value for Name is invalid" }
                });
            }

            //check existence of idCard with idName
            if (!MagneticCards.ContainsKey(idName))
            {
                return Ok(new Dictionary<string, object>
                {
                    { "response-to", $"/CardReaders/0/InsertCard/{idName}" },
                    { "Command", "Value for Name cannot be found" }
                });
            }

            TrackData tData = new TrackData { };

            MagneticCards.TryGetValue(idName, out tData);

            SelectedMagneticCard = tData;

            _eventBus.Publish(new FakeCardReaderEvent(0, SelectedMagneticCard.Track1, true));

            CardStatusText = $"{Track1Data} Inserted";

            return Ok(new Dictionary<string, object>
            {
                { "response-to", $"/CardReaders/0/InsertCard/{idName}" },
                { "Command", $"{Track1Data} Inserted" }
            });
        }

        [HttpPost]
        [Route("CardReaders/0/RemoveCard")]
        public ActionResult RemoveCard()
        {
            _eventBus.Publish(new FakeCardReaderEvent(0, string.Empty, false));
            CardStatusText = "Card Removed";

            return Ok(new Dictionary<string, object>
            {
                { "response-to", "/CardReaders/0/RemoveCard" },
                { "Command", $"{Track1Data} Removed" }
            });
        }

        [HttpPost]
        [Route("CardReaders/0/Disconnect")]
        public ActionResult CardReaderDisconnect()
        {
            var idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>().Adapters.FirstOrDefault();

            var data = new Dictionary<string, object>
            {
                { "response-to", "/CardReaders/0/Disconnect" },
                { "Command", "CardReaderDisconnect" }
            };

            if (idReader != null)
            {
                _eventBus.Publish(new DisconnectedEvent(idReader.IdReaderId));
            }
            else
            {
                data.Add("error", "no idReader found");
            }

            return Ok(data);
        }

        [HttpPost]
        [Route("CardReaders/0/Connect")]
        public ActionResult CardReaderConnect()
        {
            var idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>().Adapters.FirstOrDefault();

            var data = new Dictionary<string, object>
            {
                { "response-to", "/CardReaders/0/Connect" },
                { "Command", "CardReaderConnect" }
            };

            if (idReader != null)
            {
                _eventBus.Publish(new ConnectedEvent(idReader.IdReaderId));
            }
            else
            {
                data.Add("error", "no idReader found");
            }

            return Ok(data);
        }
    }
}