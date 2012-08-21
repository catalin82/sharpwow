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
            List<string> wmoModelList = new List<string>();
            TreeNode mdxNode = null, wmoNode = null;
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

                        Func<string, bool> groupLambda = (file) => {
                            if(file.Length <= 4)
                                return false;

                            if(file[file.Length - 4] != '_')
                                return false;

                            var last = new string(file.Skip(file.Length - 3).ToArray());
                            uint groupNum = 0;
                            if (uint.TryParse(last, out groupNum))
                                return true;

                            return false;
                        };

                        var wmoModels = from file in files
                                        where file.ToLower().EndsWith(".wmo") && groupLambda(System.IO.Path.GetFileNameWithoutExtension(file)) == false
                                        select file;

                        wmoModelList.AddRange(wmoModels);
                    }

                    mTextureFileList.AddRange(fileList);
                    mMdxModelList.AddRange(mdxFileList);
                    mdxNode = loadTreeViewItems(mdxFileList);
                    mWmoModelList.AddRange(wmoModelList);
                    wmoNode = loadTreeViewItems(wmoModelList, "WMO");
                };

            Resize += UIPanel_Resize;

            HandleCreated += (sender, e) =>
                {
                    listBox1.Items.AddRange(fileList.ToArray());
                    treeView1.Nodes.Add(mdxNode);
                    treeView1.Nodes.Add(wmoNode);
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

            Game.GameManager.GlobalDataLoaded += () => Game.GameManager.SelectionManager.ModelSelected += SelectionManager_ModelSelected;
        }

        void UIPanel_Resize(object sender, EventArgs e)
        {
            groupBox8.Location = new Point(groupBox8.Location.X, Height - 270);
            treeView1.Height = Height - 350;
        }

        void SelectionManager_ModelSelected(Models.ModelSelectionInfo obj)
        {
            if(obj == null)
            {
                groupBox8.Visible = false;
                groupBox8.Tag = null;
                return;
            }

            groupBox8.Tag = obj;
            groupBox8.Visible = true;
            textBox3.Text = obj.ModelName;
        }

        private TreeNode loadTreeViewItems(List<string> items, string nodeRoot = "Models")
        {
            TreeNode rootNode = new TreeNode(nodeRoot);
            foreach (var item in items)
            {
                TreeNode startNode = rootNode;
                var parts = item.Split('\\');
                foreach (var part in parts)
                {
                    startNode = addNode(startNode, part);
                }
            }

            return rootNode;
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
        private List<string> mWmoModelList = new List<string>();

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
        public string SelectedMdxModel
        { 
            get 
            {
                if (treeView1.SelectedNode == null)
                    return "(none)";

                int io = treeView1.SelectedNode.FullPath.IndexOf('\\');
                return treeView1.SelectedNode.FullPath.Substring(io + 1);
            } 
        }

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

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length < 2)
            {
                label5.Text = "Search (min. 2 characters):";
                return;
            }

            label5.Text = "Search:";

            var node = loadTreeViewItems((from file in mMdxModelList where file.ToLower().Contains(textBox2.Text.ToLower()) select file).ToList());
            var node2 = loadTreeViewItems((from file in mWmoModelList where file.ToLower().Contains(textBox2.Text.ToLower()) select file).ToList(), "WMO");
            treeView1.SuspendLayout();
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(node);
            treeView1.Nodes.Add(node2);
            treeView1.ResumeLayout();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (groupBox8.Tag == null)
                return;

            ((Models.ModelSelectionInfo)groupBox8.Tag).AlignToGround();
        }
    }
}
