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
            Video.Input.InputManager.Input.MousePress += Input_MousePress;
            TerrainBrush = new Brushes.TerrainBrush();
            Radius = 30.0f;
            InnerRadius = Radius * 0.66f;
            InnerChangeMode = Logic.ChangeMode.Flat;
            TerrainBrush.InnerSharpness = 0.75f;
            TerrainBrush.OuterSharpness = 0.8f;

            Game.GameManager.ActiveChangeModeChanged += () =>
                {
                    if (Game.GameManager.ActiveChangeType == ActiveChangeType.Height)
                    {
                        switch (mChangeMode)
                        {
                            case Logic.ChangeMode.Flat:
                            case Logic.ChangeMode.Linear:
                            case Logic.ChangeMode.Quadratic:
                            case Logic.ChangeMode.Smooth:
                            case Logic.ChangeMode.Spline:
                                Video.ShaderCollection.TerrainShader.SetValue("brushType", Game.GameManager.GameWindow.PropertyPanel.BrushType);
                                break;

                            case Logic.ChangeMode.Special:
                                Video.ShaderCollection.TerrainShader.SetValue("brushType", 3);
                                break;
                        }
                    }
                };
        }

        void Input_MousePress(int x, int y, System.Windows.Forms.MouseButtons pressedButton)
        {
            var mpos = Game.GameManager.GraphicsThread.GraphicsManager.MousePosition;
            if (Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.CapsLock))
                ADT.ADTManager.AddModel(Game.GameManager.GameWindow.ToolsPanel.SelectedMdxModel, mpos);
            
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
                    InnerRadius += 0.5f;
                    break;
                case 't':
                    InnerRadius -= 0.5f;
                    break;
                case 'R':
                    Radius += 0.5f;
                    break;
                case 'T':
                    Radius -= 0.5f;
                    break;

                case 'y':
                case 'Y':
                    {
                        if (Game.GameManager.ActiveChangeType == ActiveChangeType.Height)
                            Game.GameManager.ActiveChangeType = ActiveChangeType.Texturing;
                        else
                            Game.GameManager.ActiveChangeType = ActiveChangeType.Height;
                    }
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
                if (Game.GameManager.ActiveChangeType == ActiveChangeType.Height)
                    ChangeTerrain(mpos, lower);
                else
                    TextureTerrain(mpos, lower);
                
            }
        }

        void TextureTerrain(SlimDX.Vector3 mpos, bool lower)
        {
            if (Game.GameManager.GameWindow.ToolsPanel.SelectedTexture == "(none)")
                return;

            Game.Logic.TextureChangeParam tcp = new TextureChangeParam()
            {
                TextureName = SelectedTexture,
                Strength = Game.GameManager.GameWindow.ToolsPanel.TextureStrength,
                FalloffTreshold = Game.GameManager.GameWindow.ToolsPanel.TextureFalloff,
                Falloff = Game.GameManager.GameWindow.ToolsPanel.TextureFallofMode,
                AlphaCap = Game.GameManager.GameWindow.ToolsPanel.TextureAlphaCap,
                ActionSource = new SlimDX.Vector2(mpos.X, mpos.Y),
                InnerRadius = InnerRadius,
                OuterRadius = Radius
            };

            tcp.ConvertValuesToUShort();

            ADT.ADTManager.TextureTerrain(tcp);
        }

        void ChangeTerrain(SlimDX.Vector3 mpos, bool lower)
        {
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

                if (mInnerRadius >= mRadius)
                    InnerRadius = mRadius;

                Video.ShaderCollection.TerrainShader.SetValue("CircleRadius", value);
                TerrainBrush.OuterRadius = mRadius;
                TerrainBrush.InnerRadius = mRadius * 0.75f;
            } 
        }

        public float InnerRadius
        {
            get { return mInnerRadius; }
            set
            {
                if (mInnerRadius == value)
                    return;

                mInnerRadius = value;

                if (mInnerRadius >= mRadius)
                    mInnerRadius = mRadius;

                Video.ShaderCollection.TerrainShader.SetValue("InnerRadius", value);
            }
        }

        /// <summary>
        /// Gets or sets the Intensity with which interactions with the terrain take effect. 
        /// The meaning of this value may change with the different types of change.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Gets or sets the way the changes are interpolated inside the outer radius of interaction.
        /// </summary>
        public ChangeMode ChangeMode
        {
            get { return mChangeMode; }
            set
            {
                mChangeMode = value;
                switch (value)
                {
                    case Logic.ChangeMode.Flat:
                    case Logic.ChangeMode.Linear:
                    case Logic.ChangeMode.Quadratic:
                    case Logic.ChangeMode.Smooth:
                    case Logic.ChangeMode.Spline:
                        Video.ShaderCollection.TerrainShader.SetValue("brushType", Game.GameManager.GameWindow.PropertyPanel.BrushType);
                        break;

                    case Logic.ChangeMode.Special:
                        Video.ShaderCollection.TerrainShader.SetValue("brushType", 3);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the way the changes are interpolated inside the inner radius of interaction.
        /// </summary>
        public ChangeMode InnerChangeMode { get; set; }

        /// <summary>
        /// Gets of sets the type of interaction performed on the terrain
        /// </summary>
        public ChangeType ChangeType { get; set; }

        /// <summary>
        /// Gets the currently used spline for interpolation (only valid if the <see cref="ChangeMode">ChangeMode</see> is Spline).
        /// </summary>
        public MathNet.Numerics.Interpolation.IInterpolationMethod TerrainSpline { get { return Game.GameManager.GameWindow.TerrainSpline; } }

        public string SelectedTexture { get { return Game.GameManager.GameWindow.SelectedTexture; } }

        public Brushes.TerrainBrush TerrainBrush { get; private set; }

        private float mRadius = 6.0f;
        private float mInnerRadius = 6.0f * 0.66f;
        private ChangeMode mChangeMode;
    }

    public enum ChangeMode
    {
        Flat,
        Linear,
        Smooth,
        Quadratic,
        Spline,
        Special
    }

    public enum ChangeType
    {
        Raise,
        Flatten,
        Blur
    }
}
