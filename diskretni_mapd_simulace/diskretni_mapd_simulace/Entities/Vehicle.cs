using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Agent
    {
        public string id;
        public byte[] color;
        public int solverLocationIndex;
        public Location baseLocation;
        public Location targetLocation;
        public List<Order> orders = new List<Order>();
        public List<Order> assignedOrders = new List<Order>();
        
        public Plan plan { get; set; }

        public void display()
        {

        }
    }
}
