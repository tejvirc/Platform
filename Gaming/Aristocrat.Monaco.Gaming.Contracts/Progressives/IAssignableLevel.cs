namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Provides an interface for assignable progressive levels
    /// </summary>
    public interface IAssignableLevel
    {
        /// <summary>
        ///     Gets the level assignment key for this level
        /// </summary>
        string LevelAssignmentKey { get; }
    }
}