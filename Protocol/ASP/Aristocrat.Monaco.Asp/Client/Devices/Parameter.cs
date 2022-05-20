namespace Aristocrat.Monaco.Asp.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Contracts;
    using Fields;

    public class Parameter : IParameter
    {
        private readonly List<Field> _fields;

        public Parameter(IParameterPrototype prototype)
        {
            Prototype = prototype;
            _fields = FieldsPrototype.Select(FieldFactory.CreateField).Cast<Field>().ToList();
        }

        public INamedId ClassId => Prototype?.ClassId;

        public INamedId TypeId => Prototype?.TypeId;

        public IReadOnlyList<IFieldPrototype> FieldsPrototype => Prototype?.FieldsPrototype;

        public AccessType EgmAccessType => Prototype.EgmAccessType;

        public EventAccessType EventAccessType => Prototype.EventAccessType;

        public AccessType MciAccessType => Prototype.MciAccessType;

        public int Id => Prototype.Id;

        public string Name => Prototype.Name;

        public IReadOnlyList<IField> Fields => _fields.Cast<IField>().ToList();

        public int SizeInBytes => Prototype.SizeInBytes;

        public IParameterPrototype Prototype { get; }

        public void ReadBytes(IByteArrayReader reader)
        {
            foreach (var field in _fields)
            {
                field.ReadBytes(reader);
            }
        }

        public void WriteBytes(IByteArrayWriter writer)
        {
            foreach (var field in _fields)
            {
                field.WriteBytes(writer);
            }
        }

        public void Load()
        {
            ICollection<IParameterLoadActions> distinctParameterPreLoadConditionExecutors = _fields
                .Select(x => x.DataSource).Distinct()
                .Where(x => x is IParameterLoadActions).Cast<IParameterLoadActions>().ToList();

            foreach (var executor in distinctParameterPreLoadConditionExecutors)
            {
                executor.PreLoad();
            }

            foreach (var field in _fields)
            {
                field.Load();
            }
        }

        public void Save()
        {
            ICollection<ITransaction> distinctTransactions = _fields.Select(x => x.DataSource).Distinct()
                .Where(x => x is ITransaction).Cast<ITransaction>().ToList();

            //Build list of data members that will be updated in this transaction
            var dataMemberNames = _fields.Select(s => s.DataMemberName).ToList();

            using (var transaction = new TransactionWrapper(distinctTransactions, dataMemberNames))
            {
                foreach (var field in _fields)
                {
                    field.Save();
                }

                transaction.Commit();
            }
        }

        public void SetFields(Dictionary<string, object> members)
        {
            foreach (var field in _fields)
            {
                if (!members.ContainsKey(field.DataMemberName))
                {
                    throw new MissingDataMembersException($"Failed to set {field.DataMemberName}, value was not found");
                }

                field.Value = members[field.DataMemberName];
            }
        }

        public override string ToString()
        {
            return $"Name:{Name}, Fields:[{string.Join(",", Fields)}]";
        }

        private sealed class TransactionWrapper : IDisposable
        {
            private readonly ICollection<ITransaction> _transactions;

            public TransactionWrapper(ICollection<ITransaction> transactions, IReadOnlyList<string> dataMemberNames)
            {
                _transactions = transactions;

                if (dataMemberNames == null || !dataMemberNames.Any()) throw new MissingDataMembersException($"{dataMemberNames} cannot be null or empty");

                foreach (var transaction in _transactions)
                {
                    transaction.Begin(dataMemberNames);
                }
            }

            public void Dispose()
            {
                foreach (var transaction in _transactions)
                {
                    transaction.RollBack();
                }
            }

            public void Commit()
            {
                foreach (var transaction in _transactions)
                {
                    transaction.Commit();
                }

                _transactions.Clear();
            }
        }

        [Serializable]
        public class MissingDataMembersException : Exception
        {
            public MissingDataMembersException() { }

            public MissingDataMembersException(string message) : base(message) { }

            public MissingDataMembersException(string message, Exception innerException) : base(message, innerException) { }

            protected MissingDataMembersException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }
}