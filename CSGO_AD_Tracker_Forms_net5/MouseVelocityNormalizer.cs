using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Microsoft.VisualBasic.Devices;

namespace CSGO_AD_Tracker_Forms_net5
{
    public delegate void AddPointHandler(object source, AddPointArgs e);

    public class AddPointArgs : EventArgs
    {
        private int sum;

        public AddPointArgs(int sum)
        {
            this.sum = sum;
        }

        public int GetSum()
        {
            return sum;
        }
    }

    public struct MouseEventData
    {
        public int movementEntry;

        public long ticks;

        public MouseEventData(int movementEntry)
        {
            ticks = DateTime.Now.Ticks;
            this.movementEntry = movementEntry;
        }

        public MouseEventData(int movementEntry, long ticks)
        {
            this.ticks = ticks;
            this.movementEntry = movementEntry;
        }

        public override string ToString()
        {
            return movementEntry.ToString();
        }
    }

    public sealed class MouseVelocityNormalizer
    {
        private static readonly Lazy<MouseVelocityNormalizer>
            Lazy = new(() => new MouseVelocityNormalizer());

        public static MouseVelocityNormalizer Instance => Lazy.Value;

        private LinkedList<MouseEventData> historicalMouseEventData = new();

        private const int IntervalMs = 20;
        private const long IntervalTicks = IntervalMs * 10000;
        private Timer processEventTimer = new(IntervalMs);

        public event AddPointHandler OnPointAdd;

        private MouseVelocityNormalizer()
        {
            processEventTimer.AutoReset = true;
            processEventTimer.Elapsed += processBatch;
            processEventTimer.Enabled = true;
        }

        public void AddMouseEventData(MouseEventData data)
        {
            historicalMouseEventData.AddFirst(data);
        }

        private void processBatch(Object source, ElapsedEventArgs e)
        {
            long startTime;
            if (historicalMouseEventData.Last != null) startTime = historicalMouseEventData.Last.Value.ticks;
            else
            {
                OnPointAdd?.Invoke(this, new AddPointArgs(0));
                return;
            }

            LinkedListNode<MouseEventData> currentNode = historicalMouseEventData.Last;
            int sum = 0;
            // Go over the nodes until the start time node's tick value is greater than the equation below. This is:
            //  The interval in ticks divided by the number of batches, or the number of ticks per batch
            //  Then, this is multiplied by i to get the number of ticks from start this batch should go to
            //  Finally, this is offset by startTime to get the correct max value of this batch.
            // These nodes are then dumped into sum and then removed from the list, and the current node is incremented.
            long intervalEndTimeTicks = startTime + IntervalTicks;
            while (currentNode != null && currentNode.Value.ticks <= intervalEndTimeTicks)
            {
                sum = currentNode.Value.movementEntry + sum;
                historicalMouseEventData.RemoveLast();
                currentNode = historicalMouseEventData.Last;
            }

            OnPointAdd?.Invoke(this, new AddPointArgs(sum));
        }
    }
}