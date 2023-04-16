using System.Windows;
using System.Windows.Controls;

namespace LibLite.Inchange.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void SetProgress(double value)
        {
            ProgressBar.Value = value;
        }

        public void AddProgress(double value)
        {
            ProgressBar.Value += value;
        }
    }
}
