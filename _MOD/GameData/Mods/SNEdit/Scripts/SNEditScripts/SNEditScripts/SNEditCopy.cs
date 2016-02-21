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
                return "Command to temporarily copy the selected area for paste operations.";
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
            try
            {
                //save area between pos1 and pos2 into temporary schematic
                if (!_Utils.checkParameterCount(parameters, 0, actor))
                    return false;

                string time             = string.Format("_{0:yyyy-MM-dd_hh-mm-ss}", DateTime.Now);

                String schematicName    = "_tmpcpy_" + actor.Name + time;
                String sanitizedString  = _Utils.sanitizeString(schematicName);

                if (!_Utils.storeAreaAsSchematic(actor, sanitizedString, false))
                    return false;

                Dictionary<string, string> loadInfo = new Dictionary<string, string>();

                loadInfo["schematicName"]   = schematicName;
                loadInfo["rotation"]        = "0";

                return _Utils.storeSessionVar(actor, "SNEditSchematicClipboard", (Object)loadInfo, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}
