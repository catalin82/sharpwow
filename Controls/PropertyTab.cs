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
            trackBar1.Value = (int)Game.GameManager.TerrainLogic.Radius;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Video.ShaderCollection.TerrainShader.SetValue("brushType", 2);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Video.ShaderCollection.TerrainShader.SetValue("brushType", 0);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Video.ShaderCollection.TerrainShader.SetValue("brushType", 1);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Game.GameManager.TerrainLogic.Radius = trackBar1.Value;
        }
    }
}
