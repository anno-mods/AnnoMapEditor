using AnnoMapEditor.DataArchives.Assets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public class AssetRepository
    {
        private static Dictionary<Type, AssetRepository> _repositories = new();


        public static AssetRepository<T> Get<T>()
            where T : StandardAsset
        {
            if (!_repositories.TryGetValue(typeof(T), out AssetRepository? result))
            {
                result = new AssetRepository<T>();
                _repositories.Add(typeof(T), result);
            }

            return (AssetRepository<T>) result;
        }
    }
}
