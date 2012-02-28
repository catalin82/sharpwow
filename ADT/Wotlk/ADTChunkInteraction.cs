using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.ADT.Wotlk
{
    public partial class ADTChunk
    {
        public override bool GetLandHeightFast(float x, float y, out float h)
        {
            h = 0.0f;

            float dx = x - MinPosition.X;
            float dy = y - MinPosition.Y;

            int row = (int)(dy / (Utils.Metrics.Unitsize * 0.5f) + 0.5f);
            int col = (int)((dx - Utils.Metrics.Unitsize * 0.5f * (row % 2)) / Utils.Metrics.Unitsize + 0.5f);
            if (row < 0 || col < 0 || row > 16 || col > (((row % 2) != 0) ? 8 : 9))
                return false;

            h = vertices[17 * (row / 2) + (((row % 2) != 0) ? 9 : 0) + col].Z;
            return true;
        }

        public override bool Intersect(SlimDX.Ray ray, ref float dist)
        {
            if (mMesh == null)
                return false;

            float nDist = 0.0f;
            if (mMesh.Intersects(ray, out nDist))
            {
                dist = nDist;
                return true;
            }
            return false;
        }

        public override void ChangeTerrain(Vector3 pos, bool lower)
        {
            float radius = Game.GameManager.TerrainLogic.Radius;
            float amount = Game.GameManager.TerrainLogic.Intensity / 30.0f;
            bool changed = false;

            float xdiff = mHeader.position.X - pos.X + Utils.Metrics.Chunksize / 2;
            float zdiff = mHeader.position.Y - pos.Y + Utils.Metrics.Chunksize / 2;

            float dist = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);

            if (dist > (radius + Utils.Metrics.ChunkRadius))
                return;

            for (uint i = 0; i < 145; ++i)
            {
                xdiff = vertices[i].X - pos.X;
                zdiff = vertices[i].Y - pos.Y;

                dist = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);

                if (dist > radius)
                    continue;

                switch(Game.GameManager.TerrainLogic.ChangeMode)
                {
                    case Game.Logic.ChangeMode.Flat:
                        vertices[i].Z += amount * (lower ? -1 : 1);
                        break;

                    case Game.Logic.ChangeMode.Linear:
                        vertices[i].Z += (amount * (1.0f - dist / radius)) * (lower ? -1 : 1);
                        break;

                    case Game.Logic.ChangeMode.Smooth:
                        vertices[i].Z += (amount / (1.0f + dist / radius)) * (lower ? -1 : 1);
                        break;

                    case Game.Logic.ChangeMode.Quadratic:
                        vertices[i].Z += ((((0.0f - amount) / (float)Math.Pow(radius, 2.0f)) * ((float)Math.Pow(dist, 2.0f))) + amount) * (lower ? -1 : 1);
                        break;

                    case Game.Logic.ChangeMode.Spline:
                        vertices[i].Z += (float)Game.GameManager.TerrainLogic.TerrainSpline.Interpolate(dist / radius) * amount;
                        break;
                }


                if (vertices[i].Z < MinPosition.Z)
                {
                    MinPosition = new Vector3(MinPosition.X, MinPosition.Y, vertices[i].Z);
                    mBox = new BoundingBox(MinPosition, MaxPosition);
                }
                if (vertices[i].Z > MaxPosition.Z)
                {
                    MaxPosition = new Vector3(MaxPosition.X, MaxPosition.Y, vertices[i].Z);
                    mBox = new BoundingBox(MinPosition, MaxPosition);
                }

                changed = true;
            }

            if (changed)
                RecalcNormals();

            if (changed && mMesh != null)
            {
                var strm = mMesh.LockVertexBuffer(SlimDX.Direct3D9.LockFlags.None);
                strm.WriteRange(vertices);
                mMesh.UnlockVertexBuffer();
            }
        }

        public override void FlattenTerrain(SlimDX.Vector3 pos, bool lower)
        {
            float radius = Game.GameManager.TerrainLogic.Radius;
            float amount = Game.GameManager.TerrainLogic.Intensity / 150;
            if (amount > 1)
                amount = 1;

            amount = 1 - amount;
            bool changed = false;

            float xdiff = mHeader.position.X - pos.X + Utils.Metrics.Chunksize / 2;
            float zdiff = mHeader.position.Y - pos.Y + Utils.Metrics.Chunksize / 2;

            float dist = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);

            if (dist > (radius + Utils.Metrics.ChunkRadius))
                return;

            for (uint i = 0; i < 145; ++i)
            {
                xdiff = vertices[i].X - pos.X;
                zdiff = vertices[i].Y - pos.Y;

                dist = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);

                if (dist > radius)
                    continue;

                switch (Game.GameManager.TerrainLogic.ChangeMode)
                {
                    case Game.Logic.ChangeMode.Flat:
                        vertices[i].Z = amount * vertices[i].Z + (1 - amount) * pos.Z;
                        break;
                    case Game.Logic.ChangeMode.Linear:
                        {
                            float nremain = 1 - (1 - amount) * (1 - dist / radius);
                            vertices[i].Z = nremain * vertices[i].Z + (1 - nremain) * pos.Z;
                            break;
                        }
                    case Game.Logic.ChangeMode.Smooth:
                        {
                            float nremain = 1.0f - (float)Math.Pow(1.0f - amount, (1.0f + dist / radius));
                            vertices[i].Z = nremain * vertices[i].Z + (1 - nremain) * pos.Z;
                            break;
                        }
                }

                changed = true;
            }

            if (changed)
                RecalcNormals();

            if (changed && mMesh != null)
            {
                var strm = mMesh.LockVertexBuffer(SlimDX.Direct3D9.LockFlags.None);
                strm.WriteRange(vertices);
                mMesh.UnlockVertexBuffer();
            }
        }

        public override void BlurTerrain(Vector3 pos, bool lower)
        {
            float radius = Game.GameManager.TerrainLogic.Radius;
            float amount = Game.GameManager.TerrainLogic.Intensity / 150;
            if (amount > 1)
                amount = 1;

            amount = 1 - amount;
            bool changed = false;

            float xdiff = mHeader.position.X - pos.X + Utils.Metrics.Chunksize / 2;
            float zdiff = mHeader.position.Y - pos.Y + Utils.Metrics.Chunksize / 2;

            float dist = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);

            if (dist > (radius + Utils.Metrics.ChunkRadius))
                return;

            for (uint i = 0; i < 145; ++i)
            {
                xdiff = vertices[i].X - pos.X;
                zdiff = vertices[i].Y - pos.Y;

                dist = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);

                if (dist > radius)
                    continue;

                float TotalHeight;
                float TotalWeight;
                float tx, tz, h;
                int Rad = (int)(radius / Utils.Metrics.Unitsize);

                TotalHeight = 0;
                TotalWeight = 0;
                for (int j = -Rad * 2; j <= Rad * 2; j++)
                {
                    tz = pos.Y + j * Utils.Metrics.Unitsize / 2;
                    for (int k = -Rad; k <= Rad; k++)
                    {
                        tx = pos.X + k * Utils.Metrics.Unitsize + (j % 2) * Utils.Metrics.Unitsize / 2.0f;
                        xdiff = tx - vertices[i].X;
                        zdiff = tz - vertices[i].Y;
                        float dist2 = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);
                        if (dist2 > radius)
                            continue;
                        float height = vertices[i].Z;
                        Game.GameManager.WorldManager.GetLandHeightFast(tx, tz, ref height);
                        TotalHeight += (1.0f - dist2 / radius) * height;
                        TotalWeight += (1.0f - dist2 / radius);
                    }
                }


                h = TotalHeight / TotalWeight;
                switch (Game.GameManager.TerrainLogic.ChangeMode)
                {
                    case Game.Logic.ChangeMode.Flat:
                        vertices[i].Z = amount * vertices[i].Z + (1 - amount) * h;
                        break;

                    case Game.Logic.ChangeMode.Linear:
                        {
                            float nremain = 1 - (1 - amount) * (1 - dist / radius);
                            vertices[i].Z = nremain * vertices[i].Z + (1 - nremain) * h;
                            break;
                        }

                    case Game.Logic.ChangeMode.Smooth:
                        {
                            float nremain = 1.0f - (float)Math.Pow(1.0f - amount, (1.0f + dist / radius));
                            vertices[i].Z = nremain * vertices[i].Z + (1 - nremain) * h;
                            break;
                        }
                }

                changed = true;
            }

            if (changed)
                RecalcNormals();

            if (changed && mMesh != null)
            {
                var strm = mMesh.LockVertexBuffer(SlimDX.Direct3D9.LockFlags.None);
                strm.WriteRange(vertices);
                mMesh.UnlockVertexBuffer();
            }
        }
    }
}
