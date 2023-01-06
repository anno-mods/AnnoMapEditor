using AnnoMapEditor.Mods.Models;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnnoMapEditor.Mods.Serialization
{
    [Serializable, XmlRoot("AnnoEditorLevel")]
    public class AnnoEditorLevel
    {
        public int FileVersion { get; set; }

        public XZSize Dimensions { get; set; }

        public XZSize ChunkSize { get; set; }

        [XmlArrayItem("Column", NestingLevel = 0)]
        [XmlArrayItem("Chunk", NestingLevel = 1)]
        public List<List<string>> Chunks { get; set; }


        /// <summary>
        /// Parameterless Constructor needed for XMLSerializer
        /// </summary>
        private AnnoEditorLevel()
        {
            Dimensions = new XZSize(0, 0);
            ChunkSize = new XZSize(0, 0);
            Chunks = new List<List<string>>();
        }

        public AnnoEditorLevel(int dimensions)
        {
            FileVersion = 3;
            Dimensions = new XZSize(dimensions, dimensions);
            ChunkSize = new XZSize(64, 64);

            int chunkCount = dimensions / 64;
            Chunks = new List<List<string>>(chunkCount);

            for (int cntColumn = 0; cntColumn < chunkCount; cntColumn++)
            {
                List<string> column = new();

                for (int cntChunk = 0; cntChunk < chunkCount; cntChunk++)
                {
                    column.Add(cntColumn + "x" + cntChunk);
                }

                Chunks.Add(column);
            }
        }

    }
}
