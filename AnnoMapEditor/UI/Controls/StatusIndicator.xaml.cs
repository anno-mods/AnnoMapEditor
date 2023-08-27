using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for StatusIndicator.xaml
    /// </summary>
    public partial class StatusIndicator : UserControl
    {
        public static readonly DependencyProperty IsOkProperty =
            DependencyProperty.Register("IsOk", typeof(bool),
            typeof(StatusIndicator), new FrameworkPropertyMetadata(false));

        public bool IsOk
        {
            get { return (bool)GetValue(IsOkProperty); }
            set { SetValue(IsOkProperty, value); }
        }

        public static readonly DependencyProperty IsErrorProperty =
            DependencyProperty.Register("IsError", typeof(bool),
            typeof(StatusIndicator), new FrameworkPropertyMetadata(false));

        public bool IsError
        {
            get { return (bool)GetValue(IsErrorProperty); }
            set { SetValue(IsErrorProperty, value); }
        }
        
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool),
            typeof(StatusIndicator), new FrameworkPropertyMetadata(false));

        public bool IsLoading
        {
            get { return (bool) GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }


        public StatusIndicator()
        {
            InitializeComponent();
        }
    }
}
