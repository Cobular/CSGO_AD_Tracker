using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace CSGO_AD_Tracker_Forms_net5
{
    public partial class Form1 : Form
    {
        private IKeyboardMouseEvents _mGlobalHook;
        private KeyboardData _keyboardData;
        private Int32 mouseEvents = 0;
        private System.Timers.Timer aTimer;


        public Form1()
        {
            aTimer = new System.Timers.Timer(2000);
            _keyboardData = KeyboardData.Instance;
            Subscribe();
            InitializeComponent();
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Unsubscribe();
        }


        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            _mGlobalHook = Hook.GlobalEvents();

            _mGlobalHook.KeyDown += OnKeyDown;
            _mGlobalHook.KeyUp += OnKeyUp;
            _mGlobalHook.MouseMove += HookManager_MouseMove;


            // m_GlobalHook.KeyPress += HookManager_KeyPress;
        }

        private void Unsubscribe()
        {
            if (_mGlobalHook == null) return;
            _mGlobalHook.KeyDown -= OnKeyDown;
            _mGlobalHook.KeyUp -= OnKeyUp;
            _mGlobalHook.MouseMove -= HookManager_MouseMove;
        }


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _keyboardData.UpdateKeyStatus((CsgoKeys) e.KeyCode, true);
            Console.WriteLine($"KeyDown  \t\t [{string.Join(",", _keyboardData.GetKeyStatuses)}]\n");
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _keyboardData.UpdateKeyStatus((CsgoKeys) e.KeyCode, false);
            Console.WriteLine($"KeyUp  \t\t [{string.Join(",", _keyboardData.GetKeyStatuses)}]\n");
        }

        private void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            // Console.WriteLine($"x={e.X:0000}; y={e.Y:0000}");
            if (aTimer.Enabled == false)
            {
                Console.WriteLine("Starting Timer...");
                SetTimer();
            }

            mouseEvents++;
        }

        private void SetTimer()
        {
            Console.WriteLine("StaringTimer");
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine($"{mouseEvents / 2}");
            mouseEvents = 0;
        }
    }
}