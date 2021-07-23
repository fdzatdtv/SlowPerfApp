using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

namespace SlowPerfWpfApp
{
    /// <summary>
    /// Interaction logic for PieChartView.xaml
    /// </summary>
    public partial class PieChartView : UserControl
    {
        private TextBlock[] _monthTexts;

        public PieChartView()
        {
            InitializeComponent();
            CreateMonthTexts();
        }

        private void CreateMonthTexts()
        {
            int top = 370;
            _monthTexts = new TextBlock[13];
            for (int i = 0; i < _monthTexts.Length; i++)
            {
                var txtBlock = new TextBlock();
                _monthTexts[i] = txtBlock;
                Canvas.SetTop(txtBlock, top);
                Canvas.SetLeft(txtBlock, 10);
                Legend2.Children.Add(txtBlock);
                top += 20;
            }
        }

        private void PieChartView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RenderPieChart();
            RenderLegend();
            RenderLineChart();
            RenderLegend2();
        }

        private void RenderLegend()
        {
            Rect rect = new Rect(10, 110, 20, 20);
            Legend.DrawRectangle(Brushes.Yellow, Brushes.Orange, 3, rect);
            rect.Offset(0, 25);
            Legend.DrawRectangle(Brushes.Pink, Brushes.Red, 3, rect);
            //rect = new Rect(10, 110, 20, 20);
            rect.Offset(0, 25);
            Legend.DrawRectangle(Brushes.LightBlue, Brushes.Blue, 3, rect);
            rect.Offset(0, 25);
            Legend.DrawRectangle(Brushes.LightGreen, Brushes.Green, 3, rect);

            DataRow datarow = DataContext as DataRow;
            double total = (double)datarow["Gesamt"];
            double q1 = (double)datarow["Quartal 1"];
            double q2 = (double)datarow["Quartal 2"];
            double q3 = (double)datarow["Quartal 3"];
            double q4 = (double)datarow["Quartal 4"];
            if (total == 0.0)
            {
                txtQ1.Text = $"Q1: {q1:C2}";
                txtQ2.Text = $"Q2: {q2:C2}";
                txtQ3.Text = $"Q3: {q3:C2}";
                txtQ4.Text = $"Q4: {q4:C2}";
            }
            else
            {
                double q1rel = q1 / total;
                double q2rel = q2 / total;
                double q3rel = q3 / total;
                double q4rel = q4 / total;
                txtQ1.Text = $"Q1: {q1:C2} ({q1rel:P})";
                txtQ2.Text = $"Q2: {q2:C2} ({q2rel:P})";
                txtQ3.Text = $"Q3: {q3:C2} ({q3rel:P})";
                txtQ4.Text = $"Q4: {q4:C2} ({q4rel:P})";
            }


        }

        private void RenderLegend2()
        {
            DataRow dataRow = DataContext as DataRow;
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
            };
            double[] values = new double[12];

            for (int i = 0; i < values.Length; i++)
                values[i] = (double)dataRow[columnNames[i]];

            double total = (double)dataRow["Gesamt"];

            if (total != 0.0)
            {
                for (int i = 0; i < values.Length; i++)
                    _monthTexts[i].Text = $"{columnNames[i]}: {values[i]:C2} ({values[i] / total:P})";
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                    _monthTexts[i].Text = $"{columnNames[i]}: {values[i]:C2}";
            }

            double min = values.Min();
            double max = values.Max();
            double avg = values.Average();

            _monthTexts[12].Text = $"Min: {min:C2}  Max: {max:C2}  Avg: {avg:C2}";
        }

        private void RenderPieChart()
        {
            Rect rect = new Rect(25, 10, 200, 90);
            const double minAngle = 0.02;
            //double total = 57550.68;
            //double q1 = 4858.78;
            //double q2 = 11043.8;
            //double q3 = 16176.45;
            //double q4 = 25471.65;
            DataRow datarow = DataContext as DataRow;
            double total = (double)datarow["Gesamt"];
            if (total == 0.0)
            {
                PieChart.DrawRectangle(Brushes.Transparent, Brushes.Transparent, 1, rect);
                return;
            }
            double q1 = (double)datarow["Quartal 1"];
            double q2 = (double)datarow["Quartal 2"];
            double q3 = (double)datarow["Quartal 3"];
            double q4 = (double)datarow["Quartal 4"];
            double q1rel = q1 / total * 2;
            double q2rel = q2 / total * 2 + q1rel;
            double q3rel = q3 / total * 2 + q2rel;
            double q4rel = q4 / total * 2 + q3rel;
            bool largeArcQ1 = q1rel > 1.0;
            bool largeArcQ2 = (q2rel - q1rel) > 1.0;
            bool largeArcQ3 = (q3rel - q2rel) > 1.0;
            bool largeArcQ4 = (q4rel - q3rel) > 1.0;


            Rect shadowRect = rect;
            shadowRect.Offset(2, 6);

            // Draw the ellipse.
            Ellipse ellipse = PieChart.DrawEllipse(
                Brushes.DimGray, Brushes.DimGray, 10, shadowRect);

            Point point1, point2;
            if (q1rel > minAngle)
            {
                // Draw a pie slice.
                Path pie_slice1 = PieChart.DrawPieSlice(
                    Brushes.Yellow, Brushes.Orange,
                    5, rect, 0,  q1rel * Math.PI, largeArcQ1,
                    SweepDirection.Clockwise,
                    out point1, out point2);
                pie_slice1.StrokeLineJoin = PenLineJoin.Round;
            }

            if (q2rel - q1rel > minAngle)
            {
                // Draw a pie slice.
                Path pie_slice2 = PieChart.DrawPieSlice(
                    Brushes.Pink, Brushes.Red,
                    5, rect, q1rel * Math.PI, q2rel * Math.PI, largeArcQ2,
                    SweepDirection.Clockwise,
                    out point1, out point2);
                pie_slice2.StrokeLineJoin = PenLineJoin.Round;
            }

            if (q3rel - q2rel > minAngle)
            {
                // Draw a pie slice.
                Path pie_slice3 = PieChart.DrawPieSlice(
                    Brushes.LightBlue, Brushes.Blue,
                    5, rect, q2rel * Math.PI, q3rel * Math.PI, largeArcQ3,
                    SweepDirection.Clockwise,
                    out point1, out point2);
                pie_slice3.StrokeLineJoin = PenLineJoin.Round;
            }

            if (q4rel - q3rel > minAngle)
            {
                // Draw a pie slice.
                Path pie_slice4 = PieChart.DrawPieSlice(
                    Brushes.LightGreen, Brushes.Green,
                    5, rect, q3rel * Math.PI, 2.0 * Math.PI, largeArcQ4,
                    SweepDirection.Clockwise,
                    out point1, out point2);
                pie_slice4.StrokeLineJoin = PenLineJoin.Round;
            }
        }

        private void RenderLineChart()
        {
            Rect outerRect = new Rect(19, 214, 192, 152);
            // clear rect
            LineChart.DrawRectangle(Brushes.Black, Brushes.Black, 1, outerRect);

            Rect rect = new Rect(25, 220, 180, 140);

            //double total = 57550.68;
            //double q1 = 4858.78;
            //double q2 = 11043.8;
            //double q3 = 16176.45;
            //double q4 = 25471.65;
            DataRow dataRow = DataContext as DataRow;

            double[] values = new double[12];
            values[0] = (double)dataRow["Jan."];
            values[1] = (double)dataRow["Feb."];
            values[2] = (double)dataRow["Mär."];
            values[3] = (double)dataRow["Apr."];
            values[4] = (double)dataRow["Mai"];
            values[5] = (double)dataRow["Jun."];
            values[6] = (double)dataRow["Jul."];
            values[7] = (double)dataRow["Aug."];
            values[8] = (double)dataRow["Sep."];
            values[9] = (double)dataRow["Okt."];
            values[10] = (double)dataRow["Nov."];
            values[11] = (double)dataRow["Dez."];

            double max = values.Max();
            if (max != 0.0)
            {
                for (int i = 0; i < 12; i++)
                    values[i] = values[i] / max;
            }

            const int padding = 20;
            Point currPoint = rect.BottomLeft;
            double currValue = values[0];
            double y = (rect.Height - padding) * (1.0 - currValue) + padding + rect.Y;
            double x = currPoint.X;
            Point lastPoint = new Point(x, y);
            for (int i = 1; i < 12; i++)
            {
                x += 15;
                currValue = values[i];
                y = (rect.Height - padding) * (1.0 - currValue) + padding + rect.Y;
                currPoint = new Point(x, y);
                LineChart.DrawLine(Brushes.DarkGray, 0.5, new Point(currPoint.X, rect.Top + 10), new Point(currPoint.X, rect.Bottom));
                LineChart.DrawLine(Brushes.DarkCyan, 1.5, lastPoint, currPoint);
                lastPoint = currPoint;
            }
            
            double avg = values.Average();
            y = (rect.Height - padding) * (1.0 - avg) + padding + rect.Y;

            LineChart.DrawLine(Brushes.DarkOrange, 0.5, new Point(rect.Left, y), new Point(rect.Left + 11 * 15, y));
            LineChart.DrawLine(Brushes.DimGray, 2, rect.TopLeft, rect.BottomLeft);
            LineChart.DrawLine(Brushes.DimGray, 2, rect.BottomLeft, rect.BottomRight);
        }

    }
}
