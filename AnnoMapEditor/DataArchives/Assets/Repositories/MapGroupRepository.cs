using AnnoMapEditor.UI.Windows.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public class MapGroupRepository : Repository
    {
        public IEnumerable<MapGroup> MapGroups 
        {
            get => _mapGroups ?? throw new Exception("MapRepository has not yet been initialized.");
            private set => SetProperty(ref _mapGroups, value);
        }
        private IEnumerable<MapGroup>? _mapGroups;

        private readonly IDataArchive _dataArchive;


        public MapGroupRepository(IDataArchive dataArchive)
        {
            _dataArchive = dataArchive;
        }


        public override Task InitializeAsync()
        {
            IEnumerable<string> mapTemplatePaths = _dataArchive.Find("*.a7tinfo");

            MapGroups = new MapGroup[]
            {
                new MapGroup("Campaign", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/campaign")), new(@"\/campaign_([^\/]+)\.")),
                new MapGroup("Moderate, Archipelago", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_archipel")), new(@"\/([^\/]+)\.")),
                new MapGroup("Moderate, Atoll", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_atoll")), new(@"\/([^\/]+)\.")),
                new MapGroup("Moderate, Corners", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_corners")), new(@"\/([^\/]+)\.")),
                new MapGroup("Moderate, Island Arc", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_islandarc")), new(@"\/([^\/]+)\.")),
                new MapGroup("Moderate, Snowflake", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_snowflake")), new(@"\/([^\/]+)\.")),
                new MapGroup("New World, Large", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_l_")), new(@"\/([^\/]+)\.")),
                new MapGroup("New World, Medium", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_m_")), new(@"\/([^\/]+)\.")),
                new MapGroup("New World, Small", mapTemplatePaths.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_s_")), new(@"\/([^\/]+)\.")),
                new MapGroup("DLCs", mapTemplatePaths.Where(x => !x.StartsWith(@"data/sessions/")), new(@"data\/([^\/]+)\/.+\/maps\/([^\/]+)"))
                //new MapGroup("Moderate", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate")), new(@"\/([^\/]+)\."))
            };

            return Task.CompletedTask;
        }
    }
}
