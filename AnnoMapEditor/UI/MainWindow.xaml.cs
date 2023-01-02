using AnnoMapEditor.UI.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; } = new MainWindowViewModel(Settings.Instance);
        private readonly string title;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;

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

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            CreateImportMenu(openMapMenu, ViewModel?.Maps);
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Session":
                    if (ViewModel?.Session is not null)
                    {
                        Title = $"{title} - {Path.GetFileName(ViewModel.SessionFilePath)}";
                    }
                    else
                        Title = title;
                    break;
                case "Maps":
                case "IsValidDataPath":
                    CreateImportMenu(openMapMenu, ViewModel?.Maps);
                    break;
            }
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new OpenFileDialog
            {
                Filter = "Map templates (*.a7tinfo, *.xml)|*.a7tinfo;*.xml"
            };

            if (true == picker.ShowDialog())
            {
                if (!Settings.Instance.IsValidDataPath)
                {
                    int end = picker.FileName.IndexOf(@"\data\session");
                    if (end == -1)
                        end = picker.FileName.IndexOf(@"\data\dlc");
                    if (end != -1)
                        Settings.Instance.DataPath = picker.FileName[..end];
                }

                await ViewModel.OpenMap(picker.FileName);
            }
        }

        private void Configure_Click(object _, RoutedEventArgs _1)
        {
            var picker = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                UseDescriptionForTitle = true,
                Description = "Select your game (i.e. \"Anno 1800/\") folder or a folder where all .rda files are extracted into"
            };
            if (true == picker.ShowDialog())
            {
                Settings.Instance.DataPath = picker.SelectedPath;
            }
        }

        private void AutoDetect_Click(object _, RoutedEventArgs _1)
        {
            Settings.Instance.DataPath = Settings.GetInstallDirFromRegistry();
        }

        private void CreateImportMenu(ContextMenu parentMenu, List<MapGroup>? mapGroups)
        {
            parentMenu.Items.Clear();

            MenuItem openFile = new() { Header = "Open file..." };
            openFile.Click += OpenFile_Click;
            parentMenu.Items.Add(openFile);
            parentMenu.Items.Add(new Separator());

            MenuItem newFile = new() { Header = "New Map file" };
            newFile.Click += NewMapFile_Click;
            parentMenu.Items.Add(newFile);
            parentMenu.Items.Add(new Separator());

            if (mapGroups is null || mapGroups.Count == 0)
            {
                parentMenu.Items.Add(new MenuItem() {
                    Header = Settings.Instance.IsLoading ? "(loading RDA...)" : "Set game/RDA path to import",
                    IsEnabled = false
                });
                return;
            }

            foreach (var group in mapGroups)
            {
                MenuItem groupMenu = new() { Header = group.Name };

                foreach (var map in group.Maps)
                {
                    MenuItem mapMenu = new() { Header = map.Name, DataContext = map };
                    mapMenu.Click += MapMenu_Click;
                    groupMenu.Items.Add(mapMenu);
                }

                parentMenu.Items.Add(groupMenu);
            }
        }

        private async void MapMenu_Click(object sender, RoutedEventArgs e)
        {
            MapInfo? mapInfo = (sender as MenuItem)?.DataContext as MapInfo;
            if (mapInfo?.FileName is not null)
            {
                await ViewModel.OpenMap(mapInfo.FileName, true);
            }
        }

        private async void ExportMap_Click(object sender, RoutedEventArgs e)
        {
            var picker = new SaveFileDialog
            {
                DefaultExt = ".a7tinfo",
                Filter = "Map template (*.a7tinfo)|*.a7tinfo|XML map template (*.xml)|*.xml",
                FilterIndex = Path.GetExtension(ViewModel.SessionFilePath)?.ToLower() == ".xml" ? 2 : 1,
                FileName = Path.GetFileName(ViewModel.SessionFilePath),
                OverwritePrompt = true
            };

            if (true == picker.ShowDialog() && picker.FileName is not null)
            {
                await ViewModel.SaveMap(picker.FileName);
            }
        }

        private void ExportMod_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Session is null)
                return;

            exportDialog.Show(ViewModel.Session);
        }

        private void NewMapFile_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateNewMap();
        }

        private void Hyperlink_OpenBrowser(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var info = new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
            };
            Process.Start(info);
        }
    }
}
