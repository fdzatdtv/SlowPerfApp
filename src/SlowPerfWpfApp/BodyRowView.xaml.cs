using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SlowPerfWpfApp
{
    /// <summary>
    /// Interaction logic for BodyRowView.xaml
    /// </summary>
    public partial class BodyRowView : UserControl
    {
        public BodyRowView()
        {
            InitializeComponent();

            DataContextChanged += WhenDataContextChanged;
        }

        private void WhenDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dataRow = e.NewValue as DataRow;

            FillRow(dataRow);
        }

        private void FillRow(DataRow dataRow)
        {
            lblArticle.Content = dataRow["Artikel"]?.ToString();
            lblNr.Content = dataRow["Nr"]?.ToString();
            lblSize.Content = dataRow["Größe"] != DBNull.Value ? ((ArticleSize)dataRow["Größe"]).ToString() : string.Empty;
            lblYear.Content = dataRow["Jahr"]?.ToString();
            SetValueAndColor(lblSaleM01, dataRow, "Jan.");
            SetValueAndColor(lblSaleM02, dataRow, "Feb.");
            SetValueAndColor(lblSaleM03, dataRow, "Mär.");
            SetValueAndColor(lblSaleM04, dataRow, "Apr.");
            SetValueAndColor(lblSaleM05, dataRow, "Mai");
            SetValueAndColor(lblSaleM06, dataRow, "Jun.");
            SetValueAndColor(lblSaleM07, dataRow, "Jul.");
            SetValueAndColor(lblSaleM08, dataRow, "Aug.");
            SetValueAndColor(lblSaleM09, dataRow, "Sep.");
            SetValueAndColor(lblSaleM10, dataRow, "Okt.");
            SetValueAndColor(lblSaleM11, dataRow, "Nov.");
            SetValueAndColor(lblSaleM12, dataRow, "Dez.");
            SetValueAndColor(lblSaleQ1, dataRow, "Quartal 1");
            SetValueAndColor(lblSaleQ2, dataRow, "Quartal 2");
            SetValueAndColor(lblSaleQ3, dataRow, "Quartal 3");
            SetValueAndColor(lblSaleQ4, dataRow, "Quartal 4");
            SetValueAndColor(lblSaleTotal, dataRow, "Gesamt");
        }

        private static void SetValueAndColor(Label label, DataRow dataRow, string columnName)
        {
            double? placeHolder = dataRow[columnName] as double?;
            if (placeHolder.HasValue)
            {
                label.Content = placeHolder.Value.ToString("F2");
                if (placeHolder.Value < 10.0)
                {
                    label.Foreground = Brushes.White;
                    label.Background = Brushes.DarkRed;
                }
                else if (placeHolder.Value >= 10_000.0)
                {
                    label.Foreground = Brushes.Black;
                    label.Background = Brushes.PaleGreen;
                }
                else
                {
                    label.ClearValue(ForegroundProperty);
                    label.ClearValue(BackgroundProperty);
                }
            }
            else
            {
                label.Content = dataRow[columnName]?.ToString();
                label.ClearValue(ForegroundProperty);
                label.ClearValue(BackgroundProperty);
            }
        }
    }
}
