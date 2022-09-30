using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Routing_solverManager
    {
        Database db;

        public Routing_solverManager(Database db)
        {
            this.db = db;
        }

        //Map from location/order to index in solver
        public Dictionary<Location, int> locationToIndexMap = new Dictionary<Location, int>();
        public Dictionary<int, Location> indexToLocationMap = new Dictionary<int, Location>();

        //map from vehicle to index in solver
        public Dictionary<Vehicle, int> vehicleToIndexMap =  new Dictionary<Vehicle, int>();
        public Dictionary<int, Vehicle> indexToVehicleMap = new Dictionary<int, Vehicle>();




        int vehicleToIndexCounter = 0;
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
            getPickupsAndDeliveries();
            getVehicleNumber();
            getDemands();
            getCapacities();
            getDepot();
            getTimeWindows();
            getTimeMatrix();
        }

        //TODO: vrati vzdalenosti v jednotkach
        public void getTimeMatrix()
        {
            int numOfLocation = locationToIndexMap.Count;
            long[][] timeMatrix = new long[numOfLocation][];
            for (int i = 0; i < numOfLocation; i++)
            {
                timeMatrix[i] = new long[numOfLocation];
                for (int j = 0; j < numOfLocation; j++)
                {
                    timeMatrix[i][j] = Location.getDistance(indexToLocationMap[i], indexToLocationMap[j]);
                }
            }
            this.TimeMatrix = timeMatrix;
        }

        //get pickup and delivery pairs, assign each start and target location unique index for this problem
        public void getPickupsAndDeliveries()
        {
            int[][] pickupsAndDeliveries = new int[db.orders.Count][];
            for (int i = 0; i < db.orders.Count; i++)
            {
                Order order = db.orders[i];
                //adds map for location and indexes for current problem
                indexToLocationMap.Add(locationToIndexCounter, order.currLocation);
                locationToIndexMap.Add(order.currLocation, locationToIndexCounter++);
                indexToLocationMap.Add(locationToIndexCounter, order.targetLocation);
                locationToIndexMap.Add(order.targetLocation, locationToIndexCounter++);

                pickupsAndDeliveries[i] = new int[] { locationToIndexMap[order.currLocation], locationToIndexMap[order.targetLocation]};
            }
            this.PickupsDeliveries = pickupsAndDeliveries;
        }

        //gets time windows for orders to be delivered in
        public void getTimeWindows()
        {
            long[][] tw = new long[db.orders.Count][];
            for (int i = 0; i < db.orders.Count; i++)
            {
                tw[i] = new long[] { 0, 30 };
            }
            this.TimeWindows = tw;
        }

        //gets size of orders, base = 1, on target locations -1, on depots 0
        public void getDemands()
        {
            long[] demands = new long[locationToIndexMap.Count];
            for (int i = 0; i < db.orders.Count; i++)
            {
                demands[locationToIndexMap[db.orders[i].currLocation]] = 1;
                demands[locationToIndexMap[db.orders[i].targetLocation]] = -1;
            }

            for (int i = 0; i < db.vehicles.Count; i++)
            {
                demands[locationToIndexMap[db.vehicles[i].baseLocation]] = 0;
            }
            this.Demands = demands;
        }

        //gets capacities of vehicle, base = 1
        public void getCapacities()
        {
            long[] vehicleCapacities = new long[db.vehicles.Count];
            for (int i = 0; i < db.vehicles.Count; i++)
            {
                vehicleCapacities[i] = 1;
            }
            this.VehicleCapacities = vehicleCapacities;
        }


        //get number of vehicle and assign each location of vehicle unique index for this problem and assign each vehicle unique number
        public void getVehicleNumber()
        {
            VehicleNumber =  db.vehicles.Count;
            for (int i = 0; i < db.vehicles.Count; i++)
            {
                vehicleToIndexMap.Add(db.vehicles[i], vehicleToIndexCounter);
                indexToVehicleMap.Add(vehicleToIndexCounter++, db.vehicles[i]);

                indexToLocationMap.Add(locationToIndexCounter, db.vehicles[i].baseLocation);
                locationToIndexMap.Add(db.vehicles[i].baseLocation, locationToIndexCounter++);
            }

        }

        //Gets starting position for all vehicles base on index of location for current problem
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
