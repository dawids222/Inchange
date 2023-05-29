using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LibLite.Inchange.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public event RoutedEventHandler? OkClick;

        public ProgressWindow()
        {
            InitializeComponent();
            ProgressBar.ValueChanged += ProgressBar_ValueChanged;
            OkButton.Click += (sender, e) => OkClick?.Invoke(sender, e);
            Closed += (_, _) => Environment.Exit(0);
        }

        public void AddProgress(double value)
        {
            ProgressBar.Value += value;
        }

        public void SetProgress(double value)
        {
            ProgressBar.Value = value;
        }

        public void AddSuccessEntry(string entry)
        {
            AddEntry($"[Success] : {entry}", Brushes.Green);
        }

        public void AddErrorEntry(string entry)
        {
            ProgressBar.Foreground = Brushes.Red;
            IndeterminateProgressBar.Foreground = Brushes.Red;
            AddEntry($"[Error] : {entry}", Brushes.Red);
        }

        private void AddEntry(string entry, Brush foreground)
        {
            var textBlock = new TextBlock
            {
                Text = entry,
                FontSize = 12,
                Foreground = foreground
            };
            ProgressTextStackPanel.Children.Add(textBlock);
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var done = ProgressBar.Value >= ProgressBar.Maximum;
            OkButton.Visibility = done ? Visibility.Visible : Visibility.Collapsed;
            IndeterminateProgressBar.Visibility = done ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
