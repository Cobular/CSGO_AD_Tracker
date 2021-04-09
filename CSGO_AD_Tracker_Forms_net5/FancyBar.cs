using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CSGO_AD_Tracker_Forms_net5
{
    public class FancyBar
    {
        private Form parent; 
        
        private PictureBox background;
        private Color backColor;
        private Point location;
        private Size maxSize;
        
        private BarBox[] bar;
        private int currBar;
        
        private PointF hueRange;
        private PointF satRange;
        private double currentSat;
        private float percent;

        private Timer saturationWave;
        private int saturationWaveRefreshRate;
        private int saturationWaveLocation = 0;
        private int waveThickness;

        private Timer redrawTimer;
        private int redrawRefreshRate;


        /// <summary>
        /// Creates a horizontal bar that fills up based on percentage, has a gradient color, and a wave of changing saturation values.
        /// wave stuff doesn't work c# is a bad language
        /// </summary>
        /// <param name="location"> Top left coord for the bar </param>
        /// <param name="maxSize"> The max size of the bar at 100% </param>
        /// <param name="hueRange"> Range of Hues from 0 to 360 </param>
        /// <param name="satRange"> Range of Saturation from 0 to 1 </param>
        /// <param name="waveThickness"> How thick the aesthetic wave that travels through the graph is</param>
        /// <param name="backColor"> The color of the bar background </param>
        /// <param name="initPercent"> The percent that you start at </param>
        /// <param name="satWaveRefreshRate"> How fast the saturation wave updates </param>
        /// <param name="redrawRefreshRate"> How fast the bar updates </param>
        public FancyBar(Form form, Point location, Size maxSize, PointF hueRange, PointF satRange, int waveThickness, Color backColor, float initPercent, int satWaveRefreshRate, int redrawRefreshRate)
        {
            parent = form;
            this.location = location;
            this.maxSize = maxSize;
            this.hueRange = hueRange;
            this.satRange = satRange;
            this.percent = initPercent;
            this.saturationWaveRefreshRate = satWaveRefreshRate;
            this.waveThickness = waveThickness;
            if (this.waveThickness % 2 == 0) this.waveThickness -= 1;
            this.redrawRefreshRate = redrawRefreshRate;
            this.backColor = backColor;
            this.currentSat = satRange.X;

            
            
            background = new PictureBox() {
                Size = this.maxSize,
                BackColor = this.backColor,
                Location = this.location
            };
            parent.Controls.Add(background);
            generateInitialBar();
            background.SendToBack();
            saturationWave = new Timer(this.saturationWaveRefreshRate) {AutoReset = true};
            saturationWave.Elapsed += saturationWaveUpdate;
            saturationWave.Enabled = true;

            redrawTimer = new Timer(this.redrawRefreshRate) {AutoReset = true};
            redrawTimer.Elapsed += redraw;
            redrawTimer.Enabled = true;
        }

        public void updatePercent(float percent)
        {
            this.percent = percent * 100;
            //Console.WriteLine(percent);

        }
        
        private void generateInitialBar()
        {
            bar = new BarBox[maxSize.Width];
            
            float workingPercent = 0;
            var percentIncrement = 100 / (float) maxSize.Width;
            var hueInc = (hueRange.Y - hueRange.X) / maxSize.Width;
            
            //populates the BarBox array
            for (var i = 0; i < bar.Length; i++) {
                bar[i] = new BarBox(
                    new PictureBox() {
                        Size = new Size(1, maxSize.Height),
                        Location = new Point(location.X + i, location.Y),
                        BackColor = HsvToRgbColor(hueRange.X + i * hueInc, satRange.X, 1)
                    },
                    workingPercent,
                    parent);

                workingPercent += percentIncrement;
            }
            

            //reverse backwards to disable all boxes that are too full for initPercent
            currBar = maxSize.Width - 1;
            while (bar[currBar].Level > percent)
            {
                bar[currBar].Box.Visible = false;
                if (currBar == 0)
                    break;
                currBar--;
            }
        }
        private static Color HsvToRgbColor(double hue, double saturation, double value)
        {
            Debug.Assert(hue >= 0);
            Debug.Assert(hue <= 1);
            Debug.Assert(saturation >= 0);
            Debug.Assert(saturation <= 1);
            Debug.Assert(value >= 0);
            Debug.Assert(value <= 1);

            double chroma = value * saturation;
            double h = 6 * hue;
            double x = chroma * (1 - Math.Abs(h % 2 - 1));
            double r1 = 0;
            double g1 = 0;
            double b1 = 0;
            switch ((int)Math.Floor(h))
            {
                case 0: r1 = chroma; g1 = x; break;
                case 1: r1 = x; g1 = chroma; break;
                case 2: g1 = chroma; b1 = x; break;
                case 3: g1 = x; b1 = chroma; break;
                case 4: b1 = chroma; r1 = x; break;
                case 5: b1 = x; r1 = chroma; break;
                default: break;
            }
            double m = value - chroma;
            double r = r1 + m;
            double g = g1 + m;
            double b = b1 + m;
            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }
        private void saturationWaveUpdate(object o, ElapsedEventArgs e)
        {
            float waveCenter = (float) (waveThickness + 1) / 2;

            for (var i = 1 - waveCenter * 2; i < waveCenter * 2; i++)
            {
                if (i + saturationWaveLocation >= 0 && i + saturationWaveLocation < maxSize.Width)
                {
                    updateSaturation(bar[(int) (saturationWaveLocation + i)], Math.Abs(i), waveCenter);
                }
            }

            
            saturationWaveLocation++;
            if (saturationWaveLocation >= maxSize.Width + waveCenter) saturationWaveLocation = (int) (-1 * waveCenter);

        }

        private void updateSaturation(BarBox box, float distanceFromCenter, float centerLocation)
        {
            var saturationInterval = (satRange.Y - satRange.X) / centerLocation;
            //Console.WriteLine((satRange.Y - saturationInterval * Math.Abs(distanceFromCenter)).ToString());
            
            var current = box.Box.BackColor;
            
            var R = current.R / (float) 255;
            var G = current.G / (float) 255;
            var B = current.B / (float) 255;
            var max = Math.Max(Math.Max(R, G), B);
            var min = Math.Min(Math.Min(R, G), B);

            float hue = 0;
            
            if (max.Equals(R))
                hue = (G - B) / (max - min);
            if (max.Equals(G))
                hue = (2.0f + (B - R) / (max - min));
            if (max.Equals(B))
                hue = (4.0f + (R - G) / (max - min));

            hue *= 60;
            if (hue < 0)
                hue += 360;
            


            Console.WriteLine(current.GetHue());
            //gets range / half of the thickness, rounded up
            

            //new color = high saturation in center, low on outsides
            var newColor = HsvToRgbColor(
                hue / 360.0f, 
                (satRange.Y - saturationInterval * Math.Abs(distanceFromCenter)) > 0 ? (satRange.Y - saturationInterval * Math.Abs(distanceFromCenter)) : 0, 
                1);
            box.Box.BackColor = newColor;
            
        }
        
        private void redraw(object o, ElapsedEventArgs e)
        {
            foreach (var barBox in bar.Where(x => x.isTooLarge(this.percent) && x.Box.Visible))
            {
                barBox.Box.Visible = false;
                currBar--;
            }
            foreach (var barBox in bar.Where(x => x.isTooSmall(this.percent) && !x.Box.Visible))
            {
                barBox.Box.Visible = true;
                currBar++;
            }
        }
    }

    internal class BarBox
    {
        public readonly PictureBox Box;
        public readonly float Level;

        public BarBox(PictureBox box, float level, Control parent)
        {
            this.Box = box;
            this.Level = level;
            parent.Controls.Add(this.Box);
        }

        public bool isTooLarge(float percent)
        {
            return Level > percent; 
        }

        public bool isTooSmall(float percent)
        {
            return Level < percent;
        }

    }
}