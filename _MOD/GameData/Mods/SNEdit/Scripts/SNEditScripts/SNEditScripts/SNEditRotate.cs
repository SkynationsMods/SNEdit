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
                return "Command to rotate the clipboard (a loaded Schematic or a Copied area).";
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

            int inputRotate = new int();
            //is integer?
            try {
                inputRotate = Int32.Parse(parameters[1]);}
            catch (FormatException e) {
                errorNotifyUser(actor);
            }

            int rotate = new int();
            switch(inputRotate) {
                case (90):
                    rotate = 1;
                    break;
                case (180):
                    rotate = 2;
                    break;
                case (270):
                    rotate = 3;
                    break;
                default:
                    errorNotifyUser(actor);
                    return false;
            }

            Dictionary<string, string> loadInfo = (Dictionary<string, string>)actor.SessionVariables["SNEditSchematicClipboard"];
            //check if dictionary entry for rotation exists already
            int finalRotate = 0;
            if (loadInfo.ContainsKey("rotation"))
            {//if so, change to new value
                int savedRotate = Int32.Parse(loadInfo["rotation"]);
                loadInfo.Remove("rotation");
                finalRotate = ((savedRotate + rotate) % 4);
                loadInfo["rotation"] = finalRotate.ToString();

            }
            else
            {//else add new one
                loadInfo["rotation"] = rotate.ToString();
            }

            Server.ChatManager.SendActorMessage("Loaded Schematic rotated by " + inputRotate.ToString() + "°. New Rotation from base is: " + (finalRotate * 90).ToString() + "°.", actor);
            return true;
        }
    }
}
