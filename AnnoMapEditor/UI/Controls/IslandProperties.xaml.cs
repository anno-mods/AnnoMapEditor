using AnnoMapEditor.MapTemplates;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls
{

    public class UserIslandType
    {
        public static readonly UserIslandType Small = new("Small Island") { Size = IslandSize.Small, Type = IslandType.Normal };
        public static readonly UserIslandType Medium = new("Medium Island") { Size = IslandSize.Medium, Type = IslandType.Normal };
        public static readonly UserIslandType Large = new("Large Island") { Size = IslandSize.Large, Type = IslandType.Normal };
        public static readonly UserIslandType ThirdParty = new("3rd Party Slot") { Size = IslandSize.Small, Type = IslandType.ThirdParty };
        public static readonly UserIslandType Pirate = new("Pirate Slot") { Size = IslandSize.Small, Type = IslandType.PirateIsland };
        public static readonly UserIslandType LargeStarter = new("Larger Starter") { Size = IslandSize.Large, Type = IslandType.Starter };
        public static readonly UserIslandType MediumStarter = new("Medium Starter") { Size = IslandSize.Medium, Type = IslandType.Starter };

        public static readonly UserIslandType[] All = new[] {
            LargeStarter,
            MediumStarter,
            Large,
            Medium,
            Small,
            ThirdParty,
            Pirate
        };

        public string Name { get; init; }
        public IslandSize? Size { get; init; }
        public IslandType? Type { get; init; }

        public UserIslandType(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Interaction logic for IslandProperties.xaml
    /// </summary>
    public partial class IslandProperties : UserControl
    {
        Island? _island;

        public IslandProperties()
        {
            InitializeComponent();

            TypeComboBox.ItemsSource = UserIslandType.All;

            DataContextChanged += OnDataContextChanged;
            TypeComboBox.SelectionChanged += OnTypeSelectionChanged;
        }

        private void OnTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is Island island && TypeComboBox.SelectedItem is UserIslandType type)
            {
                if (type.Size is not null && type.Size != island.Size ||
                    type.Type is not null && type.Type != island.Type)
                {
                    if (type.Size is not null)
                        island.Size = (IslandSize)type.Size;
                    if (type.Type is not null)
                        island.Type = (IslandType)type.Type;
                    island.MapPath = null;

                    // triggers reselection from pool
                    island.UpdateAsync().ContinueWith((x) => { });
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_island is not null)
            {
                _island.IslandChanged -= Island_IslandChanged;
                _island = null;
            }

            if (DataContext is Island island)
            {
                UpdateIslandProperties(island);
                _island = island;
                _island.IslandChanged += Island_IslandChanged;
            }
        }

        private void Island_IslandChanged()
        {
            if (_island != null)
                UpdateIslandProperties(_island);
        }

        private void UpdateIslandProperties(Island island)
        {
            Header.Text = island.ElementType == 2 ? "Start" : "Island";
            IslandProps.Visibility = island.ElementType == 2 ? Visibility.Collapsed : Visibility.Visible;

            TypeComboBox.IsEnabled = !island.IsNew;

            TypeComboBox.Visibility = Visibility.Visible;
            if (island.IsPool && island.Type == IslandType.Normal)
            {
                // standard islands
                if (island.Size == IslandSize.Large)
                    TypeComboBox.SelectedItem = UserIslandType.Large;
                else if (island.Size == IslandSize.Medium)
                    TypeComboBox.SelectedItem = UserIslandType.Medium;
                else
                    TypeComboBox.SelectedItem = UserIslandType.Small;
            }
            else if (island.IsPool && island.Type == IslandType.Starter)
            {
                // starter islands with oil
                if (island.Size == IslandSize.Large)
                    TypeComboBox.SelectedItem = UserIslandType.LargeStarter;
                else
                    TypeComboBox.SelectedItem = UserIslandType.MediumStarter;
                // no small allowed
            }
            else if (island.Type == IslandType.PirateIsland)
            {
                TypeComboBox.SelectedItem = UserIslandType.Pirate;
            }
            else if (island.Type == IslandType.ThirdParty)
            {
                TypeComboBox.SelectedItem = UserIslandType.ThirdParty;
            }
            else
            {
                TypeComboBox.SelectedItem = null;
                TypeComboBox.Visibility = Visibility.Collapsed;
            }
        }
    }
}
