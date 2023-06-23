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
                    reallocatePath(swappedAgent, t);
                }
                a.taskList = calculateTaskPath(a, t, o);

                if (a.taskList.Count > 1)
                {
                    if (t.inProcessOrders.Contains(o) == false) t.inProcessOrders.Add(o);
                    a.orders.Add(o);
                    t.agentOrderDict[o] = a;
                    if (swappedAgent != null)
                    {
                        swappedAgent.orders.Remove(o);

                        swappedAgent.state = (int)Agent.states.idle;
                        swappedAgent.taskList.Clear();
                    }
                }
                else
                {
                    //New agent failed to find the path -> we again assign the old agent
                    if (swappedAgent!= null)
                    {
                        swappedAgent.taskList = calculateTaskPath(swappedAgent, t, o);
                        swappedAgent = null;
                    }
                }
                a.state = (int)Agent.states.occupied;
                return swappedAgent;
            }
            else
            {
                a.taskList.Enqueue(waitFunction(a, t, a.currentLocation));
            }
            return null; //returns the agent that needs token next -> we swaped his task
        }

        private static PlanStep waitFunction(Agent a, Token t, Location target)
        {
            PlanStep ps = new PlanStep();
            ps.time = t.time + 1;
            ps.agentId = a.id;
            var locations = getAccessableLocations(a.currentLocation, t.time);
            locations = locations.OrderBy(x => Database.getDistance(x, target)).ToList();
            if (locations[0].id == a.currentLocation.id) ps.type = (int)PlanStep.types.waiting;
            else ps.type = (int)PlanStep.types.movement;
            ps.locationId = locations[0].id;

            return ps;
        }



        //find closes order and return it
        private static Order findBestOrder(Agent a,Token t)
        {
            t.swapControlList.Clear();  
            if (t.orders.Count == 0) return null;

            Order newOrder = null;
            List<Order> orders = t.orders.OrderBy(x => Database.getDistance(a.currentLocation, x.currLocation)).ToList();
            foreach (var ord in orders)
            {
                if (isMoreEfficient(a, ord, newOrder, t)) return ord;
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

            if (distOLD <= distNew) return false; //not worth the swap

            int distanceGain = distOLD - distNew;
            if (originalOrder != null)
            {
                distanceGain = Database.getDistance(AssignedAgent.currentLocation, originalOrder.currLocation) - distNew;
            }

            List<Order> orders = t.orders.OrderBy(x => Database.getDistance(a.currentLocation, x.currLocation)).ToList();
            foreach (var order in orders)
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
            if (locationList.Count == 1 && locationList[0].id != o.currLocation.id)
            {
                return new Queue<PlanStep>(new List<PlanStep> { waitFunction(a, t, o.currLocation) });
            }

            List<PlanStep> tasks = planStepsFromLocList(locationList, a, time);
            time += tasks.Count + 1;

            tasks.Add(new PlanStep { agentId = a.id, locationId = o.currLocation.id, orderId = o.id, type = (int)PlanStep.types.pickup, time = time });

            foreach (Location l in t.locations)
            {
                l.entranceTime = time;
            }
            List<Location> locationList2 = Algorithm.getPathForAgentAndTask(o.currLocation, o.targetLocation, t);
            if (locationList2.Count == 1 && locationList2[0].id != o.targetLocation.id)
            {
                return new Queue<PlanStep>(new List<PlanStep> { waitFunction(a, t, o.targetLocation) });
            }
            List<PlanStep> tasks2 = planStepsFromLocList(locationList2, a, time);
            time += tasks2.Count + 1;
            tasks2.Add(new PlanStep { agentId = a.id, locationId = o.targetLocation.id, orderId = o.id, type = (int)PlanStep.types.deliver, time = time });
            tasks.AddRange(tasks2);

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
            t.orders = new List<Order>();
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

        private static List<Location> getAccessableLocations(Location l, int t)
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
