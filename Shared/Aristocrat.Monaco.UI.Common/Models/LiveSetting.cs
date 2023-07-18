namespace Aristocrat.Monaco.UI.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Monaco.Common;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Quartz.Util;

    /// <see cref="LiveSetting{TValue}"/>
    [CLSCompliant(false)]
    public abstract class LiveSetting : ObservableObject
    {
        /// <summary>
        /// Modes for echoing a setting's live value to its edited value.
        /// </summary>
        public enum EchoMode
        {
            /// <summary>Always echoes live -> edited (replacing any edits).</summary>
            AlwaysLive,

            /// <summary>Echoes live -> edited if not focused or edited.</summary>
            LiveUntilEdited,
        }

        /// <summary>
        /// A setting's current live/edited status.
        /// </summary>
        public enum EditStatus
        {
            /// <summary>The current live and edited values are equal.</summary>
            Unedited,

            /// <summary>The edited value has changed such that it differs from the live value.</summary>
            Edited,

            /// <summary>The edited value changed, then the live value changed.</summary>
            Conflicted,
        }
    }

    /// <summary>
    /// A generic setting that can be edited by the user while receiving live updates from
    /// its underlying source (e.g. a property manager or remote host).
    /// </summary>
    /// <remarks>
    /// General usage:
    ///     - setting<->UI:     - Bind <see cref="EditedValue"/> to your UI element.
    ///                         - Optionally set/clear <see cref="IsFocused"/> to stop live updates while editing,
    ///                           and to <see cref="Status"/> to indicate live/edit conflicts.
    ///     - model->setting:   - Update <see cref="LiveValue"/> from its underlying source in real-time (e.g. a property manager or remote host).
    ///                           The new value is conditionally echoed to <see cref="EditedValue"/> based on the selected <see cref="Mode"/>
    ///                           and current focus/edit state.
    ///     - setting->model:   - Save <see cref="EditedValue"/> to the underlying source.
    ///                         - Call <see cref="IsDirty"/> to determine whether the value has been edited.
    /// </remarks>
    /// <typeparam name="TValue">The live and edited values' type, typically the same type as the underlying source.</typeparam>
    [CLSCompliant(false)]
    public class LiveSetting<TValue> : LiveSetting, IDataErrorInfo
    {
        private TValue _editedValue;
        private TValue _liveValue;
        private bool _echoing = true;
        private bool _focused, _isVisible = true, _isReadOnly;
        private bool _conflicted; // latches true when live value changes while focused/editing
        private string _errorFromView, _error;
        private IEnumerable<string> _validationErrors;

        /// <summary>
        /// Constructs a live setting.
        /// </summary>
        /// <param name="parent">Optionally a parent viewmodel, for validation, enablement, etc.</param>
        /// <param name="name">Optionally a name, for validation.</param>
        public LiveSetting(ILiveSettingParent parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        /// <summary>
        /// This live setting's parent VM.
        /// </summary>
        public ILiveSettingParent Parent { get; }

        /// <summary>
        /// The name of this live setting property in its parent VM.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The echo mode: see <see cref="LiveSetting.EchoMode"/>.
        /// </summary>
        public EchoMode Mode { get; set; } = EchoMode.LiveUntilEdited;

        /// <summary>
        /// Is this setting "quiet", i.e. should its normal decorations ("edited" marker, tooltip, etc.) be hidden?
        /// </summary>
        public bool IsQuiet { get; set; }

        /// <summary>
        /// An optional callback invoked on an attempted edit.
        /// </summary>
        /// <param name="setting">This setting.</param>
        /// <param name="newValue">The attempted new edited value.</param>
        /// <returns>The (possibly modified) actual edited value.</returns>
        public delegate TValue OnEditingDelegate(LiveSetting<TValue> setting, TValue newValue);

        /// <summary>
        /// An optional callback invoked on an attempted edit.
        /// </summary>
        public OnEditingDelegate OnEditing { get; set; }

        /// <summary>
        /// An optional callback invoked after the live or edited value has changed, but before <see cref="INotifyPropertyChanged.PropertyChanged"/>,
        /// typically for applying post-change side effects.
        /// </summary>
        public Action<LiveSetting<TValue>> OnChanged { get; set; }

        /// <summary>
        /// The latest "live" value received from this setting's underlying source.
        /// </summary>
        public TValue LiveValue
        {
            get => _liveValue;
            set
            {
                // set live value; echo to edited as appropriate
                if (!_liveValue.AreEqualOrNull(value))
                {
                    _liveValue = value;
                    Update(true);
                }
            }
        }

        /// <summary>
        /// The value displayed and edited, typically bound to the UI.
        /// </summary>
        public TValue EditedValue
        {
            get => _editedValue;
            set
            {
                // apply pre-edit effects
                if (OnEditing != null)
                {
                    value = OnEditing(this, value);
                }

                // set edited value; disable live->edited echo as appropriate
                if (!_editedValue.AreEqualOrNull(value))
                {
                    _editedValue = value;
                    switch (Mode)
                    {
                        case EchoMode.AlwaysLive:
                            break;
                        case EchoMode.LiveUntilEdited:
                            _echoing = false;
                            break;
                    }
                    Update(false);
                }
            }
        }

        /// <summary>
        /// Is this setting "focused"? This property allows UI elements to temporary disable live->edit echo,
        /// e.g. when the bound UI element is focused but the edited value has not yet changed.
        /// </summary>
        public bool IsFocused
        {
            get => _focused;
            set
            {
                // set focus; when focus is lost, echo live changes (if any) received during focus
                if (_focused != value)
                {
                    _focused = value;
                    Update(false);
                }
            }
        }

        /// <summary>
        /// Should this setting's editor be visible?
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged(nameof(IsVisible));
            }
        }

        /// <summary>
        /// Should this setting's editor be read-only?
        /// </summary>
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                RaisePropertyChanged(nameof(IsReadOnly));
            }
        }

        /// <summary>
        /// The error (if any) for this setting reported by the view
        /// (legacy support for existing controls and ObservableObject-derived VMs).
        /// </summary>
        public string ErrorFromView
        {
            set
            {
                if (_errorFromView != value)
                {
                    _errorFromView = value;
                    _error = null;
                    RaisePropertyChanged(nameof(EditedValue), nameof(Error));
                }
            }
        }

        /// <summary>
        /// Validation errors for this setting (null/empty list and/or elements are ignored).
        /// </summary>
        public IEnumerable<string> ValidationErrors
        {
            set
            {
                // notify even if errors haven't changed, to workaround validation system
                _validationErrors = value;
                _error = null;
                RaisePropertyChanged(nameof(EditedValue), nameof(Error));
            }
        }

        /// <summary>
        /// The (lazily computed) aggregate error for this live setting.
        /// </summary>
        /// <returns><see cref="ValidationErrors"/> if any exist, else <see cref="ErrorFromView"/> if exists, else "".</returns>
        public string Error
        {
            get
            {
                if (_error == null)
                {
                    var errors = _validationErrors?.Where(e => !e.IsNullOrWhiteSpace()).ToArray() ?? new string[0];
                    errors = errors.Any() ? errors : new[] { _errorFromView };
                    _error = string.Join(",", errors);
                }
                return _error;
            }
        }

        /// <summary>
        /// The (lazily computed) aggregate error for the given property.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName] => Error;

        /// <summary>
        /// The current live/edited status.
        /// </summary>
        public EditStatus Status =>
            IsConflicted ? EditStatus.Conflicted
                : IsDirty ? EditStatus.Edited
                : EditStatus.Unedited;

        /// <summary>
        /// Resets this setting to its original defaults, preserving its echo mode, visibility and read-only state.
        /// </summary>
        public void Reset()
        {
            _liveValue = _editedValue = default;
            _errorFromView = null;
            _validationErrors = null;
            _conflicted = false;
            _echoing = true;
            _focused = false;
            Update(false);
        }

        /// <summary>
        /// Does the edited value currently differ from the live value, and thus should be saved back to the underlying source?
        /// </summary>
        public bool IsDirty => !LiveValue.AreEqualOrNull(EditedValue);

        /// <summary>
        /// Is the live value still being echoed to the edited value?
        /// </summary>
        private bool IsEchoing => _echoing && !_focused;

        /// <summary>
        /// Do the live and edited values properly conflict,
        /// i.e. has the live value changed since the setting was edited, and are the live and actual values currently different?
        /// </summary>
        private bool IsConflicted => IsDirty && _conflicted;

        /// <summary>
        /// Updates the setting's edge-triggered state and notifies listeners.
        /// </summary>
        /// <param name="liveChanged">True if called due to a change to the live value.</param>
        private void Update(bool liveChanged)
        {
            // handle echo and conflicts
            if (IsEchoing)
            {
                _editedValue = _liveValue;
                _conflicted = false;
            }
            else if (liveChanged)
            {
                _conflicted = IsDirty;
            }

            // notify
            _error = null;
            OnChanged?.Invoke(this);
            RaisePropertyChanged(nameof(LiveValue), nameof(EditedValue), nameof(Status), nameof(Error));
        }
    }
}
