using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Models;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI
{
    public partial class ExportAsMod : UserControl
    {
        public ExportAsModViewModel ViewModel { get; init; } = new();

        public ExportAsMod()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            DataContext = ViewModel;
        }

        public void Show(Session session)
        {
            ViewModel.Session = session;
            Visibility = Visibility.Visible;
            nameInput.Focus();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {   
            if (await ViewModel.Save())
                Visibility = Visibility.Collapsed;
        }

        static readonly Regex regex = new(@"[^\w ,\(\)\-_]");
        private void HintTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }

        private void HintTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string? text = e.DataObject.GetData(typeof(string)) as string;
                if (text is not null && regex.IsMatch(text))
                {
                    DataObject obj = new();
                    obj.SetData(DataFormats.Text, regex.Replace(text, ""));
                    e.DataObject = obj;
                }
            }
            else
                e.CancelCommand();
        }
    }
}
