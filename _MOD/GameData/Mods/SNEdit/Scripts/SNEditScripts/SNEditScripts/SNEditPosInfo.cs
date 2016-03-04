using SharedGameData;
using SNScript;
using System;
using System.Linq;
using SNScriptUtils;
using System.Collections.Generic;

namespace SNEdit
{
    class SNEditPosInfo : GameCommand
    {
        private IGameServer Server;

        public override string[] Aliases
        {
            get { return new string[] { "//posinfo" }; }
        }

        public override string CommandDescription
        {
            get
            {
                return "Outputs information about the players current Position; For developer purposes.";
            }
        }

        public override Priviledges Priviledge
        {
            get { return Priviledges.Admin; }
        }

        public SNEditPosInfo(IGameServer server)
            : base(server)
        {
            this.Server = server;
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {

            //Get systems
            var SystemsCollection = this.Server.Biomes.GetSystems();

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
            int chunkCount = currentSystem.ChunkCollection.Count();

            //Align player with local chunk grid
            Point3D actorPos = new Point3D((int)Math.Round(actor.LocalChunkTransform.X), (int)Math.Round(actor.LocalChunkTransform.Y), (int)Math.Round(actor.LocalChunkTransform.Z));

            //Convert local Point to Sector Point
            Point3D fakeglobalPos = new Point3D((int)currentChunk.Position.X + actorPos.X, (int)currentChunk.Position.Y + actorPos.Y, (int)currentChunk.Position.Z + actorPos.Z);
            Point3D trueglobalPos = new Point3D((int)currentChunk.Position.X + actorPos.X - 32 / 2, (int)currentChunk.Position.Y + actorPos.Y - 32 / 2, (int)currentChunk.Position.Z + actorPos.Z - 32 / 2);
            //to get a global pos you can do DoubleVector3.Transform(localPoint, Chunk.World) //Ben

            Dictionary<Point3D, IChunk> ChunkDictionary = SNScriptUtils._Utils.CreateChunkDictionary(currentSystem);

            IChunk currChunkByFakeGlobalPos = new Object() as IChunk;
            Boolean foundChunk = SNScriptUtils._Utils.getChunkObjFromFakeGlobalPos(fakeglobalPos, ChunkDictionary, out currChunkByFakeGlobalPos);

            int staticCount = 0, shipCount = 0;
            foreach (IChunk c in currentSystem.ChunkCollection)
                if (c.IsStaticChunk)
                    staticCount++;
                else
                    shipCount++;

            this.Server.ChatManager.SendActorMessage("----------------------------------------------", actor);
            this.Server.ChatManager.SendActorMessage("# of Chunks in System (System.Chunks.Count): " + chunkCount.ToString(), actor);
            this.Server.ChatManager.SendActorMessage(string.Format("System Chunks: Static {0} - Ship {1}", staticCount, shipCount), actor);
            this.Server.ChatManager.SendActorMessage("----------------------------------------------", actor);
            this.Server.ChatManager.SendActorMessage("Actor Pos:" + actorPos.ToString(), actor);
            this.Server.ChatManager.SendActorMessage("Chunk Base (conn.chunk):" + currentChunk.Position.ToString(), actor);
            this.Server.ChatManager.SendActorMessage("Chunk Base(calc.by FglobPos):" + currChunkByFakeGlobalPos.Position.ToString(), actor);
            this.Server.ChatManager.SendActorMessage("Calculated global Pos: " + fakeglobalPos.ToString(), actor);
            this.Server.ChatManager.SendActorMessage("calc. True global Pos: " + trueglobalPos.ToString(), actor);
            this.Server.ChatManager.SendActorMessage("----------------------------------------------", actor);
            return true;
        }
    }
}