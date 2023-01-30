using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.UI.Controls.MapTemplates;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    /// <summary>
    /// Interaction logic for RandomIslandProperties.xaml 
    /// </summary>
    public partial class RandomIslandProperties : UserControl
    {
        private static readonly Dictionary<IslandType, List<IslandSize>> _allowedSizesPerType = new()
        {
            [IslandType.Normal] = new() { IslandSize.Large, IslandSize.Medium, IslandSize.Small },
            [IslandType.ThirdParty] = new() { IslandSize.Small },
            [IslandType.PirateIsland] = new() { IslandSize.Small }
        };


        private RandomIslandViewModel _viewModel => DataContext as RandomIslandViewModel
            ?? throw new Exception($"DataContext of {nameof(RandomIslandProperties)} must extend {nameof(RandomIslandViewModel)}.");
        
        
        public RandomIslandProperties()
        {
            InitializeComponent();
        }
    }
}
