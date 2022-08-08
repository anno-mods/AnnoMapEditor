using AnnoMapEditor.UI.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI
{
    public partial class CreateNewMap : UserControl
    {
        public CreateNewMapViewModel ViewModel { get; init; } = new();

        public event EventHandler<CreateNewMapEventArgs>? CreateNewMapEvent;

        public CreateNewMap()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            DataContext = ViewModel;
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {   
            CreateNewMapEvent?.Invoke(this, new CreateNewMapEventArgs(ViewModel.MapSize, ViewModel.PlayableSize));

            Visibility = Visibility.Collapsed;
        }
    }

    public class CreateNewMapEventArgs : EventArgs
    {
        public CreateNewMapEventArgs(int mapSize, int playableSize)
        {
            MapSize = mapSize;
            PlayableSize = playableSize;
        }

        public int MapSize { get; }
        public int PlayableSize { get; }
    }
}
