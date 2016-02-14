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
                return "Stores current players position as Pos 1 or Pos 2 for subsequent SNEdit commands.";
            }
        }

        public override Priviledges Priviledge
        {
            get { return Priviledges.Player; }
        }

        public GetPos(IGameServer server) : base(server) { }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            if (parameters.Length > 2)
            {
                Server.ChatManager.SendActorMessage("No parameter entered.", actor);
                return false;
            }
            else
            {
                return SNScriptUtils._Utils.setPos(actor, parameters[1]);
            }
        }
    }
}
