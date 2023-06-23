using diskretni_mapd_simulace.Entities;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class TokenPassingAlg
    {
        public static Plan run(ProblemObject po)
        {
            List<int> numOfIdleAgents = new List<int>();
            Random rng = new Random(43);

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

                bool CalcSucess = true;
                sort(token, rng);
                numOfIdleAgents.Add(token.agents.Where(a => a.state == (int)Agent.states.idle).Count());

                //TODO: ruzne techniky na serazeni agentu
                //TODO vetsi vstup

                //if agent is idle, it asks for token, finds closest order, compute path and return token.
                foreach (var agent in token.agents)
                {
                    if (agent.state == (int)Agent.states.idle)
                    {
                        CalcSucess = updateToken(agent, token);
                    }
                    if (CalcSucess == false)
                    {
                        foreach (var a in token.agents)
                        {
                            a.taskList = new Queue<PlanStep>();
                            a.taskList.Enqueue(new PlanStep {agentId = a.id, locationId = a.currentLocation.id, time = token.time, type = (int)PlanStep.types.waiting });
                        }
                        foreach (var l in token.locations)
                        {
                            l.occupiedAt.Clear();
                            foreach (var e in l.passages)
                            {
                                e.occupied.Clear();
                            }
                        }
                        foreach (var o in token.inProcessOrders.ToList())
                        {
                            token.orders.Add(o);
                            token.inProcessOrders.Remove(o);
                        }
                        
                        po.agents = po.agents.OrderBy(a => rng.Next()).ToList();
                        break;
                    }
                }
                updateToken(token, plan); //if all agents find their paths, update token
            }


            plan.orders = token.deliveredOrders;

            //assignments
            foreach (var agent in plan.agents)
            {
                plan.agentOrderDict.Add(agent, new List<Order>(agent.orders));
            }
            Console.WriteLine(numOfIdleAgents.Sum(x => Convert.ToInt32(x))/token.time);
            plan.serviceTime = Algorithm.getAverageServiceTimeDelay(plan.orders);
            return plan;
        }

        private static void sort(Token t, Random rng)
        {
            t.agents = t.agents.OrderBy(a => rng.Next()).ToList();
            return;
            //ten ktery ma nejdal nejblizsi objednavku
            //nahodne pokazdy
            //nejak pocitat volny pozice (bud kolem objednavky nebo okolo agenta)
            //nejakych 6-8 bodu u kterych budu porad aktualizovat mnozstvi objednavek, porovnat nejblizsi objednavky a vzit agenta, jehoz nejlepsi objednavka konci okolo lokace s nejvetsi frekvenci objednavek

            if (t.orders.Count > 0)
            {
                t.agents = t.agents.OrderByDescending(a => Database.getDistance(a.currentLocation, t.orders.OrderBy(y => Database.getDistance(a.currentLocation, y.currLocation)).ToList()[0].currLocation)).ToList();
            }
            else
            {
                t.agents = t.agents.OrderBy(a => rng.Next()).ToList();
            }
            return;
        }

        public static Location getLocationById(int Id, List<Location> locations)
        {
            foreach (Location loc in locations)
            {
                if (loc.id == Id) return loc;
            }
            return null;
        }

        private static bool updateToken(Agent a, Token t)
        {
            Order o = findBestOrder(a, t.orders);
            if (o != null)
            {
               
                a.taskList = calculateTaskPath(a, t, o);
                if (a.taskList == null)
                {
                    return false;
                } 
                if (a.taskList.Count > 1)
                {
                    t.inProcessOrders.Add(o);
                    t.orders.Remove(o);
                }
                a.state = (int)Agent.states.occupied;
            }
            else
            {
                var task = waitFunction(a, t, a.currentLocation);
                if (task != null) a.taskList.Enqueue(task);
                else return false;
            }
            if (a.taskList != null) return true;
            return false;
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
            int time = t.time;

            foreach (Location l in t.locations)
            {
                l.entranceTime = time;
            }

            List<Location> locationList = Algorithm.getPathForAgentAndTask(a.currentLocation, o.currLocation, t);

            if (locationList.Count == 1 && locationList[0].id != o.currLocation.id)
            {
                var task = waitFunction(a, t, o.currLocation);
                if (task != null) return new Queue<PlanStep>(new List<PlanStep> { task });
                return null;

            }
            List<PlanStep> tasks = planStepsFromLocList(locationList, a, time);
            time += tasks.Count + 1;

            tasks.Add(new PlanStep {agentId = a.id, locationId = o.currLocation.id, orderId = o.id, type = (int)PlanStep.types.pickup, time = time });

            foreach (Location l in t.locations)
            {
                l.entranceTime = time;
            }
            List<Location> locationList2 = Algorithm.getPathForAgentAndTask(o.currLocation, o.targetLocation, t);
            if (locationList2.Count == 1 && locationList2[0].id != o.targetLocation.id)
            {
                var task = waitFunction(a, t, o.targetLocation);
                if (task!=null) return new Queue<PlanStep>(new List<PlanStep> { task });
                return null;
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

        private static PlanStep waitFunction(Agent a, Token t, Location target)
        {
            PlanStep ps = new PlanStep();
            ps.time = t.time + 1;
            ps.agentId = a.id;
            var locations = getAccessableLocations(a.currentLocation, t.time);
            locations = locations.OrderBy(x => Database.getDistance(x, target)).ToList();
            if (locations.Count < 1) return null;
            if (locations[0].id == a.currentLocation.id) ps.type = (int)PlanStep.types.waiting;
            else ps.type = (int)PlanStep.types.movement;
            ps.locationId = locations[0].id;

            return ps;
        }

        private static List<Location> getAccessableLocations(Location l, int t)
        {

            List<Location> locations = new List<Location>();
            if(l.occupiedAt.Contains(t+1) == false) locations.Add(l);
            foreach (var nei in l.accessibleLocations)
            {
                Passage p = Location.getPassageFromLocation(l, nei);
                if (p.occupied.Contains(t + 1) == false && nei.occupiedAt.Contains(t + 1) == false) locations.Add(nei);
            }
            return locations;
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
                    a.orders.Add(o);
                    o.realServiceTime = t.time - o.timeFrom;
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
