using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls
{
    public partial class MapTemplateProperties : UserControl
    {
        public MapTemplateProperties()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void ZoomResetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is MapTemplatePropertiesViewModel viewModel)
            {
                viewModel.ResetZoom();
            }
        }
        
        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (DataContext is MapTemplatePropertiesViewModel viewModel)
            {
                viewModel.DragInProgress = false;
            }
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (DataContext is MapTemplatePropertiesViewModel viewModel)
            {
                viewModel.DragInProgress = true;
            }
        }
    }
}
