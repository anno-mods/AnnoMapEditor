using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Dragging;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    /// <summary>
    /// Interaktionslogik für PlayableAreaControl.xaml
    /// </summary>
    public partial class PlayableAreaControl : DraggingControl
    {
        public PlayableAreaControl(MapTemplate mapTemplate)
        {
            ViewModel = new PlayableAreaViewModel(mapTemplate);
            mapTemplate.MapSizeConfigChanged += OnMapTemplateResized;

            DataContext = ViewModel;
            IsEnabled = false;
            InitializeComponent();

            PositionAndScale();
        }

        public PlayableAreaViewModel ViewModel { get; }

        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (DataContext is PlayableAreaViewModel viewModel)
            {
                viewModel.ResizingInProgress = false;
            }
        }

        private void Splitter_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (DataContext is PlayableAreaViewModel viewModel)
            {
                viewModel.ResizingInProgress = true;
            }
        }

        private void MarginLeftTop_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.MarginLeftTop = (int)e.NewSize.Width;
        }

        private void MarginRightTop_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.MarginRightTop = (int)e.NewSize.Height;
        }

        private void OnMapTemplateResized(object? sender, MapTemplate.MapTemplateResizeEventArgs args)
        {
            PositionAndScale();
        }


        private void PositionAndScale()
        {
            Width = ViewModel.ControlWidth;
            Height = ViewModel.ControlHeight;

            Canvas.SetLeft(this, ViewModel.PosX);
            Canvas.SetTop(this, ViewModel.PosY);
        }

        public void SetShowPlayableAreaMargins(bool value)
        {
            ViewModel.ShowPlayableAreaMargins = value;
        }
    }
}
