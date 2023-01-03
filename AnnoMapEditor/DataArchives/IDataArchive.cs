using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public interface IDataArchive
    {
        bool IsValid { get; }
        string Path { get; }

        Stream? OpenRead(string filePath);
        IEnumerable<string> Find(string pattern);
        Task LoadAsync();
    }
}
