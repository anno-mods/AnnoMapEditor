using AnnoMapEditor.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnnoMapEditor.UI.Controls
{
    public abstract class DraggingControl : UserControl
    {
        private DraggingViewModel _viewModel => DataContext as DraggingViewModel
            ?? throw new Exception($"DataContext of {nameof(DraggingControl)} must extend {nameof(DraggingViewModel)}.");


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Vector2 mouseOffset = new(Mouse.GetPosition(this));
            _viewModel.BeginDrag(mouseOffset);

            base.OnMouseLeftButtonDown(e);
            Mouse.Capture(this);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _viewModel.EndDrag();

            base.OnMouseLeftButtonUp(e);
            Mouse.Capture(null);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _viewModel.IsDragging)
            {
                IInputElement parentElement = Parent as IInputElement
                    ?? throw new Exception($"Parent of {nameof(DraggingControl)} must be an {nameof(IInputElement)}.");
                Vector2 newPosition = new Vector2(e.GetPosition(parentElement)) - _viewModel.MouseOffset!;
                _viewModel.OnDragged(newPosition);
            }
        }
    }
}
