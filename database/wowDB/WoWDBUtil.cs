using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharkWoW.database.wowDB;

namespace SharkWoW.database.database
{
    class WoWDBUtil
    {
        public static int CalculateHealth(int exp, float health_mod, int npcClass, int level, MysqlConnector con)
        {
            int basehp = con.GetBaseHP(level, npcClass, exp);
            return CalculateImpl(health_mod, basehp);
        }

        public static int CalculateMana(float mana_mod, int npcClass, int level, MysqlConnector con)
        {
            int basemana = con.GetBaseMana(level, npcClass);
            return CalculateImpl(mana_mod, basemana);
        }

        public static int CalculateArmor(float armor_mod, int npcClass, int level, MysqlConnector con)
        {
            int basearmor = con.GetBaseArmor(level, npcClass);
            return CalculateImpl(armor_mod, basearmor);
        }

        private static int CalculateImpl(float modValue, int baseValue)
        {
            return (int)(baseValue * modValue);
        }
    }
}
