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
    class Load : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//load" }; }
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

        public Load(IGameServer server)
            : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            try
            {

                if (!_Utils.checkParameterCount(parameters, 1, actor))
                    return false;

                string schematicName = parameters[1];

                Dictionary<string, string> loadInfo = new Dictionary<string, string>();

                loadInfo["schematicName"] = schematicName;

                if (!_Utils.schematicExists(schematicName, actor))
                    return false;

                if (!_Utils.storeSessionVar(actor, "SNEditSchematicBuffer", (Object)loadInfo, true))
                    return false;

                Server.ChatManager.SendActorMessage("Schematic has been loaded.", actor);
                return true;
            }
            catch (Exception e)
            { 
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
