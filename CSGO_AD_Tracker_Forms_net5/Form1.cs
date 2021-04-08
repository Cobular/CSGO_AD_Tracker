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

        FlowingGraph firstGraph;
        private IKeyboardMouseEvents _mGlobalHook;
        private KeyboardData _keyboardData;
        private Int32 LastMouseXCoord = 0;

        public Form1()
        {
            _keyboardData = KeyboardData.Instance;
            Subscribe();
            InitializeComponent();
            FormClosing += Form1_FormClosing;
            firstGraph = new FlowingGraph(false, this, new Point(50, 50), new Size(600, 300), Color.Black, Color.Cyan, 4.0f, 200, 5, 0, 10);
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
            MouseVelocityNormalizer.Instance.OnPointAdd += addPoint;
        }

        private void Unsubscribe()
        {
            if (_mGlobalHook == null) return;
            _mGlobalHook.KeyDown -= OnKeyDown;
            _mGlobalHook.KeyUp -= OnKeyUp;
            _mGlobalHook.MouseMove -= HookManager_MouseMove;
            MouseVelocityNormalizer.Instance.OnPointAdd -= addPoint;
        }
        
        
        private void addPoint(object source, AddPointArgs e)
        {
            firstGraph.addPoint(e.GetSum() / 5.0f);
        }


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _keyboardData.UpdateKeyStatus((CsgoKeys) e.KeyCode, true);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _keyboardData.UpdateKeyStatus((CsgoKeys) e.KeyCode, false);
        }

        private void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            // Console.Write($"{LastMouseXCoord-e.X}|");
            MouseVelocityNormalizer.Instance.AddMouseEventData(new MouseEventData(LastMouseXCoord-e.X));
            LastMouseXCoord = e.X;
        }
    }
}