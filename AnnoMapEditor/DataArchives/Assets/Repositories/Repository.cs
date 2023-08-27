using AnnoMapEditor.Utilities;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public abstract class Repository : ObservableBase
    {
        public Repository()
        {
        }


        public abstract Task InitializeAsync();
    }
}
