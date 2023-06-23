using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Order
    {
        public string id { get; set; }
        public Location currLocation;
        public Location targetLocation;
        public int state;
        public byte[] color;

        public int timeFrom;
        public int timeTo = 1000000;

        public int priority { get; set; }

        public string colorBox = "";

        public int idealServiceTime = 0;
        public int realServiceTime = 0;


        public enum states
        {
            pending = 0,
            processed = 1,
            delivered = 2,
            targeted = 3,
        }

    }
}
