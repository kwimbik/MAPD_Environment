using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    internal class Routing_solverManager
    {
        Database db;

        public Routing_solverManager(Database db)
        {
            this.db = db;
        }

        public Dictionary<Location, int> locationToIndexMap = new Dictionary<Location, int>();
        int locationToIndexCounter = 0;
        public long[][] TimeMatrix = new long[][] {
            new long []{ 0, 0, 0,  0, 0, 0, 4},
            new long []{ 0, 0, 8,  3, 2, 1, 4},
            new long []{ 0, 8, 0, 11, 1, 2, 4},
            new long []{ 0, 3, 11, 0, 2, 3, 4},
            new long []{ 0, 2, 1, 2, 0,  4, 4},
            new long []{ 0, 1, 2, 3, 4,  0, 4},
            new long []{ 0, 4, 4, 4, 4,  4, 0},
            };
        public int[][] PickupsDeliveries =  new int[][] {
                new int[] { 1, 2 },
                new int[] { 4, 3 },
                new int[] { 5, 6 },
            };
        public long[][] TimeWindows = new long[][] {
            new long[]{ 0, 30},   // depot
            new long[]{ 0, 30 },  // 1
            new long[]{ 0, 30 },// 2
            new long[]{ 0, 30 }, // 3
            new long[]{ 0, 30 }, //4
            new long[]{ 0, 30 }, //4
            new long[]{ 0, 30 } }; //4;

        public long[] Demands = { 0, 8, -8, -8, 8, 5, -5 }; //default value
        public long[] VehicleCapacities = { 12, 12, 12 }; //default value
        public int VehicleNumber = 3; //default value

        //depot values must be different from any pickup or delivery location of an order
        public int[] Depot = { 0, 0 ,0}; //default value of depot will be all zeros

        public void getSolutionData()
        {
            locationToIndexCounter = 0;
            getTimeMatrix();
            getPickupsAndDeliveries();
            getVehicleNumber();
            getTimeWindows();
            getDemands();
            getDepot();
        }

        //TODO: nechci okno za vsechny lokace, jen ty uzitecne
        public void getTimeMatrix()
        {
            long[][] tw = new long[db.locations.Count][];
            for (int i = 0; i < db.locations.Count; i++)
            {
                tw[i] = new long[] { 0, 30 };
            }
            this.TimeWindows = tw;
        }


        public void getPickupsAndDeliveries()
        {
            int[][] pickupsAndDeliveries = new int[db.orders.Count][];
            for (int i = 0; i < db.orders.Count; i++)
            {
                Order order = db.orders[i];
                locationToIndexMap.Add(order.currLocation, locationToIndexCounter++);
                locationToIndexMap.Add(order.targetLocation, locationToIndexCounter++);
                pickupsAndDeliveries[i] = new int[] { locationToIndexMap[order.currLocation], locationToIndexMap[order.targetLocation]};
            }
            this.PickupsDeliveries = pickupsAndDeliveries;
        }

        public void getTimeWindows()
        {
            return;
        }

        public void getDemands()
        {
            return;
        }

        public void getCapacities()
        {
            VehicleCapacities = new long[db.vehicles.Count];
            for (int i = 0; i < db.vehicles.Count; i++)
            {
                VehicleCapacities[i] = 1;
            }
        }

        public void getVehicleNumber()
        {
            VehicleNumber =  db.vehicles.Count;
            for (int i = 0; i < db.vehicles.Count; i++)
            {
                locationToIndexMap.Add(db.vehicles[i].baseLocation, locationToIndexCounter++);
            }
            
        }
        public void getDepot()
        {
            int[] depot = new int[db.vehicles.Count];
            for (int i = 0; i < depot.Length; i++)
            {
                depot[i] = locationToIndexMap[db.vehicles[i].baseLocation];
            }
            this.Depot = depot;
        }
    }

}
