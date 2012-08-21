using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace SharpWoW.Models.MDX
{
    public class M2ModelMover : IModelMover
    {
        public M2ModelMover(MdxIntersectionResult result)
        {
            mResult = result;
        }

        public void rotateModel(SlimDX.Vector3 axis, float amount)
        {
            var newData = mResult.InstanceData;
            var invWorld = SlimDX.Matrix.RotationAxis(axis, (amount / 180.0f) * (float)Math.PI) * newData.ModelMatrix;
            newData.ModelMatrix = invWorld;
            mResult.InstanceData = newData;
            mResult.Renderer.InstanceLoader.setInstance(mResult.InstanceID, mResult.InstanceData);
            if (ModelChanged != null)
                ModelChanged(mResult.InstanceData.ModelMatrix);
        }

        public void moveModel(SlimDX.Vector3 axis, float amount)
        {
            var newData = mResult.InstanceData;
            var newMatrix = newData.ModelMatrix * SlimDX.Matrix.Translation(axis * amount);
            newData.ModelMatrix = newMatrix;
            mResult.InstanceData = newData;
            mResult.Renderer.InstanceLoader.setInstance(mResult.InstanceID, mResult.InstanceData);
            if (ModelChanged != null)
                ModelChanged(mResult.InstanceData.ModelMatrix);
        }

        public void moveModel(SlimDX.Vector3 newPos)
        {
            var newData = mResult.InstanceData;
            var newMatrix = mResult.InstanceData.ModelMatrix;
            newMatrix.M41 = newPos.X;
            newMatrix.M42 = newPos.Y;
            newMatrix.M43 = newPos.Z;
            newData.ModelMatrix = newMatrix;
            mResult.InstanceData = newData;
            mResult.Renderer.InstanceLoader.setInstance(mResult.InstanceID, mResult.InstanceData);
            if (ModelChanged != null)
                ModelChanged(mResult.InstanceData.ModelMatrix);
        }

        MdxIntersectionResult mResult;

        public event Action<SlimDX.Matrix> ModelChanged;
    }
}
