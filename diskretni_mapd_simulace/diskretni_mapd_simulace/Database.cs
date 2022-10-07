using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Database
    {
        public List<Location> locations = new List<Location>();
        public List<Vehicle> vehicles = new List<Vehicle>();

        //TODO: return only non delivered orders but keep all in db
        public List<Order> orders = new List<Order>();
        public int[] gridSize = new int[2]; // najit neco na praci se souradnicema

        public Location getLocationByID(int Id)
        {
            foreach (Location loc in locations)
            {
                if (loc.id == Id) return loc;
            }
            return null;
        }

        public List<Order> getOrdersToProcess()
        {
            List<Order> ordersToProcess = new List<Order>();
            foreach (Order order in orders)
            {
                if (order.state != (int)Order.states.delivered) ordersToProcess.Add(order);
            }
            return ordersToProcess;
        }
    }
}
