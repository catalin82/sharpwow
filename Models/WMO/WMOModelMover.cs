using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace SharpWoW.Models.WMO
{
    class WMOModelMover : IModelMover
    {
        public WMOModelMover(WMOHitInformation wmoHit)
        {
            mResult = wmoHit;
        }

        public void rotateModel(SlimDX.Vector3 axis, float amount)
        {
            var invWorld = SlimDX.Matrix.RotationAxis(axis, (amount / 180.0f) * (float)Math.PI) * mResult.ModelMatrix;
            mResult.ModelMatrix = invWorld;
            mResult.Renderer.UpdateInstance(mResult.InstanceID, mResult.ModelMatrix);
            if (ModelChanged != null)
                ModelChanged(mResult.ModelMatrix);
        }

        public void moveModel(SlimDX.Vector3 axis, float amount)
        {
            var newMatrix = mResult.ModelMatrix * SlimDX.Matrix.Translation(axis * amount);
            mResult.ModelMatrix = newMatrix;
            mResult.Renderer.UpdateInstance(mResult.InstanceID, mResult.ModelMatrix);
            if (ModelChanged != null)
                ModelChanged(mResult.ModelMatrix);
        }

        public void moveModel(SlimDX.Vector3 newPos)
        {
            var newMatrix = mResult.ModelMatrix;
            newMatrix.M41 = newPos.X;
            newMatrix.M42 = newPos.Y;
            newMatrix.M43 = newPos.Z;
            mResult.ModelMatrix = newMatrix;
            mResult.Renderer.UpdateInstance(mResult.InstanceID, mResult.ModelMatrix);
            if (ModelChanged != null)
                ModelChanged(mResult.ModelMatrix);
        }

        WMOHitInformation mResult;

        public event Action<SlimDX.Matrix> ModelChanged;
    }
}
