using SharedGameData;
using SNScript;
using GameServer;
using System;
using System.Linq;
using PreciseMaths;
using GameServer.World.Chunks;
using System.Collections.Generic;
using SNScriptUtils;

namespace SNEdit
{
    class SNEditSet : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//set" }; }
        }

        public override string CommandDescription
        {
            get
            {
                return "A test command in ScriptsExample.";
            }
        }

        public override Priviledges Priviledge
        {
            get { return Priviledges.Player; }
        }

        public SNEditSet(IGameServer server) : base(server) { }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            try
            {
                //ID of block to change to
                ushort blockID = new ushort();

                //Only takes 2 arguments //set and blockID
                if (parameters.Length > 2)
                {
                    Server.ChatManager.SendActorMessage("Too many arguments for this command.", actor);
                    return false;
                }


                IBiomeSystem checkSystem = null as IBiomeSystem;
                Server.Biomes.GetSystems().TryGetValue(actor.InstanceID, out checkSystem);
                IChunk checkChunk = checkSystem.ChunkCollection[0];

                if (checkChunk.GetTileData().Keys.Contains(blockID))
                {
                    blockID = ushort.Parse(parameters[1]);
                }
                else
                {
                    Server.ChatManager.SendActorMessage("Invalid block ID entered, Blocktype not found.", actor);
                    return false;
                }

                //initialize two position variables for user input
                Point3D pos1 = new Point3D(); Point3D pos2 = new Point3D();
                //read sessionvars
                if (!SNScriptUtils._Utils.checkPositions(actor, out pos1, out pos2))
                {
                    return false;
                }

                //calculate absolute distance (absdiff) and direction (valinc) to get from Point1 to Point2
                int absdiffx; int valincx; int absdiffy; int valincy; int absdiffz; int valincz;
                _Utils.calcAbsDiffAndValinc(pos1.X, pos2.X, out absdiffx, out valincx);
                _Utils.calcAbsDiffAndValinc(pos1.Y, pos2.Y, out absdiffy, out valincy);
                _Utils.calcAbsDiffAndValinc(pos1.Z, pos2.Z, out absdiffz, out valincz);

                /* Inform Player about stats */
                Point3D outdiff = new Point3D(absdiffx, absdiffy, absdiffz);
                Server.ChatManager.SendActorMessage("diff: " + outdiff.ToString(), actor);
                Server.ChatManager.SendActorMessage("pos1: " + pos1.ToString(), actor);
                Server.ChatManager.SendActorMessage("pos2: " + pos2.ToString(), actor);

                IChunk currentChunk = new Object() as IChunk;
                SNScriptUtils._Utils.getChunkObjFromFakeGlobalPos(pos1, actor, out currentChunk);
                Point3D ChunkPos = SNScriptUtils._Utils.GetChunkKeyFromFakeGlobalPos(pos1.ToDoubleVector3);

                int testx = pos1.X - ChunkPos.X; int testy = pos1.Y - ChunkPos.Y; int testz = pos1.Z - ChunkPos.Z;
                Point3D testpos = new Point3D(testx, testy, testz);

                Dictionary<Point3D, ushort> fakeGlobalPosAndBlockID = new Dictionary<Point3D, ushort>();

                for (int x = 0; x <= (absdiffx); x++)
                {
                    for (int y = 0; y <= (absdiffy); y++)
                    {
                        for (int z = 0; z <= (absdiffz); z++)
                        {
                            //Dictionary contains <fakeGlobalPos, blockID>
                            fakeGlobalPosAndBlockID.Add(new Point3D((pos1.X + (x * valincx)), (pos1.Y + (y * valincy)), (pos1.Z + (z * valincz))), blockID);
                        }
                    }
                }

                Dictionary<Point3D, Dictionary<Point3D, ushort>> BlocksToBePlacedInSystem = new Dictionary<Point3D, Dictionary<Point3D, ushort>>();
                _Utils.SplitFakeGlobalPosBlocklistIntoChunksAndLocalPos(fakeGlobalPosAndBlockID, out BlocksToBePlacedInSystem); 

                if (_Utils.PlaceBlocksInSystem(BlocksToBePlacedInSystem, checkSystem, false, (ushort)0))
                    return true;
                else
                    return false;

            }
            catch (Exception e)
            {
                Console.Write(e);
                return false;
            }
        }
    }
}
