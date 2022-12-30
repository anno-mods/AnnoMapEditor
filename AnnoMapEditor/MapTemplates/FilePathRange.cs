using System.Linq;

namespace AnnoMapEditor.MapTemplates
{
    public struct FilePathRange
    {
        public string filePath;
        public int size;
        public int[] ids;

        public string GetPath(int i)
        {
            return string.Format(filePath, string.Format("{0:00}", ids[i]));
        }

        public FilePathRange(string filePath, int start, int count)
        {
            this.filePath = filePath;
            this.size = count;
            this.ids = Enumerable.Range(start, count).ToArray();
        }

        public FilePathRange(string filePath, int[] ids)
        {
            this.filePath = filePath;
            this.size = ids.Length;
            this.ids = ids;
        }
    }
}
