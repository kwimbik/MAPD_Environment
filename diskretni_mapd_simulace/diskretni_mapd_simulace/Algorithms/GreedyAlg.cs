using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using diskretni_mapd_simulace.Entities;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class GreedyAlg
    {

        private static int timeCounter = 0;
        public static Plan run(Database db)
        {
            Plan plan = new Plan();
            
            //finds all neighbours of all locations
            foreach (Location l in db.locations)
            {
                if (l.type == (int)Location.types.free) getAccessibleLocations(l, db);
            }

            //creates passages for each locations
            createPassages(db);

            foreach (Agent a in db.agents)
            {
                Location current = a.baseLocation;
                foreach (Order o in a.orders)
                {
                    List<Location> loc1 = getPathForAgentAndOrder(current, o.currLocation, db);
                    formatPlan(a, current, loc1);
                    current = o.currLocation;
                    List<Location> loc2 = getPathForAgentAndOrder(current, o.targetLocation, db);
                    formatPlan(a, o.currLocation, loc2);
                    current = o.targetLocation;
                }

                //reset entrance time after each agent
                foreach (var loc in db.locations)
                {
                    loc.entranceTime = 0;
                }
            }
            return plan;
        }

        private static void formatPlan(Agent a,Location curr,  List<Location> locations1)
        {
            timeCounter = a.movesMade;
            string agentId = a.id;
            a.plan.agent = a;

            Location prev = curr;

            for (int i = locations1.Count -1 ; i >= 0; i--)
            {
                locations1[i].occupiedAt.Add(locations1[i].entranceTime);
                a.plan.steps.Add(new PlanStep { agentId = a.id, locationId = locations1[i].id, time = locations1[i].entranceTime, type = (int)PlanStep.types.movement });
                Passage p = prev.getPassage(locations1[i]);
                p.occupied.Add(locations1[i].entranceTime);
                prev = locations1[i];
            }
        }


        private static List<Location> getPathForAgentAndOrder(Location baseLocation, Location l, Database db)
        {
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
                //TODO: wont be needed if occupied tiles wont be opened at all at time k
                openList = openList.OrderBy(l => l.f).ToList();
                foreach (var location in openList)
                {
                    if (location.entranceTime <= simulationTime)
                    {
                        current = location;
                        break;
                    }
                }


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

                var adjacentSquares = current.accessibleLocations;
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
                        adjacentSquare.h = Location.getDistance(target, adjacentSquare);
                        adjacentSquare.f = adjacentSquare.g + adjacentSquare.h;

                        //TODO:  dont stop if its in open list, add Parent if different. add another entrance time.
                        //for each parent check entrance time and passage validity
                        //then select passage + parent with lowest entrance time
                        adjacentSquare.Parent = current;

                        // and add it to the open list
                        openList.Insert(0, adjacentSquare);
                        adjacentSquare.entranceTime = getEntranceTime(adjacentSquare, simulationTime);
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

        //TODO: move to some generic Solver class, so all algorithms have access to this fction
        private static void getAccessibleLocations(Location l, Database db)
        {
            List<Location> locations = new List<Location>();
            int[] coordinates = l.coordination;

            //check for ap boundaries and location type
            if (coordinates[1] + 1 < db.locationMap[0].Length)
            {
                if (db.locationMap[coordinates[0]][coordinates[1] + 1] != null && db.locationMap[coordinates[0]][coordinates[1] + 1].type == (int)Location.types.free)
                {
                    l.accessibleLocations.Add(db.locationMap[coordinates[0]][coordinates[1] + 1]);
                }
            }

            if (coordinates[1] - 1 >= 0) 
            {
                if (db.locationMap[coordinates[0]][coordinates[1] - 1] != null && db.locationMap[coordinates[0]][coordinates[1] - 1].type == (int)Location.types.free)
                {
                    l.accessibleLocations.Add(db.locationMap[coordinates[0]][coordinates[1] - 1]);
                }
            }

            if (coordinates[0] + 1 < db.locationMap.GetLength(0))
            {
                if (db.locationMap[coordinates[0] + 1][coordinates[1]] != null && db.locationMap[coordinates[0] + 1][coordinates[1]].type == (int)Location.types.free)
                {
                    l.accessibleLocations.Add(db.locationMap[coordinates[0] + 1][coordinates[1]]);
                }
            }

            if (coordinates[0] - 1 >= 0)
            {
                if (db.locationMap[coordinates[0] - 1][coordinates[1]] != null && db.locationMap[coordinates[0] - 1][coordinates[1]].type == (int)Location.types.free)
                {
                    l.accessibleLocations.Add(db.locationMap[coordinates[0] - 1][coordinates[1]]);
                }

            }
        }

        private static int getEntranceTime(Location l, int time)
        {
            while (l.occupiedAt.Contains(time))
            {
                time++;
            }
            return time;
        }

        public static void createPassages(Database db)
        {
            foreach (Location l in db.locations)
            {
                foreach (Location neighbour in l.accessibleLocations)
                {
                    bool exists = false;
                    foreach (Passage p in l.passages)
                    {
                        //TODO: assign 
                        if (p.a == neighbour || p.b == neighbour) exists = true;
                    }
                    if (!exists)
                    {
                        Passage p = new Passage {a = l, b = neighbour };
                        l.passages.Add(p);
                        neighbour.passages.Add(p);
                    }
                }
            }
        }
    }
}
