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

        public List<Location> usedLocations = new List<Location>();
        //Map from location/order to index in solver
        //List of int to enable duplicate locations (more than one vehicle or order at one place)
        //lists:
        //1: baseLocationOrders, 2: targetLocationOrders 3:baseLocationVehicles
        public Dictionary<Location, List<List<int>>> locationToIndexMap = new Dictionary<Location, List<List<int>>>();
        public const int baseLocationOrders = 0;
        public const int targetLocationOrders = 1;
        public const int baseLocationAgents = 2;


        public List<Order> ordersToProcess = new List<Order>();
        public bool freeAgents = false;

        public Dictionary<int, Location> indexToLocationMap = new Dictionary<int, Location>();

        //map from agent to index in solver
        public Dictionary<Agent, int> agentToIndexMap =  new Dictionary<Agent, int>();
        public Dictionary<int, Agent> indexToAgentMap = new Dictionary<int, Agent>();



        //TODO: zajistit, aby slo poznat ktere loakce jsou pro auta a ktere pro objednavky
        //TJ podle inexu musi byt jasne jestli je to auto nebo objednavka v te lokaci
        //jinak zlobi demans a depot
        int agentToIndexCounter = 0;
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
        public long[] AgentCapacities = { 12, 12, 12 }; //default value
        public int AgentNumber = 3; //default value

        //depot values must be different from any pickup or delivery location of an order
        public int[] Depot = { 0, 0 ,0}; //default value of depot will be all zeros

        public void getSolutionData()
        {
            locationToIndexCounter = 0;
            getPickupsAndDeliveries();
            getAgentNumber();
            getDemands();
            getCapacities();
            getDepot();
            getTimeWindows();
            getTimeMatrix();
            freeAgentssFind();
        }

        public void ResetSettings()
        {
            locationToIndexMap.Clear();
            indexToLocationMap.Clear();
            agentToIndexMap.Clear();
            indexToAgentMap.Clear();
            agentToIndexCounter = 0;
            locationToIndexCounter = 0;
        }

        public void freeAgentssFind()
        {
            foreach (Agent agent in db.agents)
            {
                if (agent.targetLocation == null)
                {
                    freeAgents = true;
                    return;
                }
            }
        }

        //TODO: vrati vzdalenosti v jednotkach
        public void getTimeMatrix()
        {
            
            int numOfLocation = locationToIndexCounter;
            long[][] timeMatrix = new long[numOfLocation][];
            for (int i = 0; i < locationToIndexCounter; i++)
            {
                timeMatrix[i] = new long[locationToIndexCounter];
                for (int j = 0; j < locationToIndexCounter; j++)
                {
                    timeMatrix[i][j] = Location.getDistance(indexToLocationMap[i], indexToLocationMap[j]);
                }
            }
            this.TimeMatrix = timeMatrix;
        }

        //get pickup and delivery pairs, assign each start and target location unique index for this problem
        public void getPickupsAndDeliveries()
        {
            //get all the orders to process
            ordersToProcess = db.getOrdersToProcess();

            int[][] pickupsAndDeliveries = new int[ordersToProcess.Count][];

            for (int i = 0; i < ordersToProcess.Count; i++)
            {
                Order order = ordersToProcess[i];
                //adds map for current location 
                indexToLocationMap.Add(locationToIndexCounter, order.currLocation);

                //adds location to used ones
                if (usedLocations.Contains(order.currLocation ) == false) usedLocations.Add(order.currLocation);

                if (locationToIndexMap.ContainsKey(order.currLocation) == false)
                {
                    locationToIndexMap.Add(order.currLocation, new List<List<int>> { new List<int> { locationToIndexCounter++ }, new List<int>(), new List<int>() } );

                }
                else locationToIndexMap[order.currLocation][baseLocationOrders].Add(locationToIndexCounter++);

                //add map for target location
                indexToLocationMap.Add(locationToIndexCounter, order.targetLocation);

                //adds location to used ones
                if (usedLocations.Contains(order.targetLocation) == false) usedLocations.Add(order.targetLocation);


                if (locationToIndexMap.ContainsKey(order.targetLocation) == false)
                {
                    locationToIndexMap.Add(order.targetLocation, new List<List<int>> { new List<int>() , new List<int>  { locationToIndexCounter++ }, new List<int>() });

                }
                else locationToIndexMap[order.targetLocation][targetLocationOrders].Add(locationToIndexCounter++);

                int baseIndex = locationToIndexMap[order.currLocation][baseLocationOrders].Count() - 1;
                int targetIndex = locationToIndexMap[order.targetLocation][targetLocationOrders].Count() - 1;
                pickupsAndDeliveries[i] = new int[] { locationToIndexMap[order.currLocation][baseLocationOrders][baseIndex], locationToIndexMap[order.targetLocation][targetLocationOrders][targetIndex] };
            }
            this.PickupsDeliveries = pickupsAndDeliveries;
        }

        //gets time windows for orders to be delivered in
        public void getTimeWindows()
        {
            long[][] tw = new long[ordersToProcess.Count][];
            for (int i = 0; i < ordersToProcess.Count; i++)
            {
                tw[i] = new long[] { 0, db.locations.Count }; //for now, time windows are arbitrary large
            }
            this.TimeWindows = tw;
        }

        //gets size of orders, base = 1, on target locations -1, on depots 0
        public void getDemands()
        {

            long[] demands = new long[locationToIndexCounter];
            foreach (Location location in usedLocations)
            {
                //base locations -> value is one
                foreach (int index in locationToIndexMap[location][baseLocationOrders])
                {
                    demands[index] = 1;
                }

                //target locations -> value is minus one
                foreach (int index in locationToIndexMap[location][targetLocationOrders])
                {
                    demands[index] = -1;
                }

                //base locations  agent -> value is zero
                foreach (int index in locationToIndexMap[location][baseLocationAgents])
                {
                    demands[index] = 0;
                }
            }
            this.Demands = demands;
        }

        //gets capacities of agents, base = 1
        public void getCapacities()
        {
            long[] agentCapacities = new long[db.agents.Count];
            for (int i = 0; i < db.agents.Count; i++)
            {
                agentCapacities[i] = 1;
            }
            this.AgentCapacities = agentCapacities;
        }


        //get number of agents and assign each location of agent unique index for this problem and assign each vehicle unique number
        public void getAgentNumber()
        {
            AgentNumber =  db.agents.Count;
            for (int i = 0; i < db.agents.Count; i++)
            {
                agentToIndexMap.Add(db.agents[i], agentToIndexCounter);
                indexToAgentMap.Add(agentToIndexCounter++, db.agents[i]);

                indexToLocationMap.Add(locationToIndexCounter, db.agents[i].baseLocation);

                //add location to used ones
                if (usedLocations.Contains(db.agents[i].baseLocation) == false) usedLocations.Add(db.agents[i].baseLocation);

                //assign solver location index
                db.agents[i].solverLocationIndex = locationToIndexCounter;

                if (locationToIndexMap.ContainsKey(db.agents[i].baseLocation) == false)
                {
                    locationToIndexMap.Add(db.agents[i].baseLocation, new List<List<int>> { new List<int>(), new List<int>(), new List<int> { locationToIndexCounter++ } });
                }
                else locationToIndexMap[db.agents[i].baseLocation][baseLocationAgents].Append(locationToIndexCounter++);
            }

        }

        //Gets starting position for all agents base on index of location for current problem
        public void getDepot()
        {
            int depotLength = 0;
            foreach (Agent agent in db.agents)
            {
                depotLength += 1;
            }

            int[] depot = new int[depotLength];
            for (int i = 0; i < depot.Length; i++)
            {
                //depot is selected as first index of location, distance to all other indexes will be zero
                depot[i] = db.agents[i].solverLocationIndex;
            }
            this.Depot = depot;
        }
    }

}
