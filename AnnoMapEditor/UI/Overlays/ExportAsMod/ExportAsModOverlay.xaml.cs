using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Overlays.ExportAsMod
{
    public partial class ExportAsModOverlay : UserControl
    {
        private ExportAsModViewModel _viewModel => DataContext as ExportAsModViewModel
            ?? throw new Exception($"DataContext of {nameof(ExportAsModOverlay)} must extend {nameof(ExportAsModViewModel)}.");

        public ExportAsModOverlay()
        {
            InitializeComponent();
            nameInput.Focus();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OverlayService.Instance.Close(_viewModel);
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.Save();
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
