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
    class Copy : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//copy" }; }
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

        public Copy(IGameServer server)
            : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            //save area between pos1 and pos2 into temporary schematic



            Dictionary<string, string> loadInfo = new Dictionary<string, string>();

            loadInfo["schematicName"] = "_tmp_ + actorname + time";

            return _Utils.storeSessionVar(actor, "SNEditSchematicClipboard", (Object)loadInfo, true);

        }
    }
}
