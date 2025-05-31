using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.ExportAsMod;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Media;

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
            Title = title;
            titleText.Text = App.Title;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.PopulateOpenMapMenu(openMapMenu);

            StateChanged += new EventHandler(Window_StateChanged);
            Activated += new EventHandler(Window_IsActiveChanged);
            Deactivated += new EventHandler(Window_IsActiveChanged);
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

        private async void ExportMap_Click(object sender, RoutedEventArgs e)
        {
            var picker = new SaveFileDialog
            {
                DefaultExt = ".a7tinfo",
                Filter = "Map template (*.a7tinfo)|*.a7tinfo|XML map template (*.xml)|*.xml",
                FilterIndex = Path.GetExtension(_viewModel.MapTemplateFilePath)?.ToLower() == ".xml" ? 2 : 1,
                FileName = Path.GetFileName(_viewModel.MapTemplateFilePath),
                OverwritePrompt = true
            };

            if (true == picker.ShowDialog() && picker.FileName is not null)
            {
                await _viewModel.SaveMap(picker.FileName);
            }
        }

        private void ExportMod_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.MapTemplate is null)
                return;

            OverlayService.Instance.Show(new ExportAsModViewModel(_viewModel.MapTemplate));
        }

        private void OverlayContainer_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void maximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            { 
                WindowState = WindowState.Normal;
                // Margin = new(0);
            }
            else
            {
                WindowState = WindowState.Maximized;
                // Margin = new(0);
            }
                
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
                maximizeButton.Content = (char)0xe922; // "&#xe922;";
            }
            else
            {
                mainWindowGrid.Margin = new(6);
                maximizeButton.Content = (char)0xe923; // "&#xe923;";
            }
        }

        private void Window_IsActiveChanged(object? sender, EventArgs e)
        {
            windowBorder.BorderBrush = (SolidColorBrush)Resources[IsActive ? "ActiveWindowBorderBrush" : "InactiveWindowBorderBrush"];
        }
    }
}
