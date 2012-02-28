using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using Microsoft.Win32;

namespace SharpWoW.UI.Dialogs
{
    public partial class DeviceDialog : Form
    {
        public DeviceDialog()
        {
            InitializeComponent();
        }

        private void DeviceDialog_Load(object sender, EventArgs e)
        {
            var dx = Game.GameManager.GraphicsThread.GraphicsManager.Direct3D;
            SelectedAdapter = dx.Adapters[0];

            foreach (var info in dx.Adapters)
            {
                mAdapterBinding.Add(new { Tag = info, Text = info.Details.Description });
            }

            comboBox2.DataSource = mAdapterBinding;
            comboBox2.DisplayMember = "Text";

            AdapterChanged();
            comboBox1.DataSource = mBindings;
            comboBox1.DisplayMember = "Text";
            comboBox1.SelectedIndex = 0;
        }

        BindingList<object> mBindings = new BindingList<object>();
        BindingList<object> mAdapterBinding = new BindingList<object>();
        BindingList<object> mStencilBinding = new BindingList<object>();
        BindingList<object> mAABinding = new BindingList<object>();

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
                return;

            int maxQuality = Utils.Reflection.GetProperty<int>(comboBox1.SelectedItem, "Tag");
            if (maxQuality > 0)
                maxQuality -= 1;

            numericUpDown1.Maximum = maxQuality;
            numericUpDown1.Value = maxQuality;
        }

        private void AdapterChanged()
        {
            mBindings.Clear();
            var dx = Game.GameManager.GraphicsThread.GraphicsManager.Direct3D;
            var msvalues = Enum.GetValues(typeof(MultisampleType));
            foreach (var val in msvalues)
            {
                int quality = 0;
                if (dx.CheckDeviceMultisampleType(0, DeviceType.Hardware, Format.A8R8G8B8, true, (MultisampleType)val, out quality))
                {
                    var tagObject = new { Tag = quality, Text = val.ToString() };

                    mBindings.Add(tagObject);
                }
            }

            LoadDepthStencil();
            LoadAntiAlias();
        }

        public AdapterInformation SelectedAdapter { get; private set; }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
                return;

            var newAdapter = Utils.Reflection.GetProperty<AdapterInformation>(comboBox2.SelectedItem, "Tag");
            if (SelectedAdapter.Adapter == newAdapter.Adapter)
                return;

            SelectedAdapter = newAdapter;
            AdapterChanged();
        }

        private void LoadDepthStencil()
        {
            var dx = Game.GameManager.GraphicsThread.GraphicsManager.Direct3D;

            mStencilBinding.Clear();
            var values = Enum.GetValues(typeof(Format));
            foreach (var val in values)
            {
                if (dx.CheckDepthStencilMatch(SelectedAdapter.Adapter, DeviceType.Hardware, Format.A8R8G8B8, Format.A8R8G8B8, (Format)val))
                {
                    var obj = new { Tag = (Format)val, Text = val.ToString() };
                    mStencilBinding.Add(obj);
                }
            }

            comboBox3.DataSource = mStencilBinding;
            comboBox3.DisplayMember = "Text";
            comboBox3.SelectedIndex = 0;
        }

        private void LoadAntiAlias()
        {
            var dx = Game.GameManager.GraphicsThread.GraphicsManager.Direct3D;
            mAABinding.Clear();

            var caps = dx.GetDeviceCaps(SelectedAdapter.Adapter, DeviceType.Hardware);

            var minLinear = (caps.TextureFilterCaps & FilterCaps.MinLinear) != 0;
            var magLinear = (caps.TextureFilterCaps & FilterCaps.MagLinear) != 0;
            var mipLinear = (caps.TextureFilterCaps & FilterCaps.MipLinear) != 0;
            var minAni = (caps.TextureFilterCaps & FilterCaps.MinAnisotropic) != 0;
            var magAni = (caps.TextureFilterCaps & FilterCaps.MagAnisotropic) != 0;

            if (minLinear && magLinear)
            {
                var obj = new { Tag = Video.TextureFilterMode.Bilinear, Text = "Bilinear", Quality = 0u };
                mAABinding.Add(obj);
                if (mipLinear)
                {
                    obj = new { Tag = Video.TextureFilterMode.Trilinear, Text = "Trilinear", Quality = 0u };
                    mAABinding.Add(obj);
                }
            }

            if (minAni && magAni)
            {
                for (uint i = 1; i <= caps.MaxAnisotropy; i <<= 1)
                {
                    var str = (i.ToString() + "x Anisotropic");
                    var obj = new { Tag = Video.TextureFilterMode.Anisotropic, Text = str, Quality = i };
                    mAABinding.Add(obj);
                }
            }

            comboBox4.DataSource = mAABinding;
            comboBox4.DisplayMember = "Text";
            comboBox4.SelectedIndex = comboBox4.Items.Count - 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Resources.Strings.StencilExplanation, Resources.Strings.Help);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked != false)
            {
                var key = Registry.CurrentUser.OpenSubKey("Software\\Yias\\SharpWoW\\Video");
                if (key == null)
                    key = Registry.CurrentUser.CreateSubKey("Software\\Yias\\SharpWoW\\Video");

                key.SetValue("MultisampleQuality", numericUpDown1.Value.ToString());
                key.SetValue("Multisampling", comboBox1.Text);
                key.SetValue("TextureFilter", Utils.Reflection.GetProperty<Video.TextureFilterMode>(comboBox4.SelectedItem, "Tag").ToString());
                key.SetValue("Anisotropy", Utils.Reflection.GetProperty<uint>(comboBox4.SelectedItem, "Quality").ToString());
                key.SetValue("AdapterOrdinal", SelectedAdapter.Adapter.ToString());
                key.SetValue("DepthStencil", comboBox3.Text);
            }

            Close();
        }

        private void DeviceDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            Multisampling = (MultisampleType)Enum.Parse(typeof(MultisampleType), comboBox1.Text);
            MultisampleQuality = (int)numericUpDown1.Value;
            FilterMode = Utils.Reflection.GetProperty<Video.TextureFilterMode>(comboBox4.SelectedItem, "Tag");
            Anisotropy = Utils.Reflection.GetProperty<uint>(comboBox4.SelectedItem, "Quality");
            DepthStencil = Utils.Reflection.GetProperty<Format>(comboBox3.SelectedItem, "Tag");
        }

        public MultisampleType Multisampling { get; set; }
        public int MultisampleQuality { get; set; }
        public Video.TextureFilterMode FilterMode { get; set; }
        public uint Anisotropy { get; set; }
        public Format DepthStencil { get; set; }
    }
}
