using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Subscriber
{
    public class TransferCompletedObserver
    {
        public void OnTransferCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("Transfer completed");
        }
    }
}
