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
            List<string> fileList = new List<string>();
            List<string> mdxFileList = new List<string>();
            Stormlib.MPQArchiveLoader.Instance.Initialized += () =>
                {
                    foreach (var archive in Stormlib.MPQArchiveLoader.Instance.ArchiveList)
                    {
                        var files = archive.Value.GetFiles();
                        var tilesets = from file in files
                                       where file.ToLower().StartsWith("tileset") && file.ToLower().EndsWith(".blp")
                                       select file;

                        fileList.AddRange(tilesets);

                        var mdxModels = from file in files
                                        where file.ToLower().EndsWith(".mdx") || file.ToLower().EndsWith(".m2")
                                        select file;

                        mdxFileList.AddRange(mdxModels);
                    }

                    mTextureFileList.AddRange(fileList);
                    mMdxModelList.AddRange(mdxFileList);
                };

            HandleCreated += (sender, e) =>
                {
                    listBox1.Items.AddRange(fileList.ToArray());
                    loadTreeViewItems(mdxFileList);
                };

            Game.GameManager.ActiveChangeModeChanged += () =>
                {
                    switch (Game.GameManager.ActiveChangeType)
                    {
                        case Game.Logic.ActiveChangeType.Height:
                            tabControl1.SelectedIndex = 0;
                            break;

                        case Game.Logic.ActiveChangeType.Texturing:
                            tabControl1.SelectedIndex = 1;
                            break;
                    }
                };
        }

        private void loadTreeViewItems(List<string> items)
        {
            TreeNode rootNode = new TreeNode("Models");
            foreach (var item in items)
            {
                TreeNode startNode = rootNode;
                var parts = item.Split('\\');
                foreach (var part in parts)
                {
                    startNode = addNode(startNode, part);
                }
            }

            treeView1.Nodes.Add(rootNode);
        }

        private TreeNode addNode(TreeNode parentNode, string part)
        {
            if (parentNode.Nodes.ContainsKey(part.ToLower()))
                return parentNode.Nodes[part];

            return parentNode.Nodes.Add(part.ToLower(), part);
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

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            groupBox4.Visible = radioButton12.Checked;
            Video.ShaderCollection.TerrainShader.SetValue("brushType", 3);
            radioButton1_CheckedChanged(sender, e);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackBar2.Maximum = trackBar1.Value;
            Game.GameManager.TerrainLogic.Radius = trackBar1.Value;
        }

        private void radioButton18_CheckedChanged(object sender, EventArgs e)
        {
            Game.GameManager.TerrainLogic.ChangeMode = (Game.Logic.ChangeMode)Enum.Parse(typeof(Game.Logic.ChangeMode), (sender as RadioButton).Tag as string);
        }

        private void radioButton15_CheckedChanged(object sender, EventArgs e)
        {
            Game.GameManager.TerrainLogic.InnerChangeMode = (Game.Logic.ChangeMode)Enum.Parse(typeof(Game.Logic.ChangeMode), (sender as RadioButton).Tag as string);
        }

        private List<string> mTextureFileList = new List<string>();
        private List<string> mMdxModelList = new List<string>();

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            listBox1.SuspendLayout();
            listBox1.Items.Clear();
            var files = (from file in mTextureFileList where file.ToLower().Contains(textBox1.Text.ToLower()) select file).ToArray();
            listBox1.Items.AddRange(files);
            listBox1.ResumeLayout();
        }

        public Controls.PropertyTab PropertyPanel { get { return propertyTab1; } }
        public string SelectedTexture { get { return (listBox1.SelectedItem != null) ? (string)listBox1.SelectedItem : "(none)"; } }
        public float TextureStrength { get { return trackBar3.Value / 10.0f; } }
        public float TextureFalloff { get { return trackBar4.Value / 10.0f; } }
        public int TextureAlphaCap { get { return trackBar5.Value; } }
        public Game.Logic.TextureChangeParam.FalloffMode TextureFallofMode
        {
            get
            {
                if (radioButton19.Checked == true)
                    return Game.Logic.TextureChangeParam.FalloffMode.Linear;
                else if (radioButton20.Checked == true)
                    return Game.Logic.TextureChangeParam.FalloffMode.Cosinus;
                else if (radioButton21.Checked == true)
                    return Game.Logic.TextureChangeParam.FalloffMode.Flat;

                return Game.Logic.TextureChangeParam.FalloffMode.Flat;
            }
        }
        public string SelectedMdxModel { get { return (treeView1.SelectedNode != null) ? (treeView1.SelectedNode.FullPath.Remove(0, 7)) : "(none)"; } }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            trackBar4.Maximum = trackBar3.Value;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    if (Game.GameManager.ActiveChangeType != Game.Logic.ActiveChangeType.Height)
                        Game.GameManager.ActiveChangeType = Game.Logic.ActiveChangeType.Height;
                    break;

                case 1:
                    if (Game.GameManager.ActiveChangeType != Game.Logic.ActiveChangeType.Texturing)
                        Game.GameManager.ActiveChangeType = Game.Logic.ActiveChangeType.Texturing;
                    break;
            }
        }

        private void radioButton21_CheckedChanged(object sender, EventArgs e)
        {
            trackBar4.Enabled = !radioButton21.Checked;
        }
    }
}
