using SharedGameData;
using SNScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNScriptUtils;

namespace SNEdit
{
    class Replace : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//replace" }; }
        }

        public override string CommandDescription
        {
            get
            {
                return "Command to replace all Blocks of a certain type with a new one.";
            }
        }

        public override Priviledges Priviledge
        {
            get { return Priviledges.Admin; }
        }

        public Replace(IGameServer server)
            : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            ushort replaceThisBlockID = new ushort();
            ushort newBlockID = new ushort();
            bool replaceAllSolids = false;

            if (parameters.Length == 3)
            {
                replaceThisBlockID = ushort.Parse(parameters[1]);
                newBlockID = ushort.Parse(parameters[2]);
            }

            if (parameters.Length == 2)
            {
                replaceAllSolids = true;
                replaceThisBlockID = 0;
                newBlockID = ushort.Parse(parameters[1]);
            }
               
            if (!_Utils.checkParameterCount(parameters, 2, actor))
                return false;

            IBiomeSystem checkSystem = Server.Biomes.GetSystems()[actor.InstanceID];
            IChunk checkChunk = checkSystem.ChunkCollection[0];

            if (!_Utils.blockTypeExists(checkChunk, replaceThisBlockID, actor))
                return false;

            if (!_Utils.blockTypeExists(checkChunk, newBlockID, actor))
                return false;

            Point3D pos1 = new Point3D(); Point3D pos2 = new Point3D();
            if (!_Utils.checkStoredPositions(actor, out pos1, out pos2))
                return false;

            Point3D posOrigin = _Utils.calcCuboidOrigin(pos1, pos2);
            Point3D cuboidDimensions = _Utils.calcCuboidDimensions(pos1, pos2);

            Dictionary<Point3D, IChunk> chunkDictionary = _Utils.CreateChunkDictionary(checkSystem);
            Dictionary<Point3D, ushort> fakeGlobalPosAndBlockID = new Dictionary<Point3D, ushort>();

            ushort blockID;
            Point3D tmpPoint;
            for (int y = 0; y < (cuboidDimensions.Y); y++)
            {
                for (int z = 0; z < (cuboidDimensions.Z); z++)
                {
                    for (int x = 0; x < (cuboidDimensions.X); x++)
                    {
                        tmpPoint = new Point3D(posOrigin.X + x, posOrigin.Y + y, posOrigin.Z + z);
                        _Utils.GetBlockIdAtFakeGlobalPos(chunkDictionary, tmpPoint, out blockID);
                        if ((blockID == replaceThisBlockID) && !replaceAllSolids)
                            fakeGlobalPosAndBlockID.Add(tmpPoint, newBlockID);
                        if ((blockID != 0) && replaceAllSolids)
                            fakeGlobalPosAndBlockID.Add(tmpPoint, newBlockID);
                    }
                }
            }

            Dictionary<Point3D, Dictionary<Point3D, ushort>> BlocksToBePlacedInSystem = new Dictionary<Point3D, Dictionary<Point3D, ushort>>();
            if (!_Utils.SplitFakeGlobalPosBlocklistIntoChunksAndLocalPos(fakeGlobalPosAndBlockID, out BlocksToBePlacedInSystem))
                return false;

            if (_Utils.PlaceBlocksInSystem(BlocksToBePlacedInSystem, checkSystem))
                return true;
            else
                return false;
        }
    }
}
