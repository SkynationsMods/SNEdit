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
    class Rotate : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//rotate" }; }
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

        public Rotate(IGameServer server)
            : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {


            return true;
        }
    }
}
