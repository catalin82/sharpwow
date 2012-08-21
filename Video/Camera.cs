using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;
using System.Windows.Forms;

namespace SharpWoW.Video
{
    public class Camera
    {
        public Camera()
        {
            ViewFrustum = new Frustum();
        }

        public void UpdateCamera(Device dev, TimeSpan diff)
        {
            if (Input.InputManager.Input.HasFocus == false)
                return;

            bool changed = false;
            var inp = Input.InputManager.Input;
            float sensitivity = Game.GameManager.GameWindow.PropertyPanel.CameraSensitivity;

            if (inp[Keys.W])
            {
                mPosition += (float)diff.TotalSeconds * mFront * 50;
                mTarget += (float)diff.TotalSeconds * mFront * 50;
                changed = true;
            }
            if (inp[Keys.S])
            {
                mPosition -= (float)diff.TotalSeconds * mFront * 50;
                mTarget -= (float)diff.TotalSeconds * mFront * 50;
                changed = true;
            }
            if (inp[Keys.D])
            {
                var change = (float)diff.TotalSeconds * mRight * 50;
                mPosition += change;
                mTarget += change;
                changed = true;
            }
            if (inp[Keys.A])
            {
                var change = (float)diff.TotalSeconds * mRight * 50;
                mPosition -= change;
                mTarget -= change;
                changed = true;
            }
            if (inp[Keys.Q])
            {
                var change = (float)diff.TotalSeconds * mUp * 50;
                mPosition += change;
                mTarget += change;
                changed = true;
            }
            if (inp[Keys.E])
            {
                var change = (float)diff.TotalSeconds * mUp * 50;
                mPosition -= change;
                mTarget -= change;
                changed = true;
            }

            var state = Input.InputManager.Input.Mouse.State;
            if (state.IsPressed((int)SlimDX.DirectInput.MouseObject.Button2) && Game.GameManager.SelectionManager.IsModelMovement == false)
            {
                if (state.X != 0 || state.Y != 0)
                {
                    if (state.X != 0)
                    {
                        int fac = (state.X < 0) ? -1 : 1;
                        Matrix rot = Matrix.RotationZ(state.X * 0.005f * sensitivity);
                        mFront = Vector3.TransformCoordinate(mFront, rot);
                        mFront.Normalize();
                        mTarget = mPosition + mFront;
                        mRight = Vector3.TransformCoordinate(mRight, rot);
                        mRight.Normalize();
                        mUp = Vector3.TransformCoordinate(mUp, rot);
                        mUp.Normalize();
                        changed = true;
                    }
                    if (state.Y != 0)
                    {
                        int fac = (state.Y < 0) ? -1 : 1;
                        Matrix rot = Matrix.RotationAxis(mRight, state.Y * 0.005f * sensitivity);
                        mFront = Vector3.TransformCoordinate(mFront, rot);
                        mFront.Normalize();
                        mTarget = mPosition + mFront;
                        mUp = Vector3.TransformCoordinate(mUp, rot);
                        mUp.Normalize();
                        changed = true;
                    }
                }
            }


            if (changed)
            {
                dev.SetTransform(TransformState.View, Matrix.LookAtLH(mPosition, mTarget, mUp));
                ShaderCollection.CameraChanged(this);
                Game.GameManager.WorldManager.Update(this);
                ViewFrustum.BuildViewFrustum(dev.GetTransform(TransformState.View), dev.GetTransform(TransformState.Projection));
                Game.GameManager.InformPropertyChanged(Game.GameProperties.CameraPosition);
            }
        }

        public void SetPosition(Vector3 position)
        {
            mPosition = position;
            mTarget = mPosition + mFront;
            mDevice.SetTransform(TransformState.View, Matrix.LookAtLH(mPosition, mTarget, mUp));
            ShaderCollection.CameraChanged(this);
            Game.GameManager.WorldManager.Update(this);
            ViewFrustum.BuildViewFrustum(mDevice.GetTransform(TransformState.View), mDevice.GetTransform(TransformState.Projection));
            Game.GameManager.InformPropertyChanged(Game.GameProperties.CameraPosition);
        }

        private Vector3 mPosition = Vector3.UnitX;
        private Vector3 mTarget = Vector3.Zero;
        private Vector3 mUp = Vector3.UnitZ;
        private Vector3 mRight = Vector3.Negate(Vector3.UnitY);
        private Vector3 mFront = Vector3.Negate(Vector3.UnitX);
        private Device mDevice { get { return Game.GameManager.GraphicsThread.GraphicsManager.Device; } }
        

        public Vector3 Position { get { return mPosition; } }
        public Vector3 Target { get { return mTarget; } }
        public Vector3 Front { get { return mFront; } }
        public Vector3 Up { get { return mUp; } }
        public Matrix ViewProj { get { return (mDevice.GetTransform(TransformState.View) * mDevice.GetTransform(TransformState.Projection)); } }
        public Frustum ViewFrustum { get; private set; }
    }
}
