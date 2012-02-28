using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SharpWoW.Controls
{
    public partial class UIPanel : UserControl
    {
        public UIPanel()
        {
            InitializeComponent();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked == true)
                Game.GameManager.TerrainLogic.ChangeMode = (Game.Logic.ChangeMode)Enum.Parse(typeof(Game.Logic.ChangeMode), (sender as RadioButton).Tag as string);
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Visible = radioButton5.Checked;
            radioButton1_CheckedChanged(sender, e);
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton9.Checked == true)
            {
                radioButton4.Enabled = true;
                radioButton5.Enabled = true;
            }

            Game.GameManager.TerrainLogic.ChangeType = Game.Logic.ChangeType.Raise;
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton10.Checked == true)
            {
                if (radioButton4.Checked == true || radioButton5.Checked == true)
                    radioButton3.Checked = true;

                radioButton4.Enabled = false;
                radioButton5.Enabled = false;
            }

            Game.GameManager.TerrainLogic.ChangeType = Game.Logic.ChangeType.Flatten;
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton11.Checked == true)
            {
                if (radioButton4.Checked == true || radioButton5.Checked == true)
                    radioButton3.Checked = true;

                radioButton4.Enabled = false;
                radioButton5.Enabled = false;
            }

            Game.GameManager.TerrainLogic.ChangeType = Game.Logic.ChangeType.Blur;
        }

        public MathNet.Numerics.Interpolation.IInterpolationMethod TerrainSpline { get { return multiPointSplineControl1.Spline; } }
    }
}
