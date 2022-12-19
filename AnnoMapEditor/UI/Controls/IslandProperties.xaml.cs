using AnnoMapEditor.MapTemplates;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls
{
    public class UserIslandType
    {
        public static readonly UserIslandType Small = new("Small Island", IslandSize.Small, IslandType.Normal);
        public static readonly UserIslandType Medium = new("Medium Island", IslandSize.Medium, IslandType.Normal);
        public static readonly UserIslandType Large = new("Large Island", IslandSize.Large, IslandType.Normal);
        public static readonly UserIslandType ThirdParty = new("3rd Party Slot", IslandSize.Small, IslandType.ThirdParty);
        public static readonly UserIslandType Pirate = new("Pirate Slot", IslandSize.Small, IslandType.PirateIsland);
        //public static readonly UserIslandType LargeStarter = new("Larger Starter") { Size = IslandSize.Large, Type = IslandType.Starter };
        //public static readonly UserIslandType MediumStarter = new("Medium Starter") { Size = IslandSize.Medium, Type = IslandType.Starter };

        public static readonly UserIslandType[] All = new[] {
            //LargeStarter,
            //MediumStarter,
            Large,
            Medium,
            Small,
            ThirdParty,
            Pirate
        };

        public string Name { get; init; }
        public IslandSize Size { get; init; }
        public IslandType Type { get; init; }

        public UserIslandType(string name, IslandSize size, IslandType type)
        {
            Name = name;
            Type = type;
            Size = size;
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
            IsStarterCheckBox.ValueChanged += OnIsStarterCheckChanged;
        }

        private void OnIsStarterCheckChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Island island)
                return;

            island.IsStarter = IsStarterCheckBox.IsChecked;
        }

        private void OnTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is Island island && TypeComboBox.SelectedItem is UserIslandType type)
            {
                if (type.Size != island.Size || !type.Type.IsSameWithoutOil(island.Type))
                {
                    island.Size = type.Size;
                    if (type.Size == IslandSize.Small || !island.Type.IsSameWithoutOil(type.Type))
                        island.Type = type.Type;

                    // triggers reselection from pool
                    island.RandomizeIslandMap();
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_island is not null)
            {
                _island.PropertyChanged -= Island_PropertyChanged;
                _island = null;
            }

            if (DataContext is Island island)
            {
                UpdateIslandProperties(island);
                _island = island;
                _island.PropertyChanged += Island_PropertyChanged;
            }
        }

        private void Island_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (_island != null)
                UpdateIslandProperties(_island);
        }

        private void UpdateIslandProperties(Island island)
        {
            Header.Text = island.ElementType == 2 ? "Start" : "Island";
            IslandProps.Visibility = island.ElementType == 2 ? Visibility.Collapsed : Visibility.Visible;

            IsStarterCheckBox.IsChecked = island.IsStarter;
            IsStarterCheckBox.IsEnabled = !island.IsNew && (island.Type == IslandType.Starter || island.Type == IslandType.Normal) && island.Size != IslandSize.Small;

            TypeComboBox.IsEnabled = !island.IsNew;
            TypeComboBox.Visibility = Visibility.Visible;
            if (island.IsPool && island.Type.IsNormalOrStarter)
            {
                // standard islands
                if (island.Size == IslandSize.Large)
                    TypeComboBox.SelectedItem = UserIslandType.Large;
                else if (island.Size == IslandSize.Medium)
                    TypeComboBox.SelectedItem = UserIslandType.Medium;
                else
                    TypeComboBox.SelectedItem = UserIslandType.Small;
            }
            //else if (island.IsPool && island.Type == IslandType.Starter)
            //{
            //    // starter islands with oil
            //    if (island.Size == IslandSize.Large)
            //        TypeComboBox.SelectedItem = UserIslandType.Large;
            //    else
            //        TypeComboBox.SelectedItem = UserIslandType.Medium;
            //    // no small allowed
            //}
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
