using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Subscriber
{
    public class OutOfBandWarningObserver
    {
        public void OnOutOfBandWarning(object sender, WarningEventArguments e)
        {
            Console.WriteLine($"Out of band warning detected with direction {e.Direction} expected mean.");
        }
    }
}
