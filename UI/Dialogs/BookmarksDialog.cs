using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpWoW.UI.Dialogs
{
    public partial class BookmarksDialog : Form
    {
        public BookmarksDialog()
        {
            InitializeComponent();

            var bookmarks = Game.Bookmark.Bookmarks;
            foreach (var bookmark in bookmarks)
            {
                listBox1.Items.Add(bookmark.Name);
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            var bookmark = Game.Bookmark.Bookmarks[listBox1.SelectedIndex];
            
            foreach (var entry in DBC.DBCStores.Map.Records)
            {
                if (entry.ID == bookmark.Map)
                {
                    var pos = Utils.Metrics.ToClientCoords(bookmark.Position);
                    Game.GameManager.WorldManager.EnterWorld(bookmark.Map, entry.InternalName, pos.X, pos.Y);
                    return;
                }
            }

        }
    }
}
