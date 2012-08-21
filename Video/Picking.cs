using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

namespace SharpWoW.Video
{
    static class Picking
    {
        public static bool HitModelTransformed(Ray ray, Mesh mesh, Matrix worldTrans, out float distance)
        {
            var newRay = TransformRay(ray, worldTrans);

            return mesh.Intersects(newRay, out distance);
        }

        public static Ray TransformRay(Ray ray, Matrix mat)
        {
            ray.Position = Vector3.TransformCoordinate(ray.Position, mat);
            return ray;
        }

        public static Ray CalcRayForTransform(Matrix mat)
        {
            var Device = Game.GameManager.GraphicsThread.GraphicsManager.Device;
            System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
            pt = Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.PointToClient(pt);

            Vector3 screenCoord = new Vector3();
            screenCoord.X = (((2.0f * pt.X) / Device.Viewport.Width) - 1);
            screenCoord.Y = -(((2.0f * pt.Y) / Device.Viewport.Height) - 1);

            var invProj = Matrix.Invert(Device.GetTransform(TransformState.Projection));
            var invView = Matrix.Invert(Device.GetTransform(TransformState.View));
            var invWorld = Matrix.Invert(mat);

            var nearPos = new Vector3(screenCoord.X, screenCoord.Y, 0);
            var farPos = new Vector3(screenCoord.X, screenCoord.Y, 1);

            nearPos = Vector3.TransformCoordinate(nearPos, invProj * invView * invWorld);
            farPos = Vector3.TransformCoordinate(farPos, invProj * invView * invWorld);

            return new Ray(nearPos, Vector3.Normalize((farPos - nearPos)));
        }

        public static void InitPicking()
        {
            Video.Input.InputManager.Input.MousePress += new Input.InputManager.MousePressDlg(_MouseClick);
        }

        static void _MouseClick(int x, int y, System.Windows.Forms.MouseButtons pressedButton)
        {
            if (Game.GameManager.SelectionManager.HadModelMovement)
            {
                Game.GameManager.SelectionManager.HadModelMovement = false;
                return;
            }

            if (pressedButton == System.Windows.Forms.MouseButtons.Left)
            {
                bool shift = Video.Input.InputManager.Input[System.Windows.Forms.Keys.ShiftKey];
                bool ctrl = Video.Input.InputManager.Input[System.Windows.Forms.Keys.ControlKey];

                if (shift || ctrl)
                    return;

                Models.WMO.WMOHitInformation hit = null;
                Vector3 hitPos;
                var ray = CalcRayForTransform(Matrix.Identity);
                Models.MDX.MdxIntersectionResult result;
                Game.GameManager.M2ModelManager.HitModels(ray, out result);

                var terrainPos = Game.GameManager.GraphicsThread.GraphicsManager.MousePosition;
                var camPos = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position;
                var terrainDist = (terrainPos - camPos).Length();
                var mdxDistance = (result.HitPoint - camPos).Length();

                if (Models.WMO.WMOManager.IsWmoHit(out hit, out hitPos))
                {
                    var wmoDist = (hitPos - camPos).Length();

                    if (wmoDist < terrainDist)
                    {
                        if (mdxDistance >= 0 && mdxDistance < wmoDist)
                        {
                            Game.GameManager.SelectionManager.SelectMdxModel(result);
                            return;
                        }
                        Game.GameManager.GameWindow.WMOEditor.SetWMO(hit.Name);
                        Game.GameManager.SelectionManager.SelectWMOModel(hit);
                        return;
                    }
                    else if (mdxDistance >= 0 && mdxDistance < terrainDist)
                    {
                        Game.GameManager.SelectionManager.SelectMdxModel(result);
                        return;
                    }
                }
                else if (mdxDistance >= 0 && mdxDistance < terrainDist)
                {
                    Game.GameManager.SelectionManager.SelectMdxModel(result);
                    return;
                }

                Game.GameManager.SelectionManager.ClearSelection();
            }
        }
    }
}
