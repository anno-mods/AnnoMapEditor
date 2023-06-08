using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Mods.Enums;
using AnnoMapEditor.Mods.Models;
using AnnoMapEditor.Mods.Serialization;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

        public MapTemplate MapTemplate { get; init; }

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

        public bool ShowMapTypeSelection { get; init; }

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

        public IEnumerable<MapType> AllowedMapTypes { get; init; } = MapType.All;


        public ExportAsModViewModel(MapTemplate mapTemplate)
        {
            MapTemplate = mapTemplate;

            ShowMapTypeSelection = mapTemplate.Session == SessionAsset.OldWorld;
            if (ShowMapTypeSelection)
                SelectedMapType = AllowedMapTypes.First();

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
            string? modsFolderPath = Settings.Instance.DataPath;
            if (modsFolderPath is not null)
                modsFolderPath = Path.Combine(modsFolderPath, "mods");

            if (!Directory.Exists(modsFolderPath))
            {
                _logger.LogWarning("mods/ path not set. This shouldn't have happened.");
                return false;
            }

            Mod mod = new(ResultingModName, ModID, MapTemplate, SelectedMapType);

            CheckExistingMod();

            try
            {
                IsSaving = true;
                ModWriter modWriter = new();
                await modWriter.WriteAsync(mod, Path.Combine(modsFolderPath, ResultingFullModName));

                OverlayService.Instance.Close(this);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Failed to save the mod.\n\nIt looks like some files are locked, possibly by another application.\n\nThe mod may be broken now.", App.TitleShort, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to save the mod.\n\nThe mod may be broken now.", App.TitleShort, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}
