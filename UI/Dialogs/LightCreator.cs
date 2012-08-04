using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpWoW.UI.Dialogs
{
    public partial class LightCreator : Form
    {
        public LightCreator()
        {
            InitializeComponent();
        }

        private void LightCreator_Load(object sender, EventArgs e)
        {
            if (mLightEntry == null)
                setCurrentPosition();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            setCurrentPosition();
        }

        private void setCurrentPosition()
        {
            var camPos = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position;
            numericUpDown1.Value = (decimal)camPos.X;
            numericUpDown2.Value = (decimal)camPos.Y;
            numericUpDown3.Value = (decimal)camPos.Z;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MinimapDialog mp = new MinimapDialog();
            mp.SetMap(DBC.DBCStores.Map[Game.GameManager.WorldManager.MapID]);
            mp.PointSelected += new MinimapDialog.MinimapSelectedDlg(ptSelect);
            mp.ShowDialog();
        }

        void ptSelect(uint mapid, string continent, float x, float y)
        {
            numericUpDown1.Value = (decimal)(x - Utils.Metrics.MidPoint);
            numericUpDown2.Value = (decimal)(y - Utils.Metrics.MidPoint);
            numericUpDown3.Value = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MapRadiusSelector mrs = new MapRadiusSelector();
            mrs.InnerRadius = (float)numericUpDown4.Value;
            mrs.OuterRadius = (float)numericUpDown5.Value;
            mrs.SetInfo(DBC.DBCStores.Map[Game.GameManager.WorldManager.MapID], (float)numericUpDown1.Value + Utils.Metrics.MidPoint, (float)numericUpDown2.Value + Utils.Metrics.MidPoint);
            mrs.RadiusChanged += new Action<MapRadiusSelector>(lightRadiusChanged);
            mrs.ShowDialog();
        }

        void lightRadiusChanged(MapRadiusSelector sender)
        {
            numericUpDown4.Value = (decimal)sender.InnerRadius;
            numericUpDown5.Value = (decimal)sender.OuterRadius;
        }

        private World.WorldLightEntry mLightEntry = null;
        private bool ShowWarningOnSave = true;

        public static LightCreator InitFromExistingLight(World.WorldLightEntry worldLight)
        {
            var dlg = new LightCreator();
            var pos = worldLight.Position;
            dlg.numericUpDown1.Value = (decimal)(pos.X - Utils.Metrics.MidPoint);
            dlg.numericUpDown2.Value = (decimal)(pos.Z - Utils.Metrics.MidPoint);
            dlg.numericUpDown3.Value = (decimal)pos.Y;
            dlg.numericUpDown4.Value = (decimal)worldLight.InnerRadius;
            dlg.numericUpDown5.Value = (decimal)worldLight.OuterRadius;

            // ToList is used to make sure a copy of the list is created to prevent the existing tables from being changed.
            dlg.lightColorSelector1.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.GlobalDiffuse).ToList(), worldLight.GetColorLine(World.ColorTableValues.GlobalDiffuse).ToList());
            dlg.lightColorSelector2.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.GlobalAmbient).ToList(), worldLight.GetColorLine(World.ColorTableValues.GlobalAmbient).ToList());
            dlg.lightColorSelector3.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.Fog).ToList(), worldLight.GetColorLine(World.ColorTableValues.Fog).ToList());
            dlg.lightColorSelector4.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.Color0).ToList(), worldLight.GetColorLine(World.ColorTableValues.Color0).ToList());
            dlg.lightColorSelector5.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.Color1).ToList(), worldLight.GetColorLine(World.ColorTableValues.Color1).ToList());
            dlg.lightColorSelector6.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.Color2).ToList(), worldLight.GetColorLine(World.ColorTableValues.Color2).ToList());
            dlg.lightColorSelector7.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.Color3).ToList(), worldLight.GetColorLine(World.ColorTableValues.Color3).ToList());
            dlg.lightColorSelector8.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.Color4).ToList(), worldLight.GetColorLine(World.ColorTableValues.Color4).ToList());
            dlg.lightColorSelector9.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.SunColor).ToList(), worldLight.GetColorLine(World.ColorTableValues.SunColor).ToList());
            dlg.lightColorSelector10.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.SunHaloColor).ToList(), worldLight.GetColorLine(World.ColorTableValues.SunHaloColor).ToList());
            dlg.lightColorSelector11.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.WaterDark).ToList(), worldLight.GetColorLine(World.ColorTableValues.WaterDark).ToList());
            dlg.lightColorSelector12.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.WaterLight).ToList(), worldLight.GetColorLine(World.ColorTableValues.WaterLight).ToList());
            dlg.lightColorSelector13.InitFromWorldLight(worldLight.GetTimeLine(World.ColorTableValues.Shadow).ToList(), worldLight.GetColorLine(World.ColorTableValues.Shadow).ToList());

            dlg.mLightEntry = worldLight;
            dlg.numericUpDown6.Value = (decimal)worldLight.LightParams.glow;
            dlg.comboBox1.SelectedIndex = (int)worldLight.LightParams.cloudID;
            dlg.checkBox1.Checked = worldLight.LightParams.HighlightSky > 0;

            var skyb = worldLight.LightParams.skyboxID;
            if (DBC.DBCStores.LightSkyBox.ContainsKey(skyb))
            {
                dlg.textBox1.Text = DBC.DBCStores.LightSkyBox[skyb].Path;
                dlg.textBox1.Tag = DBC.DBCStores.LightSkyBox[skyb].ID;
            }
            else
            {
                dlg.textBox1.Text = "(none)";
                dlg.textBox1.Tag = (uint)0;
            }

            dlg.numericUpDown7.Value = (decimal)worldLight.LightParams.waterShallowAlpha;
            dlg.numericUpDown8.Value = (decimal)worldLight.LightParams.waterDeepAlpha;
            dlg.numericUpDown9.Value = (decimal)worldLight.LightParams.oceanShallowAlpha;
            dlg.numericUpDown10.Value = (decimal)worldLight.LightParams.oceanDeepAlpha;
            return dlg;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (mLightEntry == null)
                return;

            if (ShowWarningOnSave)
            {
                var res = MessageBox.Show("If you save your changes an existing light is overwritten. Are you sure this is intended?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (res == System.Windows.Forms.DialogResult.No)
                    return;

                ShowWarningOnSave = false;
            }

            mLightEntry.SetNewTables(World.ColorTableValues.GlobalDiffuse, lightColorSelector1.Interpolator.TimeTable.ToList(), lightColorSelector1.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.Fog, lightColorSelector3.Interpolator.TimeTable.ToList(), lightColorSelector3.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.GlobalAmbient, lightColorSelector2.Interpolator.TimeTable.ToList(), lightColorSelector2.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.Color0, lightColorSelector4.Interpolator.TimeTable.ToList(), lightColorSelector4.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.Color1, lightColorSelector5.Interpolator.TimeTable.ToList(), lightColorSelector5.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.Color2, lightColorSelector6.Interpolator.TimeTable.ToList(), lightColorSelector6.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.Color3, lightColorSelector7.Interpolator.TimeTable.ToList(), lightColorSelector7.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.Color4, lightColorSelector8.Interpolator.TimeTable.ToList(), lightColorSelector8.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.SunColor, lightColorSelector9.Interpolator.TimeTable.ToList(), lightColorSelector9.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.SunHaloColor, lightColorSelector10.Interpolator.TimeTable.ToList(), lightColorSelector10.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.WaterDark, lightColorSelector11.Interpolator.TimeTable.ToList(), lightColorSelector11.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.WaterLight, lightColorSelector12.Interpolator.TimeTable.ToList(), lightColorSelector12.Interpolator.ColorTable.ToList());
            mLightEntry.SetNewTables(World.ColorTableValues.Shadow, lightColorSelector13.Interpolator.TimeTable.ToList(), lightColorSelector13.Interpolator.ColorTable.ToList());

            mLightEntry.LightParams.glow = (float)numericUpDown6.Value;
            mLightEntry.LightParams.cloudID = (uint)comboBox1.SelectedIndex;
            mLightEntry.LightParams.HighlightSky = (checkBox1.Checked ? 1u : 0u);
            mLightEntry.LightParams.skyboxID = (uint)textBox1.Tag;
            mLightEntry.LightParams.waterShallowAlpha = (float)numericUpDown7.Value;
            mLightEntry.LightParams.waterDeepAlpha = (float)numericUpDown8.Value;
            mLightEntry.LightParams.oceanShallowAlpha = (float)numericUpDown9.Value;
            mLightEntry.LightParams.oceanDeepAlpha = (float)numericUpDown10.Value;

            mLightEntry.LightEntry.x = (((float)numericUpDown1.Value) + Utils.Metrics.MidPoint) * 36.0f;
            mLightEntry.LightEntry.z = (((float)numericUpDown2.Value) + Utils.Metrics.MidPoint) * 36.0f;
            mLightEntry.LightEntry.y = ((float)numericUpDown3.Value) * 36.0f;
            mLightEntry.LightEntry.falloff = (float)numericUpDown4.Value * 36.0f;
            mLightEntry.LightEntry.falloffEnd = (float)numericUpDown5.Value * 36.0f;

            DBC.DBCStores.LightIntBand.SaveDBC();
            DBC.DBCStores.LightParams.SaveDBC();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            UI.Dialogs.GlowHelpDialog ghd = new GlowHelpDialog();
            ghd.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            UI.Dialogs.SkyBoxSelector sbs = new SkyBoxSelector();
            sbs.ShowDialog();
            if (sbs.SelectedItem != null)
            {
                textBox1.Text = sbs.SelectedItem.Path;
                textBox1.Tag = sbs.SelectedItem.ID;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DBC.Light light = new DBC.Light();
            light.ID = DBC.DBCStores.Light.MaxKey + 1;
            light.deathParam = 0;
            light.waterParam = 0;
            light.otherParam = 0;
            light.MapID = Game.GameManager.WorldManager.MapID;
            light.sunsetParam = 0;
            light.unk1 = light.unk2 = light.unk3 = 0;
            light.x = (((float)numericUpDown1.Value) + Utils.Metrics.MidPoint) * 36.0f;
            light.z = (((float)numericUpDown2.Value) + Utils.Metrics.MidPoint) * 36.0f;
            light.y = ((float)numericUpDown3.Value) * 36.0f;
            light.falloff = (float)numericUpDown4.Value * 36.0f;
            light.falloffEnd = (float)numericUpDown5.Value * 36.0f;

            uint maxIntParam = DBC.DBCStores.LightIntBand.MaxKey;
            maxIntParam /= 18;
            ++maxIntParam;

            uint maxLightParam = DBC.DBCStores.LightParams.MaxKey;
            ++maxLightParam;

            uint skyParam = (uint)Math.Max(maxIntParam, maxLightParam);
            light.skyParam = skyParam;

            DBC.LightParams param = new DBC.LightParams()
            {
                ID = skyParam,
                glow = (float)numericUpDown6.Value,
                cloudID = (uint)comboBox1.SelectedIndex,
                HighlightSky = (checkBox1.Checked ? 1u : 0u),
                oceanDeepAlpha = (float)numericUpDown10.Value,
                oceanShallowAlpha = (float)numericUpDown9.Value,
                waterDeepAlpha = (float)numericUpDown8.Value,
                waterShallowAlpha = (float)numericUpDown7.Value,
                skyboxID = (uint)textBox1.Tag
            };

            DBC.DBCStores.LightParams.AddEntry(skyParam, param);

            #region LightIntBand
            for (uint i = 0; i < 18; ++i)
            {
                DBC.LightIntBand lib = new DBC.LightIntBand();
                lib.Times = new uint[16];
                lib.Values = new uint[16];
                lib.ID = skyParam * 18 - 17 + i;
                switch ((World.ColorTableValues)i)
                {
                    case World.ColorTableValues.GlobalDiffuse:
                        {
                            var times = lightColorSelector1.Interpolator.TimeTable;
                            var colors = lightColorSelector1.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.GlobalAmbient:
                        {
                            var times = lightColorSelector2.Interpolator.TimeTable;
                            var colors = lightColorSelector2.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.Fog:
                        {
                            var times = lightColorSelector3.Interpolator.TimeTable;
                            var colors = lightColorSelector3.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.Color0:
                        {
                            var times = lightColorSelector4.Interpolator.TimeTable;
                            var colors = lightColorSelector4.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.Color1:
                        {
                            var times = lightColorSelector5.Interpolator.TimeTable;
                            var colors = lightColorSelector5.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.Color2:
                        {
                            var times = lightColorSelector6.Interpolator.TimeTable;
                            var colors = lightColorSelector6.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.Color3:
                        {
                            var times = lightColorSelector7.Interpolator.TimeTable;
                            var colors = lightColorSelector7.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.Color4:
                        {
                            var times = lightColorSelector8.Interpolator.TimeTable;
                            var colors = lightColorSelector8.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.SunColor:
                        {
                            var times = lightColorSelector9.Interpolator.TimeTable;
                            var colors = lightColorSelector9.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.SunHaloColor:
                        {
                            var times = lightColorSelector10.Interpolator.TimeTable;
                            var colors = lightColorSelector10.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.WaterDark:
                        {
                            var times = lightColorSelector11.Interpolator.TimeTable;
                            var colors = lightColorSelector11.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.WaterLight:
                        {
                            var times = lightColorSelector12.Interpolator.TimeTable;
                            var colors = lightColorSelector12.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    case World.ColorTableValues.Shadow:
                        {
                            var times = lightColorSelector13.Interpolator.TimeTable;
                            var colors = lightColorSelector13.Interpolator.ColorTable;

                            lib.NumEntries = (uint)times.Count;
                            for (int j = 0; j < times.Count; ++j)
                            {
                                lib.Times[j] = times[j];
                                lib.Values[j] = World.WorldLightEntry.ToUint(colors[j]);
                            }
                        }
                        break;

                    default:
                        {
                            lib.NumEntries = 0;
                            break;
                        }
                }

                DBC.DBCStores.LightIntBand.AddEntry(lib.ID, lib);
            }
            #endregion

            for (int i = 0; i < 6; ++i)
            {
                DBC.LightFloatBand lfb = new DBC.LightFloatBand();
                lfb.ID = skyParam * 6 - 5 + (uint)i;
                lfb.NumEntries = 0;
                lfb.Times = new uint[16];
                lfb.Values = new float[16];
                DBC.DBCStores.LightFloatBand.AddEntry(lfb.ID, lfb);
            }

            DBC.DBCStores.Light.AddEntry(light.ID, light);

            World.WorldLightEntry wle = new World.WorldLightEntry(light);
            if (Game.GameManager.IsPandaria == false)
                Game.GameManager.SkyManager.GetSkyForMap(light.MapID).AddNewLight(wle);

            DBC.DBCStores.LightIntBand.SaveDBC();
            DBC.DBCStores.LightParams.SaveDBC();
            DBC.DBCStores.Light.SaveDBC();
            DBC.DBCStores.LightFloatBand.SaveDBC();
        }
    }
}
