using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SharpWoW.UI
{
    public class ManualPathSelector
    {
        public static string SelectPath(out bool cancelled)
        {
            cancelled = false;
            string dialogTitle = "Select the location of your WoW.exe. It should be 3.3.5a (build 12340).";
            while (true)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                fbd.ShowNewFolderButton = false;
                fbd.Description = dialogTitle;
                var res = fbd.ShowDialog();
                if (res != DialogResult.OK)
                {
                    cancelled = true;
                    return null;
                }

                if (System.IO.File.Exists(fbd.SelectedPath + "\\" + "WoW.exe") == false)
                {
                    dialogTitle = "Error: There is no WoW.exe at the location '" + fbd.SelectedPath + "'.\nSelect the location of your WoW.exe. It should be 3.3.5a (build 12340).";
                    continue;
                }

                if (Game.GameManager.IsValidWoWExe(fbd.SelectedPath) == false)
                {
                    dialogTitle = "Error: The WoW.exe at the location '" + fbd.SelectedPath + "' is not 3.3.5a.\nSelect the location of your WoW.exe. It should be 3.3.5a (build 12340).";
                    continue;
                }

                return fbd.SelectedPath;
            }
        }
    }
}
