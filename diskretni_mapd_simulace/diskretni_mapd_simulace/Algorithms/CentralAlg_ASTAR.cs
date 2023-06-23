using diskretni_mapd_simulace.Entities;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class CentralAlg_ASTAR
    {
        /// <summary>
        /// Return Location list of task ends. it has same or greater count as number of agents
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="numOfAgents"></param>
        /// <returns></returns>
        private static List<Location> createTaskList(List<Order> orders, int numOfAgents)
        {
            List<Location> result = new List<Location>();
            foreach (var o in orders)
            {
                result.Add(o.currLocation);
            }

            if (numOfAgents > orders.Count)
            {
                for (int i = 0; i < numOfAgents - orders.Count; i++)
                {
                    result.Add(new Location {  id = Location.mockLocationId});
                }
            }
            
            return result;
        }
        private static void assignTasks(Token t)
        {
            List<Agent> agents = new List<Agent>(t.agents);
            List<Order> orders = new List<Order>(t.orders);

            foreach (var a in agents.ToList())
            {
                if (a.assignedOrder != null)
                {
                    a.assignedTask = a.assignedOrder.targetLocation;
                    agents.Remove(a);
                }
            }

            List<Location> taskList = createTaskList(orders, agents.Count);

            int[,] costMatrix = createCostMatrix(taskList, agents);
            var hungarianAlg = new HungariaAlg(costMatrix);
            var matchX = hungarianAlg.Run();
            processTasks(matchX, taskList, agents);
        }

        private static void processTasks(int[] costMatrix, List<Location> tasks, List<Agent> agents)
        {
            for (int i = 0; i < agents.Count; i++)
            {
                Agent a = agents[i];
                a.assignedTask = tasks[costMatrix[i]];
            }
            return;
        }

        private static int[,] createCostMatrix(List<Location> tasks, List<Agent> agents)
        {
            int[,] costMatrix = new int[agents.Count, tasks.Count];
            //Tasks are columns, agents are rows

            for (int i = 0; i < agents.Count; i++)
            {
                for (int j = 0; j < tasks.Count; j++)
                {
                    costMatrix[i,j] = Database.getDistance(agents[i].currentLocation, tasks[j]);
                }
            }

            return costMatrix;
        }

        public static Plan run(ProblemObject po)
        {
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

                assignTasks(token);

                bool CalcSucess = true;
                //every step we update all agents, TODO: nejdriv ty co maji assigned order
                //po.agents = po.agents.OrderByDescending(x => x.assignedOrder != null).ToList(); 
                foreach (var agent in po.agents)
                {
                    CalcSucess = updateToken(agent, token);
                    if (CalcSucess == false) break;
                }
                if (CalcSucess) updateToken(token, plan);
                else
                {   
                    po.agents = po.agents.OrderBy(a => rng.Next()).ToList();
                    token.time += 1;
                }
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

        private static bool updateToken(Agent a, Token t)
        {
            if (a.assignedTask.id == -1)
            {
                //idle agent
                var adjacentSquares = getAccessableLocations(a.currentLocation, t.time + 1);
                if (adjacentSquares.Count > 0)
                {
                    adjacentSquares = adjacentSquares.OrderByDescending(x => x.accessibleLocations.Count).ToList();
                    var taskList = planStepsFromLocList(adjacentSquares[0], a, t.time, t);
                    a.taskList = new Queue<PlanStep>();
                    a.taskList.Enqueue(taskList[0]);
                    return true;
                }
                else return false;
            }
            a.taskList = calculateTaskPath(a, t, a.assignedTask);
            if (a.taskList != null) return true;
            return false;
        }

       


       
        private static void markOccupiedTaskPath(PlanStep step, Token t, Agent a)
        {
            Location l = getLocationById(step.locationId, t.locations);
            l.occupiedAt.Add(t.time + 1);
            if (l.id != a.currentLocation.id)
            {
                Location.getPassageFromLocation(l, a.currentLocation).occupied.Add(t.time + 1 );
            }
        }

        private static Queue<PlanStep> calculateTaskPath(Agent a, Token t, Location location)
        {
            List<PlanStep> list_ps = new List<PlanStep>();

            int time = t.time;

            foreach (Location l in t.locations)
            {
                l.entranceTime = time;
            }

            List<Location> locationList = Algorithm.getPathForAgentAndTask(a.currentLocation, location, t);
            PlanStep task = planStepsFromLocList(locationList[0], a, time, t)[0]; //take only the first task

            //A* did not find available path -> compute wait
            if (locationList.Count == 1 && locationList[0].id != location.id)
            {
                var planSep = waitFunction(a, t, location);
                if (planSep != null) return new Queue<PlanStep>(new List<PlanStep> { planSep });
                return null;
            }


            if (locationList.Count == 0)
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
                    Location l = Algorithm.findClosestAvailableLocation(a.currentLocation, t.time + 1);
                    if (l.id == 0) return null;
                    ps.locationId = l.id;
                    l.occupiedAt.Add(t.time + 1);
                }

                return new Queue<PlanStep>(new List<PlanStep> { ps });
            }
            if (locationList.Count == 1 && locationList[0].id == 0) return null; 

            markOccupiedTaskPath(task, t, a); // marks passages as occupied so in A*, some neighbours might not appear as avaliable
            Queue<PlanStep> planStep = new Queue<PlanStep>();
            planStep.Enqueue(task);
            return planStep;
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
            if (l.occupiedAt.Contains(t + 1) == false) locations.Add(l);
            foreach (var nei in l.accessibleLocations)
            {
                Passage p = Location.getPassageFromLocation(l, nei);
                if (p.occupied.Contains(t + 1) == false && nei.occupiedAt.Contains(t + 1) == false) locations.Add(nei);
            }
            return locations;
        }

        private static List<PlanStep> planStepsFromLocList(Location l, Agent a, int time, Token t)
        {
            List<PlanStep> task_list = new List<PlanStep>();
         
            PlanStep ps = new PlanStep();
            ps.time = ++time;
            ps.agentId = a.id;
            ps.locationId = l.id;
            if (l.id == a.currentLocation.id && l.id == a.assignedTask.id && a.assignedOrder == null)
            {
                List<Order> orderOnL = t.orders.Where(t => t.currLocation.id == l.id).ToList();
                if (orderOnL.Count > 0)
                {
                    ps.type = (int)PlanStep.types.pickup;
                    ps.orderId = orderOnL[0].id;
                }
                else
                {
                    ps.type = (int)PlanStep.types.waiting;
                }
                
            }
            else if (l.id == a.currentLocation.id && l.id == a.assignedTask.id && a.assignedOrder != null)
            {
                ps.type = (int)PlanStep.types.deliver;
                ps.orderId = a.assignedOrder.id;
            }
            else ps.type = (int)PlanStep.types.movement;
            task_list.Add(ps);
            
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
                a.currentLocation = t.locations[ps.locationId];

                if (ps.type == (int)PlanStep.types.deliver)
                {
                    Order o = getOrderById(t.inProcessOrders, ps.orderId);
                    t.deliveredOrders.Add(o);
                    t.inProcessOrders.Remove(o);
                    a.assignedOrder = null;
                }

                else if (ps.type == (int)PlanStep.types.pickup)
                {
                    Order o = getOrderById(t.orders, ps.orderId);
                    t.inProcessOrders.Add(o);
                    t.orders.Remove(o);
                    a.orders.Add(o);
                    a.assignedOrder = o;
                }
            }
            t.time++;
        }
    }
}
