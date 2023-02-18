using AnnoMapEditor.MapTemplates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    /// <summary>
    /// Interaktionslogik für PlayableAreaControl.xaml
    /// </summary>
    public partial class PlayableAreaControl : DraggingControl
    {
        public PlayableAreaControl(Session session)
        {
            ViewModel = new PlayableAreaViewModel(session);
            session.MapSizeConfigChanged += OnSessionResized;

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

        private void OnSessionResized(object? sender, Session.SessionResizeEventArgs args)
        {
            PositionAndScale();
        }


        private void PositionAndScale()
        {
            this.Width = ViewModel.ControlWidth;
            this.Height = ViewModel.ControlHeight;

            Canvas.SetLeft(this, ViewModel.PosX);
            Canvas.SetTop(this, ViewModel.PosY);
        }

        public void SetShowPlayableAreaMargins(bool value)
        {
            ViewModel.ShowPlayableAreaMargins = value;
        }
    }
}
