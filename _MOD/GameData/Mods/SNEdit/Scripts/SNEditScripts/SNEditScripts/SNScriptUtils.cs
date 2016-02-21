using SharedGameData;
using SNScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PreciseMaths;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using fNbt;
using SNEdit;

using GameServer.World;
using GameServer.World.Actors;
using GameServer.World.Chunks;

namespace SNScriptUtils
{
    public class _Utils
    {
        public _Utils() { }

        //public static bool NbtFile2Schematic(NbtFile NbtFile, IBiomeSystem system, Point3D rootfakeGlobalPos, int rotation, out String message)
        public static bool NbtFile2SchematicClass(NbtFile NbtFile, out String message, out Schematic schematic) 
        {
            message = "Schematic successfully pasted!";
            String type = "MC";
            int[] TileArray = null; byte[] MetaDataArray = null; byte[] BlockArray = null;
            int SchematicHeight = new int(); int SchematicWidth = new int(); int SchematicLength = new int(); int BlockCount = new int();
            schematic = null;

            try {

                NbtTag SNEditTag = NbtFile.RootTag["SNEdit"];
                if (SNEditTag != null)
                    type = "SN";

                SchematicHeight = NbtFile.RootTag["Height"].IntValue;
                SchematicWidth = NbtFile.RootTag["Width"].IntValue;
                SchematicLength = NbtFile.RootTag["Length"].IntValue;
                BlockCount = SchematicHeight * SchematicWidth * SchematicLength;

                switch(type) {
                    case("SN"):
                        TileArray = NbtFile.RootTag["Tiles"].IntArrayValue;
                        break;
                    case("MC"):
                        BlockArray = NbtFile.RootTag["Blocks"].ByteArrayValue;
                        MetaDataArray = NbtFile.RootTag["Data"].ByteArrayValue;
                        break;
                }
            } catch (Exception e){message = "Schematic File invalid.";return false;}
            
            Dictionary<string, ushort> translationDictionary = SNScriptUtils._Utils.GetTranslationDictionary();

            ushort[] blocks = new ushort[BlockCount];

            bool info = true;
            if (info && (type == "MC")) _Utils.translationHelper(BlockArray, MetaDataArray, translationDictionary);

            switch (type)
            {
                case("MC"):
                    for (int i = 0; i < BlockCount; i++)
                    {
                        blocks[i] = _Utils.ConvertMCBlockID2SNBlockID(BlockArray[i] + ":" + MetaDataArray[i], translationDictionary);
                    }
                    break;
                case("SN"):
                    for (int i = 0; i < BlockCount; i++)
                    {
                        blocks[i] = (ushort)TileArray[i];
                    }
                    break;
            } 
            
            schematic = new Schematic(SchematicHeight, SchematicLength, SchematicWidth, blocks);
            return true;
        }

        public static bool SchematicToFakeGlobalPosAndBlockID(
            Schematic schematic,
            Point3D root, 
            Int32 rotation,
            out Dictionary<Point3D, ushort> FakeGlobalPosAndBlockID
            )
        {
            FakeGlobalPosAndBlockID = new Dictionary<Point3D, ushort>();

            if (rotation != 0)
                schematic.blocks = RotateLib.applyRotationToSNArray(schematic.blocks, rotation);
            
            int valincx = 1; int valincz = 1;
            string mode = "xz";
            switch (rotation)
            {
                case (1):
                    valincx = -1; valincz = 1;
                    mode = "zx";
                    break;
                case (2):
                    valincx = -1; valincz = -1;
                    mode = "xz";
                    break;
                case (3):
                    valincx = 1; valincz = -1;
                    mode = "zx";
                    break;
            }

            int i = 0;

            if (mode == "xz")
            {
                for (int y = 0; y < (schematic.Height); y++)
                {
                    for (int z = 0; z < (schematic.Length); z++)
                    {
                        for (int x = 0; x < (schematic.Width); x++)
                        {
                            FakeGlobalPosAndBlockID.Add(
                                new Point3D(
                                    (root.X + (x * valincx)),
                                    (root.Y + (y * 1)),
                                    (root.Z + (z * valincz))
                                    ),
                                schematic.blocks[i]
                            );
                            i++;
                        }
                    }
                }
            }

            if (mode == "zx")
            {
                for (int y = 0; y < (schematic.Height); y++)
                {
                    for (int x = 0; x < (schematic.Width); x++)
                    {
                        for (int z = 0; z < (schematic.Length); z++)
                        {
                            FakeGlobalPosAndBlockID.Add(
                                new Point3D(
                                    (root.X + (x * valincx)),
                                    (root.Y + (y * 1)),
                                    (root.Z + (z * valincz))
                                    ),
                                schematic.blocks[i]
                            );
                            i++;
                        }
                    }
                }
            }

            return true;
        }

        public static String sanitizeString(String input)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            var sanitizedFileName = String.Join("-", input.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            return sanitizedFileName;
        }

        public static Boolean storeAreaAsSchematic(IActor actor, String schematicName, Boolean overwriteFile)
        {
            Point3D fakeGlobalPos1 = new Point3D(); Point3D fakeGlobalPos2 = new Point3D();
            if (!_Utils.checkStoredPositions(actor, out fakeGlobalPos1, out fakeGlobalPos2))
                return false;

            IBiomeSystem system = ((IGameServer)actor.State).Biomes.GetSystems()[actor.InstanceID];

            _Utils.storeAreaAsSchematic(fakeGlobalPos1, fakeGlobalPos2, system, schematicName, overwriteFile, null);

            return true;
        }
        
        public static Boolean storeAreaAsSchematic(Point3D fakeGlobalPos1, Point3D fakeGlobalPos2, IBiomeSystem system, String schematicName, Boolean overwriteFile, IActor actorForNotifications)
        {
            Point3D posOrigin           = _Utils.calcCuboidOrigin(fakeGlobalPos1, fakeGlobalPos2);
            Point3D cuboidDimensions    = _Utils.calcCuboidDimensions(fakeGlobalPos1, fakeGlobalPos2);
            
            String sanitizedFileName = _Utils.sanitizeString(schematicName);

            Dictionary<Point3D, IChunk> chunkDictionary = CreateChunkDictionary(system);

            int[] tileArray = new int[cuboidDimensions.X * cuboidDimensions.Y * cuboidDimensions.Z];
           

            ushort blockID;

            int i = 0;
            for (int y = 0; y < (cuboidDimensions.Y); y++)
            {
                for (int z = 0; z < (cuboidDimensions.Z); z++)
                {
                    for (int x = 0; x < (cuboidDimensions.X); x++)
                    {
                        GetBlockIdAtFakeGlobalPos(chunkDictionary, new Point3D(posOrigin.X + x, posOrigin.Y + y, posOrigin.Z + z), out blockID);
                        tileArray[i] = blockID;
                        i++;
                    }
                }
            }

            var schematic = new NbtCompound("tmpSchematic");
            schematic.Add(new NbtString("SNEdit", "true"));
            schematic.Add(new NbtInt("Height", cuboidDimensions.Y));
            schematic.Add(new NbtInt("Length", cuboidDimensions.Z));
            schematic.Add(new NbtInt("Width", cuboidDimensions.X));
            schematic.Add(new NbtIntArray("Tiles", tileArray));
            

            var serverFile = new NbtFile(schematic);
            serverFile.SaveToFile(SNEditSettings.SchematicDir + sanitizedFileName + ".schematic", NbtCompression.None);

            return true;
        }


        //helper function listing the requested unique translations (MCID to SNID), their result and most importantly which ones defaulted (no entry found)
        //helpful to expand the translation dictionary and figure out which ones are still missing for to paste schematic
        public static void translationHelper(byte[] BlockArray, byte[] MetaDataArray, Dictionary<string, ushort> translationDictionary)
        {

            List<string> ReqIDs = new List<string>();
            for (int i = 0; i < BlockArray.Length; i++)
            {
                ReqIDs.Add(BlockArray[i] + ":" + MetaDataArray[i]);
            }

            List<string> distReqIDs = ReqIDs.Distinct().ToList();

            string output = "";
            string req = "";
            ushort value = new ushort();
            foreach (string ID in distReqIDs)
            {
                req = translationDictionary.TryGetValue(ID, out value) ? ID + "->" + value.ToString() + " | " : ID + "->(def) | ";
                output = output + req;
            }

            Console.WriteLine(output + " - requests with (def) defaulted.");

        }

        //only works on ships (not on land)
        public static bool UsableByNationMemberOnly(IActor actor, IChunk chunk)
        {
            Chunk castedChunk = chunk as Chunk;
            return (!string.IsNullOrEmpty(castedChunk.NationOwner) && (actor.Nation != castedChunk.NationOwner));
        }

        //potential file check for safety
        public static bool schematicExists(string schematicName, IActor actor)
        {
            return true;
        }

        //calculates the dimensions of the selected Area, needed for //save and //copy operations, directly written into Schematic (Width, Length, Height)
        public static Point3D calcCuboidDimensions(Point3D pos1, Point3D pos2)
        {
            int diffx = pos2.X - pos1.X;
            int absdiffx = System.Math.Abs(diffx);

            int diffy = pos2.Y - pos1.Y;
            int absdiffy = System.Math.Abs(diffy);

            int diffz = pos2.Z - pos1.Z;
            int absdiffz = System.Math.Abs(diffz);
            
            return new Point3D(absdiffx + 1, absdiffy + 1, absdiffz + 1);
        }

        //calculates the Point of Origin of a selected Area (cuboid), thats the bottom left point, or /ingame/ the point in the north-east of the selection
        public static Point3D calcCuboidOrigin(Point3D pos1, Point3D pos2)
        {
            int x;int y;int z;

            if (pos1.X <= pos2.X)
                x = pos1.X;
            else
                x = pos2.X;

            if (pos1.Y <= pos2.Y)
                y = pos1.Y;
            else
                y = pos2.Y;

            if (pos1.Z <= pos2.Z)
                z = pos1.Z;
            else
                z = pos2.Z;

            return new Point3D(x,y,z);
        }

        public static bool checkParameterCount(string[] parameters, int parameterCount, IActor actor)
        {
            if (parameters.Length > (parameterCount+1))
            {
                ((IGameServer)actor.State).ChatManager.SendActorMessage("Too many arguments for this command.", actor);
                return false;
            }
            else
                return true;
        }
        
        public static bool blockTypeExists(IChunk chunk, ushort blockID, IActor actor)
        {
            if (chunk.GetTileData().Keys.Contains(blockID))
            {
                return true;
            }
            else
            {
                ((IGameServer)actor.State).ChatManager.SendActorMessage("Invalid Block ID, Blocktype not found.", actor);
                return false;
            }
        }

        public static bool SplitFakeGlobalPosBlocklistIntoChunksAndLocalPos(Dictionary<Point3D, ushort> fakeGlobalPosAndBlockID, out Dictionary<Point3D, Dictionary<Point3D, ushort>> BlocksToBePlacedInSystem)
        {
            BlocksToBePlacedInSystem = new Dictionary<Point3D, Dictionary<Point3D, ushort>>();

            foreach (KeyValuePair<Point3D, ushort> KVPfakeGlobalPosAndBlockID in fakeGlobalPosAndBlockID)
            {
                Point3D tmpChunkPos = _Utils.GetChunkKeyFromFakeGlobalPos(KVPfakeGlobalPosAndBlockID.Key.ToDoubleVector3);

                //calculate localpos
                int localx = KVPfakeGlobalPosAndBlockID.Key.X - tmpChunkPos.X;
                int localy = KVPfakeGlobalPosAndBlockID.Key.Y - tmpChunkPos.Y;
                int localz = KVPfakeGlobalPosAndBlockID.Key.Z - tmpChunkPos.Z;
                Point3D localPos = new Point3D(localx, localy, localz);

                if (BlocksToBePlacedInSystem.Keys.Contains(tmpChunkPos))
                {
                    BlocksToBePlacedInSystem[tmpChunkPos].Add(localPos, KVPfakeGlobalPosAndBlockID.Value);
                }
                else
                {
                    Dictionary<Point3D, ushort> localPosAndBlockID = new Dictionary<Point3D, ushort>();
                    localPosAndBlockID.Add(localPos, KVPfakeGlobalPosAndBlockID.Value);
                    BlocksToBePlacedInSystem.Add(tmpChunkPos, localPosAndBlockID);
                }
            }

            return true;
        }

        public static void calcAbsDiffAndValinc(int pos1, int pos2, out int absdiff, out int valinc)
        {
            int diff = pos2 - pos1;
            absdiff = System.Math.Abs(diff);
            if (absdiff != 0) 
            { 
                valinc = diff / absdiff; 
            } 
            else 
            { 
                valinc = absdiff; 
            };
        }

        //BlocksToBePlacedInSystem = ChunkPos -> [localpos, BlockID]
        public static bool PlaceBlocksInSystem(Dictionary<Point3D, Dictionary<Point3D, ushort>> BlocksToBePlacedInSystem, IBiomeSystem System, bool replacemode, ushort replaceThisBlockID)
        {
            List<IChunk> newChunkList = new List<IChunk>();

            Dictionary<Point3D, IChunk> ChunkDictionary = SNScriptUtils._Utils.CreateChunkDictionary(System);

            foreach (KeyValuePair<Point3D, Dictionary<Point3D, ushort>> BlockToBePlacedInChunk in BlocksToBePlacedInSystem)
            {
                
                bool chunkNeedsCleanup = false;
                IChunk workChunk = null as IChunk;
                if (ChunkDictionary.ContainsKey(BlockToBePlacedInChunk.Key))
                {//Chunk exists already, just retrieve it from the ChunkDictionary
                    workChunk = System.ChunkCollection.First(item => item.Position == BlockToBePlacedInChunk.Key.ToDoubleVector3);
                }
                else
                {//The Chunk does not exist, it has to be created first
                    ushort[] tmpBlock = new ushort[32768];
                    tmpBlock[32767] = 4;
                    System.CreateLandChunk(tmpBlock, BlockToBePlacedInChunk.Key.ToDoubleVector3);
                    workChunk = System.ChunkCollection.First(item => item.Position == BlockToBePlacedInChunk.Key.ToDoubleVector3);
                    
                    chunkNeedsCleanup = true;
                }

                List<Point3D> replacePoints = new List<Point3D>();
                if (replacemode)
                { 
                    for ( int i = 0; i < workChunk.Blocks.Count(); i++)
                    {
                        if (replaceThisBlockID == workChunk.Blocks[i])
                        {
                            replacePoints.Add(_Utils.getLocalPosFromBlockIndex(i));
                        }
                    }
                }
                
                //Place all Blocks in the Chunk
                foreach (KeyValuePair<Point3D, ushort> BlocksInChunk in BlockToBePlacedInChunk.Value)
                {
                    if (replacemode && replacePoints.Contains(BlocksInChunk.Key))
                    {
                        workChunk.ChangeBlock(BlocksInChunk.Value, BlocksInChunk.Key.X, BlocksInChunk.Key.Y, BlocksInChunk.Key.Z, true, true);
                    }

                    if (!replacemode)
                    {
                        workChunk.ChangeBlock(BlocksInChunk.Value, BlocksInChunk.Key.X, BlocksInChunk.Key.Y, BlocksInChunk.Key.Z, true, true);
                    }
                }
                
                if (chunkNeedsCleanup)
                {
                    ushort tmpBlockID = 4;
                    ushort blockID;
                    blockID = workChunk.Blocks[32767];
                    if (blockID == tmpBlockID)
                    {
                        workChunk.ChangeBlock(0, 31, 31, 31, true, true);
                    }
                    newChunkList.Add(workChunk);
                }
                
            }

            return true;
        }

        //translates the Chunk.Blocks[i] index into the Position of the Block i within the Chunk
        public static Point3D getLocalPosFromBlockIndex(int index)
        {
            int x = new int();
            int y = new int();
            int z = new int();

            _Utils.DecodeIndex(index, out x, out y, out z);

            return new Point3D(x, y, z);
        }


        public static void DecodeIndex(int index, out int x, out int y, out int z)
        {
            x = index / (32 * 32);
            index -= x * 32 * 32;

            y = index / 32;
            index -= y * 32;

            z = index / 1;
        }

        //translates a localposition into the index i of the Block within the Chunk (Chunk.Blocks[i])
        //when lookup is true, it retrieves the result from a precalculated dictionary, instead of manually performing the calculation
        public int GetBlockIndex(int x, int y, int z)
        {
            if (x >= 32 || x < 0)
                throw new IndexOutOfRangeException();
            if (y >= 32 || y < 0)
                throw new IndexOutOfRangeException();
            if (z >= 32 || z < 0)
                throw new IndexOutOfRangeException();

            return x * 32 * 32 + y * 32 + z;
        }

        //Get location of chunk from a global pos
        public static Point3D GetChunkKeyFromFakeGlobalPos(DoubleVector3 pos)
        {
            int x = (int)Math.Floor(pos.X / 32.0) * 32;
            int y = (int)Math.Floor(pos.Y / 32.0) * 32;
            int z = (int)Math.Floor(pos.Z / 32.0) * 32;
            return new Point3D(x, y, z);
        }


        //Create a Dictionary of all the Chunk in a System with their root coords
        public static Dictionary<Point3D, IChunk> CreateChunkDictionary(IBiomeSystem currentSystem)
        {
            Dictionary<Point3D, IChunk> Dictionary = new Dictionary<Point3D, IChunk>();
            for (int i = 0; i < currentSystem.ChunkCollection.Count; i++)
            {
                if (currentSystem.ChunkCollection[i].IsStaticChunk)
                {
                    Dictionary.Add(Point3D.ConvertDoubleVector3(currentSystem.ChunkCollection[i].Position), currentSystem.ChunkCollection[i]);
                }
            }
            return Dictionary;
        }

        public static bool getChunkObjFromFakeGlobalPos(Point3D fakeGlobalPos, IActor actor, out IChunk Chunk)
        {
            Chunk = new Object() as IChunk;
            //Get the server
            IGameServer Server = actor.State as IGameServer;
            //Get SystemsCollection (All Systems on the Server)
            Dictionary<uint, IBiomeSystem> SystemsCollection = Server.Biomes.GetSystems();
            //Get the system the actor is currently in (only reference to go by)
            IBiomeSystem currentSystem;
            SystemsCollection.TryGetValue(actor.InstanceID, out currentSystem);

            //call base function and return results
            return _Utils.getChunkObjFromFakeGlobalPos(fakeGlobalPos, currentSystem, out Chunk);
        }

        public static bool getChunkObjFromFakeGlobalPos(Point3D fakeGlobalPos, IBiomeSystem System, out IChunk Chunk)
        {
            Chunk = new Object() as IChunk;
            //Get ChunkDictionary
            Dictionary<Point3D, IChunk> ChunkDictionary = _Utils.CreateChunkDictionary(System);

            //call base function and return results
            return _Utils.getChunkObjFromFakeGlobalPos(fakeGlobalPos, ChunkDictionary, out Chunk);
        }

        public static bool getChunkObjFromFakeGlobalPos(Point3D fakeGlobalPos, Dictionary<Point3D, IChunk> ChunkDictionary, out IChunk Chunk)
        {
            Chunk = new Object() as IChunk;
            Point3D staticChunkKey = _Utils.GetChunkKeyFromFakeGlobalPos(fakeGlobalPos.ToDoubleVector3);

            if (!ChunkDictionary.ContainsKey(staticChunkKey))
                return false;
            Chunk = ChunkDictionary[staticChunkKey];
            return true;
        }

        //For commands to check if positions have been set
        public static bool checkStoredPositions(IActor actor, out Point3D pos1, out Point3D pos2)
        {
            //Get the server
            IGameServer Server = actor.State as IGameServer;

            pos1 = (Point3D)actor.SessionVariables["SNEditPos1"];
            pos2 = (Point3D)actor.SessionVariables["SNEditPos2"];

            if (pos1 == null)
            {
                Server.ChatManager.SendActorMessage("Position 1 is not set.", actor);
                return false;
            }
            else if (pos2 == null)
            {
                Server.ChatManager.SendActorMessage("Position 2 is not set.", actor);
                return false;
            }
            else if (pos2 != null && pos1 != null)
            {
                return true;
            }
            else
            {
                Server.ChatManager.SendActorMessage("Position 1 and Position 2 are not set.", actor);
                return false;
            }
        }

        public static bool FindCustomSpecialBlocksAround(Point3D sourceLocalPos, IChunk Chunk, List<Point3D> offsetList, uint blockID, Dictionary<Point3D, IChunk> ChunkDictionary, out List<Object[,]> SpecialBlockList)
        {
            SpecialBlockList = new List<Object[,]>();
            
            for (int i = 0; i <= offsetList.Count() - 1; i++)
            {
                KeyValuePair<Point3D, IChunk> SpecialBlock = new KeyValuePair<Point3D, IChunk>();
                if (_Utils.FindCustomSpecialBlockByOffSet(sourceLocalPos, offsetList[i].X, offsetList[i].Y, offsetList[i].Z, blockID, Chunk, ChunkDictionary, out SpecialBlock))
                {
                    SpecialBlockList.Add(new Object[,] { { SpecialBlock.Key, SpecialBlock.Value } });
                }
                
            }
            
            if (SpecialBlockList.Count > 0)
                return true;
            else
                return false;
        }

        public static bool FindCustomSpecialBlockByOffSet(Point3D sourceLocalPos, int xOffset, int yOffset, int zOffset, uint blockID, IChunk Chunk, Dictionary<Point3D, IChunk> ChunkDictionary, out KeyValuePair<Point3D, IChunk> SpecialBlock)
        {
            SpecialBlock = new KeyValuePair<Point3D, IChunk>();
            Point3D tmpFakeGlobalPos = new Point3D(
                (int)Chunk.Position.X + sourceLocalPos.X + xOffset,
                (int)Chunk.Position.Y + sourceLocalPos.Y + yOffset,
                (int)Chunk.Position.Z + sourceLocalPos.Z + zOffset
                );
            Point3D staticChunkKey = _Utils.GetChunkKeyFromFakeGlobalPos(tmpFakeGlobalPos.ToDoubleVector3);
            if (!ChunkDictionary.ContainsKey(staticChunkKey))
                return false;
            IChunk tmpChunk = ChunkDictionary[staticChunkKey];
            Point3D tmpFakeLocalPos = new Point3D(
                tmpFakeGlobalPos.X - (int)tmpChunk.Position.X,
                tmpFakeGlobalPos.Y - (int)tmpChunk.Position.Y,
                tmpFakeGlobalPos.Z - (int)tmpChunk.Position.Z
                );
            
            ushort targetBlockID = new ushort();
            targetBlockID = tmpChunk.Blocks[tmpChunk.GetBlockIndex(tmpFakeLocalPos.X, tmpFakeLocalPos.Y, tmpFakeLocalPos.Z)];
            
            if ((targetBlockID) == (ushort)blockID)
            {
                SpecialBlock = new KeyValuePair<Point3D, IChunk>(tmpFakeLocalPos, tmpChunk);
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool GetBlockIdAtFakeGlobalPos(Dictionary<Point3D, IChunk> ChunkDictionary, Point3D fakeGlobalPos, out ushort blockID)
        {
            blockID = (ushort)0;
            Point3D staticChunkKey = _Utils.GetChunkKeyFromFakeGlobalPos(fakeGlobalPos.ToDoubleVector3);
            int x1 = fakeGlobalPos.X - staticChunkKey.X;
            int y1 = fakeGlobalPos.Y - staticChunkKey.Y;
            int z1 = fakeGlobalPos.Z - staticChunkKey.Z;
            if (!ChunkDictionary.ContainsKey(staticChunkKey))
                return false;
            IChunk chunk = ChunkDictionary[staticChunkKey];
            blockID = chunk.Blocks[chunk.GetBlockIndex(x1, y1, z1)];
            return true;
        }

        public static bool FakeGlobalPosToChunkAndLocalPos(Point3D fakeGlobalPos, Dictionary<Point3D, IChunk> ChunkDictionary, out IChunk Chunk, out Point3D localPos)
        {
            if (_Utils.getChunkObjFromFakeGlobalPos(fakeGlobalPos, ChunkDictionary, out Chunk))
            {
                localPos = new Point3D(
                fakeGlobalPos.X - (int)Chunk.Position.X,
                fakeGlobalPos.Y - (int)Chunk.Position.Y,
                fakeGlobalPos.Z - (int)Chunk.Position.Z
                );
                return true;
            }
            else
            {
                localPos = new Point3D();
                return false;
            }
        }

        //returns fakeGlobalPos of Actor
        public static Point3D GetActorFakeGlobalPos(IActor actor, Point3D offset)
        {
            //Server reference
            IGameServer Server = actor.State as IGameServer;
            //Get systems
            var SystemsCollection = Server.Biomes.GetSystems();
            //Get system player is in
            uint currentSystemID = actor.InstanceID;
            //Define currentSystems for TryGetValue
            IBiomeSystem currentSystem;
            //Find the currentSystem based on its ID
            SystemsCollection.TryGetValue(currentSystemID, out currentSystem);
            //Get the chunk's ID that the player is in
            uint currentChunkID = actor.ConnectedChunk;
            //Search current system for the chunk based on its ID
            IChunk currentChunk = currentSystem.ChunkCollection.First(item => item.ID == currentChunkID);
            //Align player with local Chunk grid
            Point3D actorPos = new Point3D((int)Math.Round(actor.LocalChunkTransform.X), (int)Math.Round(actor.LocalChunkTransform.Y), (int)Math.Round(actor.LocalChunkTransform.Z));
            //Convert local Point to Sector Point
            Point3D fakeglobalPos = new Point3D((int)currentChunk.Position.X + actorPos.X + offset.X, (int)currentChunk.Position.Y + actorPos.Y + offset.Y, (int)currentChunk.Position.Z + actorPos.Z + offset.Z);

            return fakeglobalPos;
        }

        public static bool setPos(IActor actor, string parameter)
        {
            Point3D offset = new Point3D(0, -1, 0);
            Point3D fakeglobalPos = _Utils.GetActorFakeGlobalPos(actor, offset);

            return setPos(actor, parameter, fakeglobalPos);
        }


        public static bool setPos(IActor actor, string parameter, Point3D fakeGlobalPos)
        {
            //Server reference
            IGameServer Server = actor.State as IGameServer;
            //Store if user wants to set pos 1 or 2
            int _ID = new int();
            //Try to parse from string to int
            try
            {
                _ID = Int32.Parse(parameter);
            }
            catch (FormatException e)
            {
                Server.ChatManager.SendActorMessage("Use '1' or '2' as parameter.", actor);
                return false;
            }

            //Check if entered paramter is within range
            if (2 < _ID || _ID < 0)
            {
                Server.ChatManager.SendActorMessage("Use parameter 1 or 2. You used: " + _ID, actor);
                return false;
            }

            switch (_ID)
            {
                case 1:
                    //Push position to Player's Session storage for Pos1
                    _Utils.storeSessionVar(actor, "SNEditPos1", (object)fakeGlobalPos, true);
                    break;
                case 2:
                    //Push position to Player's Session storage for Pos2
                    _Utils.storeSessionVar(actor, "SNEditPos2", (object)fakeGlobalPos, true);
                    break;
            }

            //Return message to the player
            Server.ChatManager.SendActorMessage("Pos" + _ID + " set: " + fakeGlobalPos.ToString(), actor);
            //Command executed successfully 
            return true;
        }

        //simple helper function to store a variable
        public static bool storeSessionVar(IActor actor, string varName, Object variable, bool overwrite)
        {
            if (actor.SessionVariables.ContainsKey(varName))
            {
                if (overwrite)
                {
                    actor.SessionVariables.Remove(varName);
                    actor.SessionVariables.Add(varName, variable);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                actor.SessionVariables.Add(varName, variable);
                return true;
            }
        }

        public static Dictionary<string, ushort> GetTranslationDictionary()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(SNEditSettings.SchematicDir + "MCBlockID2SNBlockID.xml"); 

            string xmlcontents = xdoc.InnerXml;
            var xpathquery = "/ArrayOfBlockTranslations/BlockTranslation";
            XmlNodeList BlockTranslationList = xdoc.DocumentElement.SelectNodes(xpathquery);

            Dictionary<string, ushort> translationDictionary = new Dictionary<string, ushort>();

            foreach (XmlNode BlockTranslation in BlockTranslationList)
            {
                translationDictionary.Add(
                    BlockTranslation["MCBlockID"].InnerText,
                    ushort.Parse((BlockTranslation["SNTileID"].InnerText)) 
                    );
            }

            return translationDictionary;
        }

        internal static ushort ConvertMCBlockID2SNBlockID(string MCBlockID, Dictionary<string, ushort> translationDictionary)
        {
            ushort value = new ushort();
            return translationDictionary.TryGetValue(MCBlockID, out value) ? value : translationDictionary["default"];
        }

    }
}