using AnnoMapEditor.DataArchives.Assets.Deserialization;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate("SessionModerate", "SessionSouthAmerica", "SessionArctic")]
    public class SessionAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "Session";

        public const long SESSION_OLDWORLD_GUID = 180023;
        public const long SESSION_NEWWORLD_GUID = 180025;
        public const long SESSION_SUNKENTREASURES_GUID = 110934;
        public const long SESSION_ARCTIC_GUID = 180045;
        public const long SESSION_ENBESA_GUID = 112132;


        [StaticAsset(SESSION_OLDWORLD_GUID)]
        public static SessionAsset OldWorld { get; private set; }

        [StaticAsset(SESSION_NEWWORLD_GUID)]
        public static SessionAsset NewWorld { get; private set; }

        [StaticAsset(SESSION_SUNKENTREASURES_GUID)]
        public static SessionAsset CapeTrelawney { get; private set; }

        [StaticAsset(SESSION_ARCTIC_GUID)]
        public static SessionAsset Arctic { get; private set; }

        [StaticAsset(SESSION_ENBESA_GUID)]
        public static SessionAsset Enbesa { get; private set; }

        public static IEnumerable<SessionAsset> SupportedSessions => new[] { OldWorld, NewWorld, CapeTrelawney, Arctic, Enbesa };


        /// <summary>
        /// The session assets for The Old World and The New World to not properly reference their
        /// respective regions in assets.xml.
        /// </summary>
        private static readonly Dictionary<long, long> REGIONID_HARDCODED = new()
        {
            [SESSION_OLDWORLD_GUID] = RegionAsset.REGION_MODERATE_GUID, // The Old World => Moderate
            [SESSION_NEWWORLD_GUID] = RegionAsset.REGION_SOUTHAMERICA_GUID  // The New World => Colony01
        };


        public string DisplayName { get; init; }

        public long? MapTemplateGuid { get; init; }

        [GuidReference(nameof(MapTemplateGuid))]
        public MapTemplateAsset? MapTemplate { get; set; }

        public long? MapTemplateForMultiplayerGuid { get; init; }

        [GuidReference(nameof(MapTemplateForMultiplayerGuid))]
        public MapTemplateAsset? MapTemplateForMultiplayer { get; set; }

        public long RegionGuid { get; init; }

        [GuidReference(nameof(RegionGuid))]
        public RegionAsset Region { get; set; }


        public SessionAsset(XElement valuesXml)
            : base(valuesXml)
        {
            DisplayName = valuesXml.Element("Text")!
                .Element("LocaText")?
                .Element("English")!
                .Element("Text")!
                .Value!
                ?? "Meta";

            XElement sessionValues = valuesXml.Element(TEMPLATE_NAME)
                ?? throw new Exception($"XML is not a valid {nameof(SessionAsset)}. It does not have '{TEMPLATE_NAME}' section in its values.");

            string? mapTemplateGuidStr = sessionValues.Element(nameof(MapTemplate))?.Value;
            if (mapTemplateGuidStr != null)
            {
                if (long.TryParse(mapTemplateGuidStr, out long mapTemplateGuid))
                    MapTemplateGuid = mapTemplateGuid;
                else
                    throw new Exception($"XML is not a valid {nameof(SessionAsset)}. Invalid value '{mapTemplateGuidStr}' for attribute '{nameof(MapTemplate)}'.");
            }

            string? mapTemplateForMultiplayerGuidStr = sessionValues.Element(nameof(MapTemplateForMultiplayer))?.Value;
            if (mapTemplateForMultiplayerGuidStr != null)
            {
                if (long.TryParse(mapTemplateForMultiplayerGuidStr, out long mapTemplateForMultiplayerGuid))
                    MapTemplateForMultiplayerGuid = mapTemplateForMultiplayerGuid;
                else
                    throw new Exception($"XML is not a valid {nameof(SessionAsset)}. Invalid value '{mapTemplateGuidStr}' for attribute '{nameof(MapTemplateForMultiplayer)}'.");
            }

            string? regionGuidStr = sessionValues.Element(nameof(Region))?.Value;
            if (regionGuidStr != null)
            {
                if (long.TryParse(regionGuidStr, out long regionGuid))
                    RegionGuid = regionGuid;
                else
                    throw new Exception($"XML is not a valid {nameof(SessionAsset)}. Invalid value '{regionGuidStr}' for attribute '{nameof(Region)}'.");
            }
            // special handling for The Old World and The New World
            else if (REGIONID_HARDCODED.ContainsKey(GUID))
                RegionGuid = REGIONID_HARDCODED[GUID];
            // default to Moderate
            else
                RegionGuid = RegionAsset.REGION_MODERATE_GUID;
        }


        public static SessionAsset DetectFromPath(string filePath)
        {
            if (filePath.Contains("colony01") || filePath.Contains("ggj") || filePath.Contains("scenario03"))
                return NewWorld;
            else if (filePath.Contains("dlc03") || filePath.Contains("colony_03"))
                return Arctic;
            else if (filePath.Contains("dlc06") || filePath.Contains("colony02") || filePath.Contains("scenario02"))
                return Enbesa;
            else if (filePath.Contains("sunken_treasures"))
                return CapeTrelawney;
            else
                return OldWorld;
        }


        public override string ToString() => DisplayName;
    }
}
