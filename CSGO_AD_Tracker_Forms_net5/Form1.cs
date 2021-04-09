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

        FlowingGraph mouseVelocityGraph;
        private FancyBar syncBar;
        
        private IKeyboardMouseEvents _mGlobalHook;
        private KeyboardData _keyboardData;
        private Int32 LastMouseXCoord = 0;

        private int good;
        private int bad;

        
        private System.Timers.Timer timer;

        public Form1()
        {
            timer = new System.Timers.Timer(1);
            timer.AutoReset = true;
            timer.Elapsed += SyncCheck;
            timer.Enabled = true;
            
            _keyboardData = KeyboardData.Instance;
            
            Subscribe();
            InitializeComponent();
            
            FormClosing += Form1_FormClosing;
            
            
            mouseVelocityGraph = new FlowingGraph(false, this, new Point(10, 10), new Size(300, 300), Color.Black, Color.Cyan, 8.0f, 100, 5, 0, 1);
            syncBar = new FancyBar(this, new Point(10, 350), new Size(300, 50), new PointF(0, .33f), new PointF(1f, 1), 0, Color.Black, 100, 99999, 10);
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
            MouseVelocityNormalizer.Instance.OnADViolation += AdViolation;
            MouseVelocityNormalizer.Instance.onVelocityChange += VelocityChange;
        }

        private void Unsubscribe()
        {
            if (_mGlobalHook == null) return;
            _mGlobalHook.KeyDown -= OnKeyDown;
            _mGlobalHook.KeyUp -= OnKeyUp;
            _mGlobalHook.MouseMove -= HookManager_MouseMove;
            MouseVelocityNormalizer.Instance.OnPointAdd -= addPoint;
            MouseVelocityNormalizer.Instance.OnADViolation -= AdViolation;
        }


        private void SyncCheck(object source, ElapsedEventArgs e)
        {
            var vel = MouseVelocityNormalizer.Instance.Velocity;
            var a = MouseVelocityNormalizer.Instance.keyStatuses[1];
            var d = MouseVelocityNormalizer.Instance.keyStatuses[3];
            var dir = Math.Sign(vel);
            // -1 = left, 1 = right

            switch (dir)
            {
                case 1:
                {
                    if (a)
                        good += 1;
                    if (d)
                    {
                        bad += 1;
                        good += 1;
                    }

                    break;
                }
                case -1:
                {
                    if (a)
                    {
                        bad += 1;
                        good += 1;
                    }
                    
                    if (d)
                        good += 1;
                    break;
                }
            }
            
            if (good != 0)
                syncBar.updatePercent((good - bad) / (float) good);
        }
        
        private void AdViolation(object source, AdViolationArgs e) { }
        private void VelocityChange(object o, VelocityChangeArgs e) { }

        private void addPoint(object source, AddPointArgs e)
        {
            mouseVelocityGraph.addPoint(e.GetSum() * 10.0f);
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