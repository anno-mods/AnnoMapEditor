using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Mods.Enums;
using AnnoMapEditor.Mods.Models;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.UI.Overlays.ExportAsMod
{
    public class ExportAsModViewModel : ObservableBase, IOverlayViewModel
    {
        private static readonly Logger<ExportAsModViewModel> _logger = new();

        enum ModStatus
        {
            NotFound,
            Active,
            Inactive
        }

        public MapTemplate? MapTemplate
        { 
            get => _mapTemplate;
            set
            {
                _mapTemplate = value;
                if(_mapTemplate is not null)
                {
                    AllowedMapTypes = MapType.All;
                    SelectedMapType = AllowedMapTypes.First();
                }
                else
                {
                    AllowedMapTypes = Enumerable.Empty<MapType>();
                    SelectedMapType = null;
                }
                InfoMapTypeSelection = _mapTemplate is not null && _mapTemplate.Session == SessionAsset.OldWorld;
                CheckExistingMod();
            }
        }
        MapTemplate? _mapTemplate;

        public bool IsSaving
        {
            get => _isSaving;
            private set => SetProperty(ref _isSaving, value);
        }
        private bool _isSaving;

        public string ModName
        {
            get => _modName;
            set
            {
                SetProperty(ref _modName, value);
                CheckExistingMod();
            }
        }
        private string _modName = string.Empty;

        public string ResultingModName { get; private set; } = string.Empty;

        public string ResultingFullModName { get; private set; } = string.Empty;

        public string ModExistsWarning { get; private set; } = string.Empty;

        public bool InfoMapTypeSelection
        {
            get => _infoMapTypeSelection;
            set => SetProperty(ref _infoMapTypeSelection, value);
        }
        private bool _infoMapTypeSelection = true;

        public string ModID
        {
            get => _modID;
            set => SetProperty(ref _modID, value);
        }
        private string _modID = string.Empty;

        public MapType? SelectedMapType
        {
            get => _mapType;
            set
            {
                SetProperty(ref _mapType, value);
                CheckExistingMod();
            }
        }
        private MapType? _mapType = null;

        public IEnumerable<MapType> AllowedMapTypes
        {
            get => _allowedMapTypes;
            set => SetProperty(ref _allowedMapTypes, value);
        }
        private IEnumerable<MapType> _allowedMapTypes = Enumerable.Empty<MapType>();

        public ExportAsModViewModel()
        {
            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.IsLoading))
                CheckExistingMod();
        }

        private void CheckExistingMod()
        {
            ResultingModName = (_modName.Trim() == string.Empty ? $"Custom {_mapType}" : _modName);
            string modName = "[Map] " + ResultingModName;
            ModStatus status = ModExists(modName);
            if (status == ModStatus.Inactive)
                modName = "-" + modName;
            ResultingFullModName = modName;
            ModExistsWarning = status != ModStatus.NotFound ? $"Replace existing \"{ResultingFullModName}\"" : "";
            OnPropertyChanged(nameof(ResultingModName));
            OnPropertyChanged(nameof(ModExistsWarning));
        }

        private static ModStatus ModExists(string modName)
        {
            if (Settings.Instance.DataPath is null)
                return ModStatus.NotFound;

            string activeModPath = Path.Combine(Settings.Instance.DataPath, "mods", modName);
            if (Directory.Exists(activeModPath))
                return ModStatus.Active;

            string inactiveModPath = Path.Combine(Settings.Instance.DataPath, "mods", "-" + modName);
            return Directory.Exists(inactiveModPath) ? ModStatus.Inactive : ModStatus.NotFound;
        }

        public async Task<bool> Save()
        {
            IsSaving = true;

            string? modsFolderPath = Settings.Instance.DataPath;
            if (modsFolderPath is not null)
                modsFolderPath = Path.Combine(modsFolderPath, "mods");

            if (!Directory.Exists(modsFolderPath) || MapTemplate is null)
            {
                _logger.LogWarning("mods/ path or session not set. This shouldn't have happened.");
                return false;
            }

            Mod mod = new(MapTemplate, SelectedMapType);

            CheckExistingMod();
            bool result = await mod.Save(Path.Combine(modsFolderPath, ResultingFullModName), ResultingModName, ModID);

            OverlayService.Instance.Close(this);

            IsSaving = false;
            return result;
        }
    }
}
