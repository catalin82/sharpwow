using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace SharpWoW.Game
{
    public static class GameManager
    {
        /// <summary>
        /// Initializes the game and runs the modal loop. The calling thread is interpreted as the main thread.
        /// Once you have called you should not call RunGame again.
        /// </summary>
        public static void RunGame()
        {
            mActiveChange = Logic.ActiveChangeType.Height;
            SavePath = ".\\Save\\";
            mStartTime = DateTime.Now;
            ThreadManager = new ThreadManager();
            mMainThread = System.Threading.Thread.CurrentThread;
            Application.EnableVisualStyles();
            mForm = new UI.Form1();
            Bookmark.SetKeyUpDelegate();
            GraphicsThread = new VideoThread(mForm, mForm.panel1);
            if (GraphicsThreadCreated != null)
                GraphicsThreadCreated();

            GraphicsThread.RunLoop();
        }

        /// <summary>
        /// Called by the main windows when its initialization is done. After here all the additional stuff is loaded
        /// which may use graphical interaction with the user. 
        /// </summary>
        public static void OnFormLoaded()
        {
            Stormlib.MPQArchiveLoader.Instance.Init();
            DBC.DBCStores.LoadFiles();
            SkyManager = new World.SkyManager();
            mForm.OnGameInitialized(); 
            TerrainLogic = new Logic.TerrainLogic();
            WorldManager = new Game.WorldManager();
            M2ModelCache = new Models.MDX.M2InfoCache();
            M2ModelManager = new Models.MDX.M2Manager();
            SelectionManager = new Models.SelectionManager();
            if (GlobalDataLoaded != null)
                GlobalDataLoaded();
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        /// <param name="path">Path to the exe</param>
        /// <returns>true</returns>
        [Obsolete("All versions are now supported!", false)]
        internal static bool IsValidWoWExe(string path)
        {
            return true;
        }

        /// <summary>
        /// Informs the main thread that it should quit and terminates all running threads in the Threadmanager.
        /// </summary>
        public static void Shutdown()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == mMainThread.ManagedThreadId)
                throw new ApplicationException();

            mMainThread.Abort();
        }

        /// <summary>
        /// Called by the main thread right before the application terminates. Do not call.
        /// </summary>
        public static void OnCleanup()
        {
            ThreadManager.Shutdown();
            if (GameTerminated != null)
                GameTerminated();
        }

        /// <summary>
        /// Inform the game that a game property has been changed. This will invoke the
        /// PropertyChanged event and cause the property grids to be updated.
        /// </summary>
        public static void InformPropertyChanged(GameProperties property)
        {
            if (PropertyChanged != null)
                PropertyChanged(property);
        }

        public static bool IsPandaria { get { return BuildNumber >= 15464; } }

        internal static string GamePath
        {
            get
            {
                if (mGamePath == null)
                {
                    if (loadGamePathSaved())
                        return mGamePath;

                    if (!loadGamePathRegistry() || BuildNumber != 12340)
                    {
                        bool abort = false;
                        mGamePath = UI.ManualPathSelector.SelectPath(out abort);
                        if (abort)
                        {
                            Shutdown();
                            return "";
                        }

                        var res = MessageBox.Show("Would you like to store that path in a new registry key?", "Saving the path...", MessageBoxButtons.YesNo);
                        if (res == DialogResult.Yes)
                        {
                            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Yias\\SharpWoW", true);
                            if (key == null)
                                key = Registry.CurrentUser.CreateSubKey("Software\\Yias\\SharpWoW");

                            key.SetValue("StoredWoWPath", mGamePath);
                        }
                    }
                }
                return mGamePath;
            }
        }

        /// <summary>
        /// Returns the build number of the currently used wow. This should be 12340 currently.
        /// </summary>
        public static ushort BuildNumber
        {
            get
            {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(GamePath + "\\WoW.exe");
                return (ushort)info.FilePrivatePart;
            }
        }

        public static ThreadManager ThreadManager { get; private set; }
        public static VideoThread GraphicsThread { get; private set; }
        public static WorldManager WorldManager { get; private set; }
        public static Logic.TerrainLogic TerrainLogic { get; private set; }
        public static UI.Form1 GameWindow { get { return mForm; } }
        public static TimeSpan GameTime { get { return DateTime.Now - mStartTime; } }
        public static World.SkyManager SkyManager { get; private set; }
        public static Models.MDX.M2InfoCache M2ModelCache { get; private set; }
        public static Models.MDX.M2Manager M2ModelManager { get; private set; }
        public static string SavePath { get; set; }
        public static Models.SelectionManager SelectionManager { get; private set; }
        public static Logic.ActiveChangeType ActiveChangeType
        {
            get { return mActiveChange; }
            set
            {
                mActiveChange = value;
                if (mActiveChange == Logic.ActiveChangeType.Texturing)
                    Video.ShaderCollection.TerrainShader.SetValue("brushType", 3);

                if (ActiveChangeModeChanged != null)
                    ActiveChangeModeChanged();
            }
        }

        public static event Action<GameProperties> PropertyChanged;
        public static event Action GameTerminated;
        public static event Action ActiveChangeModeChanged;
        public static event Action GraphicsThreadCreated;
        public static event Action GlobalDataLoaded;

        private static string mGamePath = null;
        private static UI.Form1 mForm = null;
        private static System.Threading.Thread mMainThread = null;
        private static DateTime mStartTime;
        private static Logic.ActiveChangeType mActiveChange;

        private static bool loadGamePathRegistry()
        {
            RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(@"Software\Blizzard Entertainment\World of Warcraft");
            if (baseKey == null)
                return false;

            string basePath = (string)baseKey.GetValue("InstallPath");
            if (basePath == null)
                return false;

            mGamePath = basePath;
            return true;
        }

        private static bool loadGamePathSaved()
        {
            var key = Registry.CurrentUser.OpenSubKey("Software\\Yias\\SharpWoW");
            if (key == null)
                return false;

            var val = key.GetValue("StoredWoWPath");
            if (val == null)
                return false;

            if (Directory.Exists((string)val) == false)
                return false;

            mGamePath = (string)val;
            return true;
        }
    }

    public enum GameProperties
    {
        HoveredADT,
        HoveredChunk,
        Map,
        CameraPosition
    }
}
