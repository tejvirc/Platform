namespace Aristocrat.Monaco.TestController
{
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.IdReader;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.TestController.Models.Request;
    using DataModel;
    using Hardware.Contracts.CardReader;
    using Hardware.Contracts.IO;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;

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
        public ActionResult<CommandResult> InsertCard([FromBody] InsertCardRequest request)
        {
            string idName = request.CardName;
            //check for null or invalid string
            if (idName == null)
            {
                return new CommandResult
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = $"/CardReaders/0/InsertCard/{idName}"
                    },
                    Command = $"Value for Name is invalid",
                    Result = false
                };
            }

            //check existence of idCard with idName
            if (!MagneticCards.ContainsKey(idName))
            {
                return new CommandResult
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = $"/CardReaders/0/InsertCard/{idName}"
                    },
                    Command = $"Value for Name is cannot be founc",
                    Result = false
                };
            }

            TrackData tData = new TrackData { };

            MagneticCards.TryGetValue(idName, out tData);

            SelectedMagneticCard = tData;

            _eventBus.Publish(new FakeCardReaderEvent(0, SelectedMagneticCard.Track1, true));

            CardStatusText = $"{Track1Data} Inserted";


            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/CardReaders/0/InsertCard/{idName}"
                },
                Command = $"{Track1Data} Inserted",
                Result = true

            };
        }

        [HttpPost]
        [Route("CardReaders/0/RemoveCard")]
        public ActionResult<CommandResult> RemoveCard()
        {
            _eventBus.Publish(new FakeCardReaderEvent(0, string.Empty, false));
            CardStatusText = "Card Removed";

            return new CommandResult {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/CardReaders/0/RemoveCard"
                },
                Command = $"{Track1Data} Removed",  Result = true };
        }

        [HttpPost]
        [Route("CardReaders/0/Disconnect")]
        public ActionResult<CommandResult> CardReaderDisconnect()
        {
            var idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>().Adapters.FirstOrDefault();
            
            var response = new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/CardReaders/0/Disconnect" },
                Command = "CardReaderDisconnect"
            };

            if (idReader != null)
            {
                _eventBus.Publish(new DisconnectedEvent(idReader.IdReaderId));
                response.Result = true;
            }
            else
            {
                response.data.Add("error", "no idReader found");
                response.Result = false;
            }

            return response;
        }

        [HttpPost]
        [Route("CardReaders/0/Connect")]
        public ActionResult<CommandResult> CardReaderConnect()
        {
            var idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>().Adapters.FirstOrDefault();

            var response = new CommandResult()
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/CardReaders/0/Connect" },
                Command = "CardReaderConnect"
            };

            if (idReader != null)
            {
                _eventBus.Publish(new ConnectedEvent(idReader.IdReaderId));
                response.Result = true;
            }
            else
            {
                response.data.Add("error", "no idReader found");
                response.Result = false;
            }

            return response;
        }
    }
}
