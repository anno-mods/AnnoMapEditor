using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.ExportAsMod;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

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

            InitializeComponent();

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

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.PopulateOpenMapMenu(openMapMenu);
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
    }
}
