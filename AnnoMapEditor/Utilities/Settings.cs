using AnnoMapEditor.DataArchives;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.Utilities
{
    public class Settings : ObservableBase
    {
        private readonly Logger<Settings> _logger = new();

        public static Settings Instance { get; } = new();

        /// <summary>
        /// Is Invoked when loading all repositories from the game path finishes.
        /// </summary>
        public event EventHandler? LoadingFinished;

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

        public AssetRepository? AssetRepository 
        { 
            get => _assetRepository;
            private set => SetProperty(ref _assetRepository, value); 
        }
        private AssetRepository? _assetRepository;


        public string? DataPath 
        {
            get => _dataArchive.DataPath;
            set
            {
                if (value != DataPath)
                {
                    LoadDataPath(value);
                    UserSettings.Default.DataPath = DataArchive.DataPath;
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

        /// <summary>
        /// Useful if code needs to wait for a Loading Process to finish somewhere.
        /// Like unit test setups for example.
        /// </summary>
        private ManualResetEvent LoadingDoneTrigger { get; init; }

        /// <summary>
        /// Private Constructor - only used for Singleton.
        /// </summary>
        private Settings()
        {
            //We set the LoadingDoneTrigger as Reset by default, so anything that waits for the setup to finish
            //has to actually wait for the SetupAsync called in Initialize to finish.
            LoadingDoneTrigger = new ManualResetEvent(false);
            Initialize();
        }

        /// <summary>
        /// Starts the asynchronous Setup on the Tread Pool, so it doesn't block.
        /// </summary>
        private void Initialize()
        {
            IsLoading = true;

            _loadingTask = Task.Run(async () => {
                await SetupAsync(true);
            });
        }

        /// <summary>
        /// This avoids direct writes to the DataPath Property that would run
        /// its setter on the thread pool, making proper awaiting of the setup impossible.
        /// </summary>
        /// <param name="updateInvalidUserSettings">If the UserSettings should be overwritten by a valid result.</param>
        /// <returns></returns>
        private async Task SetupAsync(bool updateInvalidUserSettings)
        {
            LoadingDoneTrigger.Reset();

            if (!string.IsNullOrEmpty(UserSettings.Default.DataPath))
            {
                await LoadDataPathAsync(UserSettings.Default.DataPath);
                OnPropertyChanged(nameof(DataPath));
            }

            if (DataArchive?.IsValid != true)
            {
                // auto detect on start-up if not valid
                await LoadDataPathAsync(GetInstallDirFromRegistry());

                OnPropertyChanged(nameof(DataPath));
            }

            if (DataArchive?.IsValid == true && updateInvalidUserSettings)
            {
                UserSettings.Default.DataPath = DataArchive.DataPath;
                UserSettings.Default.Save();
            }

            LoadingDoneTrigger.Set();
        }

        /// <summary>
        /// Used to set DataPath with a synchronous method call
        /// by running the actual async code on the tread pool.
        /// </summary>
        /// <param name="path">The DataPath to load.</param>
        private void LoadDataPath(string? path)
        {
            IsLoading = true;

            _loadingTask = Task.Run(async () => {
                await LoadDataPathAsync(path);
            });
        }

        /// <summary>
        /// Asynchronously sets the DataPath and reads all the Repositories.
        /// </summary>
        /// <param name="path">The DataPath to load.</param>
        /// <returns></returns>
        private async Task LoadDataPathAsync(string? path)
        {
            //Only use the LoadingDoneTrigger, if it is not already Reset.
            //A reset LoadingDoneTrigger here means, that the Setup is using it.
            bool loadingDoneTriggerNotBlocked = LoadingDoneTrigger.WaitOne(0);

            if (loadingDoneTriggerNotBlocked)
            {
                LoadingDoneTrigger.Reset();
            }

            Dispatch(() =>
            {
                IsLoading = true;
            });
    
            var archive = await DataArchives.DataArchive.OpenAsync(path);

            try
            {
                AssetRepository assetRepository = new(archive);
                assetRepository.Register<RegionAsset>();
                assetRepository.Register<FertilityAsset>();
                assetRepository.Register<RandomIslandAsset>();
                assetRepository.Register<SlotAsset>();
                assetRepository.Register<MinimapSceneAsset>();
                assetRepository.Register<SessionAsset>();
                assetRepository.Register<MapTemplateAsset>();
                await assetRepository.LoadAsync();

                FixedIslandRepository fixedIslandRepository = new(archive);
                await fixedIslandRepository.AwaitLoadingAsync();

                IslandRepository islandRepository = new(fixedIslandRepository, assetRepository);
                await islandRepository.AwaitLoadingAsync();

                Dispatch(() =>
                {
                    DataArchive = archive;
                    AssetRepository = assetRepository;
                    IslandRepository = islandRepository;
                    IsValidDataPath = true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured during setup.", ex);
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

                if (loadingDoneTriggerNotBlocked)
                {
                    LoadingDoneTrigger.Set();
                }

                //If not Dispatched to UI thread, only the first item will be invoked somehow?
                Dispatch(() =>
                {
                    LoadingFinished?.Invoke(this, EventArgs.Empty);
                });
            }
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

        /// <summary>
        /// Waits until the current _loadingTask is finished.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown when no loading Task has been set yet.</exception>
        public async Task AwaitLoadingAsync()
        {
            if (_loadingTask != null)
                await _loadingTask;
            else
                throw new Exception($"LoadAsync has not been called.");
        }

        /// <summary>
        /// Waits until the current loading Task is finished by blocking until the LoadingDoneTrigger is set.
        /// </summary>
        public void WaitForLoadingBlocking()
        {
            LoadingDoneTrigger.WaitOne();
        }
    }
}
