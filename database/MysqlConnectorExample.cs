using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SharkWoW.database;
using SharkWoW.database.wowDB;

namespace SharpWoW.database
{
    class MysqlConnectorExample
    {
        private MysqlConnector con = new MysqlConnector("127.0.0.1", "arcemu", "root", "");
        private static Core core;

        private CreatureTable dataGridView1 = new CreatureTable();
        private TextBox textBox1 = new System.Windows.Forms.TextBox();

        public MysqlConnectorExample()
        {
            //define core
            core = new ArcEmu();
            //trinity needs mysql connection to calculate the health/mana for the hp_mod, mana_mod
            //core = new Trinity(con);
            //core = new Mangos();
            try
            {
                //connect to database
                con.Connect();

                //test methods
                testSQLTable();
                testCreatureSpawnFull();
                testCreatureViaEntry();
                testInsertCreatureSpawn();
                testDeleteCreatureSpawn();

                //disconnect from database
                con.Disconnect();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Connection error: " + ex.Message);
            }
        }

        private void testCreatureSpawnFull()
        {
            try
            {
                CreatureSpawn[] spawnData = con.GetCreatureSpawn(core);
                if (spawnData.Length <= 0)
                {
                    MessageBox.Show("No spawns found !");
                }
                else
                {
                    MessageBox.Show(spawnData[0].ToString());
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Creature Full Spawn Error: " + ex.Message);
            }
        }

        private void testCreatureViaEntry()
        {
            try
            {
                int entry = 2;
                CreatureTemplate ct = con.GetCreature(core, entry);
                if (ct == null)
                {
                    MessageBox.Show("No creature found with entry" + entry + "found !");
                }
                else
                {
                    MessageBox.Show(ct.ToString());
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Creature Via Entry Error: " + ex.Message);
            }
        }

        private void testInsertCreatureSpawn()
        {
            CreatureSpawn spawn = new CreatureSpawn(2000000, 40627, 999, 32106, 999.5f, 999.6f, 999.7f, 999.8f, 599, 599);
            try
            {
                MessageBox.Show("Inserted " + con.AddCreatureSpawn(core, spawn) + " row(s)!");
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Insert Creature Spawn failed(guid="+spawn.guid+"): " + ex.Message);
            }
        }

        private void testDeleteCreatureSpawn()
        {
            CreatureSpawn spawn = new CreatureSpawn(2000000, 40627, 999, 32106, 999.5f, 999.6f, 999.7f, 999.8f, 599, 599);
            try
            {
                MessageBox.Show("Deleted " + con.DeleteCreatureSpawn(core, spawn.guid) + " row(s)!");
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Delete Creature Spawn failed(guid=" + spawn.guid + "): " + ex.Message);
            }
        }

        private void testSQLTable()
        {
            try
            {
                //set the sqlTable the core so it knows how to convert the data in the rows
                dataGridView1.Core = core;
                dataGridView1.Load(con);
                //test CreatureTemplate dragAndDrop with a simple textfield
                textBox1.AllowDrop = true;
                textBox1.DragDrop += new System.Windows.Forms.DragEventHandler(textBox1_OnDragDrop);
                textBox1.DragEnter += new System.Windows.Forms.DragEventHandler(textBox1_OnDragEnter);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("SQLTable error: " + ex.Message);
            }
        }

        protected void textBox1_OnDragDrop(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Copy)
            {
                CreatureTemplate creatureData = (CreatureTemplate)e.Data.GetData(typeof(CreatureTemplate));
                textBox1.Text = creatureData.ToString();
            }
        }

        protected void textBox1_OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CreatureTemplate)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}
