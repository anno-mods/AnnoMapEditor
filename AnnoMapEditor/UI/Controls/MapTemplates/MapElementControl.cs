using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Windows;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class MapElementControl : DraggingControl
    {
        private MapElementViewModel _viewModel => DataContext as MapElementViewModel
            ?? throw new Exception($"DataContext of {nameof(MapElementControl)} must extend {nameof(MapElementViewModel)}.");


        public MapElementControl()
        {
            DataContextChanged += _DataContextChanged;
        }


        private void _DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MapElementViewModel oldViewModel)
                oldViewModel.Element.PropertyChanged -= Element_PropertyChanged;

            if (e.NewValue is MapElementViewModel newViewModel)
            {
                newViewModel.Element.PropertyChanged += Element_PropertyChanged;

                // adapt the element's position
                this.SetPosition(newViewModel.Element.Position);
            }
        }

        private void Element_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapElement.Position))
            {
                this.SetPosition(_viewModel.Element.Position);
            }
        }
    }
}
