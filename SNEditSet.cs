using SharedGameData;
using SNScript;
using GameServer;
using System;
using System.Linq;
using PreciseMaths;
using GameServer.World.Chunks;
using System.Collections.Generic;
using MoreBlocksScripts;

namespace MoreBlocksScripts
{
    class SNEditSet : GameCommand
    {

        private SNEdit helper;

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

        public SNEditSet(IGameServer server)
            : base(server)
        {   //constructor
            this.helper = new MoreBlocksScripts.SNEdit();
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            //ID of block to change to
            ushort blockID = new ushort();

            //Only takes 2 arguments //set and blockID
            if (parameters.Length > 2)
            {
                Server.ChatManager.SendActorMessage("Too many arguments.", actor);
                return false;
            }

            //Check if entered blockID is valid TODO
            if (true)
            {
                blockID = ushort.Parse(parameters[1]);
            }
            else
            {
                Server.ChatManager.SendActorMessage("Invalid block entered.", actor);
                return false;
            }

            //2 positions for what the user set
            Point3D pos1 = new Point3D();
            Point3D pos2 = new Point3D();

            //Maybe add this into a class later?
            if (!helper.checkPositions(actor, out pos1, out pos2))
            {
                return false;
            }

            //List of blocks need to be placed
            List<Point3D> locationList = new List<Point3D>();

            int diffx = pos2.X - pos1.X;
            int diffy = pos2.Y - pos1.Y;
            int diffz = pos2.Z - pos1.Z;
            int absdiffx = System.Math.Abs(diffx);
            int absdiffy = System.Math.Abs(diffy);
            int absdiffz = System.Math.Abs(diffz);
            int valincx = new int();
            int valincy = new int();
            int valincz = new int();
            if (absdiffx != 0) { valincx = diffx / absdiffx; } else { valincx = absdiffx; };
            if (absdiffy != 0) { valincy = diffy / absdiffy; } else { valincy = absdiffy; };
            if (absdiffz != 0) { valincz = diffz / absdiffz; } else { valincz = absdiffz; };

            /* */
            Point3D outdiff = new Point3D(diffx, diffy, diffz);
            Server.ChatManager.SendActorMessage("diff: " + outdiff.ToString(), actor);
            Server.ChatManager.SendActorMessage("pos1: " + pos1.ToString(), actor);
            Server.ChatManager.SendActorMessage("pos2: " + pos2.ToString(), actor);
            /* */
            
            IChunk  currentChunk = helper.getChunkObjFromGlobalPos(pos1, actor);
            Point3D ChunkPos = helper.GetChunkKeyFromGlobalPos(pos1.ToDoubleVector3);

            int testx = pos1.X - ChunkPos.X;
            int testy = pos1.Y - ChunkPos.Y;
            int testz = pos1.Z - ChunkPos.Z;
            Point3D testpos = new Point3D(testx, testy, testz);

            Server.ChatManager.SendActorMessage("ChunkPos: "            + ChunkPos.ToString() , actor);
            Server.ChatManager.SendActorMessage("Restored Actor Pos: "  + testpos.ToString()  , actor);
            

            for (int x = 0; x <= (absdiffx); x++)
            {
                for (int y = 0; y <= (absdiffy); y++)
                {
                    for (int z = 0; z <= (absdiffz); z++)
                    {
                        currentChunk.ChangeBlock(blockID,
                                                 (pos1.X - ChunkPos.X) + (x * (valincx)),
                                                 (pos1.Y - ChunkPos.Y) + (y * (valincy)),
                                                 (pos1.Z - ChunkPos.Z) + (z * (valincz))
                                                );
                            /*locationList.Add(new Point3D(
                            (ChunkPos.X - pos1.X) + (x * (valincx)),
                            (ChunkPos.Y - pos1.Y) + (y * (valincy)),
                            (ChunkPos.Z - pos1.Z) + (z * (valincz))
                            ));*/
                    }
                }
            }


            //Change blocks
            //currentChunk.ChangeBlockBatch(locationList, blockID, true);

            return true;
        }
    }
}
