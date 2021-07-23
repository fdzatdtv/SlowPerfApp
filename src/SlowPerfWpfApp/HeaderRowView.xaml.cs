using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace SlowPerfWpfApp
{
    /// <summary>
    /// Interaction logic for HeaderRowView.xaml
    /// </summary>
    public partial class HeaderRowView : UserControl
    {
        public event EventHandler<ColumnSortEventArgs> ColumnSortClicked;

        public HeaderRowView()
        {
            InitializeComponent();

            AttachClickHandler();
        }

        private void AttachClickHandler()
        {
            foreach (var label in ColumnsPanel.Children.OfType<Label>())
            {
                label.MouseLeftButtonUp += OnColumnClick;
                label.Tag = SortDirection.Undefined;
            }
        }

        private Label _lastLabelSort;
        private void OnColumnClick(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label) sender;
            SortDirection direction = (SortDirection) label.Tag;
            SortDirection newDirection = direction == SortDirection.Undefined
                ? SortDirection.Ascending
                : 3 - direction;
            label.Tag = newDirection;
            SetLabelDirection(label, newDirection);
            if (_lastLabelSort != label)
            {
                SetLabelDirection(_lastLabelSort, SortDirection.Undefined);
                _lastLabelSort = label;
            }
            string columnName = GetColumnNameFromLabel(label);
            ColumnSortClicked?.Invoke(this, new ColumnSortEventArgs(newDirection, columnName));
        }

        private static void SetLabelDirection(Label label, SortDirection direction)
        {
            if (label == null)
                return;

            string text = (string) label.Content;
            if (text.StartsWith("˄") || text.StartsWith("˅"))
            {
                if (direction == SortDirection.Ascending)
                    label.Content = "˄" + ((string) label.Content).Substring(1);
                else if (direction == SortDirection.Descending)
                    label.Content = "˅" + ((string) label.Content).Substring(1);
                else
                    label.Content = ((string) label.Content).Substring(1);
            }
            else
            {
                if (direction == SortDirection.Ascending)
                    label.Content = "˄" + (string) label.Content;
                else if (direction == SortDirection.Descending)
                    label.Content = "˅" + (string) label.Content;
            }
        }

        private string GetColumnNameFromLabel(Label label)
        {
            bool found = false;
            int index = -1;
            foreach (var control in ColumnsPanel.Children.OfType<Label>())
            {
                index++;
                if (control == label)
                {
                    found = true;
                    break;
                }
            }

            if (index == -1 || !found)
                return string.Empty;

            var columnNames = new []
            {
                "Artikel",
                "Nr",
                "Größe",
                "Jahr",
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
            return columnNames[index];
        }
    }

    public class ColumnSortEventArgs : EventArgs
    {
        public SortDirection Direction { get; }
        public string ColumnName { get; }

        public ColumnSortEventArgs(SortDirection direction, string columnName)
        {
            Direction = direction;
            ColumnName = columnName;
        }
    }

    public enum SortDirection
    {
        Undefined,
        Ascending,
        Descending,
    }
}
