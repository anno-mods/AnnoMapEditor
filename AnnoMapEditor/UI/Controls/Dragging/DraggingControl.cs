using AnnoMapEditor.Utilities;
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
            Vector2 localMousePos = new(Mouse.GetPosition(this));
            Rect2 actualSize = new(0, 0, (int)ActualWidth, (int)ActualHeight);

            if (localMousePos.Within(actualSize))
            {
                _viewModel.BeginDrag(localMousePos);
                CaptureMouse();
            }
            else
            {
                ReleaseMouseCapture();
                _viewModel.EndDrag();
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_viewModel.IsDragging)
            {
                ReleaseMouseCapture();
                _viewModel.EndDrag();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _viewModel.IsDragging)
            {
                Vector2 localMousePos = new(e.GetPosition(this));
                _viewModel.ContinueDrag(localMousePos);
            }
        }
    }
}
