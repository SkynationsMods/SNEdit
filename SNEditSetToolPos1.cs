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
            //GetBase Vars
            //get Actor
            IActor myActor = actor as IActor;
            //Get GameServer
            IGameServer Server = myActor.State as IGameServer;
            //Get systems
            var SystemsCollection = Server.Biomes.GetSystems();
            //Get system the player is in
            uint currentSystemID = myActor.InstanceID;
            //Define currentSystems for TryGetValue
            IBiomeSystem currentSystem;
            //Find the currentSystem based on its ID
            SystemsCollection.TryGetValue(currentSystemID, out currentSystem);
            //Get the chunk's ID, that the player is in
            uint currentChunkID = myActor.ConnectedChunk;
            //Search current system for the chunk based on its ID
            IChunk currentChunk = currentSystem.ChunkCollection.First(sys => sys.ID == currentChunkID);

            //get Starting Point from ray object (its a local Pos)
            DoubleVector3 rayStartPointAsLocalPosition = new DoubleVector3(System.Convert.ToDouble(ray.Position.X), System.Convert.ToDouble(ray.Position.Y), System.Convert.ToDouble(ray.Position.Z));
            //get direction of the Ray from ray object (essentially, where is the player looking to)
            DoubleVector3 rayDirection = new DoubleVector3(System.Convert.ToDouble(ray.Direction.X), System.Convert.ToDouble(ray.Direction.Y), System.Convert.ToDouble(ray.Direction.Z));
            //calculate true global Pos (its needed for the new ray Object)
            DoubleVector3 rayStartPointAsTrueGlobalPos = DoubleVector3.Transform(rayStartPointAsLocalPosition, currentChunk.World);
            //Create new Ray Object with calculated outputs
            PreciseRay hitRay = new PreciseRay(rayStartPointAsTrueGlobalPos, rayDirection);
            
            //execute RayCast
            //set up return objects for coming operation
            IChunk hitChunk = null as IChunk;
            DoubleVector3 hitPoint = DoubleVector3.Zero;
            DoubleVector3 buildPoint = DoubleVector3.Zero;
            //Do rayCast with calculated Ray Object
            Boolean rayCastResult = currentSystem.PreciseRayCast(hitRay, (double)6.0, ref hitChunk, ref hitPoint, ref buildPoint, false);
            
            Point3D fakeGlobalPos = Point3D.Zero;
            if (rayCastResult) { //if something was hit by the raycast
                fakeGlobalPos =  //calculate the position of what has been hit as fakeGlobalPos
                    new Point3D(
                    (int)hitChunk.Position.X + (int)hitPoint.X, 
                    (int)hitChunk.Position.Y + (int)hitPoint.Y, 
                    (int)hitChunk.Position.Z + (int)hitPoint.Z
                    ); 
            };

            SNScriptUtils._Utils.setPos(myActor, "1", fakeGlobalPos);

            Server.ChatManager.SendActorMessage("RayCast result is" + hitPoint.ToString(), myActor);
        }
    }
}
