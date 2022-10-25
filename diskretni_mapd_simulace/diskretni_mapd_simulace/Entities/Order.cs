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

        public int priority { get; set; }


        public enum states
        {
            pending,
            processed,
            delivered
        }

    }
}
