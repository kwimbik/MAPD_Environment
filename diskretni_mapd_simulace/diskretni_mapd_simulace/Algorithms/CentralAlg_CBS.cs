using diskretni_mapd_simulace.Entities;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class CentralAlg_CBS
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

                //TODO: find closest free position
                if (a.assignedTask.id == Location.mockLocationId)
                {
                    a.state = (int)Agent.states.idle;
                }
                else
                {
                    a.state = (int)Agent.states.occupied;
                }
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

                var solution = CBS.Run(po.agents, po.locations, token.orders, token.time);

                foreach (var agent in solution.Keys)
                {
                    //we assign to every agent exactly next location in their planned path
                    agent.taskList = solution[agent];
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
                ps.time = t.time;
                p.steps.Add(ps);
                a.currentLocation = getLocationById(ps.locationId, t.locations);

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
