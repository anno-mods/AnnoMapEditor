using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.UI
{
    public class ExportAsModViewModel : ViewModelBase
    {
        //public Visibility DialogVisibility
        //{
        //    get => _DialogVisibility;
        //    private set => SetProperty(ref _DialogVisibility, value);
        //}
        //private Visibility _DialogVisibility = Visibility.Collapsed; 

        public MapTemplates.Session Session { get; set; }

        public string ModName
        {
            get => _modName;
            set
            {
                SetProperty(ref _modName, value, new string[] { "CanExport" });
                ModExistsWarning = ModExists(value) ? Visibility.Visible : Visibility.Hidden;
            }
        }
        private string _modName = "";

        public string ModID
        {
            get => _modID;
            set => SetProperty(ref _modID, value);
        }
        private string _modID = "";

        public bool CanExport => ModName.Trim() != string.Empty;

        public Visibility ModExistsWarning
        {
            get => _modExistsWarning;
            set => SetProperty(ref _modExistsWarning, value);
        }
        private Visibility _modExistsWarning = Visibility.Hidden;

        public Mods.MapType SelectedMapType
        {
            get => _mapType;
            set => SetProperty(ref _mapType, value);
        }
        private Mods.MapType _mapType = Mods.MapType.Archipelago;

        public IEnumerable<Mods.MapType> AllowedMapTypes
        {
            get => _allowedMapTypes;
            set => SetProperty(ref _allowedMapTypes, value);
        }
        private IEnumerable<Mods.MapType> _allowedMapTypes = Mods.MapTypes.GetOldWorldTypes();

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
            return await mod.Save(modsFolderPath, ModName, ModID);
        }
    }
}
