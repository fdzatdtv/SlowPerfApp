using System;
using System.Windows;
using System.Windows.Controls;

namespace SlowPerfWpfApp
{
    /// <summary>
    /// Interaction logic for FilterControl.xaml
    /// </summary>
    public partial class FilterControl : UserControl
    {
        public static readonly RoutedEvent FilterChangedEvent = EventManager.RegisterRoutedEvent(
            "FilterChange", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FilterControl));

        // Provide CLR accessors for the event
        public event RoutedEventHandler FilterChanged
        {
            add { AddHandler(FilterChangedEvent, value); }
            remove { RemoveHandler(FilterChangedEvent, value); }
        }

        public string FilterText => txtFilter.Text;

        public FilterControl()
        {
            InitializeComponent();
        }

        private void TxtFilter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetWatermarkVisibility();
            RaiseEvent(new RoutedEventArgs(FilterChangedEvent));
        }

        private void SetWatermarkVisibility()
        {
            if (!string.IsNullOrEmpty(txtFilter.Text))
                WaterMark.Visibility = Visibility.Hidden;
            else
                WaterMark.Visibility = Visibility.Visible;
        }

        private void TxtFilter_OnGotFocus(object sender, RoutedEventArgs e)
        {
            SetWatermarkVisibility();
        }

        private void TxtFilter_OnLostFocus(object sender, RoutedEventArgs e)
        {
            SetWatermarkVisibility();
        }
    }
}
