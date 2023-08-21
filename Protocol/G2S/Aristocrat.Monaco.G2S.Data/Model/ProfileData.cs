namespace Aristocrat.Monaco.G2S.Data.Model
{
    using Common.Storage;

    /// <summary>
    ///     Base class that represents serialized profile data.
    /// </summary>
    public class ProfileData : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the profile type full name.
        /// </summary>
        public string ProfileType { get; set; }

        /// <summary>
        ///     Gets or sets the device id.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the serialized profile entity (e.g. profile data).
        /// </summary>
        public string Data { get; set; }
    }
}