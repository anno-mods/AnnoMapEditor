using AnnoMapEditor.UI.Overlays.ExportAsMod;
using AnnoMapEditor.UI.Overlays.SelectIsland;
using System;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Overlays
{
    /// <summary>
    /// Interaction logic for OverlayContainer.xaml
    /// </summary>
    public partial class OverlayContainer : UserControl
    {
        public OverlayContainer()
        {
            OverlayService.Instance.OverlayShown += OnOverlayShown;
            OverlayService.Instance.OverlayClosed += OnOverlayClosed;

            DataContext = OverlayService.Instance;

            InitializeComponent();
        }


        private void OnOverlayShown(object? sender, OverlayEventArgs e)
        {
            UserControl overlayControl = e.OverlayViewModel switch
            {
                ExportAsModViewModel _ => new ExportAsModOverlay(),
                SelectIslandViewModel _ => new SelectIslandOverlay(),
                _ => throw new ArgumentException()
            };
            overlayControl.DataContext = e.OverlayViewModel;

            Content.Children.Add(overlayControl);
        }

        private void OnOverlayClosed(object? sender, OverlayEventArgs e)
        {
            UserControl? closedControl = null;
            foreach (UserControl overlayControl in Content.Children)
            {
                if (overlayControl.DataContext == e.OverlayViewModel)
                {
                    closedControl = overlayControl;
                    break;
                }
            }

            if (closedControl != null)
                Content.Children.Remove(closedControl);
        }
    }
}
