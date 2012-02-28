using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpWoW.Controls
{
    public partial class FolderView : UserControl
    {
        public FolderView()
        {
            InitializeComponent();
        }

        public string SelectedPath { get; set; }
        public string RootPath { get; set; }

        private void FolderView_Load(object sender, EventArgs e)
        {
            string topPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            treeView1.ImageList = imageList1;
            //treeView1.TopNode = new TreeNode("Desktop", GetIconForFolder(topPath), 0);
            TreeNode topNode = new TreeNode("Computer", GetIconForFolder(topPath), GetIconForFolder(topPath));
            treeView1.Nodes.Add(topNode);
            foreach (var dir in Directory.GetDirectories(topPath))
            {
                var ico = GetIconForFolder(dir);
                int pos = dir.LastIndexOf('\\');
                string fileName = dir.Substring(pos + 1);
                topNode.Nodes.Add(new TreeNode(fileName, ico, ico));
            }
        }

        int GetIconForFolder(string folder)
        {
            SHFILEINFO info = new SHFILEINFO();
            SHGetFileInfo(folder, FILE_ATTRIBUTE_DIRECTORY, ref info, Marshal.SizeOf(info), SHGFI_ICON | SHGFI_SHELLICONSIZE);
            if (StoredIcons.ContainsKey(info.iIcon) == false)
            {
                var ico = Icon.FromHandle(info.hIcon);
                ico = ico.Clone() as Icon;
                DestroyIcon(info.hIcon);
                imageList1.Images.Add(ico);
                StoredIcons.Add(info.iIcon, imageList1.Images.Count - 1);
                return imageList1.Images.Count - 1;
            }

            DestroyIcon(info.hIcon);
            return StoredIcons[info.iIcon];
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
            public char[] szDisplayName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public char[] szTypeName;
        }

        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes,
            ref SHFILEINFO psfi, int cbFileInfo, int uFlags);

        [DllImport("User32.dll")]
        private static extern void DestroyIcon(IntPtr hIcon);

        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int SHGFI_ICON = 0x100;
        private const int SHGFI_SHELLICONSIZE = 0x001;
        private Dictionary<int, int> StoredIcons = new Dictionary<int, int>();
    }
}
