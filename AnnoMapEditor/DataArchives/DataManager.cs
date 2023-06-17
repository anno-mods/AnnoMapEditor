using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.Utilities;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.DataArchives
{
    public class DataManager : ObservableBase
    {
        private const string NOT_INITIALIZED_MESSAGE = "DataManager has not yet been initialized.";

        public static readonly Logger<DataManager> _logger = new();

        public static readonly DataManager Instance = new();


        public bool IsInitialized
        {
            get => _isInitialized;
            private set => SetProperty(ref _isInitialized, value);
        }
        private bool _isInitialized = false;

        public bool IsInitializing
        {
            get => _isInitializing;
            private set => SetProperty(ref _isInitializing, value);
        }
        private bool _isInitializing = false;

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }
        private string? _errorMessage;

        public bool HasError => _errorMessage != null;


        public IDataArchive DataArchive => _isInitialized && _dataArchive != null ? _dataArchive : throw new Exception(NOT_INITIALIZED_MESSAGE);
        private IDataArchive? _dataArchive;

        public AssetRepository AssetRepository => _isInitialized && _assetRepository != null ? _assetRepository : throw new Exception(NOT_INITIALIZED_MESSAGE);
        private AssetRepository? _assetRepository;

        public FixedIslandRepository FixedIslandRepository => _isInitialized && _fixedIslandRepository != null ? _fixedIslandRepository : throw new Exception(NOT_INITIALIZED_MESSAGE);
        private FixedIslandRepository? _fixedIslandRepository;

        public IslandRepository IslandRepository => _isInitialized && _islandRepository != null ? _islandRepository : throw new Exception(NOT_INITIALIZED_MESSAGE);
        private IslandRepository? _islandRepository;

        public MapGroupRepository MapGroupRepository => _isInitialized && _mapGroupRepository != null ? _mapGroupRepository : throw new Exception(NOT_INITIALIZED_MESSAGE);
        private MapGroupRepository? _mapGroupRepository;


        private DataManager()
        {

        }


        public async Task TryInitializeAsync(string dataPath)
        {
            UpdateStatus(isInitializing: true, isInitialized: false);
            _logger.LogInformation($"Initializing DataManager at '{dataPath}'.");

            try
            {
                DataArchiveFactory dataArchiveFactory = new();
                _dataArchive = dataArchiveFactory.CreateDataArchive(dataPath);

                _assetRepository = new AssetRepository(_dataArchive);
                _assetRepository.Register<RegionAsset>();
                _assetRepository.Register<FertilityAsset>();
                _assetRepository.Register<RandomIslandAsset>();
                _assetRepository.Register<SlotAsset>();
                _assetRepository.Register<MinimapSceneAsset>();
                _assetRepository.Register<SessionAsset>();
                _assetRepository.Register<MapTemplateAsset>();
                _assetRepository.Register<PositionMarker>();
                _assetRepository.Register<VisualObject>();
                _assetRepository.Register<AmbientArea>();
                await _assetRepository.InitializeAsync();

                _fixedIslandRepository = new FixedIslandRepository(_dataArchive);
                await _fixedIslandRepository.InitializeAsync();

                _islandRepository = new IslandRepository(_fixedIslandRepository, _assetRepository);
                await _islandRepository.InitializeAsync();

                _mapGroupRepository = new MapGroupRepository(_dataArchive);
                await _mapGroupRepository.InitializeAsync();
            }
            catch (Exception ex)
            {
                UpdateStatus(isInitializing: false, isInitialized: false, errorMessage: ex.Message);
                _logger.LogInformation($"Could not initialize DataManager at '{dataPath}'.");
                return;
            }

            UpdateStatus(isInitializing: false, isInitialized: true);
            _logger.LogInformation($"Successfully initialized DataManager at '{dataPath}'.");
        }


        private void Dispatch(Action action)
        {
            if (Application.Current?.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(action);
            else
                action();
        }

        private void UpdateStatus(bool isInitializing, bool isInitialized, string? errorMessage = null)
        {
            Dispatch(() =>
            {
                IsInitializing = isInitializing;
                IsInitialized = isInitialized;
                ErrorMessage = errorMessage;
                OnPropertyChanged(nameof(HasError));
            });
        }
    }
}
