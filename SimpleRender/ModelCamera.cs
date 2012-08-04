using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using SlimDX;

namespace SharpWoW.SimpleRender
{
    public class ModelCamera
    {
        public ModelCamera(Control focusControl, ControlRender dev)
        {
            mControl = focusControl;
            focusControl.MouseClick += (obj, e) => focusControl.Focus();
            mDevice = dev.Device;
            mInput = dev.InputManager;
            mDevice.SetTransform(TransformState.View, Matrix.LookAtLH(mPosition, Vector3.Zero, Vector3.UnitZ));
        }

        public void UpdateCamera(TimeSpan diff)
        {
            if (mControl.Focused == false)
                return;

            var state = mInput.Capture();
            bool changed = false;

            if (state.Contains(Keys.W))
            {
                mPosition -= (float)diff.TotalSeconds * new Vector3(1, 0, 0) * 10;
                if (mPosition.X < 1)
                    mPosition.X = 1;

                changed = true;
            }

            if (state.Contains(Keys.S))
            {
                mPosition += (float)diff.TotalSeconds * new Vector3(1, 0, 0) * 10;
                changed = true;
            }

            if (state.Contains(Keys.Q))
            {
                mPosition.Z += (float)diff.TotalSeconds * 10;
                mTarget.Z = mPosition.Z;
                changed = true;
            }

            if (state.Contains(Keys.E))
            {
                mPosition.Z -= (float)diff.TotalSeconds * 10;
                mTarget.Z = mPosition.Z;
                changed = true;
            }

            if (changed == true)
            {
                mDevice.SetTransform(TransformState.View, Matrix.LookAtLH(mPosition, mTarget, Vector3.UnitZ));
            }
        }

        public void SetPosition(float distance, float height = 0.0f)
        {
            mPosition.X = distance;
            if (mPosition.X < 1)
                mPosition.X = 1;

            mPosition.Z = mTarget.Z = height;
            mDevice.SetTransform(TransformState.View, Matrix.LookAtLH(mPosition, mTarget, Vector3.UnitZ));
        }

        private Control mControl;
        private Device mDevice;

        private Vector3 mPosition = new Vector3(1, 0, 0);
        private Vector3 mTarget = Vector3.Zero;
        private Video.Input.InputManager mInput;
    }
}
