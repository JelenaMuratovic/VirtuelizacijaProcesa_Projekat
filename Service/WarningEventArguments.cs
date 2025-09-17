using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class WarningEventArguments : EventArgs
    {
        public string Direction {  get; set; }

        public WarningEventArguments(string direction)
        {
            Direction = direction;
        }
    }
}
