using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpWoW.ADT.Cataclysm
{
    /// <summary>
    /// Represents a subchunk of a large terrain file. Implements IADTChunk for general access and rendering.
    /// </summary>
    public class ADTChunk : IADTChunk
    {
        /// <summary>
        /// Initializes all parameters of the chunk without performing any load operation.
        /// </summary>
        /// <param name="parent">The parent large terrain file of this chunk</param>
        /// <param name="baseFile">The MPQ-file which contains the general information about placement and doodads</param>
        /// <param name="texFile">The MPQ-stream which contains all the texture related information (MCLY, MCAL, ...)</param>
        /// <param name="offset">The offset inside baseFile and texFile where this chunks MCNK is located</param>
        public ADTChunk(ADTFile parent, Stormlib.MPQFile baseFile, Utils.StreamedMpq texFile, ChunkOffset offset)
        {
            mFile = baseFile;
            mTexFile = texFile;
            mOffset = offset;
            mParent = parent;
        }

        /// <summary>
        /// Performs a synchronous load of the chunk including vertices, texture layers and normals.
        /// </summary>
        public void DoLoad()
        {
            mFile.Position = mOffset.Offset + 8;
            mHeader = mFile.Read<Wotlk.MCNK>();
            var posY = (32.0f * Utils.Metrics.Tilesize - mHeader.position.X) - Utils.Metrics.MidPoint;
            var posX = (32.0f * Utils.Metrics.Tilesize - mHeader.position.Y) - Utils.Metrics.MidPoint;
            mHeader.position.X = posX;
            mHeader.position.Y = posY;

            LoadVertices();
            LoadLayers();
            LoadNormals();
        }

        /// <summary>
        /// Renders the chunk to the screen using the TerrainShader. Checks visibility of the bounding box against the current active
        /// camera.
        /// </summary>
        public void Render()
        {
            if (Game.GameManager.GraphicsThread.GraphicsManager.Camera.ViewFrustum.Contains(mBox, Matrix.Identity) == ContainmentType.Disjoint)
                return;

            if (mHeader.nLayers == 0)
                return;

            if (mMesh == null)
                LoadMesh();

            if (mAlphaTexture == null)
                LoadAlphaTexture();

            var shdr = Video.ShaderCollection.TerrainShader;
            shdr.SetTechnique(mHeader.nLayers - 1);
            shdr.SetTexture("alphaTexture", mAlphaTexture);
            for (int i = 0; i < 4; ++i)
                shdr.SetValue("TextureFlags" + i, 0);
            for (int i = 0; i < mLayers.Count; ++i)
                shdr.SetTexture("blendTexture" + i, mParent.GetTexture((int)mLayers[i].textureId));

            shdr.DoRender((SlimDX.Direct3D9.Device d) =>
            {
                mMesh.DrawSubset(0);
            }
            );
        }

        /// <summary>
        /// Creates the D3D-mesh from the loaded vertices and the ADTStaticData.Indices. Writes the indicies and vertices
        /// to the buffers of the mesh.
        /// </summary>
        private void LoadMesh()
        {
            mMesh = new Mesh(Game.GameManager.GraphicsThread.GraphicsManager.Device,
                256, 145, MeshFlags.Managed, Wotlk.ADTVertex.FVF);

            var vb = mMesh.LockVertexBuffer(LockFlags.None);
            vb.WriteRange(vertices);
            mMesh.UnlockVertexBuffer();

            var ib = mMesh.LockIndexBuffer(LockFlags.None);
            ib.WriteRange(ADTStaticData.Indices);
            mMesh.UnlockIndexBuffer();
        }

        /// <summary>
        /// Loads the normals from the file and stores them in the vertices array, overwriting vertices[i].NX/NY/NZ.
        /// </summary>
        private void LoadNormals()
        {
            mFile.Position = mOffset.Offset + mHeader.ofsNormal;
            byte[] sig = mFile.Read(4);
            string sigStr = Encoding.UTF8.GetString(sig);
            mFile.Position += 4;

            uint counter = 0;
            for (uint i = 0; i < 17; ++i)
            {
                for (uint j = 0; j < (((i % 2) != 0) ? 8u : 9u); ++j)
                {
                    float nx = -((float)(mFile.Read<sbyte>()) / 127.0f);
                    float ny = -((float)(mFile.Read<sbyte>()) / 127.0f);
                    float nz = (float)(mFile.Read<sbyte>()) / 127.0f;
                    vertices[counter].NX = nx;
                    vertices[counter].NY = ny;
                    vertices[counter++].NZ = nz;
                }
            }
        }

        /// <summary>
        /// Performs the asynchrnous unloading tasks that do not need to run in the main graphics thread.
        /// Basically unloads all I/O related work previously loaded.
        /// </summary>
        public void AsyncUnload()
        {
            vertices = null;
        }

        /// <summary>
        /// Performs all the unload of synchronous data from the graphics thread. Unloads the mesh, frees the alpha texture.
        /// </summary>
        public void Unload()
        {
            if (mMesh != null)
            {
                mMesh.Dispose();
                mMesh = null;
            }

            if (mAlphaTexture != null)
                ADT.ADTAlphaHandler.AddFreeTexture(mAlphaTexture);

            mAlphaTexture = null;
        }

        /// <summary>
        /// Populates the vertices array with the height data from the file and the X/Y offsets from the header Also loads the BoundingBox and
        /// sets the texture coordinates for each vertex.
        /// </summary>
        private void LoadVertices()
        {
            mFile.Position = mOffset.Offset + mHeader.ofsHeight;
            byte[] sig = mFile.Read(4);
            string sigStr = Encoding.UTF8.GetString(sig);
            mFile.Position += 4;

            uint counter = 0;
            Vector3 minPos = new Vector3(999999.9f);
            Vector3 maxPos = new Vector3(-999999.9f);
            minPos.X = mHeader.position.X;
            minPos.Y = mHeader.position.Y;
            maxPos.X = minPos.X + Utils.Metrics.Chunksize;
            maxPos.Y = minPos.Y + Utils.Metrics.Chunksize;
            for (int i = 0; i < 17; ++i)
            {
                for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); ++j)
                {
                    float x, y, z;
                    z = mFile.Read<float>() + mHeader.position.Z;
                    y = i * Utils.Metrics.Unitsize * 0.5f + mHeader.position.Y;
                    x = j * Utils.Metrics.Unitsize + mHeader.position.X;

                    if ((i % 2) != 0)
                        x += 0.5f * Utils.Metrics.Unitsize;

                    if (z < minPos.Z)
                        minPos.Z = z;
                    if (z > maxPos.Z)
                        maxPos.Z = z;
                    if (x < minPos.X)
                        minPos.X = x;
                    if (x > maxPos.X)
                        maxPos.X = x;
                    if (y < minPos.Y)
                        minPos.Y = y;
                    if (y > maxPos.Y)
                        maxPos.Y = y;

                    vertices[counter] = new Wotlk.ADTVertex()
                    {
                        X = x,
                        Y = y,
                        Z = z,
                        U = ADTStaticData.TexCoords[counter, 0],
                        V = ADTStaticData.TexCoords[counter, 1],
                        S = ADTStaticData.AlphaCoords[counter, 0],
                        T = ADTStaticData.AlphaCoords[counter, 1]
                    };

                    ++counter;
                }
            }

            mBox = new BoundingBox(minPos, maxPos);
            MinPosition = minPos;
            MaxPosition = maxPos;
        }

        /// <summary>
        /// Loads the texture layer information (MCLY) from the file and for each layer the corresponding AlphaData.
        /// </summary>
        private void LoadLayers()
        {
            mTexFile.Position = mOffset.OffsetTexStream + 4;
            byte[] chunkData = new byte[mTexFile.Read<uint>()];
            mTexFile.Read(chunkData, 0, chunkData.Length);
            MemoryStream memStrm = new MemoryStream(chunkData);
            SeekChunk(memStrm, "YLCM");
            mTexFile.Position = mOffset.OffsetTexStream + 8 + memStrm.Position + 4;
            uint size = mTexFile.Read<uint>();

            mHeader.nLayers = (uint)(size / Marshal.SizeOf(typeof(MCLY)));
            for (uint i = 0; i < mHeader.nLayers; ++i)
            {
                MCLY layer = mTexFile.Read<MCLY>();
                mLayers.Add(layer);
            }

            if (mHeader.nLayers > 1)
            {
                try
                {
                    SeekChunk(memStrm, "LACM");
                    mTexFile.Position = mOffset.OffsetTexStream + 8 + memStrm.Position + 4;
                }
                catch (Exception)
                {
                    mHeader.nLayers = 1;
                }
            }

            LoadAlpha();

        }

        /// <summary>
        /// Loads the alphadata for each layer according to the flags. It also loads the hole bitmap and the shadows.
        /// </summary>
        private void LoadAlpha()
        {
            for (int i = 0; i < 64; ++i)
            {
                for (int j = 0; j < 64; ++j)
                {
                    float x = i * ADTStaticData.HoleSize;
                    float y = j * ADTStaticData.HoleSize;
                    uint stepx = (uint)Math.Floor(x / ADTStaticData.HoleLen);
                    uint stepy = (uint)Math.Floor(y / ADTStaticData.HoleLen);

                    byte factor = (byte)((mHeader.holes & (ADTStaticData.HoleBitmap[stepx, stepy])) != 0 ? 0 : 1);
                    AlphaData[(i * 64 + j) * 4 + 3] = (byte)(0xFF * factor);
                }
            }

            if (Header.nLayers > 1)
            {
                uint alphaSize = mTexFile.Read<uint>();
                byte[] alphaChunk = new byte[alphaSize];
                mTexFile.Read(alphaChunk, 0, alphaChunk.Length);

                for (int j = 1; j < Header.nLayers; ++j)
                {
                    if ((mLayers[j].flags & 0x200) != 0)
                    {
                        MCLY ly = mLayers[j];
                        byte[] alpha = DecompressAlphaData(4096, ref ly.ofsMCAL, alphaChunk);
                        for (int k = 0; k < 4096; ++k)
                            AlphaData[k * 4 + j - 1] = alpha[k];
                    }
                    else if ((mLayers[j].flags & 0x100) != 0)
                    {
                        MCLY ly = mLayers[j];
                        for (int k = 0; k < 4096; ++k)
                            AlphaData[k * 4 + j - 1] = alphaChunk[ly.ofsMCAL + k];

                    }
                    else
                    {
                        for (int k = 0; k < 4096; ++k)
                            AlphaData[k * 4 + j - 1] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Decompresses a chunk of alpha data from a given starting position.
        /// </summary>
        /// <param name="numBytes">The number of bytes that should be as output</param>
        /// <param name="startPos">The index in AlphChunk where to start</param>
        /// <param name="AlphaChunk">The chunk with data.</param>
        /// <returns></returns>
        private byte[] DecompressAlphaData(uint numBytes, ref uint startPos, byte[] AlphaChunk)
        {
            uint counterOut = 0;
            byte[] ret = new byte[numBytes];

            while (counterOut < numBytes)
            {
                byte a = AlphaChunk[startPos++];
                if (((sbyte)a) < 0)
                {
                    uint n = (uint)(a & 0x7F);
                    a = AlphaChunk[startPos++];
                    for (uint k = 0; k < n && counterOut < numBytes; ++k)
                        ret[counterOut++] = a;
                }
                else
                {
                    for (uint k = 0; k < a && counterOut < numBytes; ++k)
                        ret[counterOut++] = AlphaChunk[startPos++];
                }
            }

            return ret;
        }

        /// <summary>
        /// Acquires a new or used alpha texture from the ADTAlphaHandler and then loads the surface with AlphaData
        /// </summary>
        private void LoadAlphaTexture()
        {
            mAlphaTexture = ADTAlphaHandler.FreeTexture();
            if (mAlphaTexture == null)
                mAlphaTexture = new SlimDX.Direct3D9.Texture(Game.GameManager.GraphicsThread.GraphicsManager.Device, 64, 64, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            Surface baseSurf = mAlphaTexture.GetSurfaceLevel(0);
            System.Drawing.Rectangle rec = System.Drawing.Rectangle.FromLTRB(0, 0, 64, 64);
            Surface.FromMemory(baseSurf, AlphaData, Filter.Box, 0, Format.A8R8G8B8, 4 * 64, rec);
            baseSurf.Dispose();
        }

        /// <summary>
        /// Searches a chunk in the file by its 4 byte signature
        /// </summary>
        /// <param name="strm">The stream to search in</param>
        /// <param name="id">The 4 byte ID that identifies the chunk</param>
        /// <exception cref="System.IndexOutOfRangeException">If the signature wasnt found</exception>
        private void SeekChunk(Stream strm, string id)
        {
            strm.Position = 0;
            while (GetChunkSignature(strm) != id)
            {
                byte[] szBytes = new byte[4];
                strm.Read(szBytes, 0, 4);
                uint size = BitConverter.ToUInt32(szBytes, 0);
                strm.Position += size;
            }
            strm.Position -= 4;
        }

        /// <summary>
        /// Gets a 4 byte chunk signature from the current position of a stream
        /// </summary>
        /// <param name="strm">The stream to read from</param>
        /// <returns>String with the 4 byte chunk signature</returns>
        /// <exception cref="System.IndexOutOfRangeException">If the stream has not enough space to read a signature from</exception>
        private string GetChunkSignature(Stream strm)
        {
            var bytes = new byte[4];
            strm.Read(bytes, 0, 4);
            var ret = Encoding.UTF8.GetString(bytes);
            return ret;
        }

        private Stormlib.MPQFile mFile;
        private Utils.StreamedMpq mTexFile;
        private ChunkOffset mOffset;
        private Wotlk.ADTVertex[] vertices = new Wotlk.ADTVertex[145];
        private BoundingBox mBox;
        private Mesh mMesh;
        private List<MCLY> mLayers = new List<MCLY>();
        private byte[] AlphaData = new byte[4096 * 4];
        private short[,] AlphaFloats = new short[4096, 3];
        private Texture mAlphaTexture = null;
        private ADTFile mParent;

        /// <summary>
        /// Gets the minimum position on all 3 axis of this chunk.
        /// </summary>
        public Vector3 MinPosition { get; private set; }
        /// <summary>
        /// Gets the maximum position on all 3 axis of this chunk.
        /// </summary>
        public Vector3 MaxPosition { get; private set; }

        /// <summary>
        /// Blurs the terrain on a given position with all the properties from Game.GameManager.TerrainLogic (Radius, Intensity, ...)
        /// </summary>
        /// <param name="pos">The origin of the blur</param>
        /// <param name="lower">Unused</param>
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

                switch (Game.GameManager.TerrainLogic.ChangeMode)
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

        public override void FlattenTerrain(Vector3 pos, bool lower)
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

        public override bool Intersect(Ray ray, ref float dist)
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

        public override void addModel(string name, Vector3 pos)
        {
            throw new NotImplementedException();
        }

        private void RecalcNormals()
        {
            for (uint i = 0; i < 145; ++i)
            {
                Vector3 N1, N2, N3, N4;
                Vector3 P1, P2, P3, P4;

                P1.X = vertices[i].X - Utils.Metrics.Unitsize * 0.5f;
                P1.Y = vertices[i].Y - Utils.Metrics.Unitsize * 0.5f;
                P1.Z = vertices[i].Z;
                Game.GameManager.WorldManager.GetLandHeightFast(P1.X, P1.Y, ref P1.Z);

                P2.X = vertices[i].X + Utils.Metrics.Unitsize * 0.5f;
                P2.Y = vertices[i].Y - Utils.Metrics.Unitsize * 0.5f;
                P2.Z = vertices[i].Z;
                Game.GameManager.WorldManager.GetLandHeightFast(P2.X, P2.Y, ref P2.Z);

                P3.X = vertices[i].X + Utils.Metrics.Unitsize * 0.5f;
                P3.Y = vertices[i].Y + Utils.Metrics.Unitsize * 0.5f;
                P3.Z = vertices[i].Z;
                Game.GameManager.WorldManager.GetLandHeightFast(P3.X, P3.Y, ref P3.Z);

                P4.X = vertices[i].X - Utils.Metrics.Unitsize * 0.5f;
                P4.Y = vertices[i].Y + Utils.Metrics.Unitsize * 0.5f;
                P4.Z = vertices[i].Z;
                Game.GameManager.WorldManager.GetLandHeightFast(P4.X, P4.Y, ref P4.Z);

                Vector3 vert = new Vector3(vertices[i].X, vertices[i].Y, vertices[i].Z);

                N1 = Vector3.Cross((P2 - vert), (P1 - vert));
                N2 = Vector3.Cross((P3 - vert), (P2 - vert));
                N3 = Vector3.Cross((P4 - vert), (P3 - vert));
                N4 = Vector3.Cross((P1 - vert), (P4 - vert));

                var Norm = N1 + N2 + N3 + N4;
                Norm.Normalize();
                Norm *= -1;

                vertices[i].NX = Norm.X;
                vertices[i].NY = Norm.Y;
                vertices[i].NZ = Norm.Z;
            }
        }
    }
}
