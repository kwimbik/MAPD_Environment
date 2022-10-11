﻿using System;
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

        public Location[][] locationMap;

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

        public void setTestData()
        {
            Order o1 = new Order() { Id = "1", currLocation = locations[0], targetLocation = locations[20] };
            Order o2 = new Order() { Id = "2", currLocation = locations[5], targetLocation = locations[11] };
            Order o3 = new Order { Id = "3", currLocation = locations[48], targetLocation = locations[99] };
            orders.Add(o1);
            orders.Add(o2);
            orders.Add(o3);
            locations[0].orders.Add(o1);
            locations[5].orders.Add(o2);
            locations[48].orders.Add(o3);
            vehicles.Add(new Vehicle() { Id ="1",  baseLocation = locations[32] });
            vehicles.Add(new Vehicle() { Id ="2", baseLocation = locations[32] });
        }

        public void setLocationMap()
        {
            //Only square gridns, might change later TODO
            int mapSize = (int)Math.Round(Math.Sqrt(locations.Count()));

            //init the map
            locationMap = new Location[mapSize][];
            for (int i = 0; i < mapSize; i++)
            {
                locationMap[i] = new Location[mapSize];
            }

            foreach (Location location in locations)
            {
                locationMap[location.coordination[0]][location.coordination[1]] = location;
            }
            Console.WriteLine("done");
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
