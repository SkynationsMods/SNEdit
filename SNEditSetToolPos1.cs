using SharedGameData.Items;
using SNScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PreciseMaths;
using SharedGameData;
using Microsoft.Xna.Framework;

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
            
            DoubleVector3 trueGlobalPos = DoubleVector3.Transform(rayPosition, currentChunk.World);
        
            DoubleVector3 rayDirection = new DoubleVector3(System.Convert.ToDouble(ray.Direction.X), System.Convert.ToDouble(ray.Direction.Y), System.Convert.ToDouble(ray.Direction.Z));

            PreciseRay hitRay = new PreciseRay(trueGlobalPos, rayDirection);
            IChunk hitChunk = null as IChunk;
            Boolean rayCastResult = currentSystem.PreciseRayCast(hitRay, (double)6.0, ref hitChunk, ref hitPoint, ref buildPoint, false);
            Point3D fakeGlobalPos = Point3D.Zero;
            if (rayCastResult) { fakeGlobalPos = new Point3D((int)hitChunk.Position.X + (int)hitPoint.X, (int)hitChunk.Position.Y + (int)hitPoint.Y, (int)hitChunk.Position.Z + (int)hitPoint.Z); };
            
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("rayPosition: " + rayPosition.ToString());
            Console.WriteLine("rayDirection: " + rayDirection.ToString());
            Console.WriteLine("hitpoint: " + hitPoint.ToString());
            Console.WriteLine("buildpoint: " + buildPoint.ToString());
            Console.WriteLine("rayCastResult: " + rayCastResult.ToString());
            if (rayCastResult) { Console.WriteLine("hitChunk: " + hitChunk.Position.ToString()); }
            if (rayCastResult) { Console.WriteLine("fakeGlobalPos : " + fakeGlobalPos.ToString()); }
            Console.WriteLine("--------------------------------------------------");

            string[] iAmStupid = new string[2];
            iAmStupid[1] = "1";

            SNScriptUtils._Utils.positionSet(myActor, iAmStupid, fakeGlobalPos);

            Server.ChatManager.SendActorMessage("RayCast result is" + hitPoint.ToString(), myActor);
        }
    }
}
