using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Windows;
using System.Windows.Input;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class MapElementControl : DraggingControl
    {
        private MapElementViewModel _viewModel => DataContext as MapElementViewModel
            ?? throw new Exception($"DataContext of {nameof(MapElementControl)} must extend {nameof(MapElementViewModel)}.");


        public MapElementControl()
        {
            DataContextChanged += This_DataContextChanged;
        }


        private void This_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _viewModel.IsSelected = true;

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            _viewModel.IsSelected = false;

            base.OnMouseRightButtonUp(e);
        }
    }
}
