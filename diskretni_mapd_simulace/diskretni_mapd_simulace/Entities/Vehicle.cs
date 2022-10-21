using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Vehicle
    {
        public string id;
        public byte[] color;
        public int solverLocationIndex;
        public Location baseLocation;
        public Location targetLocation;
        public List<Order> orders = new List<Order>();

        public void display()
        {

        }
    }
}
