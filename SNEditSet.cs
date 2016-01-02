using SharedGameData;
using SNScript;
using GameServer;
using SNEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PreciseMaths;
using MoreBlocksScripts;

namespace ScriptsExample
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

        public SNEditSet(IGameServer server)
            : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            
            MoreBlocksScripts.SNEdit helper = new MoreBlocksScripts.SNEdit();

            //Only takes 2 arguments //set and blockID
            if (parameters.Length > 2)
            {
                Server.ChatManager.SendActorMessage("Too many arguments.", actor);
                return false;
            }

            //Check if entered blockID is valid TODO
            if (true)
            {
                int _ID = new int();

                try
                {
                    _ID = Int32.Parse(parameters[1]);
                }
                catch (FormatException e)
                {
                    Server.ChatManager.SendActorMessage("Parameter could not be parsed.", actor);
                    return false;
                }

                ushort blockID = (ushort)_ID;
            }
            else
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
            if (!checkPositions(actor, out pos1, out pos2))
            {
                return false;
            }

            //Get the difference between the two position
            dynamic.X = pos1.X - pos2.X;
            dynamic.Y = pos1.Y - pos2.Y;
            dynamic.Z = pos1.Z - pos2.Z;

            locationList.add(pos1);
            locationList.add(pos2);

            //Get every block on X
            while (dynamic.x != 0)
            {
                Point3D temp = new Point3D();
                temp = pos1;
                temp.x = pos1 + dynamic.x;

                //Get every block on Y
                while (dynamic.y != 0)
                {
                    temp.y = dynamic.y;

                    //Get every block on Z
                    while (dynamic.z != 0)
                    {
                        temp.z = dynamic.z;
                        locationList.add(temp);

                        //Get dynamic closer to 0
                        if (dynamic.z > 0) { dynamic.z++; } else { dynamic.z--; }
                    }

                    //Get dynamic closer to 0
                    if (dynamic.y > 0) { dynamic.y++; } else { dynamic.y--; }
                }
                //Get dynamic closer to 0
                if (dynamic.x > 0) { dynamic.x++; } else { dynamic.x--; }

            }

            //TODO
            IChunk currentChunk = getChunkFromGlobal(pos1, actor);

            //Change blocks
            currentChunk.ChangeBlockBunch(locationList, blockID);

            return true;
        }
    }
}
