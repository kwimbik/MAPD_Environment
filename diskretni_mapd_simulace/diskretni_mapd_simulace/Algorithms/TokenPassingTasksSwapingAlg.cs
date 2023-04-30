using diskretni_mapd_simulace.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class TokenPassingTasksSwapingAlg
    {
        public static Plan run(ProblemObject po)
        {
            List<Order> unassignedOrders = po.orders;

            Plan plan = new Plan();
            plan.agents = po.agents;

            Token token = initializeToken(po);

            Algorithm.initialize(token.locations, token.agents);

            int target = unassignedOrders.Count;


            while (token.deliveredOrders.Count < target)
            {
                //load new orders
                for (int i = 0; i < unassignedOrders.Count; i++)
                {
                    if (unassignedOrders[i].timeFrom <= token.time)
                    {
                        token.orders.Add(unassignedOrders[i]);
                        unassignedOrders.Remove(unassignedOrders[i]);
                    }
                    else if (unassignedOrders[i].timeFrom > token.time) break; //I assume orders are sorted by time
                }

                //if agent is idle, it asks for token, finds closest order, compute path and return token.
                foreach (var agent in po.agents)
                {
                    if (agent.state == (int)Agent.states.idle)
                    {
                        //if we swap task with different agent, we run updateToken on it first
                        Agent swappedAgent = updateToken(agent, token);
                        while (swappedAgent != null)
                        {
                            swappedAgent = updateToken(swappedAgent, token);
                        };
                    }
                }

                updateToken(token, plan);
            }


            plan.orders = token.deliveredOrders;

            //assignments
            foreach (var agent in plan.agents)
            {
                plan.agentOrderDict.Add(agent, new List<Order>(agent.orders));
            }

            return plan;
        }

        public static Location getLocationById(int Id, List<Location> locations)
        {
            foreach (Location loc in locations)
            {
                if (loc.id == Id) return loc;
            }
            return null;
        }

        /// <summary>
        /// takes the path of an agent and frees all the passages and locations as it has been swapped
        /// </summary>
        /// <param name="a"></param>
        /// <param name="t"></param>
        private static void reallocatePath(Agent a, Token t)
        {
            Location init = a.currentLocation;

            while (a.taskList.Count > 0)
            {
                PlanStep ps = a.taskList.Dequeue();
                Location target = getLocationById(ps.locationId, t.locations);
                target.occupiedAt.Remove(ps.time);
                if (init.id != target.id)
                {
                    Passage p = Location.getPassageFromLocation(init, target);
                    p.occupied.Remove(ps.time);
                }
                init = target;
            }
        }

        private static Agent updateToken(Agent a, Token t)
        {
            Agent swappedAgent = null;
            Order o = findBestOrder(a, t);
            if (o != null)
            {
                if (t.agentOrderDict.ContainsKey(o)) swappedAgent = t.agentOrderDict[o];

                if (swappedAgent != null)
                {
                    swappedAgent.orders.Remove(o);
                    reallocatePath(swappedAgent, t);

                    swappedAgent.state = (int)Agent.states.idle;
                    swappedAgent.taskList.Clear();
                }
                a.taskList = calculateTaskPath(a, t, o);
                if (a.taskList.Count > 1)
                {
                    t.inProcessOrders.Add(o);
                    a.orders.Add(o);
                    t.agentOrderDict[o] = a;
                }
                a.state = (int)Agent.states.occupied;
                return swappedAgent;
            }
            else
            {
                PlanStep ps = new PlanStep();
                ps.time = t.time;
                ps.agentId = a.id;
                ps.type = (int)PlanStep.types.waiting;

                if (a.currentLocation.occupiedAt.Contains(t.time) == false)
                {
                    ps.locationId = a.currentLocation.id;
                    a.currentLocation.occupiedAt.Add(t.time);
                }
                else
                {
                    Location l = findClosestAvailableLocation(a, t.time);
                    ps.locationId = l.id;
                    l.occupiedAt.Add(t.time);
                }
                //zapsat ze agent zustane na miste nebo musi uhnout jinym agentum, pokud je jeho lokace obsazena
                a.taskList.Enqueue(ps);
            }
            return null; //returns the agent that needs token next -> we swaped his task
        }

        private static Location findClosestAvailableLocation(Agent a, int time)
        {
            //search through all neighbours to find one such that is is not occupied next turn
            foreach (var loc in a.currentLocation.accessibleLocations)
            {
                if (loc.occupiedAt.Contains(time) == false) return loc;
            }
            return new Location();
        }


        //find closes order and return it
        private static Order findBestOrder(Agent a,Token t)
        {
            t.swapControlList.Clear();  
            if (t.orders.Count == 0) return null;
            int distance = int.MaxValue;

            Order newOrder = null;
            foreach (var ord in t.orders)
            {
                if (Database.getDistance(a.currentLocation, ord.currLocation) < distance)
                {
                    if (isMoreEfficient(a, ord, newOrder, t))
                    {
                        distance = Database.getDistance(a.currentLocation, ord.currLocation);
                        newOrder = ord;
                    }
                }
            }
            return newOrder;
        }

        private static bool isMoreEfficient(Agent a, Order newOrder, Order originalOrder,  Token t)
        {
            //TODO: assign first unassigned orders, then try to find better one assigned, should decrease the computing time significantly
            if (t.agentOrderDict.ContainsKey(newOrder) == false) return true; //order is not assigned

            Agent AssignedAgent = t.agentOrderDict[newOrder];
            int distOLD = Database.getDistance(AssignedAgent.currentLocation, newOrder.currLocation);
            int distNew = Database.getDistance(a.currentLocation, newOrder.currLocation);

            //if (distOLD  <= distNew) return false; //not worth the swap

            int distanceGain = 0;
            if (originalOrder != null)
            {
                distanceGain = Database.getDistance(a.currentLocation, originalOrder.currLocation) - distNew;
            }

            foreach (var order in t.orders)
            {
                if (order.id == newOrder.id || t.swapControlList.Contains(order)) continue;
                t.swapControlList.Add(order);
                //exist a different order that is better for AssignedAgent
                if (isMoreEfficient(AssignedAgent, order, newOrder, t))
                {
                    distanceGain += distOLD - Database.getDistance(AssignedAgent.currentLocation, order.currLocation);
                    if (distanceGain > 0) return true;
                }
                t.swapControlList.Remove(order);
            }
            return false;
        }

        private static void markOccupiedTaskPath(List<PlanStep> steps, Token t, Agent a)
        {
            //block passage from vehicle to the first planStep
            if (a.currentLocation.id != getLocationById(steps[0].locationId, t.locations).id)
            {
                Location.getPassageFromLocation(a.currentLocation, getLocationById(steps[0].locationId, t.locations)).occupied.Add(steps[0].time);
            }

            for (int i = 0; i < steps.Count - 1; i++)
            {
                Location l1 = getLocationById(steps[i].locationId, t.locations);
                l1.occupiedAt.Add(steps[i].time);
                Location l2 = getLocationById(steps[i + 1].locationId, t.locations);
                if (l1.id == l2.id) continue; //no movement -> a* takes care of this itself

                Location.getPassageFromLocation(l1, l2).occupied.Add(steps[i + 1].time);
            }
            getLocationById(steps[steps.Count - 1].locationId, t.locations).occupiedAt.Add(steps[steps.Count - 1].time);
        }

        //move this to db


        private static Queue<PlanStep> calculateTaskPath(Agent a, Token t, Order o)
        {
            List<PlanStep> list_ps = new List<PlanStep>();

            //pridat veci z listu do fronty v opacnem poradi
            //pridat agentovy tenhle list jako task list
            int time = t.time;

            foreach (Location l in t.locations)
            {
                l.entranceTime = time;
            }

            List<Location> locationList = Algorithm.getPathForAgentAndTask(a.currentLocation, o.currLocation, t);
            List<PlanStep> tasks = planStepsFromLocList(locationList, a, time);
            time += tasks.Count + 1;

            tasks.Add(new PlanStep { agentId = a.id, locationId = o.currLocation.id, orderId = o.id, type = (int)PlanStep.types.pickup, time = time });

            foreach (Location l in t.locations)
            {
                l.entranceTime = time;
            }
            List<Location> locationList2 = Algorithm.getPathForAgentAndTask(o.currLocation, o.targetLocation, t);
            List<PlanStep> tasks2 = planStepsFromLocList(locationList2, a, time);
            time += tasks2.Count + 1;
            tasks2.Add(new PlanStep { agentId = a.id, locationId = o.targetLocation.id, orderId = o.id, type = (int)PlanStep.types.deliver, time = time });
            tasks.AddRange(tasks2);


            //A* DID NOT FIND PATH -> ALLOW WAITING -> write function for this, its used at least twice
            if (locationList.Count == 0 || locationList2.Count == 0)
            {
                PlanStep ps = new PlanStep();
                ps.time = t.time + 1;
                ps.agentId = a.id;
                ps.type = (int)PlanStep.types.waiting;

                if (a.currentLocation.occupiedAt.Contains(t.time + 1) == false)
                {
                    ps.locationId = a.currentLocation.id;
                    a.currentLocation.occupiedAt.Add(t.time + 1);
                }
                else
                {
                    Location l = findClosestAvailableLocation(a, t.time + 1);
                    ps.locationId = l.id;
                    l.occupiedAt.Add(t.time + 1);
                }

                return new Queue<PlanStep>(new List<PlanStep> { ps });
            }

            markOccupiedTaskPath(tasks, t, a); // marks passages as occupied so in A*, some neighbours might not appear as avaliable
            return new Queue<PlanStep>(tasks);
        }

        private static List<PlanStep> planStepsFromLocList(List<Location> locations, Agent a, int time)
        {
            List<PlanStep> task_list = new List<PlanStep>();
            foreach (Location l in locations)
            {
                PlanStep ps = new PlanStep();
                ps.time = ++time;
                ps.agentId = a.id;
                ps.locationId = l.id;
                ps.type = (int)PlanStep.types.movement;
                task_list.Add(ps);
            }
            return task_list;
        }

        private static Token initializeToken(ProblemObject po)
        {
            Token t = new Token();
            t.orders = po.orders;
            t.time = 0;
            t.locationMap = po.locationMap;
            t.locations = po.locations;
            t.agents = po.agents;
            return t;
        }

        private static Order getOrderById(List<Order> orders, string id)
        {
            foreach (var o in orders)
            {
                if (o.id == id) return o;
            }
            return null;
        }

        private static void updateToken(Token t, Plan p)
        {
            //make the step
            foreach (var a in t.agents)
            {
                PlanStep ps = a.taskList.Dequeue();
                p.steps.Add(ps);
                a.currentLocation = getLocationById(ps.locationId, t.locations);

                if (ps.type == (int)PlanStep.types.deliver)
                {
                    Order o = getOrderById(t.inProcessOrders, ps.orderId);
                    t.deliveredOrders.Add(o);
                    t.inProcessOrders.Remove(o);
                }

                //once picked up, it can no longer be swapped
                if (ps.type == (int)PlanStep.types.pickup)
                {
                    Order o = getOrderById(t.inProcessOrders, ps.orderId);
                    t.orders.Remove(o);
                }

                //finished all tasks
                if (a.taskList.Count == 0)
                {
                    a.state = (int)Agent.states.idle;
                }
            }
            t.time++;
        }

        private static List<Location> getPathForAgentAndOrder(Location baseLocation, Location l, Token t)
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
