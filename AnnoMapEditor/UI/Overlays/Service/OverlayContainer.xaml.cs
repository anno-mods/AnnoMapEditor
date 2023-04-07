using AnnoMapEditor.UI.Overlays.ExportAsMod;
using AnnoMapEditor.UI.Overlays.SelectFertilities;
using AnnoMapEditor.UI.Overlays.SelectIsland;
using AnnoMapEditor.UI.Overlays.SelectSlots;
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
                SelectFertilitiesViewModel _ => new SelectFertilitiesOverlay(),
                SelectIslandViewModel _ => new SelectIslandOverlay(),
                SelectSlotsViewModel _ => new SelectSlotsOverlay(),
                _ => throw new ArgumentException()
            };
            overlayControl.DataContext = e.OverlayViewModel;

            ContentHost.Children.Add(overlayControl);
        }

        private void OnOverlayClosed(object? sender, OverlayEventArgs e)
        {
            UserControl? closedControl = null;
            foreach (UserControl overlayControl in ContentHost.Children)
            {
                if (overlayControl.DataContext == e.OverlayViewModel)
                {
                    closedControl = overlayControl;
                    break;
                }
            }

            if (closedControl != null)
            {
                closedControl.DataContext = null;
                ContentHost.Children.Remove(closedControl);
            }
        }
    }
}
