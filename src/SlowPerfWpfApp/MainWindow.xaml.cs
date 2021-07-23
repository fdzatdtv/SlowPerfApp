using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SlowPerfLib;

namespace SlowPerfWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimeSpan _timeToLoad;
        private Stopwatch _sw;

        public MainWindow()
        {
            InitializeComponent();

            TableView1.ProgressChanged += TableViewProgressChanged;
            TableView1.HighlightedChanged += TableViewHighlightedChanged;
            TableView1.Filtered += TableView1Filtered;

            Loaded += MainWindowLoaded;
            Recent.RecentSelectionChanged += RecentOnRecentSelectionChanged;
        }

        private void TableViewHighlightedChanged(object sender, HighlightedEventArgs e)
        {
            DataRow datarow = TableView1.GetData(e.RowIndex);
            Details.DataContext = datarow;
        }

        private void RecentOnRecentSelectionChanged(object sender, EventArgs e)
        {
            if (Recent.SelectedItem != null)
            {
                Title = $"[{Recent.SelectedItem.Name}] Sales Report";
                Mouse.SetCursor(Cursors.Wait);
                TableView1.Filepath = Recent.SelectedItem.Path;
                Mouse.SetCursor(Cursors.None);
            }
            else
            {
                Title = "Sales Report";
                TableView1.Filepath = string.Empty;
            }
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            StatusTxt.Content = $"Rows: {TableView1.RowCount}. Elapsed: {_timeToLoad}s";
        }

        private void TableView1Filtered(object sender, FilteredEventArgs e)
        {
            if (e.IsActive)
                StatusTxt.Content = $"Rows: {e.FilteredRows} / {e.TotalRows}";
            else
                StatusTxt.Content = $"Rows: {e.TotalRows}";
        }

        private void TableViewProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.Action == ProgressAction.Start)
            {
                _sw = Stopwatch.StartNew();
                Progress1.Visibility = Visibility.Visible;
                Progress1.Value = 0;
                Progress1.Maximum = e.TotalRows;
                Progress1.Minimum = 0;
                StatusTxt.Content = $"{e.ProcessedRows} / {e.TotalRows}";
            }
            else if (e.Action == ProgressAction.InProgress)
            {
                Progress1.Value = e.ProcessedRows;
                StatusTxt.Content = $"{e.ProcessedRows} / {e.TotalRows}. Elapsed: {_sw.Elapsed}s";
            }
            else if (e.Action == ProgressAction.End)
            {
                _sw.Stop();
                _timeToLoad = _sw.Elapsed;
                Progress1.Visibility = Visibility.Collapsed;
                StatusTxt.Content = $"Rows: {TableView1.RowCount}. Elapsed: {_timeToLoad}s";
            }
        }
    }
}
