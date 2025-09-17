using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Subscriber
{
    public class AccelerationSpikeObserver
    {
        public void OnAccelerationSpike(object sender, WarningEventArguments e)
        {
            Console.WriteLine($"Acceleration spike detected, direction {e.Direction} threshold.");
        }
    }
}
