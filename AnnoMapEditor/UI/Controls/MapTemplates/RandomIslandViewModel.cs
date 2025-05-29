using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using System.ComponentModel;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class RandomIslandViewModel : IslandViewModel
    {
        public RandomIslandElement RandomIsland { get; init; }

        public override string? Label => _label;
        private string? _label;

        public RandomIslandViewModel(MapTemplate mapTemplate, RandomIslandElement randomIsland)
            : base(mapTemplate, randomIsland)
        {
            RandomIsland = randomIsland;

            UpdateLabel();

            RandomIsland.PropertyChanged += RandomIsland_PropertyChanged;
        }


        private void RandomIsland_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IslandElement.IslandType) || e.PropertyName == nameof(IslandElement.Label))
                UpdateLabel();
        }


        private void UpdateLabel()
        {
            // use the island's label if it has one
            if (RandomIsland.Label != null)
                _label = RandomIsland.Label;

            else
            {
                string label = RandomIsland.IslandType == IslandType.Normal 
                    ? $"Random\n{RandomIsland.IslandSize.Name}"
                    : $"Random\n{RandomIsland.IslandType.Name}";

                if (RandomIsland.IslandType == IslandType.Starter)
                    label += "\nwith Oil";

                _label = label;
            }

            OnPropertyChanged(nameof(Label));
        }
    }
}
