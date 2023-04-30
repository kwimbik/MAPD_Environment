using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
            
            //finds all neighbours of all locations -> this could be precounted while loading map
            

            //creates passages for each locations
            createPassages(db);

            
            
            foreach (Agent a in db.agents)
            {
                a.plan.steps.Clear();
                Location current = a.baseLocation;

                if (a.orders.Count > 0)
                {
                    plan.agents.Add(a);
                    foreach (Order o in a.orders)
                    {
                        plan.orders.Add(o);
                        List<Location> loc1 = getPathForAgentAndOrder(current, o.currLocation, db);
                        formatPlan(a, current, loc1, o, plan);
                        current = o.currLocation;
                        List<Location> loc2 = getPathForAgentAndOrder(current, o.targetLocation, db);
                        formatPlan(a, o.currLocation, loc2, o, plan);
                        current = o.targetLocation;
                    }
                }

                //reset entrance time after each agent
                foreach (var loc in db.locations)
                {
                    loc.entranceTime = 0;
                }
            }

            List<PlanStep> sortedStepsByTime = plan.steps.OrderBy(o => o.time).ToList();
            plan.steps = sortedStepsByTime;

            foreach (var agent in plan.agents)
            {
                plan.agentOrderDict.Add(agent, new List<Order>(agent.orders));
            }
            cleanup(db);
            return plan;
        }

        private static void cleanup(Database db)
        {
            //TODO: complete the cleanup
            foreach (Agent a in db.agents)
            {
                a.orders.Clear();
            }
        }

        private static void formatPlan(Agent a,Location curr,  List<Location> locations1, Order o, Plan overallPlan)
        {
            timeCounter = a.movesMade;
            string agentId = a.id;

            Location prev = curr;

            for (int i = locations1.Count -1 ; i >= 0; i--)
            {
                locations1[i].occupiedAt.Add(locations1[i].entranceTime);
                PlanStep step = new PlanStep { agentId = a.id, locationId = locations1[i].id, time = locations1[i].entranceTime, type = (int)PlanStep.types.movement };
                a.plan.steps.Add(step);
                overallPlan.steps.Add(step);
                //Passage p = prev.getPassage(locations1[i]);
                //p.occupied.Add(locations1[i].entranceTime);
                prev = locations1[i];
            }

            if (locations1[0] == o.currLocation)
            {
                overallPlan.steps.Add(new PlanStep { agentId = a.id, locationId = o.currLocation.id, time = ++o.currLocation.entranceTime, type = (int)PlanStep.types.pickup, orderId = o.id});
                o.currLocation.occupiedAt.Add(o.currLocation.entranceTime);
            };

            if (locations1[0] == o.targetLocation)
            {
                overallPlan.steps.Add(new PlanStep { agentId = a.id, locationId = o.targetLocation.id, time = ++o.targetLocation.entranceTime, type = (int)PlanStep.types.deliver, orderId = o.id });
                o.targetLocation.occupiedAt.Add(o.currLocation.entranceTime);
            };
        }

        private static List<Location> getPathForAgentAndOrder(Location baseLocation, Location l, Database db)
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
                if (current != null && current.id == start.id)
                {
                    current = null;
                }
            }
            return route;
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
