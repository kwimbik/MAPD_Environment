using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Order
    {
        public string Id { get; set; }
        public Location currLocation;
        public Location targetLocation;
        public int state;
        public int[] color;


        public enum states
        {
            pending,
            processed,
            delivered
        }

        public void display()
        {

        }
    }
}
