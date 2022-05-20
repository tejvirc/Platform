namespace Aristocrat.Monaco.Application.Identification
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts.Identification;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Implementation of <see cref="IEmployeeLogin"/>.
    /// </summary>
    public class EmployeeLoginService : IEmployeeLogin, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private string _loginId;

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IEmployeeLogin) };

        /// <inheritdoc />
        public bool IsLoggedIn => !string.IsNullOrEmpty(_loginId);

        /// <summary>
        ///     Construct a <see cref="EmployeeLoginService"/>.
        /// </summary>
        public EmployeeLoginService()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Construct a <see cref="EmployeeLoginService"/>.
        /// </summary>
        /// <param name="bus">Instance of <see cref="IEventBus"/>.</param>
        public EmployeeLoginService(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void Login(string identification)
        {
            if (!string.IsNullOrEmpty(_loginId))
            {
                return;
            }

            Logger.Debug($"Login {identification}");
            _loginId = identification;
            _bus.Publish(new EmployeeLoggedInEvent());
        }

        /// <inheritdoc />
        public void Logout(string identification)
        {
            if (_loginId != identification)
            {
                return;
            }

            Logger.Debug($"Logout {identification}");
            _loginId = null;
            _bus.Publish(new EmployeeLoggedOutEvent());
        }
    }
}
