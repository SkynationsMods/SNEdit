using SharedGameData;
using SNScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEdit
{
    class SelectInfo : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//selinfo" }; }
        }

        public override string CommandDescription
        {
            get
            {
                return "Command to ...";
            }
        }

        public override Priviledges Priviledge
        {
            get { return Priviledges.Admin; }
        }

        public SelectInfo(IGameServer server)
            : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            /*
            // Inform Player about stats
            Point3D outdiff = new Point3D(absdiffx, absdiffy, absdiffz);
            Server.ChatManager.SendActorMessage("diff: " + outdiff.ToString(), actor);
            Server.ChatManager.SendActorMessage("pos1: " + pos1.ToString(), actor);
            Server.ChatManager.SendActorMessage("pos2: " + pos2.ToString(), actor);


            //int testx = pos1.X - ChunkPos.X; int testy = pos1.Y - ChunkPos.Y; int testz = pos1.Z - ChunkPos.Z;
            //Point3D testpos = new Point3D(testx, testy, testz);

            */
            return true;
        }
    }
}
