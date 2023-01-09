using AnnoMapEditor.Utilities;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public abstract class Repository : ObservableBase
    {
        private Task? _loadingTask;

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }
        private bool _isLoading;


        public Repository()
        {
        }


        protected Task LoadAsync()
        {
            IsLoading = true;
            _loadingTask = Task.Run(async () => {
                await DoLoad();
                IsLoading = false;
            });
            return _loadingTask;
        }

        protected abstract Task DoLoad();

        public async Task AwaitLoadingAsync()
        {
            if (_loadingTask != null)
                await _loadingTask;
        }
    }
}
