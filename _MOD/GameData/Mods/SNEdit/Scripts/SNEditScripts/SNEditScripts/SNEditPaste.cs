using SharedGameData;
using SNScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNScriptUtils;
using fNbt;
using System.IO;

namespace SNEdit
{
    class Paste : GameCommand
    {
        public override string[] Aliases
        {
            get { return new string[] { "//paste" }; }
        }

        public override string CommandDescription
        {
            get
            {
                return "Command to paste a schematic or previously copied area.";
            }
        }

        public override Priviledges Priviledge
        {
            get { return Priviledges.Admin; }
        }

        public Paste(IGameServer server) : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            try
            {
                if (!actor.SessionVariables.ContainsKey("SNEditSchematicClipboard"))
                {
                    Server.ChatManager.SendActorMessage("Nothing found to paste. Use //load or //copy an area first.", actor);
                    return false;
                }

                Dictionary<string, string> loadInfo = (Dictionary<string, string>)actor.SessionVariables["SNEditSchematicClipboard"];
              
                var NbtFile = new NbtFile();
                NbtFile.LoadFromFile(SNEditSettings.SchematicDir + loadInfo["schematicName"] + ".schematic");

                Server.ChatManager.SendActorMessage("Paste operation started, depending on the size of the Schematic and Server Hardware, " +
                   "this can take up to 10 minutes! In that time, the Server might seem unresponsive or hanged up, but give it time to complete the operation!", actor);


                string playerNotification = "";
                Schematic Schematic = null;
                if (!_Utils.NbtFile2SchematicClass(NbtFile, out playerNotification, out Schematic))
                {
                    Server.ChatManager.SendActorMessage(playerNotification, actor);
                    return false;
                }

                Dictionary<Point3D, ushort> fakeGlobalPosAndBlockID = new Dictionary<Point3D, ushort>();
                
                int rotate;
                if (loadInfo.ContainsKey("rotation"))
                    rotate = Int32.Parse(loadInfo["rotation"]);
                else
                    rotate = 0;
                
                if(!_Utils.SchematicToFakeGlobalPosAndBlockID(Schematic, _Utils.GetActorFakeGlobalPos(actor, new Point3D(0, -1, 0)), rotate, out fakeGlobalPosAndBlockID))
                    return false;



                Dictionary<Point3D, Dictionary<Point3D, ushort>> BlocksToBePlacedInSystem = new Dictionary<Point3D, Dictionary<Point3D, ushort>>();
                _Utils.SplitFakeGlobalPosBlocklistIntoChunksAndLocalPos(fakeGlobalPosAndBlockID, out BlocksToBePlacedInSystem);

                return SNScriptUtils._Utils.PlaceBlocksInSystem(BlocksToBePlacedInSystem, ((IGameServer)actor.State).Biomes.GetSystems()[actor.InstanceID]);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
