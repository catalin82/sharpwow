using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT
{
    public static class ADTManager
    {
        private static List<IADTFile> mActiveFiles = new List<IADTFile>();
        private static Dictionary<int, List<uint>> mModelRefs = new Dictionary<int, List<uint>>();

        public static void Render()
        {
            VisibleChunks.Clear();
            foreach (var file in mActiveFiles)
            {
                file.RenderADT();
            }
        }

        public static bool Intersect(SlimDX.Ray ray, ref float dist)
        {
            if (VisibleChunks.Count == 0)
                return false;

            bool hasHit = false;
            float nearHit = 9999999;
            foreach (var file in VisibleChunks)
            {
                float curHit = 0;
                if (file.Intersect(ray, ref curHit))
                {
                    hasHit = true;
                    if (curHit < nearHit)
                        nearHit = curHit;
                }
            }

            if (hasHit)
                dist = nearHit;

            return hasHit;
        }

        public static void ChangeTerrain(SlimDX.Vector3 pos, bool lower)
        {
            foreach (var file in mActiveFiles)
                file.ChangeTerrain(pos, lower);
        }

        public static void FlattenTerrain(SlimDX.Vector3 pos, bool lower)
        {
            foreach (var file in mActiveFiles)
                file.FlattenTerrain(pos, lower);
        }

        public static void BlurTerrain(SlimDX.Vector3 pos, bool lower)
        {
            foreach (var file in mActiveFiles)
                file.BlurTerrain(pos, lower);
        }

        public static void TextureTerrain(Game.Logic.TextureChangeParam param)
        {
            foreach (var file in mActiveFiles)
                file.TextureTerrain(param);
        }

        public static void AddModel(string modelName, SlimDX.Vector3 position)
        {
            uint adtIndexX = (uint)((position.X + Utils.Metrics.MidPoint) / Utils.Metrics.Tilesize);
            uint adtIndexY = (uint)((position.Y + Utils.Metrics.MidPoint) / Utils.Metrics.Tilesize);

            float ofsX = (position.X + Utils.Metrics.MidPoint) - (adtIndexX * Utils.Metrics.Tilesize);
            float ofsY = (position.Y + Utils.Metrics.MidPoint) - (adtIndexY * Utils.Metrics.Tilesize);

            uint cnkIndexX = (uint)(ofsX / Utils.Metrics.Chunksize);
            uint cnkIndexY = (uint)(ofsY / Utils.Metrics.Chunksize);

            var adtFile = GetADTFile(adtIndexX, adtIndexY);
            if (adtFile == null)
                return;

            var chunk = adtFile.GetChunk(cnkIndexX + cnkIndexY * 16);
            chunk.addModel(modelName, position);
        }

        public static IADTFile GetADTFile(uint indexX, uint indexY)
        {
            foreach (var adt in mActiveFiles)
            {
                if (adt.IndexX == indexX && adt.IndexY == indexY)
                    return adt;
            }

            return null;
        }

        public static void AddADT(IADTFile file)
        {
            mActiveFiles.Add(file);
        }

        public static void RemoveADT(IADTFile file)
        {
            mActiveFiles.Remove(file);
        }

        public static IADTFile CreateADT(string fileName, uint indexX, uint indexY, bool initial = false)
        {
            if (Game.GameManager.BuildNumber <= 12340)
                return new Wotlk.ADTFile(fileName, indexX, indexY, initial);

            return new Cataclysm.ADTFile(fileName, indexX, indexY, initial);
        }

        public static Models.WMO.WMOHitInformation GetWmoInformation(uint uniqueId, uint refId)
        {
            foreach (var file in mActiveFiles)
            {
                var info = file.GetWmoInfo(uniqueId, refId);
                if (info != null)
                    return info;
            }

            return null;
        }

        public static bool AddUniqueMDXId(string fileName, uint id)
        {
            int hash = fileName.ToLower().GetHashCode();
            lock (mModelRefs)
            {
                if (mModelRefs.ContainsKey(hash))
                {
                    if (mModelRefs[hash].Contains(id))
                        return false;

                    mModelRefs[hash].Add(id);
                    return true;
                }

                mModelRefs.Add(hash, new List<uint>(new uint[] { id }));
                return true;
            }
        }

        public static void RemoveUniqueMdxId(string fileName, uint id)
        {
            int hash = fileName.ToLower().GetHashCode();
            lock (mModelRefs)
            {
                if (mModelRefs.ContainsKey(hash))
                {
                    mModelRefs[hash].RemoveAll((curid) => curid == id);
                }
            }
        }

        public static List<ADT.IADTChunk> VisibleChunks = new List<IADTChunk>();
    }
}
