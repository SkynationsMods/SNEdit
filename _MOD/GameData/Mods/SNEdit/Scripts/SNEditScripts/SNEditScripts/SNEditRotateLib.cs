using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEdit
{
    class SNEditRotateLib
    {

        public static int[] patternA = { 0, 2, 1, 3 };
        public static int[] patternB = { 0, 3, 2, 1 };
        public static int[] patternC = { 0, 1 };
        

        //block and metadataarray from the schematic are the inputs, rotate ranges from 0 to 3, for 0, 90, 180, 270 degree rotation (clockwise)
        public static byte[] applyRotationToMCArray(byte[] blockData, byte[] metaData, int rotate)
        {
            for (int i = 0; i < blockData.Length - 1; i++)
            {

                
            }

            return metaData;
        }

        public static byte[] applyRotationToSNArray(byte[] tileData, int rotate)
        {
            for (int i = 0; i < tileData.Length - 1; i++)
            {
                switch (tileData[i])
                {
                    case (24):
                        //PatternA: 24 59 147 151 155 171 175 179 183 187 191 195 199 203 207 211 215 219 223 227 231 240 7070 7074 7078 7082 7086 7120 7124 7130 7134 7140 7144 7150 7154
                        //PatternB: 312 316 320 324 328 332 336 340
                        //PatternC: 245 248 250 7017 7019
                        break;
                }
            }

            return tileData;
        }
        



    }
}
