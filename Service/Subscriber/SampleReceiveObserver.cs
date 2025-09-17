using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Subscriber
{
    public class SampleReceiveObserver
    {
        public void OnSampleReceived(object sender, EventArgs e)
        {
            Console.WriteLine("Sample received");
        }
    }
}
