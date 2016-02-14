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

        private void errorNotifyUser(IActor actor) {
            ((IGameServer)actor.State).ChatManager.SendActorMessage("Only 90, 180 and 270 are valid input values. Rotate Clockwise by these degrees.", actor);
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            if (!_Utils.checkParameterCount(parameters, 1, actor))
                return false;

            int rotate = new int();
            //is integer?
            try
            {
                rotate = Int32.Parse(parameters[1]);
            }
            catch (FormatException e)
            {
                errorNotifyUser(actor);
            }

            //is 90, 180, 270 ?
            if (rotate != 90 && rotate != 180 && rotate != 270)
            {
                errorNotifyUser(actor);
            }


            

            

            Dictionary<string, string> loadInfo = (Dictionary<string, string>)actor.SessionVariables["SNEditSchematicClipboard"];
            //check if dictionary entry for rotation exists already

            //if so, change to new value

            //else add new one


            return true;
        }
    }
}
