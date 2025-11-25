using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Providers;

namespace AnnoMapEditor
{
    public partial class App : Application
    {
        public readonly static string TitleShort = "Community Map Editor";
        public readonly static string SubTitle = "for Anno 1800 and 117";
        public readonly static string Title = $"{TitleShort} {SubTitle}";

        public App() {
            ResxLocalizationProvider provider = ResxLocalizationProvider.Instance;

            provider.SearchCultures = new List<System.Globalization.CultureInfo>()
            {
                System.Globalization.CultureInfo.GetCultureInfo("en"),
                System.Globalization.CultureInfo.GetCultureInfo("de"),
            };
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en");

            int i = 0;
        }
    }
}
