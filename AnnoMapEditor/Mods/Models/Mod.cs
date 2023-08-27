using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Mods.Enums;

/*
 * Modloader doesn't support a7t because they are loaded as .rda archive.
 * They specified with "mods/[Map] xyz/data/..."
 * Mistakes lead to endless loading.
 * 
 * The same map file path can't be used for differente TemplateSize at the same time.
 * Leads to endless loading.
 * I assume it's because first a pool is created, then maps are assigned to their group leading to an empty list for some groups.
 * 
 * corners_ll_01, snowflake_ll_01 are unused. do they work?
 */

namespace AnnoMapEditor.Mods.Models
{
    public class Mod
    {
        public const string AME_POOL_PATH = @"data\ame\maps\pool";


        public string Name { get; init; }

        public string? Id { get; init; }

        public MapTemplate MapTemplate { get; init; }

        public MapType? MapType { get; init; }


        public Mod(string name, string? modId, MapTemplate mapTemplate, MapType? mapType)
        {
            Name = name;
            Id = modId;
            MapTemplate = mapTemplate;
            MapType = mapType;
        }
    }
}
