using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Entities
{
    public class Token
    {
        public List<Order> orders = new List<Order>();
        public List<Order> inProcessOrders = new List<Order>();
        public List<Order> deliveredOrders = new List<Order>();
        public List<Location> locations = new List<Location>();
        public Location[][] locationMap = new Location[0][];

        public List<Order> swapControlList = new List<Order>();

        public Dictionary<Order, Agent> agentOrderDict = new Dictionary<Order, Agent>();

        public List<Agent> agents = new List<Agent>();
        public int time = 0;
    }
}
