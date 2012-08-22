using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace SharpWoW.Models.MDX
{
    public class MdxIntersectionResult
    {
        public bool HasHit { get; set; }
        public M2BatchRender Renderer { get; set; }
        public M2Info Model { get; set; }
        public float Distance { get; set; }
        public Vector3 HitPoint { get; set; }
        public uint InstanceID { get; set; }
        public MdxInstanceData InstanceData { get; set; }
    }

    public class ModelIntersector
    {
        public ModelIntersector(M2BatchRender renderer)
        {
            mRenderer = renderer;
        }

        public bool Intersect(Ray ray, out float distance, out uint id, out Vector3 hitPoint)
        {
            bool hasHit = false;
            distance = -1;
            var meshes = mRenderer.MeshPasses;
            id = 0;
            hitPoint = Vector3.Zero;
            foreach (var instance in mRenderer.LockInstances())
            {
                foreach (var mesh in meshes)
                {
                    var newRay = Video.Picking.CalcRayForTransform(instance.ModelMatrix);
                    float dist = 0.0f;
                    if (mesh.Intersects(newRay, out dist))
                    {
                        hasHit = true;
                        hitPoint = newRay.Position + dist * newRay.Direction;
                        hitPoint = Vector3.TransformCoordinate(hitPoint, instance.ModelMatrix);
                        var realDistance = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position - hitPoint;
                        dist = realDistance.Length();

                        if (dist < distance || distance < 0)
                        {
                            distance = dist;
                            id = instance.InstanceId;
                        }
                    }
                }
            }

            return hasHit;
        }

        M2BatchRender mRenderer;
    }
}
