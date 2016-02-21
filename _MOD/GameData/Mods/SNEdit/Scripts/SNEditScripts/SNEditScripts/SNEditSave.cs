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
    class Save : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//save" }; }
        }

        public override string CommandDescription
        {
            get
            {
                return "Command to save an Area as SN Schematic.";
            }
        }

        public override Priviledges Priviledge
        {
            get { return Priviledges.Admin; }
        }

        public Save(IGameServer server)
            : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            //save area between pos1 and pos2 into schematic
            if (!_Utils.checkParameterCount(parameters, 1, actor))
                return false;

            String schematicName = parameters[1];

            Point3D pos1 = new Point3D(); Point3D pos2 = new Point3D();
            if (!_Utils.checkStoredPositions(actor, out pos1, out pos2))
                return false;

            Point3D posOrigin = _Utils.calcCuboidOrigin(pos1, pos2);
            




            Dictionary<string, string> loadInfo = new Dictionary<string, string>();

            loadInfo["schematicName"] = parameters[1];

            return _Utils.storeSessionVar(actor, "SNEditSchematicClipboard", (Object)loadInfo, true);
        }
    }
}
