namespace AnnoMapEditor.UI.Overlays
{
    public class OverlayEventArgs
    {
        public IOverlayViewModel OverlayViewModel { get; }


        public OverlayEventArgs(IOverlayViewModel overlayViewModel)
        {
            OverlayViewModel = overlayViewModel;
        }
    }
}
