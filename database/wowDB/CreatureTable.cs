using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SharkWoW.database.database;
using SharkWoW.database.wowDB;


namespace SharkWoW.database.wowDB
{
    class CreatureTable : DataGridView
    {
        public Core Core;

        public CreatureTable(Core core) : this()
        {
            this.Core = core;
        }

        public CreatureTable()
        {
            this.AllowDrop = true;
        }

        public void Load(MysqlConnector conn)
        {
            DataTable data = conn.GetCreatureDataTable(Core);
            this.DataSource = data;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int rowIndexFromMouseDown = this.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1)
            {
                DataGridViewRow row = this.Rows[rowIndexFromMouseDown];
                object[] dataRow = new object[row.Cells.Count];
                for (int i = 0; i < dataRow.Length; i++)
                {
                    dataRow[i] = row.Cells[i].Value;
                }
                CreatureTemplate creatureData = Core.CreateCreatureTemplate(dataRow);
                this.DoDragDrop(creatureData, DragDropEffects.Copy);
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effect = DragDropEffects.Copy;
        }
        
        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            if (e.Effect == DragDropEffects.Copy)
            {
                CreatureTemplate creatureData = (CreatureTemplate)e.Data.GetData(typeof(CreatureTemplate));
                //maybe add this creature to table here not sure if we need this
            }
        }
    }
}
