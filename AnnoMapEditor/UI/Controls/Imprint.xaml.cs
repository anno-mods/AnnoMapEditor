using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AnnoMapEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for Imprint.xaml
    /// </summary>
    public partial class Imprint : UserControl
    {
        public Imprint()
        {
            InitializeComponent();
        }

        private void Hyperlink_OpenBrowser(object sender, RequestNavigateEventArgs e)
        {
            var info = new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
            };
            Process.Start(info);
        }
    }
}
