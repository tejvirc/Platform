namespace Aristocrat.Monaco.Gaming.Contracts.Lobby
{
    /// <summary>
    ///     Contract for opening and closing the lobby.
    /// </summary>
    public interface ILobby
    {
        /// <summary>
        ///     Creates the lobby window.
        /// </summary>
        void CreateWindow();

        /// <summary>
        ///     Shows the lobby.
        /// </summary>
        void Show();

        /// <summary>
        ///     Hides the lobby.
        /// </summary>
        void Hide();

        /// <summary>
        ///     Closes the lobby.
        /// </summary>
        void Close();
    }
}
