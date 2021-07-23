using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SlowPerfLib;
using ProgressChangedEventArgs = SlowPerfLib.ProgressChangedEventArgs;

namespace SlowPerfWpfApp
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : UserControl
    {
        private CsvImporter<ArticleSale> _importer;
        private DataTable _tableData;
        private string _filepath;
        private ObservableCollection<DataRow> _itemsSource;

        /// <summary>
        /// Notifies about progress during <see cref="M:SlowPerfLib.CsvImporter.Import"/>
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public event EventHandler<HighlightedEventArgs> HighlightedChanged;
        public event EventHandler<FilteredEventArgs> Filtered;

        public string Filepath
        {
            get => _filepath;
            set
            {
                if (value != _filepath)
                {
                    _filepath = value;
                    _importer.Import(_filepath);
                }
            }
        }

        public int RowCount => _tableData?.Rows.Count ?? 0;

        public TableView()
        {
            InitializeComponent();

            _importer = new CsvImporter<ArticleSale>();
            _importer.ProgressChanged += ImporterProgressChanged;
            _tableData = _importer.Data.Tables["RecordsTable"];

            HookEvents();
        }

        public DataRow GetData(int rowIndex)
        {
            return _itemsSource.ElementAtOrDefault(rowIndex);
        }

        private void ImporterProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        private void TableView_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void TableDataRowChanged(object sender, DataRowChangeEventArgs e)
        {
            RefreshView();
        }
        private void TableDataRowDeleted(object sender, DataRowChangeEventArgs e)
        {
            RefreshView();
        }
        private void TableDataColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            RecalcTotals();
        }

        private void HookEvents()
        {
            Loaded += TableView_Loaded;

            _importer.Data.Tables.CollectionChanged += TablesChanged;
            _importer.DataChanged += ImporterDataChanged;
            if (_tableData != null)
            {
                _tableData.RowChanged += TableDataRowChanged;
                _tableData.RowDeleted += TableDataRowDeleted;
                _tableData.ColumnChanged += TableDataColumnChanged;
            }

            OuterTable.AddHandler(ScrollViewer.ScrollChangedEvent,new RoutedEventHandler(OuterTableScrollChanged));
            OuterTable.AddHandler(ScrollBar.ScrollEvent, new RoutedEventHandler(OuterTableScrollBarScrollChanged));
            Header.ColumnSortClicked += Header_ColumnSortClicked;
            AddHandler(FilterControl.FilterChangedEvent, new RoutedEventHandler(OnFilterChanged));
        }

        private void Header_ColumnSortClicked(object sender, ColumnSortEventArgs e)
        {
            if (e.Direction == SortDirection.Undefined)
                return;

            SortByColumn(e.Direction, e.ColumnName);
        }

        private string _currentFilterText;
        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            var control = e.OriginalSource as FilterControl;
            if (control == null)
                return;

            _currentFilterText = control.FilterText;
            RefreshView();
            Filtered?.Invoke(this, new FilteredEventArgs(_itemsSource.Count, _tableData.Rows.Count, !string.IsNullOrEmpty(_currentFilterText)));
        }

        private void OuterTableScrollBarScrollChanged(object sender, RoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer ?? e.OriginalSource as ScrollViewer;
            if (scrollViewer != null)
            {
                HeaderScroll.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
                FooterScroll.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
            }
        }

        private void OuterTableScrollChanged(object sender, RoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer ?? e.OriginalSource as ScrollViewer;
            if (scrollViewer != null)
            {
                HeaderScroll.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
                FooterScroll.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
            }
        }

        private void TablesChanged(object sender, CollectionChangeEventArgs e)
        {
            _tableData = _importer.Data.Tables["RecordsTable"];
            HookEvents();
            RefreshView();
        }

        private void ImporterDataChanged(object sender, EventArgs e)
        {
            _tableData = _importer.Data.Tables["RecordsTable"];
            HookEvents();
            RefreshView();
        }

        private void RefreshView()
        {
            if (_tableData != null)
            {
                var dataRows = new List<DataRow>();
                foreach (DataRow dataRow in _tableData.Rows)
                    dataRows.Add(dataRow);
                FilterData(dataRows);
                SortData(dataRows);
                if (_itemsSource == null)
                    _itemsSource =  new ObservableCollection<DataRow>(dataRows);
                else
                {
                    _itemsSource.Clear();
                    foreach (var dataRow in dataRows)
                        _itemsSource.Add(dataRow);
                }
            }
            else
            {
                _itemsSource = new ObservableCollection<DataRow>();
            }

            OuterTable.ItemsSource = _itemsSource;
        }

        private void RecalcTotals()
        {
            var valueBuffer = new double[17];
            var columnNames = new []
            {
                "Jan.",
                "Feb.",
                "Mär.",
                "Apr.",
                "Mai",
                "Jun.",
                "Jul.",
                "Aug.",
                "Sep.",
                "Okt.",
                "Nov.",
                "Dez.",
                "Quartal 1",
                "Quartal 2",
                "Quartal 3",
                "Quartal 4",
                "Gesamt",
            };

            for (int i = 0; i < columnNames.Length; i++)
            {
                string colName = columnNames[i];
                var columnValues = from datarow in _itemsSource
                                                    select (double)datarow[colName];
                valueBuffer[i] = columnValues.Sum();
            }

            Footer.ColumnTotals = valueBuffer;
        }

        private void ListviewItem_MouseMove(object sender, MouseEventArgs e)
        {
            HighlightedChanged?.Invoke(this, new HighlightedEventArgs(_itemsSource.IndexOf(((BodyRowView)sender).DataContext as DataRow)));
        }

        private void SortData(List<DataRow> dataRows)
        {
            if (string.IsNullOrEmpty(_currentSortColumnName))
                dataRows.Sort(CompareDataRow);
            else
            {
                if (_currentSortDirection == SortDirection.Ascending)
                    dataRows.Sort(CompareDataRowByColumnAsc);
                else
                    dataRows.Sort(CompareDataRowByColumnDesc);
            }
        }

        private int CompareDataRow(DataRow x, DataRow y)
        {
            int comp = CompareDataRowByTotalDesc(x, y);
            if (comp != 0)
                return comp;
            comp = CompareDataRowByQ4Desc(x, y);
            if (comp != 0)
                return comp;
            comp = CompareDataRowByQ3Desc(x, y);
            if (comp != 0)
                return comp;
            comp = CompareDataRowByQ2Desc(x, y);
            if (comp != 0)
                return comp;
            comp = CompareDataRowByQ1Desc(x, y);
            if (comp != 0)
                return comp;
            return CompareDataRowByNrAsc(x, y);
        }

        private int CompareDataRowByTotalDesc(DataRow x, DataRow y)
        {
            double? totalX = null, totalY = null;
            if (x.Table.Columns.Contains("Gesamt"))
            {
                try
                {
                    totalX = (double)x["Gesamt"];
                }
                catch (Exception)
                {
                    totalX = null;
                }
            }
            if (y.Table.Columns.Contains("Gesamt"))
            {
                try
                {
                    totalY = (double)y["Gesamt"];
                }
                catch (Exception)
                {
                    totalY = null;
                }
            }

            if (totalX != null && totalY != null)
            {
                if (totalY.Value < totalX.Value) return -1;
                if (totalY.Value > totalX.Value) return 1;
                return 0;
            }

            if (totalX == null && totalY != null)
                return -1;

            if (totalX != null && totalY == null)
                return 1;

            if (totalX == null && totalY == null)
                return 0;

            return 0;
        }
        private int CompareDataRowByQ4Desc(DataRow x, DataRow y)
        {
            double? saleX = null, saleY = null;
            if (x.Table.Columns.Contains("Quartal 4"))
            {
                try
                {
                    saleX = (double)x["Quartal 4"];
                }
                catch (Exception)
                {
                    saleX = null;
                }
            }
            if (y.Table.Columns.Contains("Quartal 4"))
            {
                try
                {
                    saleY = (double)y["Quartal 4"];
                }
                catch (Exception)
                {
                    saleY = null;
                }
            }

            if (saleX != null && saleY != null)
            {
                if (saleY.Value < saleX.Value) return -1;
                if (saleY.Value > saleX.Value) return 1;
                return 0;
            }

            if (saleX == null && saleY != null)
                return -1;

            if (saleX != null && saleY == null)
                return 1;

            if (saleX == null && saleY == null)
                return 0;

            return 0;
        }
        private int CompareDataRowByQ3Desc(DataRow x, DataRow y)
        {
            double? saleX = null, saleY = null;
            if (x.Table.Columns.Contains("Quartal 3"))
            {
                try
                {
                    saleX = (double)x["Quartal 3"];
                }
                catch (Exception)
                {
                    saleX = null;
                }
            }
            if (y.Table.Columns.Contains("Quartal 3"))
            {
                try
                {
                    saleY = (double)y["Quartal 3"];
                }
                catch (Exception)
                {
                    saleY = null;
                }
            }

            if (saleX != null && saleY != null)
            {
                if (saleY.Value < saleX.Value) return -1;
                if (saleY.Value > saleX.Value) return 1;
                return 0;
            }

            if (saleX == null && saleY != null)
                return -1;

            if (saleX != null && saleY == null)
                return 1;

            if (saleX == null && saleY == null)
                return 0;

            return 0;
        }
        private int CompareDataRowByQ2Desc(DataRow x, DataRow y)
        {
            double? saleX = null, saleY = null;
            if (x.Table.Columns.Contains("Quartal 2"))
            {
                try
                {
                    saleX = (double)x["Quartal 2"];
                }
                catch (Exception)
                {
                    saleX = null;
                }
            }
            if (y.Table.Columns.Contains("Quartal 2"))
            {
                try
                {
                    saleY = (double)y["Quartal 2"];
                }
                catch (Exception)
                {
                    saleY = null;
                }
            }

            if (saleX != null && saleY != null)
            {
                if (saleY.Value < saleX.Value) return -1;
                if (saleY.Value > saleX.Value) return 1;
                return 0;
            }

            if (saleX == null && saleY != null)
                return -1;

            if (saleX != null && saleY == null)
                return 1;

            if (saleX == null && saleY == null)
                return 0;

            return 0;
        }
        private int CompareDataRowByQ1Desc(DataRow x, DataRow y)
        {
            double? saleX = null, saleY = null;
            if (x.Table.Columns.Contains("Quartal 1"))
            {
                try
                {
                    saleX = (double)x["Quartal 1"];
                }
                catch (Exception)
                {
                    saleX = null;
                }
            }
            if (y.Table.Columns.Contains("Quartal 1"))
            {
                try
                {
                    saleY = (double)y["Quartal 1"];
                }
                catch (Exception)
                {
                    saleY = null;
                }
            }

            if (saleX != null && saleY != null)
            {
                if (saleY.Value < saleX.Value) return -1;
                if (saleY.Value > saleX.Value) return 1;
                return 0;
            }

            if (saleX == null && saleY != null)
                return -1;

            if (saleX != null && saleY == null)
                return 1;

            if (saleX == null && saleY == null)
                return 0;

            return 0;
        }
        private int CompareDataRowByNrAsc(DataRow x, DataRow y)
        {
            int? nrX = null, nrY = null;
            if (x.Table.Columns.Contains("Nr"))
            {
                try
                {
                    nrX = (int)x["Nr"];
                }
                catch (Exception)
                {
                    nrX = null;
                }
            }
            if (y.Table.Columns.Contains("Nr"))
            {
                try
                {
                    nrY = (int)y["Nr"];
                }
                catch (Exception)
                {
                    nrY = null;
                }
            }

            if (nrX != null && nrY != null)
            {
                if (nrY.Value > nrX.Value) return -1;
                if (nrY.Value < nrX.Value) return 1;
                return 0;
            }

            if (nrX == null && nrY != null)
                return -1;

            if (nrX != null && nrY == null)
                return 1;

            if (nrX == null && nrY == null)
                return 0;

            return 0;
        }

        private string _currentSortColumnName;
        private SortDirection _currentSortDirection;
        private void SortByColumn(SortDirection direction, string columnName)
        {
            _currentSortDirection = direction;
            _currentSortColumnName = direction == SortDirection.Undefined ? null : columnName;
            RefreshView();
        }

        private int CompareDataRowByColumnAsc(DataRow x, DataRow y)
        {
            if (string.IsNullOrEmpty(_currentSortColumnName))
                return 0;
            object valueX = x[_currentSortColumnName];
            object valueY = y[_currentSortColumnName];
            bool isComparable = valueX is IComparable;
            if (isComparable)
            {
                var comparableX = valueX as IComparable;
                return comparableX.CompareTo(valueY);
            }
            if (valueY is IComparable)
            {
                var comparableY = valueY as IComparable;
                return -1 * comparableY.CompareTo(valueX);
            }
            return string.Compare(valueX as string ?? valueX.ToString(), valueY as string ?? valueY.ToString());
        }

        private int CompareDataRowByColumnDesc(DataRow x, DataRow y)
        {
            return CompareDataRowByColumnAsc(y, x);
        }

        private void FilterData(List<DataRow> dataRows)
        {
            if (string.IsNullOrEmpty(_currentFilterText))
                return;

            var query = from datarow in dataRows
                from value in datarow.ItemArray
                where value != null
                where (value.ToString().ToUpper().StartsWith(_currentFilterText.ToUpper())
                    || value.ToString().ToUpper().Contains(_currentFilterText.ToUpper()))
                select datarow;

            var filtered = query.ToList();
            dataRows.Clear();
            dataRows.AddRange(filtered);
        }
    }

    public class FilteredEventArgs : EventArgs
    {
        public int FilteredRows { get; }
        public int TotalRows { get; }
        public bool IsActive { get; }

        public FilteredEventArgs(int filteredRows, int totalRows, bool isActive)
        {
            FilteredRows = filteredRows;
            TotalRows = totalRows;
            IsActive = isActive;
        }
    }

    public class HighlightedEventArgs : EventArgs
    {
        public int RowIndex { get; }

        public HighlightedEventArgs(int rowIndex)
        {
            RowIndex = rowIndex;
        }
    }
}
