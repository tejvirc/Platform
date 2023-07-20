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
    ///     This provides an inverted "is dirty" model using the <see cref="Committed" /> flag. The entity
    ///     starts out as "non-committed" and exposes a commit command. The command is allowed if the
    ///     entity has non-committed changes and those changes pass validations, so you can bind your
    ///     submit buttons to the command directly. It will call <see cref="ValidateAll" /> prior to
    ///     making any commitments so you can hook into final validation, and only if everything passes
    ///     will the command invoke <see cref="OnCommitted" /> for you to apply your changes.
    /// </remarks>
    [CLSCompliant(false)]
    public abstract class TrackableObservableValidator : ObservableValidator
    {
        private readonly IReadOnlyCollection<string> _propertiesToIgnoreForCommitted;

        /// <summary>
        ///     True when changes have been committed
        /// </summary>
        private bool _committed = true;

        /// <summary>
        ///     This value will raise property changed when committed without errors
        /// </summary>
        [IgnoreTracking]
        public bool Committed
        {
            get => _committed;
            protected set
            {
                _committed = value;
                CommitCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(Committed));
            }
        }

        /// <summary>
        ///     Commit changes
        /// </summary>
        public RelayCommand<object> CommitCommand { get; }

        /// <inheritdoc />
        protected TrackableObservableValidator()
        {
            _propertiesToIgnoreForCommitted = GetType().GetProperties()
                .Where(property => property.IsDefined(typeof(IgnoreTrackingAttribute), false))
                .Select(property => property.Name)
                .ToHashSet();

            // can only commit if no errors
            CommitCommand = new RelayCommand<object>(
                obj =>
                {
                    ValidateAll();
                    if (HasErrors)
                    {
                        return;
                    }
                    OnCommitted();
                    Committed = true;
                },
                obj => !HasErrors && !Committed);

            // any time a property changes that is not the committed flag, reset committed
            PropertyChanged += (o, e) =>
            {
                if (Committed && !_propertiesToIgnoreForCommitted.Contains(e.PropertyName))
                {
                    Committed = false;
                    CommitCommand.NotifyCanExecuteChanged();
                }
            };
        }

        /// <summary>
        ///     Use this to validate any late-bound fields before committing
        /// </summary>
        protected virtual void ValidateAll()
        {
            ValidateAllProperties();
        }

        /// <summary>
        ///     Use this to perform whatever action is needed when committed
        /// </summary>
        protected virtual void OnCommitted()
        {
        }

        /// <summary>
        ///     Raises this object's ErrorsChangedChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has new errors.</param>
        protected virtual void RaiseErrorsChanged(string propertyName)
        {
            CommitCommand.NotifyCanExecuteChanged();
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
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                base.OnPropertyChanged(name);
            }
        }

        /// <summary>
        ///     Sets the backing field value and raises the PropertyChanged 
        ///     event for each property name listed. 
        /// </summary>
        /// <typeparam name="T">The type of the field being changed</typeparam>
        /// <param name="property">The backing field for the property</param>
        /// <param name="value">The new value to set</param>
        /// <param name="propertyNames">Optional list of names to send in PropertyChanged events</param>
        /// <returns>false if the new and existing values are equal, true if they are not</returns>
        protected virtual bool SetProperty<T>(ref T property, T value, params string[] propertyNames)
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
            {
                return false;
            }

            property = value;
            OnPropertyChanged(propertyNames);

            return true;
        }
    }
}