using diskretni_mapd_simulace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class Algorithm
    {
        public static void initialize(List<Location> locations, List<Agent> agents)
        {
            foreach (var l in locations)
            {
                l.occupiedAt.Clear();
                foreach (var p in l.passages) p.occupied.Clear();
            }

            foreach (var a in agents)
            {
                a.state = (int)Agent.states.idle;
                a.orders.Clear();
            }
        }

        public static Agent getAgentById(string id, List<Agent> agents)
        {
            foreach (Agent v in agents)
            {
                if (v.id == id) return v;
            }
            return null;
        }

        public static Order getOrderByCurrLocation(int id, List<Order> orders)
        {
            foreach (Order o in orders)
            {
                if (o.currLocation.id == id) return o;
            }
            return null;
        }

        public static List<Location> getPathForAgentAndTask(Location baseLocation, Location l, Token t)
        {
            if (baseLocation.id == l.id) return new List<Location> { l };
            int g = 0;
            int h = 0;
            var openList = new List<Location>();
            var closedList = new List<Location>();
            Location start = baseLocation;
            Location target = l;
            Location current = start;
            List<Location> route = new List<Location>();

            openList.Add(start);
            int simulationTime = baseLocation.entranceTime;

            while (openList.Count > 0)
            {
                openList = openList.OrderBy(l => l.f).ToList();
                foreach (var location in openList)
                {
                    if (location.entranceTime <= simulationTime)
                    {
                        current = location;
                        break;
                    }
                }
                //current = openList[0];

                // if no avalible tile is found, wait, else continue;
                if (closedList.Contains(current))
                {
                    simulationTime++;
                    continue;
                }
                else
                {
                    closedList.Add(current);
                    simulationTime = current.entranceTime + 1;
                }

                // remove it from the open list
                openList.Remove(current);

                if (current.id == target.id)
                {
                    break;
                }

                //var adjacentSquares = current.accessibleLocations;
                var adjacentSquares = getAccessableLocations(current, simulationTime);
                g++;

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it
                    if (closedList.Contains(adjacentSquare)) continue;


                    // if it's not in the open list...
                    if (openList.Contains(adjacentSquare) == false)
                    {
                        // compute its score, set the parent
                        adjacentSquare.g = g;
                        adjacentSquare.h = Database.getDistance(target, adjacentSquare);
                        adjacentSquare.f = adjacentSquare.g + adjacentSquare.h;

                        //TODO:  dont stop if its in open list, add Parent if different. add another entrance time.
                        //for each parent check entrance time and passage validity
                        //then select passage + parent with lowest entrance time
                        adjacentSquare.Parent = current;

                        // and add it to the open list
                        openList.Insert(0, adjacentSquare);
                        adjacentSquare.entranceTime = simulationTime;
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

            if (current.id != target.id) return new List<Location>();

            while (current != null)
            {
                route.Add(current);
                current = current.Parent;
                if (current != null && current.id == start.id)
                {
                    current = null;
                }
            }

            route.Reverse();
            return route;
        }

        private static List<Location> getAccessableLocations(Location l, int t)
        {
            //here is time t, not t+1 because the A* simulation increases the time counter right after selecting location
            List<Location> locations = new List<Location>();
            foreach (var nei in l.accessibleLocations)
            {
                Passage p = Location.getPassageFromLocation(l, nei);
                if (p.occupied.Contains(t) == false && nei.occupiedAt.Contains(t) == false) locations.Add(nei);
            }
            if (l.occupiedAt.Contains(t) == false) locations.Add(l);
            return locations;
        }
    }
}
