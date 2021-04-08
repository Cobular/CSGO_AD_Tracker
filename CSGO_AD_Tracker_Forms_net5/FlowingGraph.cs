using System;
using System.Drawing;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CSGO_AD_Tracker_Forms_net5
{
    class FlowingGraph
    {
        //Point information
        int pointArraySize;
        int pointSpread;
        LinkedList<PointF> points = new LinkedList<PointF>();

        //Debug
        Random debugRandom = new Random();
        bool debug;

        //Picturebox Info
        PictureBox box;
        Point boxPosition;
        Size boxSize;
        Color boxColor;
        Color lineColor;
        float graphX;
        float thickness;

        //Timers
        System.Timers.Timer update;

        public FlowingGraph(bool debug, Form form, Point position, Size size, Color backColor, Color lineColor,
            float penThickness, int elementCount, int pointSpread, int bufferDistance, int refreshRate)
        {
            box = new PictureBox()
            {
                Name = "box",
                Size = size,
                Location = position,
                BackColor = backColor,
            };

            form.Controls.Add(box);
            pointArraySize = elementCount;
            this.debug = debug;
            this.pointSpread = pointSpread;
            this.thickness = penThickness;
            this.lineColor = lineColor;

            graphX = box.Width + pointSpread - bufferDistance;

            points.AddFirst(new PointF(graphX, (float) ((size.Height * 0.5))));
            for (int i = 0; i < pointArraySize - 1; i++)
                addPoint(generatePoint());

            //Timer stuff for updating the graph
            update = new System.Timers.Timer(refreshRate);
            update.AutoReset = true;
            update.Elapsed += timerUpdate;
            update.Enabled = true;

            box.Paint += paintBox;
        }

        public void updateBox(Size size, Point position, Color backColor)
        {
            if (size != null)
            {
                box.Size = size;
                this.boxSize = box.Size;
            }

            if (position != null)
            {
                box.Location = position;
                this.boxPosition = new Point(box.Left, box.Top);
            }

            if (backColor != null)
            {
                box.BackColor = boxColor = backColor;
            }
        }

        public Point getBoxLocation()
        {
            return boxPosition;
        }

        public Size getBoxSize()
        {
            return boxSize;
        }

        private void addPoint(PointF point)
        {
            points.AddFirst(point);
            if (points.Count > pointArraySize)
                points.RemoveLast();
        }

        public void addPoint(float point)
        {
            points.AddFirst(new PointF(graphX, point));
            if (points.Count > pointArraySize)
                points.RemoveLast();
        }

        //private methods
        private PointF generatePoint()
        {
            return new PointF(graphX, points.Last.Value.Y + debugRandom.Next(-5, 5));
        }

        private void updateGraph()
        {
            LinkedListNode<PointF> workingNode = points.Last;

            while (workingNode != null && workingNode != points.First)
            {
                workingNode.Value = new PointF(workingNode.Value.X - pointSpread, workingNode.Value.Y);
                workingNode = workingNode.Next;
            }

            box.Invalidate();
        }

        private void timerUpdate(Object sender, ElapsedEventArgs e)
        {
            if (debug) addPoint(generatePoint());
            updateGraph();
        }

        private void paintBox(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Pen test = new Pen(lineColor) {Width = thickness};
            var workingNode = points.First;

            if (workingNode != null && workingNode.Next != null)
            {
                for (int i = 0; i < pointArraySize - 1; i++)
                {
                    g.DrawLine(test, (PointF) workingNode.Value, workingNode.Next.Value);
                    workingNode = workingNode.Next;
                }
            }

            test.Dispose();
        }
    }
}