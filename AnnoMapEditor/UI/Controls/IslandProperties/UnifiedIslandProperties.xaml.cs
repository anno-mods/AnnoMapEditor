using System;
using System.Windows;
using System.Windows.Controls;
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
    }
}