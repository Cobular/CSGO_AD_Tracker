using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Microsoft.VisualBasic.Devices;

namespace CSGO_AD_Tracker_Forms_net5
{
    public delegate void AdViolationHandler(object source, AdViolationArgs e);

    public delegate void VelocityChangeHandler(object source, VelocityChangeArgs e);

    public class AdViolationArgs : EventArgs
    {
        // -1 is left, 1 is right
        private readonly int _direction;

        public AdViolationArgs(int direction)
        {
            _direction = direction;
        }

        public int GetDir()
        {
            return _direction;
        }
    }
    public delegate void AddPointHandler(object source, AddPointArgs e);

    public class AddPointArgs : EventArgs
    {
        private float sum;

        public AddPointArgs(float sum)
        {
            this.sum = sum;
        }

        public float GetSum()
        {
            return sum;
        }
    }
    public class VelocityChangeArgs : EventArgs
    {
        // -1 is left, 1 is right
        private readonly int _direction;

        public VelocityChangeArgs(int direction)
        {
            _direction = direction;
        }

        public int GetDir()
        {
            return _direction;
        }
    }


    public readonly struct MouseEventData
    {
        public readonly int MovementEntry;
        
        public MouseEventData(int movementEntry)
        { 
            MovementEntry = movementEntry;
        }

        public override string ToString()
        {
            return MovementEntry.ToString();
        }
    }

    public sealed class MouseVelocityNormalizer
    {
        private static readonly Lazy<MouseVelocityNormalizer>
            Lazy = new(() => new MouseVelocityNormalizer());

        public static MouseVelocityNormalizer Instance => Lazy.Value;

        private LinkedList<MouseEventData> historicalMouseEventData = new();

        private const int IntervalMs = 1;
        private float previousVelocity = 0;
        public float Velocity = 0;
        public bool[] keyStatuses;
        private readonly Timer processEventTimer = new(IntervalMs);

        public event AddPointHandler OnPointAdd;
        public event AdViolationHandler OnADViolation;
        public event VelocityChangeHandler onVelocityChange;
        

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
                sum += (currentNode.Value.MovementEntry + currentNode.Next.Value.MovementEntry) / 2;
                currentNode = currentNode.Next;
            }

            historicalMouseEventData = new LinkedList<MouseEventData>();

            keyStatuses = KeyboardData.Instance.GetKeyStatuses;
            
            if (len != 0)
                Velocity = sum / (float) len;
            else Velocity = 0;

            if (!previousVelocity.Equals(Math.Sign(Velocity)) && (keyStatuses[1] || keyStatuses[3]))
            {
                switch (Velocity)
                {
                    case > 0:
                    // Moving left
                        onVelocityChange?.Invoke(this, new VelocityChangeArgs(-1));
                        break; 
                    // Moving right
                    case < 0:
                        onVelocityChange?.Invoke(this, new VelocityChangeArgs(1));
                        break;
                }
            }

            previousVelocity = Math.Sign(Velocity);


            if (keyStatuses[1] || keyStatuses[3])
            {
                // Good
                switch (Velocity)
                {
                    // Moving left
                    case > 0:
                        // A is not pressed
                        if (!keyStatuses[1])
                            OnADViolation?.Invoke(this, new AdViolationArgs(-1));
                        break;
                    // Moving right
                    case < 0:
                        // D is not pressed
                        if (!keyStatuses[3])
                            OnADViolation?.Invoke(this, new AdViolationArgs(1));
                        break;
                }
            }

            OnPointAdd?.Invoke(this, new AddPointArgs(Velocity));
        }
    }
}