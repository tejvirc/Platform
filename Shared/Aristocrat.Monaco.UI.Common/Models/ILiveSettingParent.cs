namespace Aristocrat.Monaco.UI.Common.Models
{
    /// <summary>
    /// The parent viewmodel of a <see cref="LiveSetting{TValue}"/>.
    /// </summary>
    public interface ILiveSettingParent
    {
        /// <summary>
        /// Is user input enabled for the live settings in this viewmodel (legacy)?
        /// </summary>
        bool IsInputEnabled { get; }
    }
}
