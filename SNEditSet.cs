using SharedGameData;
using SNScript;
using GameServer;
using System;
using System.Linq;
using PreciseMaths;
using GameServer.World.Chunks;
using System.Collections.Generic;

namespace MoreBlocksScripts
{ 
    class SNEditSet : GameCommand
    {
        MoreBlocksScripts.SNEdit helper = new MoreBlocksScripts.SNEdit();

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

        public SNEditSet(IGameServer server) : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            //ID of block to change to
            ushort blockID = new ushort();
            
            //Only takes 2 arguments //set and blockID
            if(parameters.Length > 2)
            {
                Server.ChatManager.SendActorMessage("Too many arguments.", actor);
                return false;
            }

            //Check if entered blockID is valid TODO
            if(true)
            {
                blockID = ushort.Parse(parameters[1]);
            } else
            {
                Server.ChatManager.SendActorMessage("Invalid block entered.", actor);
                return false;
            }

            //Varible Point3D
            Point3D dynamic = new Point3D();

            //2 positions for what the user set
            Point3D pos1 = new Point3D();
            Point3D pos2 = new Point3D();

            //List of blocks need to be plased
            List<Point3D> locationList = new List<Point3D>();

            //Maybe add this into a class later?
            if(!helper.checkPositions(actor, out pos1, out pos2))
            {
                return false;
            }

            //Get the difference between the two position
            //dynamic = pos1 - pos2;
            dynamic.X = pos1.X - pos2.X;
            dynamic.Y = pos1.Y - pos2.Y;
            dynamic.Z = pos1.Z - pos2.Z;


            locationList.Add(pos1);
            locationList.Add(pos2);

            //Get every block on X
            while(dynamic.X != 0)
            {
                Point3D temp = new Point3D();
                temp = pos1;
                temp.X = pos1.X + dynamic.X;

                //Get every block on Y
                while(dynamic.Y != 0)
                {
                    temp.Y = dynamic.Y;

                    //Get every block on Z
                    while(dynamic.Z != 0)
                    {
                        temp.Z = dynamic.Z;
                        locationList.Add(temp);

                        //Get dynamic closer to 0
                        if (dynamic.Z > 0) { dynamic.Z++; } else { dynamic.Z--; }
                    }

                    //Get dynamic closer to 0
                    if (dynamic.Y > 0) { dynamic.Y++; } else { dynamic.Y--; }
                }
                //Get dynamic closer to 0
                if (dynamic.X > 0) { dynamic.X++; } else { dynamic.X--; }

            }

            //TODO
            Chunk currentChunk = (Chunk)helper.getChunkFromGlobal(pos1, actor);

            //Change blocks
            currentChunk.ChangeBlockBatch(locationList, blockID, true);

            return true;
        }
    }
}
