using SharedGameData;
using SNScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PreciseMaths;
using GameServer;
using GameServer.World.Chunks;

namespace SNScriptUtils
{
    public class _Utils
    {
        public _Utils() { }

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
        public static bool checkPositions(IActor actor, out Point3D pos1, out Point3D pos2)
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

    }
}