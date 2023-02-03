using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.UI.Overlays
{
    public class OverlayService : ObservableBase
    {
        public static readonly OverlayService Instance = new();


        public event OverlayShownEventHandler? OverlayShown;

        public event OverlayClosedEventHandler? OverlayClosed;

        public IOverlayViewModel? CurrentOverlay 
        { 
            get => _currentOverlay;
            private set
            {
                if (value != _currentOverlay)
                {
                    if (_currentOverlay != null)
                        OverlayClosed?.Invoke(this, new(_currentOverlay));

                    _currentOverlay = value;
                    OnPropertyChanged();

                    if (_currentOverlay != null)
                        OverlayShown?.Invoke(this, new(_currentOverlay));
                }
            } 
        }
        private IOverlayViewModel? _currentOverlay;

        private List<IOverlayViewModel> _overlayStack = new();


        public void Show(IOverlayViewModel overlayViewModel)
        {
            if (_overlayStack.Contains(overlayViewModel))
                _overlayStack.Remove(overlayViewModel);
            
            _overlayStack.Add(overlayViewModel);
            CurrentOverlay = overlayViewModel;
        }

        public void Close(IOverlayViewModel overlayViewModel)
        {
            _overlayStack.Remove(overlayViewModel);
            CurrentOverlay = _overlayStack.LastOrDefault();
        }
    }
}
