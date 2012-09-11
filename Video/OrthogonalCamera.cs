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
    public class OrthogonalCamera : ICamera
    {
        public OrthogonalCamera()
        {
            float aspect =
                (float)Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Width /
                (float)Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height;
            ViewFrustum = new Frustum();
            mMatProjection = Matrix.OrthoOffCenterLH(
                -(Utils.Metrics.Tilesize * aspect / 2.0f),
                Utils.Metrics.Tilesize * aspect / 2.0f,
                -(Utils.Metrics.Tilesize / 2.0f),
                Utils.Metrics.Tilesize / 2.0f,
                0.1f,
                10000.0f
            );

            Game.GameManager.GraphicsThread.GraphicsManager.Device.SetTransform(TransformState.Projection, mMatProjection);
            PreventWorldUpdate = false;
        }

        public OrthogonalCamera(float w, float h, bool offCenter = true)
        {
            float aspect =
                w /
                h;

            ViewFrustum = new Frustum();
            if (offCenter)
            {
                mMatProjection = Matrix.OrthoOffCenterLH(
                    -(Utils.Metrics.Tilesize * aspect / 2.0f),
                    Utils.Metrics.Tilesize * aspect / 2.0f,
                    -(Utils.Metrics.Tilesize / 2.0f),
                    Utils.Metrics.Tilesize / 2.0f,
                    0.1f,
                    10000.0f
                );
            }
            else
            {
                mMatProjection = Matrix.OrthoLH(Utils.Metrics.Tilesize * aspect, Utils.Metrics.Tilesize, 0.1f, 10000.0f);
            }

            Game.GameManager.GraphicsThread.GraphicsManager.Device.SetTransform(TransformState.Projection, mMatProjection);
            PreventWorldUpdate = false;
        }

        void MouseWheelTurned(int delta)
        {
            if (Game.GameManager.GraphicsThread.GraphicsManager.IsIn2D == false)
                return;

            int realTurn = delta / 127;
            float newZoom = mZoomFactor + realTurn * 0.01f;
            if (newZoom < 0.1f)
                newZoom = 0.1f;
            if (newZoom > 1.0f)
                newZoom = 1.0f;

            mZoomFactor = newZoom;

            float aspect =
                (float)Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Width /
                (float)Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height;

            mMatProjection = Matrix.OrthoOffCenterLH(
                -(Utils.Metrics.Tilesize * mZoomFactor * aspect / 2.0f),
                Utils.Metrics.Tilesize * mZoomFactor * aspect / 2.0f,
                -(Utils.Metrics.Tilesize * mZoomFactor / 2.0f),
                Utils.Metrics.Tilesize * mZoomFactor / 2.0f,
                0.1f,
                10000.0f
            );

            Game.GameManager.GraphicsThread.GraphicsManager.Device.SetTransform(TransformState.Projection, mMatProjection);
            var dev = Game.GameManager.GraphicsThread.GraphicsManager.Device;

            dev.SetTransform(TransformState.View, Matrix.LookAtLH(mPosition, mTarget, mUp));
            ShaderCollection.CameraChanged(this);
            Game.GameManager.WorldManager.Update(this);
            Game.GameManager.InformPropertyChanged(Game.GameProperties.CameraPosition);

            ViewFrustum.BuildViewFrustum(dev.GetTransform(TransformState.View), mMatProjection);
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
                mPosition += (float)diff.TotalSeconds * 50.0f * Vector3.UnitX;
                mTarget += (float)diff.TotalSeconds * 50.0f * Vector3.UnitX;
                changed = true;
            }
            if (inp[Keys.S])
            {
                mPosition -= (float)diff.TotalSeconds * 50.0f * Vector3.UnitX;
                mTarget -= (float)diff.TotalSeconds * 50.0f * Vector3.UnitX;
                changed = true;
            }
            if (inp[Keys.D])
            {
                var change = (float)diff.TotalSeconds * 50.0f * Vector3.UnitY;
                mPosition += change;
                mTarget += change;
                changed = true;
            }
            if (inp[Keys.A])
            {
                var change = (float)diff.TotalSeconds * 50.0f * Vector3.UnitY;
                mPosition -= change;
                mTarget -= change;
                changed = true;
            }
            if (inp[Keys.Q])
            {
                MouseWheelTurned(127);
            }
            if (inp[Keys.E])
            {
                MouseWheelTurned(-127);
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

        public void SetPosition(Vector3 position, bool nonUpdate = false)
        {
            mPosition = position;
            mTarget = mPosition + mFront;
            mDevice.SetTransform(TransformState.View, Matrix.LookAtLH(mPosition, mTarget, mUp));
            ShaderCollection.CameraChanged(this);
            if (nonUpdate == false)
            {
                Game.GameManager.WorldManager.Update(this);
                ViewFrustum.BuildViewFrustum(mDevice.GetTransform(TransformState.View), mDevice.GetTransform(TransformState.Projection));
                Game.GameManager.InformPropertyChanged(Game.GameProperties.CameraPosition);
            }
        }

        public void DeviceAttached(Device dev)
        {
            Game.GameManager.GraphicsThread.GraphicsManager.Device.SetTransform(TransformState.Projection, mMatProjection);
            mDevice.SetTransform(TransformState.View, Matrix.LookAtLH(mPosition, mTarget, mUp));
            if (PreventWorldUpdate == false)
            {
                ShaderCollection.CameraChanged(this);
                Game.GameManager.WorldManager.Update(this);
                ViewFrustum.BuildViewFrustum(mDevice.GetTransform(TransformState.View), mDevice.GetTransform(TransformState.Projection));
                Game.GameManager.InformPropertyChanged(Game.GameProperties.CameraPosition);
            }
        }

        private Vector3 mPosition = Vector3.Zero;
        private Vector3 mTarget = Vector3.Zero;
        private Vector3 mUp = Vector3.UnitX;
        private Vector3 mRight = Vector3.Negate(Vector3.UnitY);
        private Vector3 mFront = Vector3.Negate(Vector3.UnitZ);
        private Device mDevice { get { return Game.GameManager.GraphicsThread.GraphicsManager.Device; } }
        private Matrix mMatProjection;
        private float mZoomFactor = 1.0f;

        public bool PreventWorldUpdate { get; set; }
        public Vector3 Position { get { return mPosition; } }
        public Vector3 Target { get { return mTarget; } }
        public Vector3 Front { get { return mFront; } }
        public Vector3 Up { get { return mUp; } }
        public Matrix ViewProj { get { return (mDevice.GetTransform(TransformState.View) * mDevice.GetTransform(TransformState.Projection)); } }
        public Frustum ViewFrustum { get; private set; }
    }
}
