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

        public override bool Intersect(SlimDX.Ray ray, ref float dist, out IADTChunk hitChunk)
        {
            hitChunk = null;
            if (mMesh == null)
                return false;

            float nDist = 0.0f;
            if (mMesh.Intersects(ray, out nDist))
            {
                hitChunk = this;
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

                    case Game.Logic.ChangeMode.Special:
                        {
                            float inner = Game.GameManager.TerrainLogic.InnerRadius;
                            if (dist <= inner)
                            {
                                switch (Game.GameManager.TerrainLogic.InnerChangeMode)
                                {
                                    case Game.Logic.ChangeMode.Flat:
                                        vertices[i].Z += amount * (lower ? -1 : 1);
                                        break;

                                    case Game.Logic.ChangeMode.Linear:
                                        vertices[i].Z += (amount * (1.0f - dist / inner)) * (lower ? -1 : 1);
                                        break;

                                    case Game.Logic.ChangeMode.Smooth:
                                        vertices[i].Z += (amount / (1.0f + dist / inner)) * (lower ? -1 : 1);
                                        break;
                                }
                            }
                            else
                            {
                                float nradius = radius - inner;
                                float ndist = dist - inner;

                                switch (Game.GameManager.TerrainLogic.InnerChangeMode)
                                {
                                    case Game.Logic.ChangeMode.Flat:
                                        vertices[i].Z += amount * (lower ? -1 : 1);
                                        break;

                                    case Game.Logic.ChangeMode.Linear:
                                        vertices[i].Z += (amount * (1.0f - ndist / nradius)) * (lower ? -1 : 1);
                                        break;

                                    case Game.Logic.ChangeMode.Smooth:
                                        vertices[i].Z += (amount / (1.0f + ndist / nradius)) * (lower ? -1 : 1);
                                        break;
                                }
                            }
                        }
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

        /// <summary>
        /// Adds a new layer of textures to the chunk without checking if it already exists.
        /// Returns the index inside the table.
        /// </summary>
        /// <param name="textureName">The name of the texture of the layer</param>
        /// <returns></returns>
        private int addTextureLayer(string textureName)
        {
            if (mLayers.Count >= 4)
                return -1;

            MCLY layer = new MCLY();
            layer.effectId = -1;
            layer.flags = 0;
            layer.offsetMCAL = 0;
            layer.textureId = (uint)mParent.addTexture(textureName);

            mLayers.Add(layer);
            mHeader.nLayers = (uint)mLayers.Count;
            return mLayers.Count - 1;
        }

        private int getTextureLayer(string textureName)
        {
            for (int i = 0; i < mLayers.Count; ++i)
            {
                if (mParent.getTextureName(i).ToLower() == textureName.ToLower())
                    return i;
            }

            return -1;
        }

        public void textureTerrain(Game.Logic.TextureChangeParam param)
        {
            // check if we actually have something to change
            float xdiff = mHeader.position.X - param.ActionSource.X + Utils.Metrics.Chunksize / 2;
            float zdiff = mHeader.position.Y - param.ActionSource.Y + Utils.Metrics.Chunksize / 2;

            float dist = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);

            if (dist > (param.OuterRadius + Utils.Metrics.ChunkRadius))
                return;

            int layer = getTextureLayer(param.TextureName);
            if (layer == -1)
            {
                layer = addTextureLayer(param.TextureName);
                if (layer == -1)
                    return;
            }

            // doesnt make no sense to texture the ground layer, its opaque by default!
            if (layer == 0)
                return;

            // ground layer uses channel 3 of the rgba
            // layer 1 uses channel 0, layer 2 uses channel 1 and layer 3 uses channel 3
            int alphaLayer = layer - 1;

            for (int i = 0; i < 64; ++i)
            {
                for (int j = 0; j < 64; ++j)
                {
                    float posX = j * (Utils.Metrics.Chunksize / 64.0f) + mHeader.position.X;
                    float posY = i * (Utils.Metrics.Chunksize / 64.0f) + mHeader.position.Y;

                    xdiff = posX - param.ActionSource.X;
                    zdiff = posY - param.ActionSource.Y;

                    dist = (float)Math.Sqrt(xdiff * xdiff + zdiff * zdiff);

                    if (dist > param.OuterRadius)
                        continue;

                    if (dist <= param.InnerRadius)
                    {
                        float newVal = 0.0f;
                        switch(param.Falloff)
                        {
                            case Game.Logic.TextureChangeParam.FalloffMode.Cosinus:
                                newVal = Utils.SharpMath.CosinusInterpolate(param.Strength, param.FalloffTreshold, (dist / param.InnerRadius));
                                break;

                            case Game.Logic.TextureChangeParam.FalloffMode.Linear:
                                newVal = Utils.SharpMath.Lerp(param.Strength, param.FalloffTreshold, (dist / param.InnerRadius));
                                break;
                            case Game.Logic.TextureChangeParam.FalloffMode.Flat:
                                newVal = param.Strength;
                                break;
                        }

                        newVal += AlphaFloats[(i * 64 + j), alphaLayer];
                        if (newVal >= 65535.0f)
                            newVal = 65535.0f;

                        AlphaFloats[(i * 64 + j), alphaLayer] = (ushort)newVal;
                        if (AlphaFloats[(i * 64 + j), alphaLayer] > param.AlphaCap)
                            AlphaFloats[(i * 64 + j), alphaLayer] = (ushort)param.AlphaCap;

                        AlphaData[(i * 64 + j) * 4 + alphaLayer] = (byte)((AlphaFloats[(i * 64 + j), alphaLayer] / 65535.0f) * 255.0f);
                    }
                    else if (dist <= param.OuterRadius)
                    {
                        float newVal = Utils.SharpMath.Lerp((param.Falloff == Game.Logic.TextureChangeParam.FalloffMode.Flat ? param.Strength : param.FalloffTreshold), 0, (dist - param.InnerRadius) / (param.OuterRadius - param.InnerRadius));
                        newVal += AlphaFloats[(i * 64 + j), alphaLayer];
                        if (newVal >= 65535.0f)
                            newVal = 65535.0f;

                        AlphaFloats[(i * 64 + j), alphaLayer] = (ushort)newVal;
                        if (AlphaFloats[(i * 64 + j), alphaLayer] > param.AlphaCap)
                            AlphaFloats[(i * 64 + j), alphaLayer] = (ushort)param.AlphaCap;

                        AlphaData[(i * 64 + j) * 4 + alphaLayer] = (byte)((AlphaFloats[(i * 64 + j), alphaLayer] / 65535.0f) * 255.0f);
                    }

                    mAlphaDirty = true;
                }
            }
        }

        public override void addModel(string modelName, SlimDX.Vector3 pos)
        {
            bool isMdx = modelName.ToLower().EndsWith(".mdx") || modelName.ToLower().EndsWith(".m2");
            bool isWmo = modelName.ToLower().EndsWith(".wmo");

            if (!isMdx && !isWmo)
                throw new Exception("'" + modelName + "' is no valid model!");

            if (isMdx)
                addMdxModel(modelName, pos);
            else if (isWmo)
                addWmoModel(modelName, pos);
        }

        private void addMdxModel(string modelName, SlimDX.Vector3 pos)
        {
            ADT.Wotlk.MDDF mdf = new MDDF();
            mdf.orientationX = mdf.orientationY = mdf.orientationZ = 0.0f;
            mdf.posX = pos.X + Utils.Metrics.MidPoint;
            mdf.posZ = pos.Y + Utils.Metrics.MidPoint;
            mdf.posY = pos.Z;

            mdf.scaleFactor = 1024;
            mdf.idMMID = (uint)mParent.addMdxName(modelName);
            mRefs.Add(mParent.addModelDefintion(mdf));
        }

        private void addWmoModel(string modelName, SlimDX.Vector3 pos)
        {
            MODF modf = new MODF();
            modf.flags = 0;
            modf.lextentX = modf.lextentY = modf.lextentZ = 0;
            modf.padding = 0;
            modf.Position = new Vector3(pos.X + Utils.Metrics.MidPoint, pos.Z, pos.Y + Utils.Metrics.MidPoint);
            modf.Rotation = Vector3.Zero;
            modf.setIndex = 0;
            modf.uextentX = modf.uextentY = modf.uextentZ = 0;
            modf.uniqueId = 0;
            modf.unknown = 0;

            modf.idMWID = (uint)mParent.addWmoName(modelName);
            mWmoRefs.Add(mParent.addWmoDefintion(modf));
        }
    }
}
