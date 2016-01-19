using SharedGameData;
using SNScript;
using System;
using System.Linq;
using SNScriptUtils;

namespace SNEdit
{
    class GetPos : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//pos" }; }
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

        public GetPos(IGameServer server) : base(server) {}

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            if(parameters.Length > 2)
            {
                Server.ChatManager.SendActorMessage("No parameter enterd.", actor);
                return false;
            } else
            {
                return SNScriptUtils._Utils.positionSet(actor, parameters[1]);
            }
            
        }
    }
}
