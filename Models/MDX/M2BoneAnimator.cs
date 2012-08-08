using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace SharpWoW.Models.MDX
{
    public class M2BoneAnimator
    {
        public M2BoneAnimator(Stormlib.MPQFile file, M2Info parent)
        {
            M2Animation[] anims = new M2Animation[parent.Header.nAnimations];
            file.Position = parent.Header.ofsAnimations;
            file.Read(anims);

            Animations.AddRange(anims);

            this.file = file;
            var bones = new M2Bone[parent.Header.nBones];
            file.Position = parent.Header.ofsBones;
            file.Read(bones);

            foreach (var bone in bones)
            {
                M2AnimationBone ab = new M2AnimationBone(bone, this, file, parent.GlobalSequences);
                Bones.Add(ab);
                ab.BoneIndex = Bones.Count - 1;
            }
            foreach (var bone in Bones)
            {
                bone.Init();
            }
        }

        public void OnFrame()
        {
            foreach (var b in Bones)
                b.End();

            foreach (var b in Bones)
                b.CalcMatrix();
        }

        public M2AnimationBone GetBone(short index)
        {
            if (index == -1 || index >= Bones.Count)
                return null;

            return Bones[index];
        }

        List<M2AnimationBone> Bones = new List<M2AnimationBone>();
        public List<M2Animation> Animations = new List<M2Animation>();
        Stormlib.MPQFile file;
    }

    public class M2AnimationBone
    {
        public int BoneIndex = 0;

        public M2AnimationBone(M2Bone bone, M2BoneAnimator Anim, Stormlib.MPQFile f, uint[] gs)
        {
            Animator = Anim;
            fileInfo = bone;
            var ap = new M2Animator<Vector3, Vector3>(fileInfo.Translation, f, gs);
            ap.Load();
            ap.SelectedAnim = 0;
            AnimPos = new PositionAnimator(ap);
            AnimPos.MaxTime = TimeSpan.FromMilliseconds(Anim.Animations[0].Length);
            ap = new M2Animator<Vector3, Vector3>(fileInfo.Scaling, f, gs);
            ap.Load();
            ap.SelectedAnim = 0;
            AnimScale = new PositionAnimator(ap);
            AnimScale.Default = new Vector3(1, 1, 1);
            AnimScale.MaxTime = TimeSpan.FromMilliseconds(Anim.Animations[0].Length);
            var ar = new M2Animator<Quaternion16, Quaternion>(fileInfo.Rotation, f, gs);
            ar.Load();
            ar.SelectedAnim = 0;
            AnimRot = new RotationAnimator(ar);
            AnimRot.MaxTime = TimeSpan.FromMilliseconds(Anim.Animations[0].Length);
        }

        public void Init()
        {
            ParentBone = Animator.GetBone(fileInfo.ParentBone);
        }

        public void End()
        {
            if (ParentBone != null)
                ParentBone.End();
            shouldCalcMat = true;
        }

        public void CalcMatrix()
        {
            if (!shouldCalcMat)
                return;

            matFrame = Matrix.Identity;
            matFrame *= Matrix.Translation(-fileInfo.PivotPoint.X, -fileInfo.PivotPoint.Y, -fileInfo.PivotPoint.Z);
            var pos = AnimPos.GetValue();
            var sca = AnimScale.GetValue();


            matFrame *= Matrix.Scaling(sca.X, sca.Y, sca.Z);
            matFrame *= Matrix.RotationQuaternion(AnimRot.GetValue());
            matFrame *= Matrix.Translation(pos.X, pos.Y, pos.Z);


            matFrame *= Matrix.Translation(fileInfo.PivotPoint.X, fileInfo.PivotPoint.Y, fileInfo.PivotPoint.Z);

            if (ParentBone != null)
                matFrame *= ParentBone.Matrix;

            shouldCalcMat = false;
        }

        public M2AnimationBone Parent { get { return ParentBone; } }

        public Matrix Matrix
        {
            get
            {
                CalcMatrix();
                return matFrame;
            }
        }

        bool shouldCalcMat = true;
        Matrix matFrame;
        M2BoneAnimator Animator;
        M2AnimationBone ParentBone;
        PositionAnimator AnimPos;
        PositionAnimator AnimScale;
        RotationAnimator AnimRot;
        M2Bone fileInfo;
    }
}
