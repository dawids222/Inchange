using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace LibLite.Inchange.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : UserControl
    {
        public ExplorerView()
        {
            InitializeComponent();

            InitializeWebBrowser();
            InitializeDrivesStackPanel();
            InitializeActionButtons();
        }

        private void InitializeWebBrowser()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            WebBrowser.Navigated += WebBrowser_Navigated;
            WebBrowser.Navigate(desktopPath);
        }

        private void InitializeDrivesStackPanel()
        {
            var drives = DriveInfo
                .GetDrives()
                .Where(x => x.DriveType == DriveType.Fixed)
                .Select(x => x.Name)
                .ToList();
            foreach (var drive in drives)
            {
                var button = new Button
                {
                    Height = 20,
                    Content = $"Dysk lokalny ({drive})",
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                };
                button.Click += (object sender, RoutedEventArgs e) => WebBrowser.Navigate(drive);
                DrivesStackPanel.Children.Add(button);
            }
        }

        private void InitializeActionButtons()
        {
            BackButton.Click += BackButton_Click;
            ForwardButton.Click += ForwardButton_Click;
            UpButton.Click += UpButton_Click;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.GoBack();

            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.GoForward();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            var currentPathFragments = GetCurerntPathFragments();
            var upperPathFragments = currentPathFragments
                .Take(currentPathFragments.Count() - 1)
                .ToList();
            var path = string.Join("\\", upperPathFragments) + "\\";

            WebBrowser.Navigate(path);
        }

        private IEnumerable<string> GetCurerntPathFragments()
        {
            return WebBrowser.Source.LocalPath
                .Split("\\")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            UpdateBreadcrumbs();
            BackButton.IsEnabled = WebBrowser.CanGoBack;
            ForwardButton.IsEnabled = WebBrowser.CanGoForward;
            UpButton.IsEnabled = GetCurerntPathFragments().Count() > 1;
        }

        private void UpdateBreadcrumbs()
        {
            var breadcrumbs = BreadcrumbsStackPanel.Children;
            breadcrumbs.Clear();
            var fragments = GetCurerntPathFragments().ToList();
            fragments[0] = $"Dysk lokalny ({fragments[0]})";
            foreach (var fragment in fragments)
            {
                var breadcrumb = new Label
                {
                    Content = fragment,
                };
                var separator = new Label
                {
                    Content = ">",
                };
                breadcrumbs.Add(breadcrumb);
                breadcrumbs.Add(separator);
            }
            breadcrumbs.RemoveAt(breadcrumbs.Count - 1);
        }
    }
}
