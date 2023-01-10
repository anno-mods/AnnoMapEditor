using AnnoMapEditor.Utilities;
using System;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public abstract class Repository : ObservableBase
    {
        private Task? _loadingTask;

        public bool IsLoaded
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }
        private bool _isLoading = false;


        public Repository()
        {
        }


        protected Task LoadAsync()
        {
            _loadingTask = Task.Run(async () => {
                await DoLoad();
                IsLoaded = true;
            });
            return _loadingTask;
        }

        protected abstract Task DoLoad();

        public async Task AwaitLoadingAsync()
        {
            if (_loadingTask != null)
                await _loadingTask;
            else
                throw new Exception($"LoadAsync has not been called.");
        }
    }
}
