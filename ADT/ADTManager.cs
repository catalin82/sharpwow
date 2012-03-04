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

        public static List<ADT.IADTChunk> VisibleChunks = new List<IADTChunk>();
    }
}
