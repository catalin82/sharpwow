using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT
{
    public class MinimapDirectory
    {
        public MinimapDirectory()
        {
            loadMinimaps();
        }

        private void loadMinimaps()
        {
            Stormlib.MPQFile file = new Stormlib.MPQFile(@"textures\Minimap\md5translate.trs");
            var fullContent = file.Read((uint)file.Length);
            var fullString = Encoding.UTF8.GetString(fullContent);
            var lines = fullString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawline in lines)
            {
                var line = rawline.Trim();

                if (line.StartsWith("dir: "))
                {
                    beginNewDirectoryEntry(line);
                    continue;
                }

                if (mIsCurrentEntryValid == false)
                    continue;

                addNewMapEntry(line);
            }
        }

        private void beginNewDirectoryEntry(string line)
        {
            var mapInternalName = line.Substring(5).ToLower();
            if (mFileMap.ContainsKey(mapInternalName))
                throw new System.Data.ConstraintException("Every map can only have one directory entry!");

            uint mapId;
            if (!getMapId(mapInternalName, out mapId))
            {
                mIsCurrentEntryValid = false;
                return;
            }

            mCurrentEntry = new Dictionary<int, string>();
            mFileMap.Add(mapInternalName, mCurrentEntry);
            mIsCurrentEntryValid = true;
        }

        private void addNewMapEntry(string line)
        {
            var keyValuePair = line.Split('\t');
            if (keyValuePair.Count() != 2)
                throw new System.Data.SyntaxErrorException("Mapentry does not contain a key and a value!");

            if (mCurrentEntry == null)
                throw new System.InvalidOperationException("Adding a new mapentry to a non existing dictionary is not allowed!");

            mCurrentEntry.Add(keyValuePair[0].ToLower().GetHashCode(), keyValuePair[1]);
        }

        private bool getMapId(string internalName, out uint id)
        {
            id = 0;

            foreach (var entry in DBC.DBCStores.Map.Records)
            {
                if (entry.InternalName.ToLower() == internalName)
                {
                    id = entry.ID;
                    return true;
                }
            }

            return false;
        }

        public bool getMinimapEntry(string continent, int indexX, int indexY, ref string fileName)
        {
            continent = continent.ToLower();
            if (mFileMap.ContainsKey(continent) == false)
                return false;

            var minimapName = continent + "\\map" + indexX + "_" + indexY + ".blp";
            var curEntry = mFileMap[continent];
            var entryHash = minimapName.ToLower().GetHashCode();
            if(curEntry.ContainsKey(entryHash) == false)
                return false;

            fileName = curEntry[entryHash];
            return true;
        }

        private bool mIsCurrentEntryValid = false;
        private Dictionary<int, string> mCurrentEntry = null;
        private Dictionary<string, Dictionary<int, string>> mFileMap = new Dictionary<string, Dictionary<int, string>>();
    }
}
