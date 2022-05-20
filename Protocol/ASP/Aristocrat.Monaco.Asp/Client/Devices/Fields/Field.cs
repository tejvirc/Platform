namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    public abstract class Field : IField
    {
        private object _value;

        protected Field(IFieldPrototype field)
        {
            Prototype = field;
            _value = this.FromString(DefaultValue);
            var maskedValue = Masks != null && Masks.Count > 0 &&
                              Masks?.Where(
                                      x => x.MaskOperation == MaskOperation.Equal &&
                                           !string.IsNullOrEmpty(x.DataMemberName))
                                  .Count() == Masks?.Count;
            LoadSave = maskedValue ? new MaskedLoadSave(this) : new NormalLoadSave(this) as ILoadSave;
        }

        private ILoadSave LoadSave { get; }

        public string DefaultValue => Prototype.DefaultValue;

        public FieldType Type => Prototype.Type;

        public int SizeInBytes => Prototype.SizeInBytes;

        public int Id => Prototype.Id;

        public string Name => Prototype.Name;

        public string DataSourceName => Prototype.DataSourceName;
        public string DataMemberName => Prototype.DataMemberName;
        public IReadOnlyList<IMask> Masks => Prototype.Masks;

        public IDataSource DataSource
        {
            get => Prototype.DataSource;
            set => Prototype.DataSource = value;
        }

        public object Value
        {
            get => _value;
            set => SetValue(value);
        }

        public IFieldPrototype Prototype { get; }

        public virtual void Load()
        {
            LoadSave.Load();
        }

        public virtual void Save()
        {
            LoadSave.Save();
        }

        public abstract void WriteBytes(IByteArrayWriter writer);
        public abstract void ReadBytes(IByteArrayReader reader);

        protected virtual void SetValue(object value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return $"{Name}:{Value}";
        }

        private class NormalLoadSave : ILoadSave
        {
            private readonly IField _field;

            public NormalLoadSave(IField field)
            {
                _field = field;
            }

            public void Load()
            {
                _field.Value = _field.DataSource?.GetMemberValue(_field.DataMemberName) ?? _field.Value;
            }

            public void Save()
            {
                _field.DataSource?.SetMemberValue(_field.DataMemberName, _field.Value);
            }
        }

        private class MaskedLoadSave : ILoadSave
        {
            private readonly IField _field;

            public MaskedLoadSave(IField field)
            {
                _field = field;
            }

            public void Load()
            {
                _field.Value = _field.Masks.Aggregate(
                    0,
                    (total, next) => total |
                                     ((bool)(_field.DataSource?.GetMemberValue(next.DataMemberName) ?? false)
                                         ? next.Value
                                         : 0));
            }

            public void Save()
            {
                var value = Convert.ToInt32(_field.Value);
                foreach (var mask in _field.Masks)
                {
                    _field.DataSource?.SetMemberValue(mask.DataMemberName, (value & mask.Value) != 0);
                }
            }
        }
    }
}