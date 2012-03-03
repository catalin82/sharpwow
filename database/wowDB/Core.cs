using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkWoW.database.wowDB
{
    interface Core
    {
        CreatureTemplate CreateCreatureTemplate(object[] data);
        string GetCreatureSelect();
        string GetCreatureSelect(int entry);
        CreatureSpawn CreateCreatureSpawn(object[] data);
        string GetCreatureSpawnSelect();
        string GetCreatureSpawnSelect(int map);
        string GetCreatureSpawnInsertQuery(CreatureSpawn cs);
        string GetCreatureSpawnDeleteQuery();
        string[] GetSQLSpawnPlaceholder();
        object[] GetSQLSpawnValues(CreatureSpawn cs);
    }
}
