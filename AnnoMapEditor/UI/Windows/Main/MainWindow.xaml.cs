using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.ExportAsMod;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoMapEditor.UI.Controls.Toolbar;

namespace AnnoMapEditor.UI.Windows.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        private readonly string title;

        public MainWindow(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext= _viewModel;

            InitializeComponent();

            WindowChrome.SetIsHitTestVisibleInChrome(closeButton, true);
            WindowChrome.SetIsHitTestVisibleInChrome(minimizeButton, true);
            WindowChrome.SetIsHitTestVisibleInChrome(maximizeButton, true);
            WindowChrome.SetIsHitTestVisibleInChrome(ToolbarControl, true);

            var exePath = Path.Join(AppContext.BaseDirectory, "AnnoMapEditor.exe");
            var productVersion = "";
            if (File.Exists(exePath))
            {
                try
                {
                    productVersion = FileVersionInfo.GetVersionInfo(exePath).ProductVersion ?? "";
                }
                catch { }
            }
            title = $"{App.Title} {productVersion}";
            versionStatusText.Text = $"Version: {productVersion}";
            Title = title;
            titleText.Text = App.Title;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            // _viewModel.PopulateOpenMapMenu(openMapMenu);
            // _viewModel.PopulateOpenMapMenu(toolbarImportContextMenu, true);

            StateChanged += new EventHandler(Window_StateChanged);
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainWindowViewModel.MapTemplate):
                    if (_viewModel?.MapTemplate is not null)
                        Title = $"{title} - {Path.GetFileName(_viewModel.MapTemplateFilePath)}";
                    else
                        Title = title;
                    break;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }

        private void maximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }
        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                mainWindowGrid.Margin = new(0);
                maximizeButton.Content = (char)0xe922;
            }
            else
            {
                mainWindowGrid.Margin = new(5);
                maximizeButton.Content = (char)0xe923;
            }
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindowGrid.Focus();
            // Keyboard.ClearFocus();
        }
    }
}
