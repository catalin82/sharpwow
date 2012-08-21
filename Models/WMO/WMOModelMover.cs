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
            Vector3 oldPos = new Vector3(mResult.ModelMatrix.M41, mResult.ModelMatrix.M42, mResult.ModelMatrix.M43);
            var diff = newPos - oldPos;
            var newMatrix = Matrix.Translation(diff) * mResult.ModelMatrix;
            mResult.ModelMatrix = newMatrix;
            mResult.Renderer.UpdateInstance(mResult.InstanceID, mResult.ModelMatrix);
            if (ModelChanged != null)
                ModelChanged(mResult.ModelMatrix);
        }

        WMOHitInformation mResult;

        public event Action<SlimDX.Matrix> ModelChanged;
    }
}
