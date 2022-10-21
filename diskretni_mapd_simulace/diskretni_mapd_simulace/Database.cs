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

        

        public void setTestData()
        {
            // Colors definiton, TODO: color assigner with some clever heuristic
            byte[] pink = new byte[] { 255, 192, 203 };
            byte[] red = new byte[] { 255, 0, 0 };
            byte[] green = new byte[] { 0, 255, 0 };
            byte[] blue = new byte[] { 0, 0, 255 };
            Order o1 = new Order() { id = "1", currLocation = locations[0], targetLocation = locations[20] };
            Order o2 = new Order() { id = "2", currLocation = locations[5], targetLocation = locations[11] };
            Order o3 = new Order { id = "3", currLocation = locations[48], targetLocation = locations[99] };
            Order o4 = new Order { id = "4", currLocation = locations[92], targetLocation = locations[14] };
            Order o5 = new Order { id = "5", currLocation = locations[87], targetLocation = locations[12] };
            orders.Add(o1);
            orders.Add(o2);
            orders.Add(o3);
            orders.Add(o4);
            orders.Add(o5);
            locations[0].orders.Add(o1);
            locations[5].orders.Add(o2);
            locations[48].orders.Add(o3);
            locations[92].orders.Add(o4);
            locations[87].orders.Add(o5);
            vehicles.Add(new Vehicle() { id ="1",  baseLocation = locations[32], color = pink });
            vehicles.Add(new Vehicle() { id ="2", baseLocation = locations[32], color = blue });
        }

        public void setLocationMap(int rows, int cols)
        {
            int mapSize = rows;

            //init the map
            locationMap = new Location[mapSize][];
            for (int i = 0; i < mapSize; i++)
            {
                locationMap[i] = new Location[cols];
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

        public Location getLocationByID(int Id)
        {
            foreach (Location loc in locations)
            {
                if (loc.id == Id) return loc;
            }
            return null;
        }

        public Vehicle getVehicleById(string id)
        {
            foreach (Vehicle v in vehicles)
            {
                if (v.id == id) return v;
            }
            return null;
        }
    }
}
