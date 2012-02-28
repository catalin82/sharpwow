using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpWoW
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Game.GameManager.RunGame();
            }
            catch (ApplicationException)
            {
                Application.Exit();
                Application.DoEvents();
            }
            catch (System.Threading.ThreadAbortException)
            {
                Application.Exit();
                Application.DoEvents();
            }

            Game.GameManager.OnCleanup();
        }
    }
}
