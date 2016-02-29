using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEdit
{
    public class Schematic
    {
        public Schematic() { }
        public Schematic(int Height, int Length, int Width, ushort[] blocks)
        {
            this.Height = Height;
            this.Length = Length;
            this.Width = Width;
            this.blocks = blocks;
        }

        public int Height;
        public int Length;
        public int Width;
        public int WEOffsetX;
        public int WEOffsetY;
        public int WEOffsetZ;

        public ushort[] blocks;
    }
}
