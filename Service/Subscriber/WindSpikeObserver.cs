using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Subscriber
{
    public class WindSpikeObserver
    {
        public void OnWindSpike(object sender, WarningEventArguments e)
        {
            Console.WriteLine($"Wind spike detected with direction {e.Direction} threshold");
        }
    }
}
