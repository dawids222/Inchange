using Microsoft.Win32;
using System;
using System.Windows;

namespace LibLite.Inchange.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;

            var dialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "Pdf Files|*.pdf",
            };

            if (dialog.ShowDialog() == true)
            {
                var files = dialog.FileNames;
                // TODO: Add files processing here.
            }

            Environment.Exit(0);
        }
    }
}
