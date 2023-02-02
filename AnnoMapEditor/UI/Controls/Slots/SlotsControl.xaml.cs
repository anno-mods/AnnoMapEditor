using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.SelectSlots;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls.Slots
{
    /// <summary>
    /// Interaction logic for SlotsControl.xaml
    /// </summary>
    public partial class SlotsControl : UserControl
    {
        public SlotsControl()
        {
            InitializeComponent();
        }


        public void OnConfigureSlots_Cicked(object sender, RoutedEventArgs args)
        {
            SlotsViewModel viewModel = (SlotsViewModel)DataContext;
            SelectSlotsViewModel selectViewModel = new(viewModel.Region, viewModel.FixedIsland);
            OverlayService.Instance.Show(selectViewModel);
        }
    }
}
