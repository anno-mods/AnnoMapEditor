using AnnoMapEditor.DataArchives;
using System;
using System.Windows;

namespace AnnoMapEditor.UI.Windows.Start
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private StartWindowViewModel _viewModel => DataContext as StartWindowViewModel
            ?? throw new Exception();


        public StartWindow()
        {
            StartWindowViewModel viewModel = new(this);
            viewModel.DataManager.PropertyChanged += DataManager_PropertyChanged;

            DataContext = viewModel;

            InitializeComponent();
        }


        private void DataManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataManager.IsInitialized) && _viewModel.DataManager.IsInitialized)
            {
                _viewModel.PopulateOpenMapMenu(openMapMenu);
            }
        }

        public void SelectGamePath_Click(object? sender, RoutedEventArgs args)
            => _viewModel.SelectGamePath();

        public void SelectDataPath_Click(object? sender, RoutedEventArgs args)
            => _viewModel.SelectDataPath();

        public void SelectModsPath_Click(object? sender, RoutedEventArgs args)
            => _viewModel.SelectModsPath();

        public void NewMap_Click(object? sender, RoutedEventArgs args)
            => _viewModel.NewMap();

        public void OpenMap_Click(object? sender, RoutedEventArgs args)
        {
            openMapMenu.IsOpen = true;
        }
    }
}
