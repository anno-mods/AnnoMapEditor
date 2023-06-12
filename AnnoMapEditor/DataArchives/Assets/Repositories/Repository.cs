using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public abstract class Repository
    {
        public Repository()
        {
        }


        public abstract Task Initialize();
    }
}
