using SharedGameData;
using SNScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fNbt;

namespace SNEdit
{
    class Paste : GameCommand
    {
        //private IGameServer Server;

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
            //this.Server = server;
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            /*
            if (parameters.Length > 1)
            {
                Server.ChatManager.SendActorMessage("Wrong number of parameters for //paste. //paste schematicname", actor);
                return true;
            }*/


            Dictionary<string, string> loadInfo = (Dictionary<string, string>)actor.SessionVariables["load"];

            string schematicDir = "Schematics/";

            var Schematic = new NbtFile();
            Schematic.LoadFromFile(schematicDir + loadInfo["schematicName"] + ".schematic");
            var myCompoundTag = Schematic.RootTag;

            int SchematicHeight = Schematic.RootTag["Height"].IntValue;
            int SchematicWidth = Schematic.RootTag["Width"].IntValue;
            int SchematicLength = Schematic.RootTag["Length"].IntValue;

            byte[] BlockArray = Schematic.RootTag["Blocks"].ByteArrayValue;
            byte[] MetaDataArray = Schematic.RootTag["Data"].ByteArrayValue;

            string testOutPut = BitConverter.ToString(BlockArray);

            Dictionary<string, uint> translationDictionary = SNScriptUtils._Utils.GetTranslationDictionary();
            int i = 0;
            Dictionary<Point3D, ushort> fakeGlobalPosAndBlockID = new Dictionary<Point3D, ushort>();


            Point3D pos1 = SNScriptUtils._Utils.GetActorPos(actor, new Point3D(0,-1,0));

            //TODO: rotation!
            int valincx = 1; int valincy = 1; int valincz = 1;
            int maxDimQ = SchematicHeight; int maxDimR = SchematicLength; int maxDimS = SchematicWidth;

            for (int q = 0; q <= (maxDimQ); q++)
            {
                for (int r = 0; r <= (maxDimR); r++)
                {
                    for (int s = 0; s <= (maxDimS); s++)
                    {
                        //Dictionary contains <fakeGlobalPos, blockID>
                        //SNScriptUtils._Utils.ConvertMCBlockID2SNBlockID(BlockArray[i] + ":" + MetaDataArray[i], translationDictionary)
                        fakeGlobalPosAndBlockID.Add(new Point3D((pos1.X + (s * valincx)), (pos1.Y + (q * valincy)), (pos1.Z + (r * valincz))), (ushort)10 );
                        i++;
                    }
                }
            }

            Dictionary<Point3D, Dictionary<Point3D, ushort>> BlocksToBePlacedInSystem = new Dictionary<Point3D, Dictionary<Point3D, ushort>>();
            SNScriptUtils._Utils.SplitFakeGlobalPosBlocklistIntoChunksAndLocalPos(fakeGlobalPosAndBlockID, out BlocksToBePlacedInSystem); 
            
            return SNScriptUtils._Utils.PlaceBlocksInSystem(BlocksToBePlacedInSystem, ((IGameServer)actor.State).Biomes.GetSystems()[actor.InstanceID], false, (ushort)0);

        }
    }
}
