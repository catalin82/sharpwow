using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models
{
    public class SelectionManager
    {
        public SelectionManager()
        {
            HadModelMovement = false;
            Video.Input.InputManager.Input.MouseMoved += MouseMove;
        }

        void MouseMove(int x, int y, System.Windows.Forms.MouseButtons pressedButtons, SlimDX.Vector2 diff)
        {
            if (mModelMover == null)
                return;

            if (pressedButtons == System.Windows.Forms.MouseButtons.None)
                return;

            if (Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.Menu))
            {
                if ((pressedButtons & System.Windows.Forms.MouseButtons.Left) != 0)
                {
                    mModelMover.moveModel(SlimDX.Vector3.TransformNormal(SlimDX.Vector3.UnitX, mMdxResult.InstanceData.ModelMatrix), diff.X / 2.0f);
                    HadModelMovement = true;
                }
                if ((pressedButtons & System.Windows.Forms.MouseButtons.Middle) != 0)
                {
                    mModelMover.moveModel(SlimDX.Vector3.TransformNormal(SlimDX.Vector3.UnitY, mMdxResult.InstanceData.ModelMatrix), diff.X / 2.0f);
                    HadModelMovement = true;
                }
                if ((pressedButtons & System.Windows.Forms.MouseButtons.Right) != 0)
                {
                    mModelMover.moveModel(SlimDX.Vector3.TransformNormal(SlimDX.Vector3.UnitZ, mMdxResult.InstanceData.ModelMatrix), diff.X / 2.0f);
                    HadModelMovement = true;
                }
            }
            else if (Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                
            }
        }

        public bool IsModelMovement
        {
            get
            {
                bool isAlt = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.Menu);
                bool isMiddle = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.MButton);
                bool isLeft = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.LButton);
                bool isRight = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.RButton);

                return (isAlt && (isMiddle || isLeft || isRight));
            }
        }

        public bool HadModelMovement { get; set; }

        public void SelectMdxModel(MDX.MdxIntersectionResult result)
        {
            mMdxResult = result;
            mWmoResult = null;
            mSelectionBox.UpdateSelectionBox(result.Model.BoundingBox, result.InstanceData.ModelMatrix);
            mModelMover = new MDX.M2ModelMover(result);
            mModelMover.ModelChanged += mSelectionBox.UpdateMatrix;
            var modelOverlay = Game.GameManager.GraphicsThread.GetOverlay<UI.Overlays.ModelInfoOverlay>();
            if (modelOverlay != null)
                modelOverlay.UpdateModel(result);
            else
            {
                modelOverlay = new UI.Overlays.ModelInfoOverlay(result);
                Game.GameManager.GraphicsThread.PushOverlay(modelOverlay);
            }
        }

        public void SelectWMOModel(WMO.WMOHitInformation wmoHit)
        {
            mMdxResult = null;
            mWmoResult = wmoHit;
        }

        public void ClearSelection()
        {
            mMdxResult = null;
            mWmoResult = null;
            mSelectionBox.ClearSelectionBox();
            mModelMover = null;
            Game.GameManager.GraphicsThread.RemoveOverlay<UI.Overlays.ModelInfoOverlay>();
        }

        public bool IsMdxInstanceSelected(MDX.M2Info info, uint id)
        {
            if(mMdxResult == null)
                return false;

            return (mMdxResult.Model == info && mMdxResult.InstanceID == id);
        }

        public void renderSelection()
        {
            mSelectionBox.RenderBox();
        }

        MDX.MdxIntersectionResult mMdxResult = null;
        WMO.WMOHitInformation mWmoResult = null;
        SelectionBox mSelectionBox = new SelectionBox();
        IModelMover mModelMover = null;
    }
}
