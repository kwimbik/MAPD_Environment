using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class GreedyAlg
    {
        public static Plan run(Database db)
        {
            Plan plan = new Plan();
            foreach (Agent a in db.agents)
            {
                foreach (Order o in a.orders)
                {
                    List<Location> loc = getPathForAgentAndOrder(a, o, db);
                    plan.value = formatPlan(a, loc);
                }
            }
            return plan;
        }

        private static string formatPlan(Agent a, List<Location> locations)
        {
            string newPlan = "";
            string agent = "A";
            string agentId = a.id;
            int timeCounter = 0;

            for (int i = locations.Count -1 ; i >= 0; i--)
            {
                newPlan += $"{timeCounter++}-{agent}-{agentId}-{locations[i].id}\n";
            }
            return newPlan;
        }


        private static List<Location> getPathForAgentAndOrder(Agent a, Order o, Database db)
        {
            int g = 0;
            int h = 0;
            var openList = new List<Location>();
            var closedList = new List<Location>();
            Location start = a.baseLocation;
            Location target1 = o.currLocation;
            Location target2 = o.targetLocation;
            Location current = start;
            List<Location> route = new List<Location>();    

            openList.Add(start);

            while (openList.Count > 0)
            {
                // get the square with the lowest F score
                var lowest = openList.Min(l => l.f);
                current = openList.First(l => l.f == lowest);

                // add the current square to the closed list
                closedList.Add(current);

                // remove it from the open list
                openList.Remove(current);

                if (current.id == target1.id)
                {
                    target1.id = current.id;
                    break;
                }
                var adjacentSquares = getAccessibleLocations(current, db);
                g++;

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it
                    if ( closedList.Contains(adjacentSquare)) continue;


                    // if it's not in the open list...
                    if (openList.Contains(adjacentSquare) == false)
                    {
                        // compute its score, set the parent
                        adjacentSquare.g = g;
                        adjacentSquare.h = Location.getDistance(target1, adjacentSquare);
                        adjacentSquare.f = adjacentSquare.g + adjacentSquare.h;
                        adjacentSquare.Parent = current;

                        // and add it to the open list
                        openList.Insert(0, adjacentSquare);
                    }
                    else
                    {
                        // test if using the current G score makes the adjacent square's F score
                        // lower, if yes update the parent because it means it's a better path
                        if (g + adjacentSquare.h < adjacentSquare.f)
                        {
                            adjacentSquare.g = g;
                            adjacentSquare.f = adjacentSquare.g + adjacentSquare.h;
                            adjacentSquare.Parent = current;
                        }
                    }
                }
            }
            while (current != null)
            {
                route.Add(current);
                current = current.Parent;
                if (current != null && current.id == start.id) current = null;
            }
            return route;
        }

        private static List<Location> getAccessibleLocations(Location l, Database db)
        {
            //TODO: more elegant way of returning access. positions
            List<Location> locations = new List<Location>();
            int[] coordinates = l.coordination;

            //check for ap boundaries and location type
            if (coordinates[1] + 1 < db.locationMap[0].Length)
            {
                if (db.locationMap[coordinates[0]][coordinates[1] + 1] != null && db.locationMap[coordinates[0]][coordinates[1] + 1].type == (int)Location.types.free)
                {
                    locations.Add(db.locationMap[coordinates[0]][coordinates[1] + 1]);
                }
            }

            if (coordinates[1] - 1 >= 0) 
            {
                if (db.locationMap[coordinates[0]][coordinates[1] - 1] != null && db.locationMap[coordinates[0]][coordinates[1] - 1].type == (int)Location.types.free)
                {
                    locations.Add(db.locationMap[coordinates[0]][coordinates[1] - 1]);
                }
            }

            if (coordinates[0] + 1 < db.locationMap.GetLength(0))
            {
                if (db.locationMap[coordinates[0] + 1][coordinates[1]] != null && db.locationMap[coordinates[0] + 1][coordinates[1]].type == (int)Location.types.free)
                {
                    locations.Add(db.locationMap[coordinates[0] + 1][coordinates[1]]);
                }
            }

            if (coordinates[0] - 1 >= 0)
            {
                if (db.locationMap[coordinates[0] - 1][coordinates[1]] != null && db.locationMap[coordinates[0] - 1][coordinates[1]].type == (int)Location.types.free)
                {
                    locations.Add(db.locationMap[coordinates[0] - 1][coordinates[1]]);
                }

            }
            return locations;
        }
    }
}
