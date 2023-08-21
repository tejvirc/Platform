namespace Aristocrat.Monaco.Hardware.Contracts.Ticket
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using log4net;
    using ProtoBuf;

    /// <summary>Provides access to the ticket object. </summary>
    [ProtoContract]
    public class Ticket
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Gets or sets the ticket data. For use by serialization.
        /// </summary>
        [ProtoMember(1)]
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        ///     Overrides square brackets to get or set the property value.
        /// </summary>
        /// <param name="ticketProperty">The ticket property. </param>
        /// <returns>A string of the ticketProperty</returns>
        [ProtoMember(2)]
        public string this[string ticketProperty]
        {
            get
            {
                if (ticketProperty == null)
                {
                    return null;
                }

                if (Data.ContainsKey(ticketProperty))
                {
                    return Data[ticketProperty]?.ToString();
                }

                Logger.Warn($"{ticketProperty} was not found.");

                return null;
            }

            set
            {
                if (Data.ContainsKey(ticketProperty))
                {
                    Data[ticketProperty] = value;
                }
                else
                {
                    Data.Add(ticketProperty, value);
                }
            }
        }

        /// <summary>
        ///     Adds fields to the ticket for each item in the provided data collection.
        /// </summary>
        /// <param name="fieldBase">The root field name</param>
        /// <param name="data">The collection of items for which to create fields</param>
        public void AddFields(string fieldBase, IEnumerable<object> data)
        {
            var count = 0;

            foreach (var entry in data)
            {
                this[fieldBase + "_" + count] = entry.ToString();
                ++count;
            }

            this[fieldBase + "_count"] = count.ToString();
        }

        /// <summary>
        ///     List the contents of the current ticket
        /// </summary>
        /// <returns>A string containing the ticket keys and respective contents, one per line</returns>
        /// <remarks>
        ///     Used mostly to create a new printed ticket from a logic one
        /// </remarks>
        public string AllFields()
        {
            return Data.Keys.Aggregate(string.Empty, (current, key) => current + $"{key} => {Data[key]}\n");
        }
    }
}