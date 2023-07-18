namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using Contracts.Central;
    using Kernel;
    using Models;
    using Monaco.UI.Common.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///     ViewModel for VoucherInHistory.
    /// </summary>
    [CLSCompliant(false)]
    public class CentralDeterminationLogViewModel : OperatorMenuPageViewModelBase
    {
        private CentralTransactionData _selectedRow;

        /// <summary>
        ///     The entire set of persisted bill events in reverse order
        /// </summary>
        private ObservableCollection<CentralTransactionData> _transactionData = new ObservableCollection<CentralTransactionData>();

        public CentralTransactionData SelectedRow
        {
            get => _selectedRow;

            set
            {
                if (_selectedRow != value)
                {
                    _selectedRow = value;
                    OnPropertyChanged("SelectedRow");
                }
            }
        }

        /// <summary>
        ///     Gets the data to show in the data grid.
        /// </summary>
        public ObservableCollection<CentralTransactionData> TransactionData
        {
            get => _transactionData;
            set
            {
                if (_transactionData != value)
                {
                    _transactionData = value;
                    OnPropertyChanged(nameof(TransactionData));
                }
            }
        }

        public override bool DataEmpty => (TransactionData?.Count ?? 0) == 0;

        protected override void InitializeData()
        {
            var provider = ServiceManager.GetInstance().GetService<ICentralProvider>();
            if (provider != null)
            {
                Execute.OnUIThread(
                    () =>
                    {
                        TransactionData.Clear();
                        var dataList = new List<CentralTransactionData>();
                        foreach (var transaction in provider.Transactions.OrderBy(t => t.TransactionId))
                        {
                            dataList.Insert(0, new CentralTransactionData(transaction));
                        }

                        TransactionData.AddRange(dataList);
                    });
            }
        }
    }
}
