using diskretni_mapd_simulace.Entities;
using Org.BouncyCastle.Asn1.X509;
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
        public List<Agent> agents = new List<Agent>();
        public Scenario scenario = new Scenario();

        public Location[][] locationMap = new Location[0][];

        public string selectedAlgo = "TP"; // default
        public List<string> algorithms = new List<string> { "TP", "Greedy", "TPTS", "CENTRAL_ASTAR", "CENTRAL_CBS",};

        public List<Order> orders = new List<Order>();
        public int[] gridSize = new int[2]; // najit neco na praci se souradnicema

        public string outputFile = "plans/plan.txt";
        public string mapName = "Room";
        public string scenarioName = "";

        public Plan currentPlan = new Plan();

        public bool LoadScenario(int numberOfOrders)
        {
            if (numberOfOrders > scenario.orders.Count) return false;
            //resets the orders and set a new ones
            foreach (var o in orders)
            {
                o.currLocation.orders.Clear();
                o.targetLocation.orders.Clear();
            }
            orders.Clear();

            for (int i = 0; i < numberOfOrders; i++)
            {
                Order o = scenario.orders[i];
                orders.Add(o);
                o.currLocation.orders.Add(o);
                o.idealServiceTime = Database.getDistance(o.currLocation, o.targetLocation);
            }

            
            agents.Clear();
            foreach (var a in scenario.agents)
            {
                agents.Add((Agent)a.Clone());
            }
            return true;
        }


        public void clearOrders()
        {
            foreach (var o in orders)
            {
                o.currLocation.orders.Clear();
                o.targetLocation.orders.Clear();
            }
            orders.Clear();
            currentPlan = new Plan();
        }

        public static int getDistance(Location loc1, Location loc2)
        {
            return Math.Abs(loc1.coordination[0] - loc2.coordination[0]) + Math.Abs(loc1.coordination[1] - loc2.coordination[1]);
            if (loc1.locationDistanceValue.ContainsKey(loc2)) return loc1.locationDistanceValue[loc2];
            else if (loc1.id == Location.mockLocationId || loc2.id == Location.mockLocationId) return int.MaxValue; // Mock location
            else
            {
                findDistance(loc1, loc2);
            }
            return loc1.locationDistanceValue[loc2];
        }

        public void setFrequencies(double freq)
        {
            double counter = 0;
            int time = 1;
            for (int i = 0; i < scenario.orders.Count; i++)
            {
                if (freq < 1)
                {
                    while (counter < 1)
                    {
                        time++;
                        counter += freq;
                    }
                    scenario.orders[i].timeFrom = time;
                    counter = 0;
                }
                else
                {
                    if (counter < freq)
                    {
                        counter++;
                    }
                    else
                    {
                        time++;
                        counter = 1;
                    }
                    scenario.orders[i].timeFrom = time;
                }
            }
        }

        public void generateAgents(int numOfAgents, int seed)
        {
            Random r = new Random(seed);
            List<int> used = new List<int>();
            scenario.agents.Clear();
            for (int i = 0; i < numOfAgents; i++)
            {
                bool validLocation = false;
                while (validLocation == false)
                {
                    int rInt = r.Next(0, locations.Count);
                    if (locations[rInt].type == (int)Location.types.free && scenario.orders.Where(x => x.currLocation.id == locations[rInt].id).ToList().Count == 0 && used.Contains(rInt) == false)
                    {
                        Console.WriteLine($"Baselocation: {locations[rInt].coordination[0]}-{locations[rInt].coordination[1]}\n");
                        validLocation = true;
                        used.Add(rInt);
                        scenario.agents.Add(new Agent { id = i.ToString(), baseLocation = locations[rInt], currentLocation = locations[rInt] });
                    }
                }
            }
        }
        private static void findDistance(Location loc1, Location loc2)
        {
            List<(Location, int)> locationList = new List<(Location, int)>();
            List<int> closedLocationIds = new List<int>();
            Location current = loc1;
            int time;

            locationList.Add((loc1, 0));
            closedLocationIds.Add(loc1.id);

            while (locationList.Count > 0)
            {
                //locationList = locationList.OrderBy(l => l.Item2).ToList();
                (current, time) = locationList[0];
                locationList.RemoveAt(0);

                if (current.id == loc2.id)
                {
                    loc1.locationDistanceValue[loc2] = time;
                    loc2.locationDistanceValue[loc1] = time;
                    return;
                }

                var adjacentSquares = current.accessibleLocations;

                foreach (var neigh in adjacentSquares)
                {
                   if (closedLocationIds.Contains(neigh.id) == false)
                    {
                        locationList.Add((neigh, time + 1));
                        closedLocationIds.Add(neigh.id);
                    }
                }
            }
        }

        private void precountAccessibleLocations(Location l)
        {
            if (l.type == (int)Location.types.free)
            {
                List<Location> neigLocations = new List<Location>();
                int[] coordinates = l.coordination;

                //check for ap boundaries and location type
                if (coordinates[1] + 1 < locationMap[0].Length)
                {
                    if (locationMap[coordinates[0]][coordinates[1] + 1] != null && locationMap[coordinates[0]][coordinates[1] + 1].type == (int)Location.types.free)
                    {
                        l.accessibleLocations.Add(locationMap[coordinates[0]][coordinates[1] + 1]);
                    }
                }

                if (coordinates[1] - 1 >= 0)
                {
                    if (locationMap[coordinates[0]][coordinates[1] - 1] != null && locationMap[coordinates[0]][coordinates[1] - 1].type == (int)Location.types.free)
                    {
                        l.accessibleLocations.Add(locationMap[coordinates[0]][coordinates[1] - 1]);
                    }
                }

                if (coordinates[0] + 1 < locationMap.GetLength(0))
                {
                    if (locationMap[coordinates[0] + 1][coordinates[1]] != null && locationMap[coordinates[0] + 1][coordinates[1]].type == (int)Location.types.free)
                    {
                        l.accessibleLocations.Add(locationMap[coordinates[0] + 1][coordinates[1]]);
                    }
                }

                if (coordinates[0] - 1 >= 0)
                {
                    if (locationMap[coordinates[0] - 1][coordinates[1]] != null && locationMap[coordinates[0] - 1][coordinates[1]].type == (int)Location.types.free)
                    {
                        l.accessibleLocations.Add(locationMap[coordinates[0] - 1][coordinates[1]]);
                    }
                }
            }
        }

        public void clearScenario()
        {
            foreach (var l in locations)
            {
                l.orders.Clear();
            }
            foreach (var a in agents)
            {
                a.baseLocation.agents.Clear();
            }
            agents.Clear();
            orders.Clear();
            scenario = new Scenario();
            currentPlan = new Plan();
        }

        public void clearMap()
        {
            clearScenario();
            locationMap = new Location[0][];
            locations.Clear();
        }

        public void loadMap(int rows, int cols)
        {
            setLocationMap(rows, cols);
        }

        public  void createPassages()
        {
            int id = 0;
            foreach (Location l in locations)
            {
                foreach (Location neighbour in l.accessibleLocations)
                {
                    bool exists = false;
                    foreach (Passage p in l.passages)
                    {
                        if (p.a == neighbour || p.b == neighbour) exists = true;
                    }
                    if (!exists)
                    {
                        Passage p = new Passage { a = l, b = neighbour, Id = id++ };
                        l.passages.Add(p);
                        neighbour.passages.Add(p);
                    }
                }
            }
        }

        public void setLocationMap(int rows, int cols)
        {
            int mapSize = rows+1;

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
            foreach (var location in locations)
            {
                precountAccessibleLocations(location);
            }

            createPassages(locations);
        }

        public static void createPassages(List<Location> locations)
        {
            int id = 0;
            foreach (Location l in locations)
            {
                foreach (Location neighbour in l.accessibleLocations)
                {
                    bool exists = false;
                    foreach (Passage p in l.passages)
                    {
                        if (p.a == neighbour || p.b == neighbour) exists = true;
                    }
                    if (!exists)
                    {
                        Passage p = new Passage { a = l, b = neighbour, Id = id++ };
                        l.passages.Add(p);
                        neighbour.passages.Add(p);
                    }
                }
            }
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

        public Order getOrderById(string Id)
        {
            foreach (Order o in orders)
            {
                if (o.id == Id) return o;
            }
            return null;
        }

        public Agent getAgentById(string id)
        {
            foreach (Agent v in agents)
            {
                if (v.id == id) return v;
            }
            return null;
        }

        public int getNumOfDeliveredOrders(List<Agent> agents)
        {
            int count = 0;

            foreach (var a in agents)
            {
                foreach (var o in a.orders)
                {
                    if (o.state == (int)Order.states.delivered) count++;
                }
            }
            return count;
        }

        public int getNumOfNonDeliveredOrders(List<Agent> agents)
        {
            int count = 0;
            foreach (var a in agents)
            {
                foreach (var o in a.orders)
                {
                    if (o.state != (int)Order.states.delivered) count++;
                }
            }
            return count;
        }
    }
}
