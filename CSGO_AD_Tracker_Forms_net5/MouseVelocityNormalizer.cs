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

        private const int IntervalMs = 10;
        private readonly Timer processEventTimer = new(IntervalMs);

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
            int sum = 0;
            int len = historicalMouseEventData.Count;
            // Go over the nodes until the start time node's tick value is greater than the equation below. This is:
            //  The interval in ticks divided by the number of batches, or the number of ticks per batch
            //  Then, this is multiplied by i to get the number of ticks from start this batch should go to
            //  Finally, this is offset by startTime to get the correct max value of this batch.
            // These nodes are then dumped into sum and then removed from the list, and the current node is incremented.
            LinkedListNode<MouseEventData> currentNode = historicalMouseEventData.First;
            while (currentNode?.Next != null && currentNode != historicalMouseEventData.Last)
            {
                sum += (currentNode.Value.movementEntry + currentNode.Next.Value.movementEntry) / 2;
                currentNode = currentNode.Next;
            }

            historicalMouseEventData.Clear();

            bool[] keyStatuses = KeyboardData.Instance.GetKeyStatuses;
            int vel = sum / len;
            Console.WriteLine($"Sum: {sum} Len: {len} Vel: {vel}");

            // Good
            switch (vel)
            {
                // Moving left
                case > 0:
                    // A is pressed
                    if (keyStatuses[1])
                    {
                    }
                    else
                    {
                        // Console.WriteLine("BAD SURFER A");
                    }

                    break;
                // Moving right
                case < 0:
                    if (keyStatuses[3])
                    {
                    }
                    else
                    {
                        // Console.WriteLine("BAD SURFER D");
                    }

                    break;
            }

            OnPointAdd?.Invoke(this, new AddPointArgs(vel));
        }
    }
}