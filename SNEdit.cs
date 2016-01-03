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

namespace MoreBlocksScripts
{
    public class SNEdit
    {
        //TODOD, need to refernece the game server properly...
        public static IGameServer Server;

        public SNEdit()
        {
            //Contstructor
        }

        //Get location of chunk from a global pos
        public Point3D GetChunkKeyFromGlobalPos(DoubleVector3 pos)
        {
            int x = (int)Math.Floor(pos.X / 32.0) * 32;
            int y = (int)Math.Floor(pos.Y / 32.0) * 32;
            int z = (int)Math.Floor(pos.Z / 32.0) * 32;
            return new Point3D(x, y, z);
        }

        //Create a Dictionary of all the cunks with their cords
        public Dictionary<Point3D, IChunk> CreateChunkDictionary(IBiomeSystem currentSystem)
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

        public IChunk getChunkFromGlobal(Point3D pos, IActor actor)
        {
            //Get the server
            Server = actor.State as IGameServer;

            //Chunk ID
            Point3D staticChunkKey = this.GetChunkKeyFromGlobalPos(pos.ToDoubleVector3);

            var allSystems = Server.Biomes.GetSystems();

            IBiomeSystem currentSystem;

            allSystems.TryGetValue(actor.InstanceID, out currentSystem);

            //Dictionary of chunks
            var ChunkDictionary = CreateChunkDictionary(currentSystem);
            Chunk sourceChunk = (Chunk)ChunkDictionary[staticChunkKey];
            return sourceChunk;
        }

        //For commands to check if positions have been set
        public bool checkPositions(IActor actor, out Point3D pos1, out Point3D pos2)
        {
            //Get the server
            Server = actor.State as IGameServer;

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
    }
}