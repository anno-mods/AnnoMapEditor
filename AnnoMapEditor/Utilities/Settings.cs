using AnnoMapEditor.DataArchives;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.Utilities
{
    public class Settings : ObservableBase
    {
        public static Settings Instance { get; } = new();

        public IDataArchive DataArchive
        {
            get => _dataArchive;
            private set
            {
                if (_dataArchive is System.IDisposable disposable)
                    disposable.Dispose();
                SetProperty(ref _dataArchive, value);
                OnPropertyChanged(nameof(DataPath));
            }
        }
        private IDataArchive _dataArchive = DataArchives.DataArchive.Default;

        public IslandRepository? IslandRepository
        {
            get => _islandRepository;
            private set => SetProperty(ref _islandRepository, value);
        }
        private IslandRepository? _islandRepository;  


        public string? DataPath 
        {
            get => _dataArchive.Path;
            set
            {
                if (value != DataPath)
                {
                    LoadDataPath(value);
                    UserSettings.Default.DataPath = DataArchive.Path;
                    UserSettings.Default.Save();
                    OnPropertyChanged(nameof(DataPath));
                    OnPropertyChanged(nameof(IsValidDataPath));
                }
            }
        }

        public bool IsValidDataPath
        {
            get => _isValidDataPath;
            private set => SetProperty(ref _isValidDataPath, value);
        }
        private bool _isValidDataPath = false;

        public bool IsLoading 
        { 
            get => _isLoading; 
            private set => SetProperty(ref _isLoading, value); 
        }
        private bool _isLoading;

        private Task? _loadingTask;


        public Settings()
        {
            DataPath = UserSettings.Default.DataPath;
            if (DataArchive?.IsValid != true)
            {
                // auto detect on start-up if not valid
                DataPath = GetInstallDirFromRegistry();
            }
        }


        public void LoadDataPath(string? path)
        {
            IsLoading = true;

            _loadingTask = Task.Run(async () => {
                var archive = await DataArchives.DataArchive.OpenAsync(path);

                try
                {
                    AssetRepository<RandomIslandAsset> randomIslandRepository = AssetRepository.Create<RandomIslandAsset>(archive);
                    await randomIslandRepository.AwaitLoadingAsync();

                    FixedIslandRepository fixedIslandRepository = new(archive);
                    await fixedIslandRepository.AwaitLoadingAsync();

                    IslandRepository islandRepository = new(fixedIslandRepository, randomIslandRepository);
                    await islandRepository.AwaitLoadingAsync();

                    Dispatch(() =>
                    {
                        DataArchive = archive;
                        IslandRepository = islandRepository;
                        IsValidDataPath = true;
                    });

                }
                catch (Exception ex)
                {
                    Dispatch(() =>
                    {
                        IsValidDataPath = false;
                    });
                }
                finally
                {
                    Dispatch(() =>
                    {
                        IsLoading = false;
                    });
                }
            });
        }

        private void Dispatch(Action action)
        {
            if (Application.Current?.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(action);

            else
                action();
        }

        public static string? GetInstallDirFromRegistry()
        {
            string installDirKey = @"SOFTWARE\WOW6432Node\Ubisoft\Anno 1800";
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(installDirKey);
            return key?.GetValue("InstallDir") as string;
        }

        public async Task AwaitLoadingAsync()
        {
            if (_loadingTask != null)
                await _loadingTask;
            else
                throw new Exception($"LoadAsync has not been called.");
        }
    }
}
