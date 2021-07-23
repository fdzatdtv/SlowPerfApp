using System.Windows.Controls;

namespace SlowPerfWpfApp
{
    /// <summary>
    /// Interaction logic for FooterRowView.xaml
    /// </summary>
    public partial class FooterRowView : UserControl
    {
        private double[] _columnTotals;

        public FooterRowView()
        {
            InitializeComponent();
            RefreshUI();
        }

        public double[] ColumnTotals
        {
            get => _columnTotals;
            set
            {
                _columnTotals = value;
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            if (ColumnTotals == null)
                ClearTexts();
            else
                SetTexts();
        }

        private void SetTexts()
        {
            lblSaleM01.Content = ColumnTotals[0].ToString("F2");
            lblSaleM02.Content = ColumnTotals[1].ToString("F2");
            lblSaleM03.Content = ColumnTotals[2].ToString("F2");
            lblSaleM04.Content = ColumnTotals[3].ToString("F2");
            lblSaleM05.Content = ColumnTotals[4].ToString("F2");
            lblSaleM06.Content = ColumnTotals[5].ToString("F2");
            lblSaleM07.Content = ColumnTotals[6].ToString("F2");
            lblSaleM08.Content = ColumnTotals[7].ToString("F2");
            lblSaleM09.Content = ColumnTotals[8].ToString("F2");
            lblSaleM10.Content = ColumnTotals[9].ToString("F2");
            lblSaleM11.Content = ColumnTotals[10].ToString("F2");
            lblSaleM12.Content = ColumnTotals[11].ToString("F2");
            lblSaleQ1.Content = ColumnTotals[12].ToString("F2");
            lblSaleQ2.Content = ColumnTotals[13].ToString("F2");
            lblSaleQ3.Content = ColumnTotals[14].ToString("F2");
            lblSaleQ4.Content = ColumnTotals[15].ToString("F2");
            lblSaleTotal.Content = ColumnTotals[16].ToString("F2");
        }

        private void ClearTexts()
        {
            lblSaleM01.Content = null;
            lblSaleM02.Content = null;
            lblSaleM03.Content = null;
            lblSaleM04.Content = null;
            lblSaleM05.Content = null;
            lblSaleM06.Content = null;
            lblSaleM07.Content = null;
            lblSaleM08.Content = null;
            lblSaleM09.Content = null;
            lblSaleM10.Content = null;
            lblSaleM11.Content = null;
            lblSaleM12.Content = null;
            lblSaleQ1.Content = null;
            lblSaleQ2.Content = null;
            lblSaleQ3.Content = null;
            lblSaleQ4.Content = null;
            lblSaleTotal.Content = null;
        }
    }
}
