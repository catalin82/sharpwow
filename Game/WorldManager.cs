using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.Game
{
    public class WorldManager
    {
        public WorldManager()
        {
            MouseHoverChunk = null;
        }

        public void EnterWorld(uint mapid, string continent, float x, float y)
        {
            MapID = mapid;
            foreach (var file in mFiles)
            {
                file.Unload();
            }

            mFiles.Clear();

            int ix = (int)(x / Utils.Metrics.Tilesize);
            int iy = (int)(y / Utils.Metrics.Tilesize);

            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    int valX = (int)ix + j;
                    int valY = (int)iy + i;
                    if (valX < 0 || valY < 0)
                        continue;

                    ADT.IADTFile file = ADT.ADTManager.CreateADT(mWdtManager.getWDT(continent), @"World\Maps\" + continent + @"\" + continent + "_" + valX + "_" + valY + ".adt",
                        (uint)valX, (uint)valY, (i == 0 && j == 0));

                    file.Continent = continent;

                    if (file != null)
                        mFiles.Add(file);
                }
            }

            float h = 0.0f;
            GetLandHeightFast(-Utils.Metrics.MidPoint + x, -Utils.Metrics.MidPoint + y, ref h);
            h += 100.0f;
            var inpos = new SlimDX.Vector3(x, y, h);
            inpos.X = -Utils.Metrics.MidPoint + inpos.X;
            inpos.Y = -Utils.Metrics.MidPoint + inpos.Y;
            Game.GameManager.GraphicsThread.GraphicsManager.Camera.SetPosition(inpos);
            isInWorld = true;
            mContinent = continent;
            Game.GameManager.InformPropertyChanged(GameProperties.Map);
        }

        public void EnterWorld(string continent, uint x, uint y)
        {
            foreach (var file in mFiles)
            {
                file.Unload();
            }

            mFiles.Clear();

            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    int valX = (int)x + j;
                    int valY = (int)y + i;
                    if (valX < 0 || valY < 0)
                        continue;

                    ADT.IADTFile file = ADT.ADTManager.CreateADT(mWdtManager.getWDT(continent), @"World\Maps\" + continent + @"\" + continent + "_" + valX + "_" + valY + ".adt",
                        (uint)valX, (uint)valY);

                    file.Continent = continent;

                    if (file != null)
                        mFiles.Add(file);
                }
            }

            foreach (var file in mFiles)
            {
                file.WaitLoad();
            }

            var inpos = new SlimDX.Vector3(x * Utils.Metrics.Tilesize + 0.5f * Utils.Metrics.Tilesize, y * Utils.Metrics.Tilesize + 0.5f * Utils.Metrics.Tilesize, 100.0f);
            inpos.X = -Utils.Metrics.MidPoint + inpos.X;
            inpos.Y = -Utils.Metrics.MidPoint + inpos.Y;
            Game.GameManager.GraphicsThread.GraphicsManager.Camera.SetPosition(inpos);
            isInWorld = true;
            mContinent = continent;
        }

        public void Update(Video.ICamera cam)
        {
            if (isInWorld == false)
                return;

            var pos = cam.Position;
            pos.X = Utils.Metrics.MidPoint + pos.X;
            pos.Y = Utils.Metrics.MidPoint + pos.Y;
            int myX = (int)(pos.X / Utils.Metrics.Tilesize);
            int myY = (int)(pos.Y / Utils.Metrics.Tilesize);

            var qry = from file in mFiles
                      where (
                      file.IndexX > myX + 1 ||
                      file.IndexX < myX - 1 ||
                      file.IndexY > myY + 1 ||
                      file.IndexY < myY - 1
                      )
                      select file;

            foreach (var old in qry)
            {
                old.Unload();
            }

            mFiles.RemoveAll((ADT.IADTFile af) => { return qry.Contains(af); });

            uint indexValue = 0;
            List<uint> indices = new List<uint>();
            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    if (myX + j < 0 || myY + i < 0)
                        continue;

                    indexValue = ((uint)(myX + j)) * 1000 + ((uint)(myY + i));
                    indices.Add(indexValue);
                }
            }

            var sel = from index in indices
                      where (mFiles.Exists((ADT.IADTFile file) =>
                          {
                              var adtIndex = file.IndexX * 1000 + file.IndexY;
                              return adtIndex == index;
                          }
                      ) == false)
                      select index;

            foreach (var index in sel)
            {
                uint ix = index / 1000;
                uint iy = index % 1000;
                var str = @"World\Maps\" + mContinent + "\\" + mContinent + "_" + ix + "_" + iy + ".adt";
                ADT.IADTFile file = ADT.ADTManager.CreateADT(mWdtManager.getWDT(mContinent), str, ix, iy);
                file.Continent = mContinent;
                if (file != null)
                    mFiles.Add(file);                    
            }

            var curTile = GetCurrentTile();
            if (curTile != mHoveredTile)
            {
                mHoveredTile = curTile;
                Game.GameManager.InformPropertyChanged(GameProperties.HoveredADT);
            }
            var curChunk = GetCurrentChunk();
            if (curChunk != mHoveredChunk)
            {
                mHoveredChunk = curChunk;
                Game.GameManager.InformPropertyChanged(GameProperties.HoveredChunk);
            }
        }

        public bool GetLandHeightFast(float x, float y, ref float h)
        {
            float tmpx = Utils.Metrics.MidPoint + x;
            float tmpy = Utils.Metrics.MidPoint + y;

            int myX = (int)(tmpx / Utils.Metrics.Tilesize);
            int myY = (int)(tmpy / Utils.Metrics.Tilesize);

            var qry = from file in mFiles
                      where file.IndexX == myX && file.IndexY == myY
                      select file;

            if (qry.Count() == 0)
                return false;

            var fl = qry.ElementAt(0);

            float nx = tmpx - myX * Utils.Metrics.Tilesize;
            float ny = tmpy - myY * Utils.Metrics.Tilesize;

            myX = (int)(nx / Utils.Metrics.Chunksize);
            myY = (int)(ny / Utils.Metrics.Chunksize);

            uint index = (uint)(myY * 16 + myX);
            if (index >= 256)
                return false;

            try
            {
                var chunk = fl.GetChunk(index);
                float hval = 0.0f;
                bool ret = chunk.GetLandHeightFast(x, y, out hval);
                if (ret)
                    h = hval;

                return ret;
            }
            catch (Exception)
            {
            }
            return false;
        }

        private ADT.IADTFile GetCurrentTile()
        {
            var pos = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position;
            pos.X = Utils.Metrics.MidPoint + pos.X;
            pos.Y = Utils.Metrics.MidPoint + pos.Y;
            int myX = (int)(pos.X / Utils.Metrics.Tilesize);
            int myY = (int)(pos.Y / Utils.Metrics.Tilesize);

            var qry = from file in mFiles
                      where file.IndexX == myX && file.IndexY == myY
                      select file;

            if (qry.Count() == 0)
                return null;

            return qry.First();
        }

        private ADT.IADTChunk GetCurrentChunk()
        {
            var pos = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position;
            pos.X = Utils.Metrics.MidPoint + pos.X;
            pos.Y = Utils.Metrics.MidPoint + pos.Y;
            var tile = GetCurrentTile();
            if (tile == null)
                return null;

            float x = pos.X - tile.IndexX * Utils.Metrics.Tilesize;
            float y = pos.Y - tile.IndexY * Utils.Metrics.Tilesize;

            if (x < 0 || y < 0 || x >= Utils.Metrics.Tilesize || y >= Utils.Metrics.Tilesize)
                return null;

            uint ix = (uint)(x / Utils.Metrics.Chunksize);
            uint iy = (uint)(y / Utils.Metrics.Chunksize);
            uint index = iy * 16 + ix;
            try
            {
                return tile.GetChunk(index);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ADT.IADTFile HoveredTile { get { return mHoveredTile; } }
        public ADT.IADTChunk HoveredChunk { get { return mHoveredChunk; } }
        public ADT.IADTChunk MouseHoverChunk { get; set; }

        public uint MapID { get; private set; }
        public bool IsInWorld { get { return mFiles.Count != 0; } }
        public float FogStart { get { return mFogStart; } set { mFogStart = value; Game.GameManager.InformPropertyChanged(GameProperties.FogStart); } }
        public float FogDistance { get { return mFogDistance; } set { mFogDistance = value; Game.GameManager.InformPropertyChanged(GameProperties.FogDistance); } }

        private List<ADT.IADTFile> mFiles = new List<ADT.IADTFile>();
        string mContinent;
        bool isInWorld = false;
        private ADT.IADTFile mHoveredTile = null;
        private ADT.IADTChunk mHoveredChunk = null;
        private float mFogStart, mFogDistance;
        private ADT.WDTManager mWdtManager = new ADT.WDTManager();
    }
}
