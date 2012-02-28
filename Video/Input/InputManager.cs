using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectInput;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SharpWoW.Video.Input
{
    public class InputManager
    {
        public InputManager()
        {
            Mouse = null;
        }

        private DirectInput mInput = new DirectInput();
        private Control mInputWindow;
        private List<Keys> mKeysDown = new List<Keys>();

        public Mouse Mouse { get; private set; }
        public Control InputWindow { get { return mInputWindow; } set { inputWindowChanged(value); } }
        public bool HasFocus
        {
            get
            {
                if (mInputWindow == null)
                    return false;

                return mInputWindow.Focused;
            }
        }

        /// <summary>
        /// Returns if a specific key is currently held down on the keyboard.
        /// </summary>
        /// <param name="key">The key to check if its pressed or not</param>
        /// <returns>A boolean indicating if the key is pressed.</returns>
        public bool this[Keys key]
        {
            get
            {
                lock (mKeysDown)
                {
                    return mKeysDown.Contains(key);
                }
            }
        }

        public void Update()
        {
            if (Mouse != null)
                Mouse.Update();
        }

        public bool IsAsyncKeyDown(Keys key)
        {
            return (GetKeyState(key) & 0x8000) != 0;
        }

        private void inputWindowChanged(Control ctrl)
        {
            if (ctrl == mInputWindow)
                return;

            if (mInputWindow != null)
            {
            }

            Mouse = new Video.Input.Mouse(mInput, ctrl.Parent.Handle);
            mInputWindow = ctrl;
            ctrl.KeyDown += new KeyEventHandler(_KeyDown);
            ctrl.KeyUp += new KeyEventHandler(_KeyUp);
            ctrl.GotFocus += new EventHandler(_UpdateFocus);
            ctrl.MouseMove += new MouseEventHandler(_MouseMoved);
            ctrl.KeyPress += new KeyPressEventHandler(_KeyPress);
        }

        void _KeyPress(object sender, KeyPressEventArgs e)
        {
            if (KeyPressed != null)
                KeyPressed(e.KeyChar);
        }

        void _MouseMoved(object sender, MouseEventArgs e)
        {
            if (MouseMoved != null)
                MouseMoved(e.X, e.Y, e.Button);
        }

        void _UpdateFocus(object sender, EventArgs e)
        {
            var vals = Enum.GetValues(typeof(Keys));
            lock (mKeysDown)
            {
                foreach (var val in vals)
                {
                    bool isDown = (GetKeyState((Keys)val) & 0x8000) != 0;
                    if (isDown)
                    {
                        if (mKeysDown.Contains((Keys)val) == false)
                            mKeysDown.Add((Keys)val);
                    }
                    else
                    {
                        if (mKeysDown.Contains((Keys)val) == true)
                            mKeysDown.Remove((Keys)val);
                    }
                }
            }
        }

        void _KeyUp(object sender, KeyEventArgs e)
        {
            lock (mKeysDown)
            {
                if (mKeysDown.Contains(e.KeyCode) == true)
                    mKeysDown.Remove(e.KeyCode);
            }
        }

        void _KeyDown(object sender, KeyEventArgs e)
        {
            lock (mKeysDown)
            {
                if (mKeysDown.Contains(e.KeyCode) == false)
                    mKeysDown.Add(e.KeyCode);
            }
        }

        public delegate void MouseMoveDlg(int x, int y, MouseButtons pressedButtons);
        public delegate void KeyPressDlg(char chr);
        public event MouseMoveDlg MouseMoved;
        public event KeyPressDlg KeyPressed;
        

        public static InputManager Input { get; private set; }
        static InputManager()
        {
            Input = new InputManager();
        }

        [DllImport("user32.dll")]
        static extern short GetKeyState(Keys nVirtKey);
    }
}
