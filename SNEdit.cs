using SharedGameData;
using SharedGameData.Networking;
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
using Lidgren.Network;


namespace MoreBlocksScripts
{
    public class SNEdit
    {
        //TODO, need to referece the game server properly...
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

        public IChunk getChunkObjFromGlobalPos(Point3D pos, IActor actor)
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

        //This function takes a List with Global Point3Ds for the blocks to place
        //Once they are placed on server side they will be networked to the clients
        public void VPlaceBatch(List<Point3D> blocksToPlace, byte[] blockTypes ,IActor actor)
        {
            IChunk currentChunk;                                                            //Stores the last chunk worked with
            Point3D chunkPos;                                                               //Stores the current position being worked with
            Point3D blockPos;                                                               //Stores position of block inside the chunk
            Point3D lastChunk = new Point3D();                                              //Last positions used for optimization
            ushort currentBlock;                                                            //Check to place one type or different type of blocks
            Dictionary<Chunk, Point3D> blocksToSend = new Dictionary<Chunk, Point3D>();     //Stores all blocks to send

            //Loop to run through every block that needs placeing
            for(int i = 0; i < blocksToPlace.Count; i++)
            {
                //Get the position of the chunk from Global Position
                chunkPos.X = (int)blocksToPlace[i].X / 32;
                chunkPos.Y = (int)blocksToPlace[i].Y / 32;
                chunkPos.Z = (int)blocksToPlace[i].Z / 32;

                //Get the position of the block based on remainder of Global Position
                blockPos.X = (int)blocksToPlace[i].X % 32;
                blockPos.Y = (int)blocksToPlace[i].Y % 32;
                blockPos.Z = (int)blocksToPlace[i].Z % 32;

                //Check if block falls in same chunk as last one based on Global Position
                if (chunkPos != lastChunk)
                {
                    currentChunk = (IChunk)getChunkObjFromGlobalPos(chunkPos, actor);
                }

                if(blockTypes.Length != blocksToPlace.Count)
                {
                    currentBlock = blockTypes[0];
                } else
                {
                    currentBlock = blockTypes[i];
                }

                //Place the block on SERVER SIDE ONLY
                currentChunk.ChangeBlock(blockTypes[currentBlock], (int)blockPos.X, (int)blockPos.Y, (int)blockPos.Z, false, false);

                //Add block to the list to send
                blocksToSend.Add((Chunk)currentChunk, blockPos);

                //Set the position of current chunk to the last one edited
                chunkPos = lastChunk;

            }

            //Network changes
            //var allChunks = blocksToSend.Keys.ToArray();

            IChunk keyChunk;
            List<Point3D> blockList = new List<Point3D>();

            foreach(KeyValuePair<Chunk, Point3D> values in blocksToSend)
            {
                if(keyChunk == values.Key) {
                    blockList.Add(values.Value);
                } else
                {
                    VSendBlockBatch(blockList, keyChunk);
                    blockList.Clear();
                    keyChunk = values.Key;
                    blockList.Add(values.Value);
                }
            }



        }


        //Modified version of Chunk.SendBlockBatch
        private void VSendBlockBatch(List<Point3D> blocks, Chunk _chunk)
        {
            NetOutgoingMessage entityMessage = _chunk.CreateEntityMessage();
            entityMessage.Write((byte)1);
            entityMessage.Write(blocks.Count);
            for (int index = 0; index < blocks.Count; ++index)
            {
                int blockIndex = _chunk.GetBlockIndex(blocks[index]);
                entityMessage.Write((byte)blocks[index].X);
                entityMessage.Write((byte)blocks[index].Y);
                entityMessage.Write((byte)blocks[index].Z);
                entityMessage.Write(_chunk.Blocks[blockIndex]);
            }
            _chunk.SendPacket(entityMessage, NetDeliveryMethod.ReliableOrdered);
        }
    }
}