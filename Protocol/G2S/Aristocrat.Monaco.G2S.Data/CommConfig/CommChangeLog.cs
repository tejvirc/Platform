namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using Model;
    using Newtonsoft.Json;

    /// <summary>
    ///     Represents record from CommChangeLog data table.
    /// </summary>
    public class CommChangeLog : ConfigChangeLog
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommChangeLog" /> class.
        /// </summary>
        public CommChangeLog()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommChangeLog" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public CommChangeLog(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets the change data.
        /// </summary>
        /// <returns>Change commConfig request</returns>
        public ChangeCommConfigRequest GetChangeRequest()
        {
            return (ChangeCommConfigRequest)JsonConvert.DeserializeObject(ChangeData, typeof(ChangeCommConfigRequest));
        }
    }
}