using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Controls.Progress
{
    public class ProgressViewModel : ObservableBase, IProgress
    {
        public int Value 
        { 
            get
            {
                lock (this)
                {
                    return _value;
                }
            } 
            set
            {
                lock (this)
                {
                    SetProperty(ref _value, value);
                    Update();
                }
            }
        }
        private int _value;

        public int Maximum
        {
            get
            {
                lock (this)
                {
                    return _maximum;
                }
            }
            set
            {
                lock (this)
                {
                    SetProperty(ref _maximum, value);
                    Update();
                }
            }
        }
        private int _maximum;

        public bool IsInProgress { get; private set; } = false;

        public bool IsDone { get; private set; } = true;


        private object _messageLock = new();
        public string? Message { 
            get
            {
                lock (_messageLock)
                {
                    return _message;
                }
            }
            set
            {
                lock (_messageLock)
                {
                    SetProperty(ref _message, value);
                }
            }
        }
        private string? _message;


        private void Update()
        {
            if (_value >= _maximum && IsInProgress)
            {
                IsInProgress = false;
                IsDone = true;
            }
            else if (_value < _maximum && IsDone)
            {
                IsInProgress = true;
                IsDone = false;
            }
            else
                return;

            OnPropertyChanged(nameof(IsInProgress));
            OnPropertyChanged(nameof(IsDone));
        }
    }
}
