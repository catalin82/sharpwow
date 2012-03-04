using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.Models.MDX
{
    public class M2Info
    {
        public M2Info(string modelName)
        {
            mFile = new Stormlib.MPQFile(modelName);
            Header = mFile.Read<M2Header>();

            BoundingBox = new SlimDX.BoundingBox(Header.VertexMin, Header.VertexMax);

            mFile.Position = Header.ofsName;
            byte[] data = mFile.Read(Header.lenName - 1);
            ModelName = Encoding.UTF8.GetString(data);

            FileDirectory = System.IO.Path.GetDirectoryName(modelName) + "\\";
            var vertices = new M2Vertex[Header.nVertices];
            mFile.Position = Header.ofsVertices;
            mFile.Read(vertices);

            LoadTextures();
            ParseVertices(vertices);
            LoadSkins();
        }

        private void LoadTextures()
        {
            var textures = new M2Texture[Header.nTextures];
            mFile.Position = Header.ofsTextures;
            mFile.Read(textures);

            foreach (var tex in textures)
            {
                mFile.Position = tex.ofsName;
                byte[] data = mFile.Read(tex.lenName);
                Textures.Add(Encoding.UTF8.GetString(data));
            }
        }

        private void ParseVertices(M2Vertex[] vertices)
        {
            foreach (var v in vertices)
            {
                MdxVertex vw = new MdxVertex()
                {
                    X = v.x,
                    Y = v.y,
                    Z = v.z,
                    NX = v.nx,
                    NY = v.ny,
                    NZ = v.nz,
                    U = v.u,
                    V = v.v
                };

                Vertices.Add(vw);
            }
        }

        private void LoadSkins()
        {
            string skinFile = FileDirectory + ModelName + "00.skin";
            Stormlib.MPQFile skin = new Stormlib.MPQFile(skinFile);
            skin.Dispose();
            SKINView mView = skin.Read<SKINView>();
            ushort[] indexLookup = new ushort[mView.nIndices];
            skin.Position = mView.ofsIndices;
            skin.Read(indexLookup);
            ushort[] triangles = new ushort[mView.nTriangles];
            skin.Position = mView.ofsTriangles;
            skin.Read(triangles);

            SKINSubMesh[] SubMeshes = new SKINSubMesh[mView.nSubMeshes];
            skin.Position = mView.ofsSubMeshes;
            skin.Read(SubMeshes);

            SKINTexUnit[] TexUnits = new SKINTexUnit[mView.nTexUnits];
            skin.Position = mView.ofsTexUnits;
            skin.Read(TexUnits);

            ushort[] texLookUp = new ushort[Header.nTexLookups];
            mFile.Position = Header.ofsTexLookups;
            mFile.Read(texLookUp);

            ushort[] texUnitLookUp = new ushort[Header.nTexUnits];
            mFile.Position = Header.ofsTexUnits;
            mFile.Read(texUnitLookUp);

            ushort[] indices = new ushort[mView.nTriangles];
            for (int i = 0; i < mView.nTriangles; ++i)
                indices[i] = indexLookup[triangles[i]];

            for (int i = 0; i < mView.nTexUnits; ++i)
            {
                M2RenderPass pass = new M2RenderPass();
                SKINSubMesh mesh = SubMeshes[TexUnits[i].SubMesh1];
                pass.Vertices = new MdxVertex[mesh.nTriangles];
                pass.Texture = Textures[(int)texLookUp[TexUnits[i].Texture]];

                for (ushort t = mesh.startTriangle, k = 0; k < mesh.nTriangles; ++t, ++k)
                {
                    ushort index = indices[t];
                    pass.Vertices[k] = Vertices[index];
                }

                Passes.Add(pass);
            }
        }

        Stormlib.MPQFile mFile;

        public M2Header Header { get; private set; }
        public string ModelName { get; private set; }
        public string FileDirectory { get; private set; }
        public List<string> Textures = new List<string>();
        public List<MdxVertex> Vertices = new List<MdxVertex>();
        public List<M2RenderPass> Passes = new List<M2RenderPass>();
        public SlimDX.BoundingBox BoundingBox { get; private set; }
    }
}
