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
                bool debug = true;

                if (!actor.SessionVariables.ContainsKey("SNEditSchematicClipboard"))
                {
                    Server.ChatManager.SendActorMessage("Nothing found to paste. Use //load or //copy first.", actor);
                    return false;
                }

                Dictionary<string, string> loadInfo = (Dictionary<string, string>)actor.SessionVariables["SNEditSchematicClipboard"];

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

                int rotate;
                if (loadInfo.ContainsKey("rotation"))
                {
                    rotate = Int32.Parse(loadInfo["rotation"]);
                }
                else
                {
                    rotate = 0;
                }

                int valincq = 1; int valincr = 1; int valincs = 1;
                int maxDimQ = SchematicHeight; int maxDimR = SchematicLength; int maxDimS = SchematicWidth;
                
                if (rotate == 1) //90 degrees clockwise
                {
                    valincq = 1; maxDimQ = SchematicHeight;

                    valincr = 1; valincs = 1;
                    maxDimR = SchematicLength; maxDimS = SchematicWidth;
                    //call rotate MC or SN array
                }

                if (rotate == 2) //180 degrees
                {
                    valincq = 1; maxDimQ = SchematicHeight;

                    valincr = 1; valincs = 1;
                    maxDimR = SchematicLength; maxDimS = SchematicWidth;
                    //call rotate MC or SN array
                }

                if (rotate == 3) //270 degrees clockwise
                {
                    valincq = 1; maxDimQ = SchematicHeight;

                    valincr = 1; valincs = 1;
                    maxDimR = SchematicLength; maxDimS = SchematicWidth;
                    //call rotate MC or SN array
                }

                if (debug) _Utils.translationHelper(BlockArray, MetaDataArray, translationDictionary);
                
                int i = 0;
                for (int q = 0; q < (maxDimQ); q++)
                {
                    for (int r = 0; r < (maxDimR); r++)
                    {
                        for (int s = 0; s < (maxDimS); s++)
                        {
                            fakeGlobalPosAndBlockID.Add(
                                new Point3D(
                                    (pos1.X + (s * valincq)),
                                    (pos1.Y + (q * valincr)),
                                    (pos1.Z + (r * valincs))
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
