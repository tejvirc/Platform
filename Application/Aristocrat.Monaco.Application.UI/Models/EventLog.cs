namespace Aristocrat.Monaco.Application.UI.Models
{
    using System.Linq;
    using Contracts.TiltLogger;

    public class EventLog
    {
        public EventLog(EventDescription description)
        {
            Description = description;
        }

        public EventDescription Description { get; }

        public bool HasAdditionalInfo => Description.AdditionalInfos != null && Description.AdditionalInfos.Any();
    }
}
