namespace Aristocrat.Monaco.Application.Contracts.Identification
{
    /// <summary>
    ///     Define operations related to employee login.
    /// </summary>
    public interface IEmployeeLogin
    {
        /// <summary>
        ///     New employee is logging in.
        /// </summary>
        /// <param name="identification">Employee identification</param>
        void Login(string identification);

        /// <summary>
        ///     Current employee is logging out.
        /// </summary>
        /// <param name="identification">Employee identification</param>
        void Logout(string identification);

        /// <summary>
        ///     Get whether an employee is logged in.
        /// </summary>
        bool IsLoggedIn { get; }
    }
}
