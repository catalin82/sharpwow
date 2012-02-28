using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.Game.Logic
{
    public class TerrainLogic
    {
        public TerrainLogic()
        {
            Game.GameManager.GraphicsThread.OnFrame += new VideoThread.FrameRenderDelegate(UpdateLogic);
            ChangeMode = Logic.ChangeMode.Flat;
            Intensity = 30.0f;
            Video.Input.InputManager.Input.KeyPressed += new Video.Input.InputManager.KeyPressDlg(keyPressed);
            TerrainBrush = new Brushes.TerrainBrush();
            Radius = 30.0f;
            TerrainBrush.InnerSharpness = 0.75f;
            TerrainBrush.OuterSharpness = 0.8f;
        }

        void keyPressed(char chr)
        {
            switch (chr)
            {
                case 'V':
                case 'v':
                    Intensity -= 0.3f;
                    break;
                case 'C':
                case 'c':
                    Intensity += 0.3f;
                    break;
                case 'r':
                    Radius += 0.5f;
                    break;
                case 't':
                    Radius -= 0.5f;
                    break;
            }

            if (Intensity < 0)
                Intensity = 0;
            if (Radius < Utils.Metrics.Unitsize)
                Radius = Utils.Metrics.Unitsize;

        }

        void UpdateLogic(SlimDX.Direct3D9.Device device, TimeSpan deltaTime)
        {
            bool shift = Video.Input.InputManager.Input[System.Windows.Forms.Keys.ShiftKey];
            bool ctrl = Video.Input.InputManager.Input[System.Windows.Forms.Keys.ControlKey];
            if ((shift || ctrl) && Video.Input.InputManager.Input.Mouse.State.IsPressed(0))
            {
                var mpos = Game.GameManager.GraphicsThread.GraphicsManager.MousePosition;
                if (mpos.X > 900000)
                    return;

                bool lower = ctrl;
                switch (ChangeType)
                {
                    case Logic.ChangeType.Raise:
                        ADT.ADTManager.ChangeTerrain(mpos, lower);
                        break;

                    case Logic.ChangeType.Flatten:
                        ADT.ADTManager.FlattenTerrain(mpos, lower);
                        break;

                    case Logic.ChangeType.Blur:
                        ADT.ADTManager.BlurTerrain(mpos, lower);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the radius in which interactions with the terrain have an effect.
        /// </summary>
        public float Radius
        { 
            get 
            { 
                return mRadius; 
            } 
            set 
            {
                if (mRadius == value)
                    return;

                mRadius = value;
                Video.ShaderCollection.TerrainShader.SetValue("CircleRadius", value);
                TerrainBrush.OuterRadius = mRadius;
                TerrainBrush.InnerRadius = mRadius * 0.75f;
            } 
        }

        /// <summary>
        /// Gets or sets the Intensity with which interactions with the terrain take effect. 
        /// The meaning of this value may change with the different types of change.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Gets or sets the way the changes are interpolated inside the radius of interaction.
        /// </summary>
        public ChangeMode ChangeMode { get; set; }

        /// <summary>
        /// Gets of sets the type of interaction performed on the terrain
        /// </summary>
        public ChangeType ChangeType { get; set; }

        /// <summary>
        /// Gets the currently used spline for interpolation (only valid if the <see cref="ChangeMode">ChangeMode</see> is Spline).
        /// </summary>
        public MathNet.Numerics.Interpolation.IInterpolationMethod TerrainSpline { get { return Game.GameManager.GameWindow.TerrainSpline; } }

        public Brushes.TerrainBrush TerrainBrush { get; private set; }

        private float mRadius = 6.0f;
    }

    public enum ChangeMode
    {
        Flat,
        Linear,
        Smooth,
        Quadratic,
        Spline
    }

    public enum ChangeType
    {
        Raise,
        Flatten,
        Blur
    }
}
