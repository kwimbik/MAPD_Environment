using diskretni_mapd_simulace.Entities;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
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
            if (l.id == Location.mockLocationId)
            {
                var adjacentSquares = getAccessableLocations(baseLocation, t.time + 1);
                if (adjacentSquares.Count > 0)
                {
                    if (adjacentSquares[0].accessibleLocations.Count < 4)
                    {
                        adjacentSquares = adjacentSquares.OrderByDescending(x => x.accessibleLocations.Count).ToList();
                    }
                    return new List<Location> { adjacentSquares[0] };
                }
                else return new List<Location>();

            }
            int g = 0;
            int h = 0;
            var openList = new List<Location>();
            var closedList = new bool[t.locations.Count];
            var openListCheck = new bool[t.locations.Count];
            Location start = baseLocation;
            Location target = l;
            Location current = start;
            List<Location> route = new List<Location>();

            openList.Add(start);
            openListCheck[start.id] = true;
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
                if (closedList[current.id] == true)
                {
                    simulationTime++;
                    continue;
                }
                else
                {
                    closedList[current.id] = true;
                    simulationTime = current.entranceTime + 1;
                }

                // remove it from the open list
                openList.Remove(current);
                openListCheck[current.id] = false;

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
                    if (closedList[adjacentSquare.id] == true) continue;


                    // if it's not in the open list...
                    if (openListCheck[adjacentSquare.id] == false)
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
                        openListCheck[adjacentSquare.id] = true;
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

            if (current.id != target.id)
            {
                return new List<Location> { findClosestAvailableLocation(baseLocation, t.time + 1) };
            }

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

        public static Location findClosestAvailableLocation(Location l, int time)
        {
            //search through all neighbours to find one such that is is not occupied next turn
            foreach (var loc in l.accessibleLocations)
            {
                if (loc.occupiedAt.Contains(time) == false) return loc;
            }
            return new Location();
        }

        public static double getAverageServiceTimeDelay(List<Order> orders)
        {
            double ideal_serviceTimes = 0;
            double real_serviceTimes = 0;
            foreach (var o in orders)
            {
                ideal_serviceTimes += o.idealServiceTime;
                real_serviceTimes += o.realServiceTime;
            }
            ideal_serviceTimes /= orders.Count;
            real_serviceTimes /= orders.Count;
            return ideal_serviceTimes / real_serviceTimes;
        }

        public static List<Location> getAccessableLocations(Location l, int t)
        {
            //here is time t, not t+1 because the A* simulation increases the time counter right after selecting location
            List<Location> locations = new List<Location>();
            if (l.occupiedAt.Contains(t) == false) locations.Add(l);
            foreach (var nei in l.accessibleLocations)
            {
                Passage p = Location.getPassageFromLocation(l, nei);
                if (p.occupied.Contains(t) == false && nei.occupiedAt.Contains(t) == false) locations.Add(nei);
            }
            return locations;
        }
    }
}
