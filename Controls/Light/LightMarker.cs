using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SharpWoW.Controls.Light
{
    public class LightMarker
    {
        public LightMarker(Control parent, uint time, Point pos, Color clr)
        {
            BasePosition = pos;
            Fixed = false;
            mMarker = new Panel();
            mMarker.BackColor = clr;
            mMarker.Size = new Size(6, 10);
            mMarker.BorderStyle = BorderStyle.FixedSingle;
            mMarker.BringToFront();

            mMarker.Location = new Point(pos.X - 3, pos.Y - 7);
            mMarker.MouseClick += new MouseEventHandler(MouseClicked);
            mMarker.MouseDoubleClick += new MouseEventHandler(MouseDoubleClick);
            mMarker.MouseDown += new MouseEventHandler(MarkerMouseDown);
            mMarker.MouseUp += new MouseEventHandler(MarkerMouseUp);

            mMarker.MouseMove += new MouseEventHandler(MarkerMouseMove);
            parent.Controls.Add(mMarker);
            Time = time;
            Color = clr;
        }

        void MarkerMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                mLeftDown = false;
        }

        void MarkerMouseMove(object sender, MouseEventArgs e)
        {
            if (Fixed)
                return;

            if (mLeftDown == false)
                return;

            var ptScreen = mMarker.PointToScreen(e.Location);
            var ptParent = mMarker.Parent.PointToClient(ptScreen);

            var diff = ptParent.X - BasePosition.X;
            int newTime = (int)Time + diff;
            if (newTime < MinTime || newTime > MaxTime)
                return;

            ChangeTime((uint)(Time + diff));
        }

        void MarkerMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                mLeftDown = true;
        }

        void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            UI.Dialogs.LightMarkerDialog lmd = new UI.Dialogs.LightMarkerDialog(this);
            lmd.ShowDialog();
        }

        void MouseClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (RemoveRequested != null)
                    RemoveRequested(this);
            }
        }

        public void RemoveMarker()
        {
            mMarker.Parent.Controls.Remove(mMarker);
            mMarker.Dispose();
        }

        public void ChangeColor(Color clr)
        {
            Color = clr;
            mMarker.BackColor = clr;
            if (ValueChanged != null)
                ValueChanged(this);
        }

        public void ChangeTime(uint newTime)
        {
            var oldTime = Time;
            var diff = ((int)newTime) - (int)oldTime;
            Time = newTime;
            if (TimeChanged != null)
                TimeChanged(this, oldTime, newTime);

            mMarker.Location = new Point(mMarker.Location.X + diff, mMarker.Location.Y);
            BasePosition = mMarker.Location;
            BasePosition.X += 3;
        }

        public uint Time { get; private set; }
        public bool Fixed { get; set; }
        public Color Color { get; private set; }
        public uint MinTime { get; set; }
        public uint MaxTime { get; set; }

        private bool mLeftDown = false;
        private Panel mMarker;
        private Point BasePosition;

        public event Action<LightMarker> RemoveRequested;
        public event Action<LightMarker> ValueChanged;
        public event Action<LightMarker, uint, uint> TimeChanged;
    }
}
