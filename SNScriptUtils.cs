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


        //Create a Dictionary of all the cunks with their cords
        public static Dictionary<Point3D, IChunk> CreateChunkDictionary(IBiomeSystem currentSystem)
        {
            Dictionary<Point3D, IChunk> Dictionary = new Dictionary<Point3D, IChunk>();
            for (int i = 0; i < currentSystem.ChunkCollection.Count - 1; i++)
            {
                if (currentSystem.ChunkCollection[i].IsStaticChunk)
                {
                    Dictionary.Add(Point3D.ConvertDoubleVector3(currentSystem.ChunkCollection[i].Position), currentSystem.ChunkCollection[i]);
                }
            }
            return Dictionary;
        }

        public static IChunk getChunkObjFromFakeGlobalPos(Point3D fakeGlobalPos, IActor actor)
        {
            //Get the server
            IGameServer Server = actor.State as IGameServer;

            //Chunk ID
            Point3D staticChunkKey = _Utils.GetChunkKeyFromFakeGlobalPos(fakeGlobalPos.ToDoubleVector3);

            Dictionary<uint, IBiomeSystem> SystemsCollection = Server.Biomes.GetSystems();

            IBiomeSystem currentSystem;

            SystemsCollection.TryGetValue(actor.InstanceID, out currentSystem);

            //Dictionary of chunks
            Dictionary<Point3D, IChunk> ChunkDictionary = _Utils.CreateChunkDictionary(currentSystem);
            Chunk sourceChunk = (Chunk)ChunkDictionary[staticChunkKey];
            return sourceChunk;
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

        public static List<Object> FindSpecialBlocksAround(Point3D sourcePos, List<Point3D> offsetList, uint blockID, IChunk Chunk, IBiomeSystem System)
        {
            List<Object> SpecialBlocksList = new List<Object>();
            for (int i = 1; i < offsetList.Count() - 1; i++)
            {
                Object SpecialBlock = new Object();
                if ((SpecialBlock = _Utils.FindSpecialBlockByOffSet(offsetList[i].X, offsetList[i].Y, offsetList[i].Z, blockID, Chunk, System)) != null)
                    SpecialBlocksList.Add(SpecialBlock);
            }
            return SpecialBlocksList;
        }
        
        public static Object FindSpecialBlockByOffSet(int xOffset, int yOffset, int zOffset, uint blockID, IChunk Chunk, IBiomeSystem System)
        {
            //WIP
            return null;
        }
       
        public static bool GetBlockIdAtPos(Dictionary<Point3D, IChunk> ChunkDictionary, Point3D fakeGlobalPos, out ushort blockID)
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

    }
}