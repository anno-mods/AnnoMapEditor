using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.UI.Models
{
    public class ExportAsModViewModel : ViewModelBase
    {
        public MapTemplates.Session? Session { get; set; }

        public string ModName
        {
            get => _modName;
            set => SetProperty(ref _modName, value, new string[] { nameof(CanExport), nameof(ResultingModName), nameof(ModExistsWarning) });
        }
        private string _modName = "";

        public string ResultingModName => _modName.Trim() == string.Empty ? $"Custom {_mapType}" : _modName;
        public bool CanExport => true;
        public string ModExistsWarning => ModExists(ResultingModName) ? $"Replace existing \"[Map] {ResultingModName}\"" : "";

        public string ModID
        {
            get => _modID;
            set => SetProperty(ref _modID, value);
        }
        private string _modID = "";

        public Mods.MapType SelectedMapType
        {
            get => _mapType;
            set => SetProperty(ref _mapType, value, new string[] { nameof(ResultingModName), nameof(ModExistsWarning) });
        }
        private Mods.MapType _mapType = Mods.MapType.Archipelago;

        public IEnumerable<Mods.MapType> AllowedMapTypes
        {
            get => _allowedMapTypes;
            set => SetProperty(ref _allowedMapTypes, value);
        }
        private IEnumerable<Mods.MapType> _allowedMapTypes = Mods.MapType.GetOldWorldTypes();

        public ExportAsModViewModel()
        {
            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.DataPath))
                OnPropertyChanged(nameof(ModExistsWarning));
        }

        private static bool ModExists(string modName)
        {
            if (Settings.Instance.DataPath is null)
                return false;

            string modPath = Path.Combine(Settings.Instance.DataPath, "mods", "[Map] " + modName);
            return Directory.Exists(modPath);
        }

        public async Task<bool> Save()
        {
            string? modsFolderPath = Settings.Instance.DataPath;
            if (modsFolderPath is not null)
                modsFolderPath = Path.Combine(modsFolderPath, "mods");

            if (!Directory.Exists(modsFolderPath) || Session is null)
            {
                Log.Warn("mods/ path or session not set. This shouldn't have happened.");
                return false;
            }

            Mods.Mod mod = new(Session)
            {
                MapType = SelectedMapType
            };
            return await mod.Save(modsFolderPath, ResultingModName, ModID);
        }
    }
}
