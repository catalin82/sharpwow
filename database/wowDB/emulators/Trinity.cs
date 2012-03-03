using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharkWoW.database.database;

namespace SharkWoW.database.wowDB
{
    /*
     * Trinity implementation
    */ 
    class Trinity : Core
    {
        public static string CreatureSelect = "SELECT * FROM creature_template;";
        public static string CreatureSpawnSelect = "SELECT * FROM `creature`";

        protected MysqlConnector connector;
        public Trinity(MysqlConnector connector)
        {
            this.connector = connector;
        }

        protected Trinity()
        {
        }

        public virtual CreatureTemplate CreateCreatureTemplate(object[] data)
        {
            long entry = long.Parse(data[0].ToString());
            string name = data[10].ToString();
            string subname = data[11].ToString();
            int minlevel = int.Parse(data[14].ToString());
            int maxlevel = int.Parse(data[15].ToString());
            float mindmg = float.Parse(data[24].ToString());
            float maxdmg = float.Parse(data[25].ToString());
            float healthmod = float.Parse(data[68].ToString());
            float manamod = float.Parse(data[69].ToString());
            int exp = int.Parse(data[16].ToString());
            int npcClass = int.Parse(data[31].ToString());
            int[] modelids = new int[4];
            int health = WoWDBUtil.CalculateHealth(exp, healthmod, npcClass, maxlevel, connector);
            int mana = WoWDBUtil.CalculateMana(manamod, npcClass, maxlevel, connector);
            for (int i = 0; i < modelids.Length; i++)
            {
                modelids[i] = int.Parse(data[6 + i].ToString());
            }
            CreatureTemplate ct = new CreatureTemplate(entry, name, subname, minlevel, maxlevel, mindmg, maxdmg, health, mana, modelids);
            return ct;
        }

        public string GetCreatureSelect()
        {
            return CreatureSelect;
        }

        public string GetCreatureSelect(int entry)
        {
            return CreatureSelect + " WHERE `entry` = '" + entry + "';";
        }

        public CreatureSpawn CreateCreatureSpawn(object[] data)
        {
            long guid = long.Parse(data[0].ToString());
            long id = long.Parse(data[1].ToString());
            int map = int.Parse(data[2].ToString());
            int modelid = int.Parse(data[5].ToString());
            float pos_x = float.Parse(data[7].ToString());
            float pos_y = float.Parse(data[8].ToString());
            float pos_z = float.Parse(data[9].ToString());
            float orientation = float.Parse(data[10].ToString());
            int curM = int.Parse(data[15].ToString());
            int curH = int.Parse(data[14].ToString());
            CreatureSpawn cs = new CreatureSpawn(guid, id, map, modelid, pos_x, pos_y, pos_z, orientation, curH, curM);
            return cs;
        }

        public string GetCreatureSpawnSelect()
        {
            return CreatureSpawnSelect;
        }

        public string GetCreatureSpawnSelect(int mapid)
        {
            return CreatureSpawnSelect + " WHERE `map` = '" + mapid + "';";
        }

        public string GetCreatureSpawnInsertQuery(CreatureSpawn cs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO `creature` (`guid`, `id`, `map`, `modelid`, `position_x`, `position_y`, `position_z`, `orientation`, `curhealth`, `curmana`) VALUES (");
            string[] placeholder = GetSQLSpawnPlaceholder();
            for (int i = 0; i < placeholder.Length; i++)
            {
                sb.Append(placeholder[i]);
                if (i + 1 < placeholder.Length)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }

        public string GetCreatureSpawnDeleteQuery()
        {
            return "DELETE FROM `creatures` WHERE `guid` = ?guid;";
        }

        public string[] GetSQLSpawnPlaceholder()
        {
            return new string[] {"?guid", "?id", "?map", "?modelid", "?pos_x", "?pos_y", "?pos_z", "?orientation", "?curH", "?cuM"};
        }

        public object[] GetSQLSpawnValues(CreatureSpawn cs)
        {
            return new object[] {cs.guid, cs.id, cs.map, cs.modelid, cs.pos_x, cs.pos_y, cs.pos_z, cs.orientation, cs.curHealth, cs.curMana};
        }
    }
}
