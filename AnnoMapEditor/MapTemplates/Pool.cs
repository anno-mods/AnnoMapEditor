namespace AnnoMapEditor.MapTemplates
{
    public class Pool
    {
        public FilePathRange[] paths { get; init; }
        public int size
        {
            get
            {
                int sum = 0;
                foreach(var path in paths)
                {
                    sum += path.size;
                }
                return sum;
            }
        }

        public string GetPath(int i)
        {
            int rangeIdx = 0;
            FilePathRange range = paths[rangeIdx];
            int skipped = 0;
            while(skipped + range.size <= i)
            {
                skipped += range.size;
                range = paths[++rangeIdx];
            }

            return range.GetPath(i - skipped);
        }

        public Pool(string filePath, int size)
        {
            this.paths = new FilePathRange[]
            {
                new FilePathRange(filePath, 1, size)
            };
        }

        public Pool(string filePath, int[] ids)
        {
            this.paths = new FilePathRange[]
            {
                new FilePathRange(filePath, ids)
            };
        }

        public Pool(FilePathRange[] paths)
        {
            this.paths = paths;
        }
    }
}
