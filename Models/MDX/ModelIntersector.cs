using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace SharpWoW.Models.MDX
{
    public class MdxIntersectionResult
    {
        public M2BatchRender Renderer { get; set; }
        public M2Info Model { get; set; }
        public float Distance { get; set; }
        public uint InstanceID { get; set; }
        public MdxInstanceData InstanceData { get; set; }
    }

    public class ModelIntersector
    {
        public ModelIntersector(M2BatchRender renderer)
        {
            mRenderer = renderer;
        }

        public bool Intersect(Ray ray, out float distance, out uint id)
        {
            bool hasHit = false;
            distance = -1;
            var meshes = mRenderer.MeshPasses;
            id = 0;
            foreach (var instance in mRenderer.LockInstances())
            {
                var newRay = Video.Picking.CalcRayForTransform(instance.ModelMatrix);
                foreach (var mesh in meshes)
                {
                    float dist = 0.0f;
                    if (mesh.Intersects(newRay, out dist))
                    {
                        hasHit = true;
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
