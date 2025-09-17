using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Subscriber
{
    public class TransferStartedObserver
    {
        public void OnTransferStarted(object sender, EventArgs e)
        {
            Console.WriteLine("Transfer started");
        }
    }
}
