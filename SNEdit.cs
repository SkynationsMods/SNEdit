using SharedGameData;
using SNScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SNEdit
{
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
        Point3D staticChunkKey = this.GetChunkKeyFromGlobalPos(pos.ToDoubleVector3);
        Dictionary ChunkDictionary = CreateChunkDictionary(actor.systemID);
        IChunk sourceChunk = this.ChunkDictionary[staticChunkKey];
        return sourceChunk;
    }

    //For commands to check if positions have been set
    public bool checkPositions(IActor actor, out Point3D pos1, out Point3D pos2)
    {
        Point3D pos1 = (Point3D)actor.SessionVariables["SNEditPos1"];
        Point3D pos2 = (Point3D)actor.SessionVariables["SNEditPos2"];

        if (pos1 == null)
        {
            Server.ChatManager.SendMessage("Position 1 is not set.", actor);
            return false;
        }
        else if (pos2 == null)
        {
            Server.ChatManager.SendMessage("Position 2 is not set.", actor);
            return false;
        }
        else if (pos2 != null && pos1 != null)
        {
            return true;
        } else
        {
            Server.ChatManager.SendMessage("Position 1 and Position 2 are not set.", actor);
            return false;
        }

    }







    //IGNORE THIS FOR NOW
    /*
    Point3D _pos1 = actor.SessionVariables.POS1;
    Point3D _pos2 = actor.SessionVariables.POS2;

    IBiomeManager BiomeManager = Server.Biomes;

    var SystemsCollection = BiomeManager.GetSystems();

    //uint TargetSystemID = Convert.ToUInt32(parameters[1]);
    uint currentSystemID = actor.InstanceID;

    IBiomeSystem currentSystem;

    SystemsCollection.TryGetValue(currentSystemID, out currentSystem);

    uint currentChunkID = actor.ConnectedChunk;

    IChunk currentChunk = currentSystem.ChunkCollection.First(item => item.ID == currentChunkID);

    Point3D actorPos = new Point3D((int)actor.LocalChunkTransform.X, (int)actor.LocalChunkTransform.Y, (int)actor.LocalChunkTransform.Z);

    Point3D globalPos = new Point3D((int)currentChunk.Position.X + actorPos.X, (int)currentChunk.Position.Y + actorPos.Y, (int)currentChunk.Position.Z + actorPos.Z);

    actor.SessionVariables.Add("POS1", (object)globalPos);
    */

}
}
