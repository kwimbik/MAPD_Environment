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
                    List<Location> loc2 = getPathForAgentAndOrder(o.currLocation, o.targetLocation, db);
                    plan.value += formatPlan(a,current, loc1, loc2);
                    current = o.targetLocation;
                }
            }
            return plan;
        }

        private static string formatPlan(Agent a,Location curr,  List<Location> locations1, List<Location> locations2)
        {
            timeCounter = a.movesMade;
            string newPlan = "";
            string agent = "A";
            string agentId = a.id;
            a.plan.agent = a;

            Location prev = curr;

            for (int i = locations1.Count -1 ; i >= 0; i--)
            {
                //TODO: delete this once planWriter supports multiagentPlan, dont forget to move ++
                a.plan.steps.Add(new PlanStep { agentId = a.id, locationId = locations1[i].id, time = timeCounter, type = (int)PlanStep.types.movement });
                newPlan += $"{timeCounter++}-{agent}-{agentId}-{locations1[i].id}\n";
                Passage p = prev.getPassage(locations1[i]);
                p.occupied.Add(timeCounter);
                prev = locations1[i];
                locations1[i].occupiedAt.Add(timeCounter);
            }

            for (int i = locations2.Count - 1; i >= 0; i--)
            {
                //TODO: delete this once planWriter supports multiagentPlan
                a.plan.steps.Add(new PlanStep { agentId = a.id, locationId = locations2[i].id, time = timeCounter, type = (int)PlanStep.types.movement });
                newPlan += $"{timeCounter++}-{agent}-{agentId}-{locations2[i].id}\n";
                Passage p = prev.getPassage(locations2[i]);
                p.occupied.Add(timeCounter);
                prev = locations2[i];
                locations2[i].occupiedAt.Add(timeCounter);
            }
            a.movesMade = timeCounter;
            return newPlan;
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
            int simulationTime = -1;
            while (openList.Count > 0)
            {
                //TODO: wont be needed if occupied tiles wont be opened at all at time k
                openList = openList.OrderBy(l => l.f).ToList();
                foreach (var loc in openList)
                {
                    if (loc.occupiedAt.Contains(loc.entranceTime + 1) == false)
                    {
                        current = loc;
                        break;
                    }
                }
                


                simulationTime = current.entranceTime + 1;

                // add the current square to the closed list
                closedList.Add(current);

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

                    //square not to be opened if its occupied by different agent at time sim + 1
                    if (adjacentSquare.occupiedAt.Contains(simulationTime + 1)) continue;

                    // if it's not in the open list...
                    if (openList.Contains(adjacentSquare) == false)
                    {
                        // compute its score, set the parent
                        adjacentSquare.g = g;
                        adjacentSquare.h = Location.getDistance(target, adjacentSquare);
                        adjacentSquare.f = adjacentSquare.g + adjacentSquare.h;
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
            while (current != null)
            {
                route.Add(current);
                current = current.Parent;
                if (current != null && current.id == start.id) current = null;
            }

            foreach (Location k in db.locations)
            {
                k.entranceTime = -1;
            }
            return route;
        }

        //TODO: move to some generic Solver class, so all algorithms have access to this fction
        private static void getAccessibleLocations(Location l, Database db)
        {
            //TODO: more elegant way of returning access. positions
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
