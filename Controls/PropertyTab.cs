using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpWoW.Controls
{
    public partial class PropertyTab : UserControl
    {
        public PropertyTab()
        {
            InitializeComponent();
        }

        private void PropertyTab_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = new UI.TerrainPropertyPanel(propertyGrid1);
            if (this.Site != null && this.Site.DesignMode == true)
                return;

            trackBar1.Value = (int)Game.GameManager.TerrainLogic.Radius;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (Game.GameManager.ActiveChangeType == Game.Logic.ActiveChangeType.Texturing)
                return;

            Video.ShaderCollection.TerrainShader.SetValue("brushType", 2);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (Game.GameManager.ActiveChangeType == Game.Logic.ActiveChangeType.Texturing)
                return;

            Video.ShaderCollection.TerrainShader.SetValue("brushType", 0);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (Game.GameManager.ActiveChangeType == Game.Logic.ActiveChangeType.Texturing)
                return;

            Video.ShaderCollection.TerrainShader.SetValue("brushType", 1);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Game.GameManager.TerrainLogic.Radius = trackBar1.Value;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (Game.GameManager.ActiveChangeType == Game.Logic.ActiveChangeType.Texturing)
                return;

            Video.ShaderCollection.TerrainShader.SetValue("brushType", 3);
        }

        public int BrushType
        {
            get
            {
                if (radioButton1.Checked)
                    return 2;
                if (radioButton2.Checked)
                    return 0;
                if (radioButton3.Checked)
                    return 1;
                if (radioButton4.Checked)
                    return 3;

                return 0;
            }
        }

        public float CameraSensitivity
        {
            get { return trackBar2.Value / 100.0f; }
        }
    }
}
