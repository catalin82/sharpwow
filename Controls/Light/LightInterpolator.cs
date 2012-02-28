using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.Controls.Light
{
    public class LightInterpolator
    {
        public LightInterpolator()
        {
            mColors.Add(Vector3.Zero);
            mColors.Add(new Vector3(1, 1, 1));
            mTimes.Add(0);
            mTimes.Add(2880);
        }

        public void RemoveTime(uint time)
        {
            int index = mTimes.FindIndex((v) => v == time);
            if (index == -1)
                return;

            mTimes.RemoveAt(index);
            mColors.RemoveAt(index);
        }

        public void ChangeColorForTime(uint time, System.Drawing.Color clr)
        {
            int index = mTimes.FindIndex((v) => v == time);
            if (index == -1)
                return;

            mColors[index] = new Vector3(clr.R / 255.0f, clr.G / 255.0f, clr.B / 255.0f);
        }

        public void ChangeTime(uint oldTime, uint newTime)
        {
            int index = mTimes.FindIndex((v) => v == oldTime);
            if (index == -1)
                return;

            var color = mColors[index];

            RemoveTime(oldTime);
            AddColor(newTime, color);
        }

        private void AddColor(uint time, Vector3 vc)
        {
            if (time > 2880)
                throw new ArgumentException("Time specified is not valid!");

            for (int i = 0; i < mTimes.Count; ++i)
            {
                if (i == mTimes.Count - 1)
                {
                    mTimes.Add(time);
                    mColors.Add(vc);
                    return;
                }

                var ts = mTimes[i];
                var te = mTimes[i + 1];
                if (ts <= time && te >= time)
                {
                    mTimes.Insert(i + 1, time);
                    mColors.Insert(i + 1, vc);
                    return;
                }
            }
        }

        public void InitFromTable(List<uint> timeList, List<Vector3> colorList)
        {
            if (timeList.Count != colorList.Count)
                throw new ArgumentException("for each time there must be a color and for each color there must be a time!!");

            if (timeList.FindIndex((u) => u > 2880) != -1)
                throw new ArgumentException("There is a time in the timeList which exceeds the limit of 2880 \"half-minutes\".");

            mTimes = timeList;
            mColors = colorList;
        }

        public void AddColor(uint time, System.Drawing.Color clr)
        {
            Vector3 vc = new Vector3(clr.R / 255.0f, clr.G / 255.0f, clr.B / 255.0f);
            AddColor(time, vc);
        }

        public Vector3 GetColorForTime(float time)
        {
            if (time < 0)
                time = (float)((Game.GameManager.GameTime.TotalMilliseconds / 10.0f) % 2880);
            else
                time %= 2880;

            if (mTimes.Count == 0)
                return Vector3.Zero;

            if (mTimes[0] > time)
                time = mTimes[0];

            if (mTimes.Count == 1)
                return mColors[0];

            Vector3 v1 = Vector3.Zero, v2 = Vector3.Zero;
            uint t1 = 0, t2 = 0;

            for (int i = 0; i < mTimes.Count; ++i)
            {
                if (i + 1 >= mTimes.Count)
                {
                    v1 = mColors[i];
                    v2 = mColors[0];
                    t1 = mTimes[i];
                    t2 = mTimes[0] + 2880;
                    break;
                }

                var ts = mTimes[i];
                var te = mTimes[i + 1];
                if (ts <= time && te >= time)
                {
                    t1 = ts;
                    t2 = te;
                    v1 = mColors[i];
                    v2 = mColors[i + 1];
                    break;
                }
            }

            uint diff = t2 - t1;
            if (diff == 0)
                return v1;

            float sat = (time - t1) / diff;
            return (v1 + sat * (v2 - v1));
        }

        List<Vector3> mColors = new List<Vector3>();
        List<uint> mTimes = new List<uint>();

        public List<uint> TimeTable { get { return mTimes; } }
        public List<Vector3> ColorTable { get { return mColors; } }
    }
}
