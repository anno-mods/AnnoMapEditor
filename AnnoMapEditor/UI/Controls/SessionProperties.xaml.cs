using AnnoMapEditor.MapTemplates;
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
    }
}
