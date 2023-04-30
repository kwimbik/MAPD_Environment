using diskretni_mapd_simulace.Entities;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class TokenPassingAlg
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
                        updateToken(agent, token);
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

        private static void updateToken(Agent a, Token t)
        {
            Order o = findBestOrder(a, t.orders);
            if (o != null)
            {
               
                a.taskList = calculateTaskPath(a, t, o);
                if (a.taskList.Count > 1)
                {
                    t.inProcessOrders.Add(o);
                    t.orders.Remove(o);
                    a.orders.Add(o);
                }
                //else: no path was found
                a.state = (int)Agent.states.occupied;
            }
            else
            {
                PlanStep ps = new PlanStep();
                ps.time = t.time+1;
                ps.agentId = a.id;
              

                if (a.currentLocation.occupiedAt.Contains(t.time+1) == false)
                {
                    ps.locationId = a.currentLocation.id;
                    a.currentLocation.occupiedAt.Add(t.time+1);
                    ps.type = (int)PlanStep.types.waiting;
                }
                else
                {
                    Location l = findClosestAvailableLocation(a, t.time+1);
                    ps.locationId = l.id;
                    l.occupiedAt.Add(t.time+1);
                    ps.type = (int)PlanStep.types.movement;
                }
                //zapsat ze agent zustane na miste nebo musi uhnout jinym agentum, pokud je jeho lokace obsazena
                a.taskList.Enqueue(ps);
            }
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
        private static Order findBestOrder(Agent a, List<Order> orders)
        {
            if (orders.Count == 0) return null;

            Order o = orders[0];
            foreach (var ord in orders)
            {
                if (Database.getDistance(a.currentLocation, ord.currLocation) < Database.getDistance(a.currentLocation, o.currLocation))
                {
                    o = ord;
                }
            }
            return o;
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

        private static Queue<PlanStep> calculateTaskPath(Agent a,Token t, Order o)
        {
            List<PlanStep> list_ps = new List<PlanStep>();

            int time = t.time;

            foreach (Location l in t.locations)
            {
                l.entranceTime = time;
            }

            List<Location> locationList = Algorithm.getPathForAgentAndTask(a.currentLocation, o.currLocation, t);
            List<PlanStep> tasks = planStepsFromLocList(locationList, a, time);
            time += tasks.Count + 1;

            tasks.Add(new PlanStep {agentId = a.id, locationId = o.currLocation.id, orderId = o.id, type = (int)PlanStep.types.pickup, time = time });

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

                if (a.currentLocation.occupiedAt.Contains(t.time+1) == false)
                {
                    ps.locationId = a.currentLocation.id;
                    a.currentLocation.occupiedAt.Add(t.time+1);
                }
                else
                {
                    Location l = findClosestAvailableLocation(a, t.time+1);
                    ps.locationId = l.id;
                    l.occupiedAt.Add(t.time+1);
                }

                return new Queue<PlanStep>(new List<PlanStep> { ps});
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

                //finished all tasks
                if (a.taskList.Count == 0)
                {
                    a.state = (int)Agent.states.idle;
                }
            }
            t.time++;
        }
    } 
}
