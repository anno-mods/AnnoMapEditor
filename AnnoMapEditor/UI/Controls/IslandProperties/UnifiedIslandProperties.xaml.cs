using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoMapEditor.MapTemplates.Enums;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    public partial class UnifiedIslandProperties : UserControl
    {
        private UnifiedIslandPropertiesViewModel ViewModel => DataContext as UnifiedIslandPropertiesViewModel
            ?? throw new Exception($"DataContext of {nameof(UnifiedIslandProperties)} must extend {nameof(UnifiedIslandPropertiesViewModel)}.");
        public UnifiedIslandProperties()
        {
            InitializeComponent();
        }

        private void IslandTypeComboBox_OnDropDownClosed(object? sender, EventArgs e)
        {
            ViewModel.ChangeIslandType(IslandTypeComboBox.SelectedItem as IslandType ?? IslandType.Normal);
        }

        private void IslandSizeComboBox_OnDropDownClosed(object? sender, EventArgs e)
        {
            ViewModel.ChangeIslandSize(IslandSizeComboBox.SelectedItem as IslandSize ?? IslandSize.Large);
        }

        private void OnRotateCounterClockwise(object sender, RoutedEventArgs e)
        {
            ViewModel.RotateIsland(false);
        }

        private void OnRotateClockwise(object sender, RoutedEventArgs e)
        {
            ViewModel.RotateIsland(true);
        }

        private void LabelTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is not (Key.Enter or Key.Return)) return;
            e.Handled = true;
            (sender as TextBox)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}