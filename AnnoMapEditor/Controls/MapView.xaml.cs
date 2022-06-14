using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AnnoMapEditor.MapTemplates;

namespace AnnoMapEditor.Controls
{
    public partial class MapView : UserControl
    {
        Session? session;

        public MapView()
        {
            InitializeComponent();

            SizeChanged += MapView_SizeChanged;
            DataContextChanged += MapView_DataContextChanged;

            Utils.Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        private void MapView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIslands(DataContext as Session);
        }

        private async void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is not Session session)
                return;

            await session.UpdateExternalDataAsync();
            UpdateIslands(session);
        }

        private void MapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateIslands(Session? session)
        {
            this.session = session;
            sessionCanvas.Children.Clear();
            if (session is null)
                return;

            sessionCanvas.Children.Add(new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 3, 19, 28)),
                Width = session.Size.X,
                Height = session.Size.Y
            });
            var playableArea = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 24, 43, 63)),
                Width = session.PlayableArea.Width,
                Height = session.PlayableArea.Height
            };
            Canvas.SetLeft(playableArea, session.PlayableArea.X);
            Canvas.SetTop(playableArea, session.Size.Y - session.PlayableArea.Height - session.PlayableArea.Y);
            sessionCanvas.Children.Add(playableArea);

            double requiredScaleX = sessionCanvas.ActualWidth / session.Size.X;
            double requiredScaleY = sessionCanvas.ActualHeight / session.Size.Y;
            float scale = (float)Math.Min(requiredScaleX, requiredScaleY);
            sessionCanvas.RenderTransform = new ScaleTransform(scale, scale);
            //sessionCanvas.Scale = new Vector3(scale, scale, 1);

            var islands = session.Islands.Where(x => !x.Hide);
            foreach (var island in islands)
            {
                var obj = new MapObject(session)
                {
                    DataContext = island
                };
                sessionCanvas.Children.Add(obj);
            }

            UpdateSize();
        }

        void UpdateSize()
        {
            if (session is null)
                return;

            double size = Math.Min(ActualWidth, ActualHeight);
            size = Math.Sqrt((size * size) / 2);

            double requiredScaleX = size / session.Size.X;
            double requiredScaleY = size / session.Size.Y;
            float scale = (float)Math.Min(requiredScaleX, requiredScaleY);

            sessionCanvas.RenderTransform = new ScaleTransform(scale, scale);
            //sessionCanvas.Scale = new Vector3(scale, scale, 1);
            rotationCanvas.Width = scale * session.Size.X;
            rotationCanvas.Height = scale * session.Size.Y;
        }
    }
}
