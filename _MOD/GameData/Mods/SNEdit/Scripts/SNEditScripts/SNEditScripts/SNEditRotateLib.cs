using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEdit
{
    class RotateLib
    {

        private static ushort[] patternA = { 0, 2, 1, 3 };
        private static ushort[] patternB = { 0, 3, 2, 1 };
        private static ushort[] patternC = { 0, 1 };

        //almost all Blocks with 4 Tiles follow this pattern. Examples: Sign, Wall Tiles, all Wedges etc.
        private static ushort[] ABlocks = { 24, 59, 147, 151, 155, 171, 175, 179, 183, 187, 191, 195, 199, 203, 207, 211, 215, 219, 223, 227, 231, 240, 9800, 9804, 9808, 9812, 9816, 11400, 11404, 11408, 11412, 11416, 11420, 11424, 11428 };
        //only h-wedges, no idea why Ben made them follow a new pattern ..
        private static ushort[] BBlocks = { 312, 316, 320, 324, 328, 332, 336, 340 };
        //Railings
        private static ushort[] CBlocks = { 245, 248, 250, 8400, 8402 };

        private static Dictionary<ushort[], ushort[]> PatternsTilesDic = new Dictionary<ushort[], ushort[]> { 
            {patternA, ABlocks},
            {patternB, BBlocks},
            {patternC, CBlocks}
        };

        //block and metadataarray from the schematic are the inputs, rotate ranges from 0 to 3, for 0, 90, 180, 270 degree rotation (clockwise)

        public static ushort[] applyRotationToSNArray(ushort[] tileData, int rotate)
        {
            Dictionary<ushort, KeyValuePair<ushort, ushort[]>> TileAndBaseIDNOffsets = new Dictionary<ushort, KeyValuePair<ushort, ushort[]>>();

            foreach (KeyValuePair<ushort[], ushort[]> offsetsAndTiles in PatternsTilesDic)
            {
                foreach (ushort tile in offsetsAndTiles.Value)
                {
                    for (int i = 0; i < offsetsAndTiles.Key.Count(); i++)
                    {
                        //End result is a Dictionary that holds all tileIDs that are part of a pattern (key) together with the base value and the pattern to follow (value)
                        TileAndBaseIDNOffsets.Add((ushort)(tile + offsetsAndTiles.Key[i]), new KeyValuePair<ushort, ushort[]>((ushort)tile, offsetsAndTiles.Key));
                        //obviously this will fail when there are two tileIDs which belong to two different patterns (which is not possible, and likely a mistake done during array setup on top of this file)
                    }
                }
            }

            for (int i = 0; i < tileData.Length; i++)
            {
                ushort value = tileData[i];

                if (TileAndBaseIDNOffsets.Keys.Contains(tileData[i]))
                {
                    ushort offset = (ushort)(tileData[i] - (TileAndBaseIDNOffsets[tileData[i]]).Key);
                    int index = Array.IndexOf(
                            (TileAndBaseIDNOffsets[tileData[i]]).Value,
                            offset
                        );
                    int rotatedIndex = ((index + rotate) % ((TileAndBaseIDNOffsets[tileData[i]]).Value).Count());

                    value = (ushort)((TileAndBaseIDNOffsets[tileData[i]]).Key + ((TileAndBaseIDNOffsets[tileData[i]]).Value)[rotatedIndex]);
                }

                tileData[i] = value;
            }

            return tileData;
        }
    }
}
