namespace Aristocrat.Monaco.Application.UI.Settings
{
    using System;
    using Contracts.Localization;
    using Kernel;
    using MVVM.Model;

    /// <summary>
    ///    Base class for settings models with displayable properties
    /// </summary>
    [CLSCompliant(false)]
    public abstract class SettingsBase : BaseNotify
    {
        protected SettingsBase()
            : this(ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        private SettingsBase(IEventBus eventBus)
        {
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        protected IEventBus EventBus { get; private set; }

        protected ILocalizer OperatorLocalizer => Localizer.For(CultureFor.Operator);

        protected virtual void RefreshAllDisplayableSettings(OperatorCultureChangedEvent evt)
        {
            var properties = GetType().GetProperties();

            foreach (var prop in properties)
            {
                RaisePropertyChanged(prop.Name);
            }
        }
    }
}
