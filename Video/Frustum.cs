using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.Video
{
    public class Frustum
    {
        private Plane[] planes;

        public Frustum()
        {
            planes = new Plane[6];
        }

        public void BuildViewFrustum(Matrix viewMatrix, Matrix projectionMatrix)
        {
            Matrix viewProjection = Matrix.Multiply(viewMatrix, projectionMatrix);

            //left plane
            planes[0] = new Plane(-viewProjection.M14 + viewProjection.M11,
                                -viewProjection.M24 + viewProjection.M21,
                                -viewProjection.M34 + viewProjection.M31,
                                -viewProjection.M44 + viewProjection.M41);

            //right plane
            planes[1] = new Plane(-viewProjection.M14 - viewProjection.M11,
                                -viewProjection.M24 - viewProjection.M21,
                                -viewProjection.M34 - viewProjection.M31,
                                -viewProjection.M44 - viewProjection.M41);

            //top plane
            planes[2] = new Plane(-viewProjection.M14 - viewProjection.M12,
                                -viewProjection.M24 - viewProjection.M22,
                                -viewProjection.M34 - viewProjection.M32,
                                -viewProjection.M44 - viewProjection.M42);

            //bottom plane
            planes[3] = new Plane(-viewProjection.M14 + viewProjection.M12,
                                -viewProjection.M24 + viewProjection.M22,
                                -viewProjection.M34 + viewProjection.M32,
                                -viewProjection.M44 + viewProjection.M42);

            //near plane
            planes[4] = new Plane(-viewProjection.M13,
                                -viewProjection.M23,
                                -viewProjection.M33,
                                -viewProjection.M43);

            //far plane
            planes[5] = new Plane(-viewProjection.M14 + viewProjection.M13,
                                -viewProjection.M24 + viewProjection.M23,
                                -viewProjection.M34 + viewProjection.M33,
                                -viewProjection.M44 + viewProjection.M43);

            for (int i = 0; i < 6; i++) planes[i].Normalize();
        }

        public ContainmentType Contains(BoundingBox boundingBox, Matrix worldMatrix)
        {
            var bboxcl = boundingBox.GetCorners().ToList();
            for (int i = 0; i < bboxcl.Count; ++i)
                bboxcl[i] = Vector3.TransformCoordinate(bboxcl[i], worldMatrix);

            Vector3 newMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 newMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            for (int i = 0; i < bboxcl.Count; ++i)
            {
                if (bboxcl[i].X < newMin.X)
                    newMin.X = bboxcl[i].X;
                if (bboxcl[i].X > newMax.X)
                    newMax.X = bboxcl[i].X;

                if (bboxcl[i].Y < newMin.Y)
                    newMin.Y = bboxcl[i].Y;
                if (bboxcl[i].Y > newMax.Y)
                    newMax.Y = bboxcl[i].Y;

                if (bboxcl[i].Z < newMin.Z)
                    newMin.Z = bboxcl[i].Z;
                if (bboxcl[i].Z > newMax.Z)
                    newMax.Z = bboxcl[i].Z;
            }

            boundingBox = new BoundingBox(newMin, newMax);

            bool flag = false;
            foreach (Plane plane in planes)
            {
                switch (Plane.Intersects(plane, boundingBox))
                {
                    case PlaneIntersectionType.Front:
                        return ContainmentType.Disjoint;
                    case PlaneIntersectionType.Intersecting:
                        flag = true;
                        break;
                }
            }
            if (!flag) return ContainmentType.Contains;
            return ContainmentType.Intersects;
        }

        public ContainmentType Contains(BoundingSphere sphere, Matrix worldMatrix)
        {
            sphere.Center = Vector3.TransformCoordinate(sphere.Center, worldMatrix);

            foreach (Plane plane in planes)
            {
                var dot = Plane.DotCoordinate(plane, sphere.Center);
                if (dot < -sphere.Radius)
                    return ContainmentType.Disjoint;
            }

            return ContainmentType.Contains;
        }
    }
}
