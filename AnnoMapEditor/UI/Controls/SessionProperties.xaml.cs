using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.UI.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Controls
{
    public partial class SessionProperties : UserControl
    {
        public SessionProperties()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (DataContext is Models.SessionPropertiesViewModel viewModel)
            {
                viewModel.DragInProgress = false;
            }
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (DataContext is Models.SessionPropertiesViewModel viewModel)
            {
                viewModel.DragInProgress = true;
            }
        }

        private void FancyToggle_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is Models.SessionPropertiesViewModel viewModel && viewModel.IsMarginNonCentered)
            {
                viewModel.Center();
            }
        }
    }
}
