namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using System.Collections.ObjectModel;

    public class PreviousRaceResultModel
    {
        public PreviousRaceResultModel(
            string name,
            string date,
            ObservableCollection<HorseModel> horseCollection)
        {
            Name = name;
            Date = date;
            HorseCollection = horseCollection;
        }

        /// <summary>
        /// The name of the racing venue
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The date the race took place
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// The horses that participated in the race
        /// </summary>
        public ObservableCollection<HorseModel> HorseCollection { get; set; }
    }
}
