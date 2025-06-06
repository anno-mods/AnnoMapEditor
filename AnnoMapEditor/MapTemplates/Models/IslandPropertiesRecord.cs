using AnnoMapEditor.MapTemplates.Enums;

namespace AnnoMapEditor.MapTemplates.Models
{
    public record IslandPropertiesRecord
    {
        public IslandPropertiesRecord()
        {
            
        }
        
        public IslandType IslandType { get; set; }
        
    }
}