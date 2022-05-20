namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using ChangeOptionConfig;
    using Model;
    using Newtonsoft.Json;

    /// <summary>
    ///     Represents record from OptionChangeLog data table.
    /// </summary>
    public class OptionChangeLog : ConfigChangeLog
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionChangeLog" /> class.
        /// </summary>
        public OptionChangeLog()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionChangeLog" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public OptionChangeLog(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets the change data.
        /// </summary>
        /// <returns>Change commConfig request</returns>
        public ChangeOptionConfigRequest GetChangeRequest()
        {
            return
                (ChangeOptionConfigRequest)JsonConvert.DeserializeObject(ChangeData, typeof(ChangeOptionConfigRequest));
        }
    }
}