using SharedGameData;
using SNScript;
using System;
using System.Linq;

namespace MoreBlocksScripts
{
    class SNEditPositionGet : GameCommand
    {
        MoreBlocksScripts.SNEdit helper = new MoreBlocksScripts.SNEdit();

        public override string[] Aliases
        {
            get { return new string[] { "//pos" }; }
        }

        public override string CommandDescription
        {
            get
            {
                return "A test command in ScriptsExample.";
            }
        }

        public override Priviledges Priviledge
        {
            get { return Priviledges.Player; }
        }

        public SNEditPositionGet(IGameServer server) : base(server)
        {
        }

        public override bool Use(IActor actor, string message, string[] parameters)
        {
            //Store if user wants pos 1 or 2
            int _ID = new int();

            try
            {
                _ID = Int32.Parse(parameters[1]);
            }
            catch (FormatException e)
            {
                Server.ChatManager.SendActorMessage("Parameter could not be parsed.", actor);
                return false;
            }

            if (2 < _ID || _ID < 0)
            {
                Server.ChatManager.SendActorMessage("Use parameter 1 or 2. You used: " + _ID, actor);
                return false;
            }
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

            //Allign player with local chunk grid
            Point3D actorPos = new Point3D((int)actor.LocalChunkTransform.X, (int)actor.LocalChunkTransform.Y, (int)actor.LocalChunkTransform.Z);

            //Convert local Point to Sector Point
            Point3D globalPos = new Point3D((int)currentChunk.Position.X + actorPos.X, (int)currentChunk.Position.Y + actorPos.Y, (int)currentChunk.Position.Z + actorPos.Z);



            //Return the saved data for testing
            Point3D returnSave = new Point3D();

            switch (_ID)
            {
                case 1:
                    //Push position to Player's Session storage for Pos1
                    if (actor.SessionVariables.ContainsKey("SNEditPos1"))
                        actor.SessionVariables.Remove("SNEditPos1");
                    actor.SessionVariables.Add("SNEditPos1", (object)globalPos);
                    returnSave = (Point3D)actor.SessionVariables["SNEditPos1"];
                    break;
                case 2:
                    //Push position to Player's Session storage for Pos2
                    if (actor.SessionVariables.ContainsKey("SNEditPos2"))
                        actor.SessionVariables.Remove("SNEditPos2");
                    actor.SessionVariables.Add("SNEditPos2", (object)globalPos);
                    returnSave = (Point3D)actor.SessionVariables["SNEditPos2"];
                    break;
            }

            //Return this to the player
            Server.ChatManager.SendActorMessage("Pos" + _ID + " set.", actor);
            //Testing only
            //Server.ChatManager.SendActorMessage("Global Pos: X=" + globalPos.X.ToString() + " Y=" + globalPos.Y.ToString() + " Z=" + globalPos.Z.ToString(), actor);
            //Server.ChatManager.SendActorMessage("Actor Pos: X=" + actorPos.X.ToString() + " Y=" + actorPos.Y.ToString() + " Z=" + actorPos.Z.ToString(), actor);
            //Server.ChatManager.SendActorMessage("Chunk Pos: X=" + ((int)currentChunk.Position.X).ToString() + " Y=" + ((int)currentChunk.Position.Y).ToString() + " Z=" + ((int)currentChunk.Position.Z).ToString(), actor);
            //Return Saved Data
            Server.ChatManager.SendActorMessage("Stored:" + returnSave.X.ToString() + " Y=" + returnSave.Y.ToString() + " Z=" + returnSave.Z.ToString(), actor);


            //Command executed successfully 
            return true;
        }
    }
}
