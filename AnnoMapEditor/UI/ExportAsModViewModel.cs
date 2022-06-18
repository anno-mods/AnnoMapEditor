using System.IO;
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

        private static bool ModExists(string modName)
        {
            if (Settings.Instance.DataPath is null)
                return false;

            string modPath = Path.Combine(Settings.Instance.DataPath, "mods", "[Map] " + modName);
            return Directory.Exists(modPath);
        }
    }
}
