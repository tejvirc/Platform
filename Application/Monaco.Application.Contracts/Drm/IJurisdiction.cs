namespace Aristocrat.Monaco.Application.Contracts.Drm
{
    /// <summary>
    ///     Provides a mechanism to interact with the restricted jurisdiction information obtained from a protection module
    ///     such as a smart card
    /// </summary>
    public interface IJurisdiction
    {
        /// <summary>
        ///     Gets the jurisdiction identifier authorized by the license
        /// </summary>
        string JurisdictionId { get; }

        /// <summary>
        ///     Determines if the specified jurisdiction Id is authorized
        /// </summary>
        /// <param name="jurisdictionId"></param>
        /// <returns>true, if the jurisdiction is authorized according per the protection module</returns>
        bool IsAuthorized(string jurisdictionId);
    }
}