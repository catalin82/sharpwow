using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SlimDX;
using System.Windows.Forms;
using SharpWoW.Video.Input;

namespace SharpWoW.Game
{
    public class Bookmark
    {
        public String Name { get; set; }
        public uint Map;
        public Vector2 Position;

        public Bookmark(String name, uint map, Vector2 position)
        {
            Name = name;
            Map = map;
            Position = position;
        }

        public Bookmark()
        {
            Name = "";
            Map = uint.MaxValue;
            Position = new Vector2();
        }

        public static void SetDelegate()
        {
            InputManager.Input.KeyUp += new InputManager.KeyUpDlg((Keys key) =>
            {
                if (key == Keys.F5)
                {
                    addBookmark();
                }
            });
        }

        private static void addBookmark()
        {
            var bookmarks = Game.Bookmark.Bookmarks;

            var camPos = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position;
            var pos = new Vector2(camPos.X, camPos.Y);
            var name = "";

            var chunk = Game.GameManager.WorldManager.HoveredChunk;
            if (chunk == null)
            {
                name = "(unknown)";
                return;
            }

            try
            {
                var ae = DBC.DBCStores.AreaTable[chunk.Header.areaId];
                name = ae.AreaName;
            }
            catch (Exception)
            {
                name = "(unknown)";
            }
            bookmarks.Add(new Game.Bookmark(name, Game.GameManager.WorldManager.MapID, pos));
            Game.Bookmark.Bookmarks = bookmarks;
        }

        public static List<Bookmark> Bookmarks
        {
            get
            {
                var tmp = new List<Bookmark>();
                using (var reader = new StreamReader(File.Open("Bookmarks.txt", FileMode.OpenOrCreate)))
                {
                    while (reader.EndOfStream == false)
                    {
                        var bookmark = new Bookmark();
                        var position = new Vector2();
                        var line = reader.ReadLine();

                        var name = trim(ref line);
                        bookmark.Map = Convert.ToUInt32(trim(ref line));

                        position.X = Convert.ToSingle(trim(ref line));
                        position.Y = Convert.ToSingle(trim(ref line));

                        bookmark.Name = name;
                        bookmark.Position = position;

                        tmp.Add(bookmark);
                    }
                }
                return tmp;
            }
            set
            {
                using (var writer = new StreamWriter(File.Open("Bookmarks.txt", FileMode.Truncate)))
                {
                    foreach (var bookmark in value)
                    {
                        writer.WriteLine("{0}\t{1}\t{2}\t{3}", bookmark.Name, bookmark.Map, bookmark.Position.X, bookmark.Position.Y);
                    }
                }
            }
        }

        private static string trim(ref string line)
        {
            var ret = line;

            var n = line.IndexOf('\t');
            if (n == -1)
            {
                n = line.IndexOf('\n');
                if (n == -1)
                    return ret;
            }

            ret = line.Substring(0, n);
            line = line.Substring(n + 1);

            return ret;
        }
    }
}
