using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SharkWoW.database.wowDB;

namespace SharkWoW.database
{
    class MysqlConnector
    {
        private MysqlData MysqlData;
        private MySqlConnection Connection;

        public MysqlConnector(string host, string database, string user, string password)
        {
            MysqlData = new MysqlData(host, database, user, password);
        }

        public void Connect()
        {
            if (Connection == null)
            {
                Connection = new MySqlConnection(MysqlData.ToString());
            }
            Connection.Open();
        }

        public int GetBaseHP(int level, int npcClass, int exp)
        {
            string columName = "basehp"+exp;
            return GetBase(level, npcClass, columName);
        }

        public int GetBaseMana(int level, int npcClass)
        {
            return GetBase(level, npcClass, "basemana");
        }

        public int GetBaseArmor(int level, int npcClass)
        {
            return GetBase(level, npcClass, "basearmor");
        }

        public int GetBase(int level, int npcClass, string columName)
        {
            MySqlCommand command = Connection.CreateCommand();
            command.CommandText = "SELECT " + columName + " FROM creature_classlevelstats WHERE `level` = '" + level + "' AND `class` = '" + npcClass + "';";
            MySqlDataReader Reader = command.ExecuteReader();
            Reader.Read();
            int value = Reader.GetInt32(columName);
            Reader.Close();
            command.Dispose();
            return value;
        }

        public DataTable GetCreatureDataTable(Core c)
        {
            DataTable myData = new DataTable();
            MySqlDataAdapter myAdapter = new MySqlDataAdapter();
            MySqlCommand command = Connection.CreateCommand();
            command.CommandText = c.GetCreatureSelect();
            myAdapter.SelectCommand = command;
            myAdapter.Fill(myData);
            myAdapter.Dispose();
            command.Dispose();
            return myData;
        }

        public CreatureTemplate GetCreature(Core c, int entry)
        {
            MySqlCommand command = Connection.CreateCommand();
            command.CommandText = c.GetCreatureSelect(entry);
            MySqlDataReader Reader = command.ExecuteReader();
            CreatureTemplate ct = null;
            while (Reader.Read())
            {
                object[] row = new object[Reader.FieldCount];
                for (int i = 0; i < Reader.FieldCount; i++)
                {
                    row[i] = Reader.GetValue(i);
                }
                ct = c.CreateCreatureTemplate(row);
                break;
            }
            Reader.Close();
            command.Dispose();
            return ct;
        }

        public CreatureSpawn[] GetCreatureSpawn(Core c, string statement)
        {
            MySqlCommand command = Connection.CreateCommand();
            command.CommandText = statement;
            MySqlDataReader Reader;
            Reader = command.ExecuteReader();

            ArrayList creatureSpawns = new ArrayList();
            while (Reader.Read())
            {
                object[] row = new object[Reader.FieldCount];
                for (int i = 0; i < Reader.FieldCount; i++)
                {
                    row[i] = Reader.GetValue(i);
                }
                creatureSpawns.Add(c.CreateCreatureSpawn(row));
            }
            Reader.Close();
            command.Dispose();
            CreatureSpawn[] creatueSpawnsArray = (CreatureSpawn[])creatureSpawns.ToArray(typeof(CreatureSpawn));
            return creatueSpawnsArray;
        }

        public CreatureSpawn[] GetCreatureSpawn(Core c)
        {
            return GetCreatureSpawn(c, c.GetCreatureSpawnSelect());
        }

        public CreatureSpawn[] GetCreatureSpawn(Core c, int map)
        {
            return GetCreatureSpawn(c, c.GetCreatureSpawnSelect(map));
        }

        public int AddCreatureSpawn(Core core, CreatureSpawn cs)
        {
            MySqlCommand command = Connection.CreateCommand();
            command.CommandText = core.GetCreatureSpawnInsertQuery(cs);
            command.Prepare();
            object[] vals = core.GetSQLSpawnValues(cs);
            string[] placeholder = core.GetSQLSpawnPlaceholder();
            for (int i = 0; i < vals.Length; i++)
            {
                string placehol = placeholder[i];
                object val = vals[i];
                command.Parameters.AddWithValue(placehol, val);
            }
            int effected = command.ExecuteNonQuery();
            command.Dispose();
            return effected;
        }

        public int DeleteCreatureSpawn(Core core, long guid)
        {
            MySqlCommand command = Connection.CreateCommand();
            command.CommandText = core.GetCreatureSpawnDeleteQuery();
            command.Prepare();
            command.Parameters.AddWithValue("?guid", guid);
            int effected = command.ExecuteNonQuery();
            command.Dispose();
            return effected;
        }

        public void Disconnect()
        {
            Connection.Close();
        }

        public MySqlConnection MySqlConnection
        {
            get
            {
                return MySqlConnection;
            }
        }
    }

    class MysqlData
    {
        public string host;
        public string database;
        public string user;
        public string password;

        public MysqlData(string host, string database, string user, string password)
        {
            this.host = host;
            this.database = database;
            this.user = user;
            this.password = password;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SERVER=").Append(host).Append(";"); ;
            sb.Append("DATABASE=").Append(database).Append(";"); ;
            sb.Append("UID=").Append(user).Append(";"); ;
            sb.Append("PASSWORD=").Append(password).Append(";"); ;
            return sb.ToString();
        }
    }
}
