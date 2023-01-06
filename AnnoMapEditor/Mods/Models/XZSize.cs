namespace AnnoMapEditor.Mods.Models
{
    public class XZSize
    {
        /// <summary>
        /// Parameterless Constructor needed for XMLSerializer
        /// </summary>
        private XZSize()
        {

        }

        public XZSize(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; set; }

        public int Z { get; set; }
    }
}
