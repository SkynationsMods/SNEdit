using SharedGameData.Items;
using SNScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PreciseMaths;
using SharedGameData;

namespace SNEdit
{
    public class PosATool : IItemScript
    {
        public void OnEquip(object actor, object item)
        {

        }

        public void OnUnequip(object actor, object item)
        {

        }

        public void OnUse(object actor, Microsoft.Xna.Framework.Ray ray, object item)
        {
            IActor myActor = actor as IActor;
            IGameServer Server = myActor.State as IGameServer;

            //Get systems
            var SystemsCollection = Server.Biomes.GetSystems();

            //Get system player is in
            uint currentSystemID = myActor.InstanceID;

            //Define currentSystems for TryGetValue
            IBiomeSystem currentSystem;

            //Find the currentSystem based on its ID
            SystemsCollection.TryGetValue(currentSystemID, out currentSystem);

            //Get the chunk's ID that the player is in
            uint currentChunkID = myActor.ConnectedChunk;

            //Search current system for the chunk based on its ID
            IChunk currentChunk = currentSystem.ChunkCollection.First(sys => sys.ID == currentChunkID);


            DoubleVector3 hitPoint = DoubleVector3.Zero;
            DoubleVector3 buildPoint = DoubleVector3.Zero;

            DoubleVector3 rayPosition = new DoubleVector3(System.Convert.ToDouble(ray.Position.X), System.Convert.ToDouble(ray.Position.Y), System.Convert.ToDouble(ray.Position.Z));
            DoubleVector3 rayDirection = new DoubleVector3(System.Convert.ToDouble(ray.Direction.X), System.Convert.ToDouble(ray.Direction.Y), System.Convert.ToDouble(ray.Direction.Z)); ;
            
            Boolean rayCastResult = currentChunk.PreciseRayCollision(rayPosition, rayDirection, 6.0, 1000000, ref hitPoint, ref buildPoint, false);


            Console.WriteLine("rayPosition: " + rayPosition.ToString());
            Console.WriteLine("rayDirection: " + rayDirection.ToString());
            Console.WriteLine("hitpoint: " + hitPoint.ToString());
            Console.WriteLine("buildpoint: " + buildPoint.ToString());
            Console.WriteLine("rayCastResult: " + rayCastResult.ToString());
            


            Server.ChatManager.SendActorMessage("RayCast result is" + hitPoint.ToString(), myActor);
        }
    }
}
