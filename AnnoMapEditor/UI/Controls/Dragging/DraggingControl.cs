using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnnoMapEditor.UI.Controls.Dragging
{
    public abstract class DraggingControl : UserControl
    {
        private DraggingViewModel _viewModel => DataContext as DraggingViewModel
            ?? throw new Exception($"DataContext of {nameof(DraggingControl)} must extend {nameof(DraggingViewModel)}.");


        public DraggingControl()
        {
            DataContextChanged += _DataContextChanged;
        }


        private void _DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is DraggingViewModel oldViewModel)
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            if (e.NewValue is DraggingViewModel newViewModel)
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DraggingViewModel.IsDragging))
            {
                if (_viewModel.IsDragging)
                    Mouse.Capture(this);
                else
                    Mouse.Capture(null);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point mouseOffset = Mouse.GetPosition(this);
            _viewModel.BeginDrag(mouseOffset);

            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_viewModel.IsDragging)
                _viewModel.EndDrag();

            ReleaseMouseCapture();

            e.Handled = true;
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _viewModel.IsDragging)
            {
                Point mouseOffset = e.GetPosition(this);
                _viewModel.ContinueDrag(mouseOffset);

                e.Handled = true;
            }
        }
    }
}
