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
using System.Windows.Shapes;

namespace AnnoMapEditor.UI.Windows.SelectIsland
{
    /// <summary>
    /// Interaction logic for SelectIslandOverlay.xaml
    /// </summary>
    public partial class SelectIslandOverlay : UserControl
    {
        public SelectIslandOverlay()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;

            DataContextChanged += This_DataContextChanged;
        }


        private void This_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
