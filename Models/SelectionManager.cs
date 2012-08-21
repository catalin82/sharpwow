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
            Video.Input.InputManager.Input.MouseDown += (x, y, button) => { HadModelMovement = IsModelMovement; };
        }

        void MouseMove(int x, int y, System.Windows.Forms.MouseButtons pressedButtons, SlimDX.Vector2 diff)
        {
            if (mModelMover == null)
                return;

            if (pressedButtons == System.Windows.Forms.MouseButtons.None)
                return;

            if (Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.Menu))
            {
                var matrix = getMatrix();
                if ((pressedButtons & System.Windows.Forms.MouseButtons.Left) != 0)
                {
                    mModelMover.moveModel(SlimDX.Vector3.TransformNormal(SlimDX.Vector3.UnitX, matrix), diff.X / 6.0f);
                    HadModelMovement = true;
                }
                if ((pressedButtons & System.Windows.Forms.MouseButtons.Middle) != 0)
                {
                    mModelMover.moveModel(SlimDX.Vector3.TransformNormal(SlimDX.Vector3.UnitY, matrix), diff.X / 6.0f);
                    HadModelMovement = true;
                }
                if ((pressedButtons & System.Windows.Forms.MouseButtons.Right) != 0)
                {
                    mModelMover.moveModel(SlimDX.Vector3.TransformNormal(SlimDX.Vector3.UnitZ, matrix), -diff.Y / 6.0f);
                    HadModelMovement = true;
                }
            }
            else if (Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                if ((pressedButtons & System.Windows.Forms.MouseButtons.Left) != 0)
                {
                    mModelMover.rotateModel(SlimDX.Vector3.UnitX, diff.X / 10.0f);
                    HadModelMovement = true;
                }

                if ((pressedButtons & System.Windows.Forms.MouseButtons.Middle) != 0)
                {
                    mModelMover.rotateModel(SlimDX.Vector3.UnitY, diff.X / 10.0f);
                    HadModelMovement = true;
                }

                if ((pressedButtons & System.Windows.Forms.MouseButtons.Right) != 0)
                {
                    mModelMover.rotateModel(SlimDX.Vector3.UnitZ, diff.X / 10.0f);
                    HadModelMovement = true;
                }
            }
        }

        private SlimDX.Matrix getMatrix()
        {
            if (mMdxResult != null)
                return mMdxResult.InstanceData.ModelMatrix;
            else if (mWmoResult != null)
                return mWmoResult.ModelMatrix;

            throw new Exception();
        }

        public bool IsModelMovement
        {
            get
            {
                bool isAlt = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.Menu);
                bool isMiddle = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.MButton);
                bool isLeft = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.LButton);
                bool isRight = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.RButton);
                bool isCtrl = Video.Input.InputManager.Input.IsAsyncKeyDown(System.Windows.Forms.Keys.ControlKey);

                return ((mMdxResult != null || mWmoResult != null) && (isAlt || isCtrl) && (isMiddle || isLeft || isRight));
            }
        }

        public bool HadModelMovement { get; set; }

        public void SelectMdxModel(MDX.MdxIntersectionResult result)
        {
            mMdxResult = result;
            mWmoResult = null;
            mSelectionBox.UpdateSelectionBox(result.Model.BoundingBox, result.InstanceData.ModelMatrix);
            if (mModelMover != null)
                mModelMover.ModelChanged -= mSelectionBox.UpdateMatrix;
            mModelMover = new MDX.M2ModelMover(result);
            mModelMover.ModelChanged += (matrix) =>
                {
                    mSelectionBox.UpdateMatrix(matrix);
                    if (mCurrentSelection != null)
                        mCurrentSelection.ModelPosition = new SlimDX.Vector3(matrix.M41, matrix.M42, matrix.M43);
                };
            var modelOverlay = Game.GameManager.GraphicsThread.GetOverlay<UI.Overlays.ModelInfoOverlay>();
            if (modelOverlay != null)
                modelOverlay.UpdateModel(result);
            else
            {
                modelOverlay = new UI.Overlays.ModelInfoOverlay(result);
                Game.GameManager.GraphicsThread.PushOverlay(modelOverlay);
            }

            ModelSelectionInfo info = new ModelSelectionInfo()
            {
                ModelName = result.Model.ModelPath,
                ModelMover = mModelMover,
                ModelPosition = new SlimDX.Vector3(result.InstanceData.ModelMatrix.M41, result.InstanceData.ModelMatrix.M42, result.InstanceData.ModelMatrix.M43)
            };

            mCurrentSelection = info;

            if (ModelSelected != null)
                ModelSelected(info);
        }

        public void SelectWMOModel(WMO.WMOHitInformation wmoHit)
        {
            mMdxResult = null;
            mWmoResult = wmoHit;
            mSelectionBox.UpdateSelectionBox(wmoHit.Model.BoundingBox, wmoHit.ModelMatrix);
            if (mModelMover != null)
                mModelMover.ModelChanged -= mSelectionBox.UpdateMatrix;

            mModelMover = new WMO.WMOModelMover(wmoHit);
            mModelMover.ModelChanged += (matrix) =>
            {
                mSelectionBox.UpdateMatrix(matrix);
                if (mCurrentSelection != null)
                    mCurrentSelection.ModelPosition = new SlimDX.Vector3(matrix.M41, matrix.M42, matrix.M43);
            };

            var modelOverlay = Game.GameManager.GraphicsThread.GetOverlay<UI.Overlays.ModelInfoOverlay>();
            if (modelOverlay != null)
                modelOverlay.UpdateModel(wmoHit);
            else
            {
                modelOverlay = new UI.Overlays.ModelInfoOverlay(wmoHit);
                Game.GameManager.GraphicsThread.PushOverlay(modelOverlay);
            }

            ModelSelectionInfo info = new ModelSelectionInfo()
            {
                ModelName = wmoHit.Model.FileName,
                ModelMover = mModelMover,
                ModelPosition = new SlimDX.Vector3(wmoHit.ModelMatrix.M41, wmoHit.ModelMatrix.M42, wmoHit.ModelMatrix.M43)
            };

            mCurrentSelection = info;

            if (ModelSelected != null)
                ModelSelected(info);
        }

        public void ClearSelection()
        {
            mMdxResult = null;
            mWmoResult = null;
            mSelectionBox.ClearSelectionBox();
            mModelMover = null;
            Game.GameManager.GraphicsThread.RemoveOverlay<UI.Overlays.ModelInfoOverlay>();
            mCurrentSelection = null;
            if (ModelSelected != null)
                ModelSelected(null);
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

        public event Action<ModelSelectionInfo> ModelSelected;

        MDX.MdxIntersectionResult mMdxResult = null;
        WMO.WMOHitInformation mWmoResult = null;
        SelectionBox mSelectionBox = new SelectionBox();
        IModelMover mModelMover = null;
        ModelSelectionInfo mCurrentSelection;
    }
}
