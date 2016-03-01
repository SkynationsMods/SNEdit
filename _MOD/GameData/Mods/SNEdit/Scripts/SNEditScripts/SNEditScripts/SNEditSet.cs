using SharedGameData;
using SNScript;
using System;
using System.Linq;
using PreciseMaths;
using System.Collections.Generic;
using SNScriptUtils;

namespace SNEdit
{
    class Set : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//set" }; }
        }

        public override string CommandDescription
        {
            get
            {
                return "Fills an area with the given Block ID.";
            }
        }
        
        public override Priviledges Priviledge
        {
            get { return Priviledges.Player; }
        }

        public Set(IGameServer server) : base(server) { }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            if (!_Utils.checkParameterCount(parameters, 1, actor))
                return false;

            ushort blockID = ushort.Parse(parameters[1]);

            IBiomeSystem checkSystem = Server.Biomes.GetSystems()[actor.InstanceID];
            IChunk checkChunk = checkSystem.ChunkCollection[0];
            if (!_Utils.blockTypeExists(checkChunk, blockID, actor))
                return false; 

            Point3D pos1 = new Point3D(); Point3D pos2 = new Point3D();
            if (!_Utils.checkStoredPositions(actor, out pos1, out pos2))
                return false;

            //calculate absolute distance (absdiff) and direction (valinc) to get from Point1 to Point2
            int absdiffx; int valincx; int absdiffy; int valincy; int absdiffz; int valincz;
            _Utils.calcAbsDiffAndValinc(pos1.X, pos2.X, out absdiffx, out valincx);
            _Utils.calcAbsDiffAndValinc(pos1.Y, pos2.Y, out absdiffy, out valincy);
            _Utils.calcAbsDiffAndValinc(pos1.Z, pos2.Z, out absdiffz, out valincz);

            Dictionary<Point3D, ushort> fakeGlobalPosAndBlockID = new Dictionary<Point3D, ushort>();
            for (int y = 0; y <= (absdiffy); y++)
            {
                for (int z = 0; z <= (absdiffz); z++)
                {
                    for (int x = 0; x <= (absdiffx); x++)
                    {   //Dictionary contains <fakeGlobalPos, blockID>
                        fakeGlobalPosAndBlockID.Add(new Point3D((pos1.X + (x * valincx)), (pos1.Y + (y * valincy)), (pos1.Z + (z * valincz))), blockID);
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
