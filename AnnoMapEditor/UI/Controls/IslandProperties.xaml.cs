using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.UI.Controls.MapTemplates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for IslandProperties.xaml 
    /// </summary>
    public partial class IslandProperties : UserControl
    {
        private static readonly Dictionary<IslandType, List<IslandSize>> _allowedSizesPerType = new()
        {
            [IslandType.Normal] = new() { IslandSize.Large, IslandSize.Medium, IslandSize.Small },
            [IslandType.ThirdParty] = new() { IslandSize.Small },
            [IslandType.PirateIsland] = new() { IslandSize.Small }
        };


        private RandomIslandViewModel _viewModel => DataContext as RandomIslandViewModel
            ?? throw new Exception($"DataContext of {nameof(DraggingControl)} must extend {nameof(DraggingViewModel)}.");


        public IslandProperties()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }


        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is RandomIslandViewModel oldViewModel)
            {
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                oldViewModel.RandomIsland.PropertyChanged -= Island_PropertyChanged;
            }

            if (e.NewValue is RandomIslandViewModel newViewModel)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
                newViewModel.RandomIsland.PropertyChanged += Island_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
//            if (_randomIslandViewModel != null)
//                UpdateIslandProperties(_randomIslandViewModel);
        }

        private void Island_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // ensure the I
        }

//        private void OnRotateClockwise(object sender, RoutedEventArgs args)
//        {
//            _viewModel.RandomIsland.Rotation = island.Rotation + 1 % 4;
//        }
//
//        private void OnRotateCounterClockwise(object sender, RoutedEventArgs args)
//        {
//            if (DataContext is Island island)
//            {
//                island.Rotation = island.Rotation - 1 % 4;
//                island.UpdateExternalData();
//            }
//        }

        private void UpdateIslandProperties(Island island)
        {
            Header.Text = island.IsStarter ? "Start" : "Island";
            IslandProps.Visibility = island.IsStarter ? Visibility.Collapsed : Visibility.Visible;

//            TypeComboBox.IsEnabled = !island.IsNew;
//            TypeComboBox.Visibility = Visibility.Visible;
//            if (island.IsPool && island.Type.IsNormalOrStarter)
//            {
//                // standard islands
//                if (island.Size == IslandSize.Large)
//                    TypeComboBox.SelectedItem = UserIslandType.Large;
//                else if (island.Size == IslandSize.Medium)
//                    TypeComboBox.SelectedItem = UserIslandType.Medium;
//                else
//                    TypeComboBox.SelectedItem = UserIslandType.Small;
//            }
//            //else if (island.IsPool && island.Type == IslandType.Starter)
//            //{
//            //    // starter islands with oil
//            //    if (island.Size == IslandSize.Large)
//            //        TypeComboBox.SelectedItem = UserIslandType.Large;
//            //    else
//            //        TypeComboBox.SelectedItem = UserIslandType.Medium;
//            //    // no small allowed
//            //}
//            else if (island.Type == IslandType.PirateIsland)
//            {
//                TypeComboBox.SelectedItem = UserIslandType.Pirate;
//            }
//            else if (island.Type == IslandType.ThirdParty)
//            {
//                TypeComboBox.SelectedItem = UserIslandType.ThirdParty;
//            }
//            else
//            {
//                TypeComboBox.SelectedItem = null;
//                TypeComboBox.Visibility = Visibility.Collapsed;
//            }
        }
    }
}
