namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using System.Collections.ObjectModel;

    public class RaceInfo
    {
        /// <summary>
        /// The horse numbers to pick from
        /// </summary>
        public ObservableCollection<HorseModel> Horses { get; set; }

        /// <summary>
        /// Where the picked horse numbers get set
        /// </summary>
        public ObservableCollection<HorsePositionModel> HorsePositionPicks { get; set; }
    }
}
