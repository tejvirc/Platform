namespace Aristocrat.Monaco.UI.Common.MVVM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    /// <summary>
    ///     Base class for view models that maintain "dirty" state (commit) and/or require validation
    /// </summary>
    /// <remarks>
    ///     This provides an inverted "is dirty" model using the <see cref="IsCommitted" /> flag. The entity
    ///     starts out as "non-committed" and exposes a commit command. The command is allowed if the
    ///     entity has non-committed changes and those changes pass validations, so you can bind your
    ///     submit buttons to the command directly,.and only if everything passes
    ///     will the command invoke <see cref="OnCommitted" /> for you to apply your changes.
    /// </remarks>
    [CLSCompliant(false)]
    public abstract class TrackableObservableValidator : ObservableValidator
    {
        private readonly IReadOnlyCollection<string> _untrackedPropertyNames;

        /// <summary>
        ///     True when changes have been committed
        /// </summary>
        private bool _committed = true;

        /// <summary>
        ///     This value will raise property changed when committed without errors
        /// </summary>
        [IgnoreTracking]
        public bool IsCommitted
        {
            get => _committed;
            protected set
            {
                _committed = value;
                CommitCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(IsCommitted));
            }
        }

        /// <summary>
        ///     Commit changes
        /// </summary>
        public RelayCommand<object> CommitCommand { get; }

        /// <inheritdoc />
        protected TrackableObservableValidator()
        {
            _untrackedPropertyNames = GetType().GetProperties()
                .Where(property => property.IsDefined(typeof(IgnoreTrackingAttribute), false))
                .Select(property => property.Name)
                .ToHashSet();

            // can only commit if no errors
            CommitCommand = new RelayCommand<object>(
                obj =>
                {
                    RunCustomValidation();
                    if (HasErrors)
                    {
                        return;
                    }
                    OnCommitted();
                    IsCommitted = true;
                },
                obj => !HasErrors && !IsCommitted
            );

            // any time a property changes that is not the committed flag, reset committed
            PropertyChanged += (o, e) =>
            {
                if (IsCommitted && !_untrackedPropertyNames.Contains(e.PropertyName))
                {
                    IsCommitted = false;
                    CommitCommand.NotifyCanExecuteChanged();
                }
            };
        }

        /// <summary>
        ///     Use this to perform whatever action is needed when committed
        /// </summary>
        protected virtual void OnCommitted()
        {
        }

        /// <summary>
        /// Use this to perform custom validation logic on specific properties
        /// </summary>
        protected virtual void RunCustomValidation()
        {

        }

        /// <summary>
        ///     Determines if a given property has associated errors.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the specified property has errors.</returns>
        protected bool PropertyHasErrors(string propertyName)
        {
            return GetErrors(propertyName) is IEnumerable<ValidationResult> errors && errors.Any();
        }

        /// <summary>
        ///     Raises this object's PropertyChanged event for each of the properties.
        /// </summary>
        /// <param name="propertyNames">The properties that have a new value.</param>
        protected void OnPropertyChanging(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                base.OnPropertyChanging(name);
            }
        }

        /// <summary>
        ///     Raises this object's PropertyChanged event for each of the properties.
        /// </summary>
        /// <param name="propertyNames">The properties that have a new value.</param>
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                base.OnPropertyChanged(name);
            }
        }

        /// <summary>
        ///     Sets the backing field value and raises the following events for each property listed:<br />
        ///     - OnPropertyChanging<br />
        ///     - OnPropertyChanged<br />
        /// </summary>
        /// <typeparam name="T">The type of the field being changed</typeparam>
        /// <param name="property">The backing field for the property</param>
        /// <param name="value">The new value to set</param>
        /// <param name="propertyNames">Optional array of dependent property names to emit property changed events for.</param>
        /// <returns>false if the new and existing values are equal, true if they are not</returns>
        protected bool SetProperty<T>(ref T property, T value, params string[] propertyNames)
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
            {
                return false;
            }

            OnPropertyChanging(propertyNames);
            property = value;
            OnPropertyChanged(propertyNames);

            return true;
        }
    }
}