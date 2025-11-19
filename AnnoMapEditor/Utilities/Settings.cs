using Microsoft.Win32;
using System.IO;

namespace AnnoMapEditor.Utilities
{
    public class Settings : ObservableBase
    {
        public static Settings Instance { get; } = new();

        public bool Quickstart
        {
            get => UserSettings.Default.Quickstart;
            set
            {
                if (value != Quickstart)
                {
                    UserSettings.Default.Quickstart = value;
                    UserSettings.Default.Save();
                    OnPropertyChanged(nameof(Quickstart));
                }
            }
        }

        public string? GamePath 
        {
            get => UserSettings.Default.GamePath;
            set
            {
                if (value != GamePath)
                {
                    UserSettings.Default.GamePath = value;
                    UserSettings.Default.Save();

                    if (value != null)
                    {
                        if (DataPath == null || !EnableExpertMode)
                            DataPath = Path.Combine(value, "maindata");

                        if (ModsPath == null || !EnableExpertMode)
                            ModsPath = Path.Combine(value, "mods");
                    }

                    OnPropertyChanged(nameof(GamePath));
                }
            }
        }

        public string? DataPath
        {
            get => UserSettings.Default.DataPath;
            set
            {
                if (value != DataPath)
                {
                    UserSettings.Default.DataPath = value;
                    UserSettings.Default.Save();
                    OnPropertyChanged(nameof(DataPath));
                }
            }
        }

        public string? ModsPath
        {
            get => UserSettings.Default.ModsPath;
            set
            {
                if (value != ModsPath)
                {
                    UserSettings.Default.ModsPath = value;
                    UserSettings.Default.Save();
                    OnPropertyChanged(nameof(ModsPath));
                }
            }
        }

        public bool EnableExpertMode
        {
            get => UserSettings.Default.EnableExpertMode;
            set
            {
                if (value != EnableExpertMode)
                {
                    UserSettings.Default.EnableExpertMode = value;
                    UserSettings.Default.Save();
                    OnPropertyChanged(nameof(EnableExpertMode));
                }
            }
        }


        private Settings()
        {
            if (GamePath == null)
                GamePath = GetInstallDirFromRegistry();
        }


        public static string? GetInstallDirFromRegistry()
        {
            string installDirKey = @"SOFTWARE\WOW6432Node\Ubisoft\Anno 1800";
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(installDirKey);

            string? installDir = key?.GetValue("InstallDir") as string;
            if (installDir == null)
                return null;

            if (!installDir.Contains(Path.DirectorySeparatorChar))
            {
                char wrongSeparator = Path.DirectorySeparatorChar == '/' ? '\\' : '/';
                return installDir.Replace(wrongSeparator, Path.DirectorySeparatorChar);
            }
            else
                return installDir;
        }
    }
}
