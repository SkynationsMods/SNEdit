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

using GameServer.World;
using GameServer.World.Actors;
using GameServer.World.Chunks;

namespace SNScriptUtils
{
    public class _Utils
    {
        public _Utils() { }

        public static void translationHelper(byte[] BlockArray, byte[] MetaDataArray, Dictionary<string, ushort> translationDictionary)
        {

            List<string> ReqIDs = new List<string>();
            for (int i = 0; i < BlockArray.Length - 1; i++)
            {
                ReqIDs.Add(BlockArray[i] + ":" + MetaDataArray[i]);
            }

            List<string> distReqIDs = ReqIDs.Distinct().ToList();

            string output = "";
            string req = "";
            ushort value = new ushort();
            foreach (string ID in distReqIDs)
            {
                req = translationDictionary.TryGetValue(ID, out value) ? ID + "->" + value.ToString() + " | " : ID + "->(def)";
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

        public static Point3D calcBottomLeftPointOfCuboid(Point3D pos1, Point3D pos2)
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
                    tmpBlock[0] = 4;
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
                    blockID = workChunk.Blocks[0];
                    if (blockID == tmpBlockID)
                    {
                        workChunk.ChangeBlock(0, 0, 0, 0, true, true);
                    }
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
            //Console.WriteLine("Before Loop, OffsetList.Count = " + offsetList.Count().ToString());
            for (int i = 0; i <= offsetList.Count() - 1; i++)
            {
                //Console.WriteLine("Inside Loop. " + i.ToString());
                KeyValuePair<Point3D, IChunk> SpecialBlock = new KeyValuePair<Point3D, IChunk>();
                if (_Utils.FindCustomSpecialBlockByOffSet(sourceLocalPos, offsetList[i].X, offsetList[i].Y, offsetList[i].Z, blockID, Chunk, ChunkDictionary, out SpecialBlock))
                {
                    //Console.WriteLine("Found a Teleporter, adding to list");
                    SpecialBlockList.Add(new Object[,] { { SpecialBlock.Key, SpecialBlock.Value } });
                    //Console.WriteLine("added to list: (count) =  " + SpecialBlockList.Count.ToString());
                }
                else
                {
                    //Console.WriteLine("Found No Teleporter at Offset (OUT)");
                }
                //Console.WriteLine("Last Line in Loop");
            }
            //Console.WriteLine("Past Loop, SpecialBlockList Count: " + SpecialBlockList.Count.ToString());
            if (SpecialBlockList.Count > 0)
                return true;
            else
                return false;
        }

        public static bool FindCustomSpecialBlockByOffSet(Point3D sourceLocalPos, int xOffset, int yOffset, int zOffset, uint blockID, IChunk Chunk, Dictionary<Point3D, IChunk> ChunkDictionary, out KeyValuePair<Point3D, IChunk> SpecialBlock)
        {
            //Console.WriteLine("Inside FindByOffset.");
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
            //Console.WriteLine("got fakelocalpos as " + tmpFakeLocalPos.ToString());
            ushort targetBlockID = new ushort();
            targetBlockID = tmpChunk.Blocks[tmpChunk.GetBlockIndex(tmpFakeLocalPos.X, tmpFakeLocalPos.Y, tmpFakeLocalPos.Z)];
            //Console.WriteLine("got targetBlockID as  " + ((int)targetBlockID).ToString());
            if ((targetBlockID) == (ushort)blockID)
            {
                //Console.WriteLine("Found Teleporter, returning as keyvaluepair");
                SpecialBlock = new KeyValuePair<Point3D, IChunk>(tmpFakeLocalPos, tmpChunk);
                return true;
            }
            else
            {
                //Console.WriteLine("Found No Teleporter at Offset, returning(IN)");
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
            if (SNScriptUtils._Utils.getChunkObjFromFakeGlobalPos(fakeGlobalPos, ChunkDictionary, out Chunk))
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
            xdoc.Load("Schematics/MCBlockID2SNBlockID.xml"); 

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
            //Console.Write("Requested MCBlockID: " + MCBlockID + " ");
            ushort value = new ushort();
            return translationDictionary.TryGetValue(MCBlockID, out value) ? value : translationDictionary["default"];
        }
    }
}