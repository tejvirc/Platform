namespace Aristocrat.Monaco.G2S.Data.Profile
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client.Devices;
    using Model;

    /// <summary>
    ///     Base interface for profile service.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        ///     Populates a profile if it exists
        /// </summary>
        /// <typeparam name="T">The device type</typeparam>
        /// <param name="profile">Profile instance to be populated.</param>
        void Populate<T>(T profile)
            where T : IDevice;

        /// <summary>
        ///     Checks for an existing device.
        /// </summary>
        /// <typeparam name="T">The device type</typeparam>
        /// <param name="profile">Profile instance to be populated.</param>
        /// <returns>true if the device exists.</returns>
        bool Exists<T>(T profile)
            where T : IDevice;

        /// <summary>
        ///     Gets the current list of device profiles.
        /// </summary>
        /// <returns>Returns a collection of profiles.</returns>
        IEnumerable<ProfileData> GetAll();

        /// <summary>
        ///     Saves a profile.
        /// </summary>
        /// <typeparam name="T">The device type</typeparam>
        /// <param name="profile">Profile instance.</param>
        void Save<T>(T profile)
            where T : IDevice;

        /// <summary>
        ///     Deletes a profile by its type and device id.
        /// </summary>
        /// <typeparam name="T">The device type</typeparam>
        /// <param name="profile">Profile instance.</param>
        void Delete<T>(T profile)
            where T : IDevice;
    }
}