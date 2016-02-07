using SharedGameData;
using SNScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNScriptUtils;
using fNbt;

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
                if (!actor.SessionVariables.ContainsKey("SNEditSchematicBuffer"))
                {
                    Server.ChatManager.SendActorMessage("Nothing found to paste. Use //load or //copy first.", actor);
                    return false;
                }

                Dictionary<string, string> loadInfo = (Dictionary<string, string>)actor.SessionVariables["SNEditSchematicBuffer"];

                string schematicDir = "Schematics/";

                var Schematic = new NbtFile();
                Schematic.LoadFromFile(schematicDir + loadInfo["schematicName"] + ".schematic");
                var myCompoundTag = Schematic.RootTag;

                int SchematicHeight = Schematic.RootTag["Height"].IntValue;
                int SchematicWidth = Schematic.RootTag["Width"].IntValue;
                int SchematicLength = Schematic.RootTag["Length"].IntValue;

                Console.WriteLine(
                    "SchematicHeight: " + SchematicHeight.ToString() +
                    " SchematicWidth: " + SchematicWidth.ToString() +
                    " SchematicLength: " + SchematicLength.ToString()
                    );
                byte[] BlockArray = Schematic.RootTag["Blocks"].ByteArrayValue;
                byte[] MetaDataArray = Schematic.RootTag["Data"].ByteArrayValue;

                string testOutPut = BitConverter.ToString(BlockArray);

                Dictionary<string, ushort> translationDictionary = SNScriptUtils._Utils.GetTranslationDictionary();

                Dictionary<Point3D, ushort> fakeGlobalPosAndBlockID = new Dictionary<Point3D, ushort>();

                Point3D pos1 = SNScriptUtils._Utils.GetActorFakeGlobalPos(actor, new Point3D(0, -1, 0));

                //TODO: rotation! v--
                int valincx = 1; int valincy = 1; int valincz = 1;
                int maxDimQ = SchematicHeight; int maxDimR = SchematicLength; int maxDimS = SchematicWidth;
                //TODO: rotation! ^--

                int i = 0;
                for (int q = 0; q < (maxDimQ); q++)
                {
                    for (int r = 0; r < (maxDimR); r++)
                    {
                        for (int s = 0; s < (maxDimS); s++)
                        {
                            fakeGlobalPosAndBlockID.Add(
                                new Point3D(
                                    (pos1.X + (s * valincx)),
                                    (pos1.Y + (q * valincy)),
                                    (pos1.Z + (r * valincz))
                                    ),
                                _Utils.ConvertMCBlockID2SNBlockID(BlockArray[i] + ":" + MetaDataArray[i], translationDictionary));
                            i++;
                        }
                    }
                }

                Dictionary<Point3D, Dictionary<Point3D, ushort>> BlocksToBePlacedInSystem = new Dictionary<Point3D, Dictionary<Point3D, ushort>>();
                SNScriptUtils._Utils.SplitFakeGlobalPosBlocklistIntoChunksAndLocalPos(fakeGlobalPosAndBlockID, out BlocksToBePlacedInSystem);

                return SNScriptUtils._Utils.PlaceBlocksInSystem(BlocksToBePlacedInSystem, ((IGameServer)actor.State).Biomes.GetSystems()[actor.InstanceID], false, (ushort)0);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
