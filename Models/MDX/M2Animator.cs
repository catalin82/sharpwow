using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.Runtime.InteropServices;

namespace SharpWoW.Models.MDX
{
    public class M2Animator<T, D>
        where T : struct
        where D : struct
    {
        public M2Animator(AnimationBlock block, Stormlib.MPQFile file, uint[] GlobalSeqs)
        {
            Block = block;
            File = file;
            if (GlobalSeqs != null && block.SequenceID >= 0 && block.SequenceID < GlobalSeqs.Length)
                Sequence = GlobalSeqs[block.SequenceID];
            else
                Sequence = 0;
            SelectedAnim = 0;
        }

        public M2Animator(AnimationBlock block, Stormlib.MPQFile file, uint[] GlobalSeqs, Stormlib.MPQFile[] AnimFiles)
            : this(block, file, GlobalSeqs)
        {
            this.AnimFiles = AnimFiles;
        }

        public void Load()
        {
            if (Block.numKeyFrames == 0 || Block.numTimeStamps == 0)
                return;

            LoadTimeLines();
            LoadKeyLines();
        }

        public bool HasAnimData()
        {
            return TimeFrameLines.Count > SelectedAnim && TimeFrameLines[SelectedAnim].Count > 0;
        }

        public D GetValueForAnim(int time, AnimConverter conv = null)
        {
            if (SelectedAnim >= TimeFrameLines.Count)
            {
                if (SelectedAnim == 0)
                    return default(D);
                throw new InvalidOperationException("anim >= TimeFrameLines.Count");
            }

            if (TimeFrameLines[SelectedAnim].Count == 0)
                return default(D);

            int tval = time;
            if (time != 0)
            {
                if (Sequence > 0)
                    tval = (int)(time % Sequence);
                else
                    tval = (int)(time % (TimeFrameLines[SelectedAnim].Last() == 0 ? time : (int)TimeFrameLines[SelectedAnim].Last()));
            }

            int pos = 0;
            bool found = false;
            bool useOne = false;
            if (TimeFrameLines[SelectedAnim].Count == 1)
            {
                useOne = true;
                found = true;
                pos = 0;
            }
            else
            {
                for (; pos < TimeFrameLines[SelectedAnim].Count - 1; ++pos)
                {
                    uint t1 = TimeFrameLines[SelectedAnim][pos];
                    uint t2 = TimeFrameLines[SelectedAnim][pos + 1];
                    if (t1 <= tval && t2 >= tval)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (found == false)
                useOne = true;

            T val1 = KeyFrameLines[SelectedAnim][pos];
            T val2;
            if (useOne)
                val2 = val1;
            else
                val2 = KeyFrameLines[SelectedAnim][pos + 1];

            uint ti1 = TimeFrameLines[SelectedAnim][pos];
            uint ti2 = 0;
            if (useOne)
                ti2 = ti1;
            else
                ti2 = TimeFrameLines[SelectedAnim][pos + 1];

            float am = 1.0f;
            if (!useOne)
                am = (tval - ti1) / ((float)ti2 - ti1);

            D cval1 = default(D);
            D cval2 = default(D);
            if (conv != null)
            {
                cval1 = (D)conv.Convert(val1);
                cval2 = (D)conv.Convert(val2);
            }
            else
            {
                cval1 = (D)AnimConverter.Default.Convert(val1);
                cval2 = (D)AnimConverter.Default.Convert(val2);
            }
            return AnimInterpolator.Current.Interpolate(cval1, cval2, am);
        }

        private void LoadTimeLines()
        {
            for (uint i = 0; i < Block.numTimeStamps; ++i)
            {
                List<uint> CurLine = new List<uint>();
                ValuePair pair;
                bool readAnim = false;
                if (AnimFiles == null || i >= AnimFiles.Length || AnimFiles[i] == null || AnimFiles[i].FileSize < (Block.ofsTimeStamps + i * 8))
                    pair = File.ReadAt<ValuePair>(Block.ofsTimeStamps + i * 8);
                else
                {
                    readAnim = true;
                    pair = AnimFiles[i].ReadAt<ValuePair>(Block.ofsTimeStamps + i * 8);
                }

                if (pair.Count == 0)
                {
                    TimeFrameLines.Add(CurLine);
                    continue;
                }

                uint[] timeValues = new uint[pair.Count];
                if (!readAnim)
                {
                    File.Position = pair.Offset;
                    File.Read(timeValues);
                }
                else
                {
                    AnimFiles[i].Position = pair.Offset;
                    AnimFiles[i].Read(timeValues);
                }
                CurLine.AddRange(timeValues);
                TimeFrameLines.Add(CurLine);
            }
        }

        private void LoadKeyLines()
        {
            for (uint i = 0; i < Block.numKeyFrames; ++i)
            {
                List<T> curLine = new List<T>();
                ValuePair pair;
                bool readAnim = false;
                if (AnimFiles == null || i >= AnimFiles.Length || AnimFiles[i] == null || AnimFiles[i].FileSize < (Block.ofsKeyFrames + i * 8))
                    pair = File.ReadAt<ValuePair>(Block.ofsKeyFrames + i * 8);
                else
                {
                    readAnim = true;
                    pair = AnimFiles[i].ReadAt<ValuePair>(Block.ofsKeyFrames + i * 8);
                }

                if (pair.Count == 0)
                {
                    KeyFrameLines.Add(curLine);
                    continue;
                }

                T[] vals = new T[pair.Count];
                if (!readAnim)
                {
                    File.Position = pair.Offset;
                    File.Read(vals);
                }
                else
                {
                    AnimFiles[i].Position = pair.Offset;
                    AnimFiles[i].Read(vals);
                }
                curLine.AddRange(vals);
                KeyFrameLines.Add(curLine);
            }
        }

        public void Dump(string fanem)
        {
            var wr = System.IO.File.AppendText(fanem);
            wr.WriteLine("-> GlobalSequence: " + Block.SequenceID);
            for (int i = 0; i < KeyFrameLines.Count; ++i)
            {
                wr.WriteLine("Line " + i);
                for (int j = 0; j < KeyFrameLines[i].Count; ++j)
                {
                    uint frame = TimeFrameLines[i][j];
                    wr.Write("\tFrame " + frame + ": ");
                    T val = KeyFrameLines[i][j];
                    wr.WriteLine(val.ToString());
                }
            }
            wr.Close();
        }

        public List<List<T>> KeyFrameLines = new List<List<T>>();
        public List<List<uint>> TimeFrameLines = new List<List<uint>>();
        public int SelectedAnim { get; set; }

        private Stormlib.MPQFile[] AnimFiles = null;
        uint Sequence;
        AnimationBlock Block;
        Stormlib.MPQFile File;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ValuePair
    {
        public uint Count;
        public uint Offset;
    }

    public class ColorAnimator
    {
        public ColorAnimator(M2Animator<Vector3, Vector3> Colors, M2Animator<short, short> Alphas)
        {
            Colorer = Colors;
            AlphaGen = Alphas;
        }

        public void SetValues()
        {
            TimeSpan diff = DateTime.Now - StartTime;
            Vector3 col = new Vector3(1, 1, 1);
            float Alpha = 1.0f;
            if (Colorer.HasAnimData())
                col = Colorer.GetValueForAnim((int)diff.TotalMilliseconds);
            if (AlphaGen.HasAnimData())
                Alpha = AlphaGen.GetValueForAnim((int)diff.TotalMilliseconds) / (float)0x7FFF;
        }

        DateTime StartTime = DateTime.Now;
        M2Animator<Vector3, Vector3> Colorer;
        M2Animator<short, short> AlphaGen;
    }

    public class AlphaAnimator
    {
        public AlphaAnimator(M2Animator<short, short> Alphas)
        {
            AlphaGen = Alphas;
        }

        public void SetValues()
        {
            TimeSpan diff = DateTime.Now - StartTime;
            float Alpha = 1.0f;
            if (AlphaGen.HasAnimData())
                Alpha = AlphaGen.GetValueForAnim((int)diff.TotalMilliseconds) / (float)0x7FFF;

        }

        DateTime StartTime = DateTime.Now;
        M2Animator<short, short> AlphaGen;
    }

    public class PositionAnimator
    {
        public PositionAnimator(M2Animator<SlimDX.Vector3, Vector3> anim)
        {
            Animator = anim;
        }

        public Vector3 GetValue()
        {
            TimeSpan diff = DateTime.Now - StartTime;
            Vector3 pos = Default;
            if (Animator.HasAnimData())
            {
                int maxMs = (int)MaxTime.Milliseconds;
                if (maxMs == 0)
                    maxMs = 1;
                int animS = (int)(diff.TotalMilliseconds);
                pos = Animator.GetValueForAnim(animS);
            }
            return pos;
        }

        public Vector3 Default = Vector3.Zero;

        public TimeSpan MaxTime { get; set; }
        DateTime StartTime = DateTime.Now;
        M2Animator<SlimDX.Vector3, Vector3> Animator;
    }

    public class RotationAnimator
    {
        public RotationAnimator(M2Animator<Quaternion16, Quaternion> anim)
        {
            Animator = anim;
        }

        public Quaternion GetValue(bool fix = false)
        {
            Converter.UseFixed = fix;
            TimeSpan diff = DateTime.Now - StartTime;
            Quaternion quad = Quaternion.Identity;
            if (Animator.HasAnimData())
            {
                int maxMs = (int)MaxTime.Milliseconds;
                if (maxMs == 0)
                    maxMs = 1;
                int animS = (int)(diff.TotalMilliseconds);// % maxMs;
                quad = Animator.GetValueForAnim(animS, Converter);
            }
            return quad;
        }

        public TimeSpan MaxTime { get; set; }
        DateTime StartTime = DateTime.Now;
        M2Animator<Quaternion16, Quaternion> Animator;
        QuaternionConverter Converter = new QuaternionConverter();
    }
}
