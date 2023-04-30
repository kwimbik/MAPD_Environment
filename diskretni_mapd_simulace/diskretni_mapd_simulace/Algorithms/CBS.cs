using diskretni_mapd_simulace.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Algorithms
{
    public static class CBS
    {
        private static Dictionary<Agent, Queue<PlanStep>> getPlanStepsFromPaths(Dictionary<Agent, List<Location>> agentLocDict, List<Order> orders)
        {
            Dictionary<Agent, Queue<PlanStep>> agenPlanStepDict= new Dictionary<Agent, Queue<PlanStep>>();
            foreach (var agent in agentLocDict.Keys)
            {
                Location l = agentLocDict[agent][0];
                if (l==null)
                {
                    Console.WriteLine("ASSERT");
                }
                PlanStep ps = new PlanStep { locationId = l.id, agentId = agent.id };
                agenPlanStepDict[agent] = new Queue<PlanStep>();
                agenPlanStepDict[agent].Enqueue(ps);

                if (agent.state == (int)Agent.states.idle) ps.type = (int)PlanStep.types.waiting;
                else if (agent.currentLocation.id == agent.assignedTask.id) 
                {
                    if (agent.assignedOrder == null) 
                    {
                        ps.type = (int)PlanStep.types.pickup;
                        ps.orderId = Algorithm.getOrderByCurrLocation(agent.currentLocation.id, orders).id;
                    } 
                    else 
                    {
                        ps.type = (int)PlanStep.types.deliver;
                        ps.orderId = agent.assignedOrder.id;
                    }
                    if (l.id !=  agent.currentLocation.id) 
                    {
                        Console.WriteLine("Invalid assignment. If agent stands on its assigned task location, the found location must be the same one");
                        throw new Exception();
                    }
                }
                else
                {
                    ps.type = (int)PlanStep.types.movement;
                }
            }
            return agenPlanStepDict;
        }
        public static Dictionary<Agent, Queue<PlanStep>> Run(List<Agent> agents, List<Location> locations, List<Order> orders, int time)
        {
            //currCost
            //Only for new agents
            //Each agent a has already assigned task = Location under  a.AssignedTask
            //Init First Node
            //While not solver
            //  LowerSearch(Node)
            //  -> split node into children
            //      sort them by price
            //      if we find goal node, we return it as a solution
            // Assign all agents first location in their path list for next step if this is call from CENTRAL

            CBS_CTNODE root = new CBS_CTNODE {};
            List<CBS_CTNODE> nodes = new List<CBS_CTNODE>();
            nodes.Add(root);
             
            foreach (var a in agents)
            {
                root.agentConstraintDict[a] = new List<CBS_CONSTRAINT>();
                root.agentPaths[a] = new List<Location>();
            }
            bool goal = false;
            while (goal == false)
            {
                foreach (var node in nodes)
                {
                    if (node.price == -1)
                    {
                        foreach (var a in agents)
                        {
                            if (node.agentPaths[a].Count == 0)
                            {
                                //path for agent, its task given its constraints
                                node.agentPaths[a] = getPathForAgentAndTask(a.currentLocation, a.assignedTask, node.agentConstraintDict[a], time);
                                if (node.agentPaths[a].Count == 0)
                                {
                                    node.price = int.MaxValue;
                                    break;
                                }
                                node.price += node.agentPaths[a].Count;
                            }
                        }
                    }
                }

                nodes = nodes.OrderBy(node => node.price).ToList();

                for (int i = 0; i < nodes.Count; i++)
                {
                    CBS_CONFLICT conflict = isValid(nodes[i].agentPaths, time);
                    if (conflict.empty == true)
                    {
                        return getPlanStepsFromPaths(nodes[i].agentPaths, orders);
                    }
                    else 
                    {
                        Agent a1 = Algorithm.getAgentById(conflict.agentId1, agents);
                        Agent a2 = Algorithm.getAgentById(conflict.agentId2, agents);
                        CBS_CONSTRAINT constraint = new CBS_CONSTRAINT {time = conflict.time, locationId = conflict.locationId, passageId = conflict.passageId };

                        List<CBS_CONSTRAINT> a1_list = new List<CBS_CONSTRAINT>(nodes[i].agentConstraintDict[a1]);
                        List<CBS_CONSTRAINT> a2_list = new List<CBS_CONSTRAINT>(nodes[i].agentConstraintDict[a2]);
                        a1_list.Add(constraint);
                        a2_list.Add(constraint);

                        CBS_CTNODE left = new CBS_CTNODE {agentConstraintDict = new Dictionary<Agent, List<CBS_CONSTRAINT>>(nodes[i].agentConstraintDict), agentPaths = new Dictionary<Agent, List<Location>>(nodes[i].agentPaths)};
                        CBS_CTNODE right = new CBS_CTNODE {agentConstraintDict = new Dictionary<Agent, List<CBS_CONSTRAINT>>(nodes[i].agentConstraintDict), agentPaths = new Dictionary<Agent, List<Location>>(nodes[i].agentPaths)};

                        left.agentConstraintDict[a1] = a1_list;
                        right.agentConstraintDict[a2] = a2_list;

                        left.agentPaths[a1] = new List<Location>();
                        right.agentPaths[a2] = new List<Location>();

                        nodes.Remove(nodes[i]); //mozna uplne smazat, nebo nejak lip vyresit ty constraint listy pres parent odkazy
                        nodes.Add(left);
                        nodes.Add(right);
                        break;
                    }
                }
            }

            return null;
        }

        public static List<Location> getPathForAgentAndTask(Location baseLocation, Location targetLocation, List<CBS_CONSTRAINT> CTList, int time)
        {
            if (baseLocation.id == targetLocation.id)
            {
                return new List<Location> { targetLocation };
            }

            //if Mock location is assigned, agent moves out of the way in case of collision otherwise stays in place
            if (targetLocation.id == Location.mockLocationId)
            {
                var adjacentSquares = getAccessableLocations(baseLocation, time+1, CTList);
                return new List<Location> { adjacentSquares[0] };
            }
            int g = 0;
            int h = 0;
            var openList = new List<Location>();
            var closedList = new List<Location>();
            Location start = baseLocation;
            Location target = targetLocation;
            Location current = start;
            List<Location> route = new List<Location>();

            openList.Add(start);
            int simulationTime = time;
            current.entranceTime = time;

            while (openList.Count > 0)
            {
                openList = openList.OrderBy(l => l.f).ToList();
                bool changed = false;
                foreach (var location in openList)
                {
                    if (location.entranceTime <= simulationTime)
                    {
                        current = location;
                        changed = true;
                        break;
                    }
                }
                if (!changed) return new List<Location>();
                current = openList[0];

                //pokud tu nejde nic vybrat, vezit nejlepsi moznost a pokracovat

                closedList.Add(current);
                simulationTime = current.entranceTime + 1;

                // remove it from the open list
                openList.Remove(current);

                if (current.id == target.id)
                {
                    break;
                }

                //var adjacentSquares = current.accessibleLocations;
                var adjacentSquares = getAccessableLocations(current, simulationTime, CTList);
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

            if (current.id != target.id) return new List<Location>();

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

        private static List<Location> getAccessableLocations(Location l, int t, List<CBS_CONSTRAINT> CTList)
        {
            
            List<Location> locations = new List<Location>();
            if (CTList.Where(x => x.time == t - 1 && x.locationId == l.id).ToList().Count == 0) locations.Add(l);
            foreach (var nei in l.accessibleLocations)
            {
                Passage p = Location.getPassageFromLocation(l, nei);
                if (CTList.Where(x => x.time == t-1 && (x.passageId == p.Id || x.locationId == nei.id)).ToList().Count == 0)
                {
                    locations.Add(nei);
                }
            }
            return locations;
        }

        //this is second part of lower search. it checks paths for conflicts. First/last conflict is returned as a new constraint.
        public static CBS_CONFLICT isValid(Dictionary<Agent, List<Location>> agentPaths, int t)
        {
            //bude se cistit po kazdem kroce
            Dictionary<int, string> locationAgentDict = new Dictionary<int, string>();
            Dictionary<int, string> passageAgentDict = new Dictionary<int, string>();
            int pointer = 0;
            int time = t;
            int maxPointerVal = 0;

            //finds the max value of pointer
            foreach (var key in agentPaths.Keys)
            {
                if (agentPaths[key].Count > maxPointerVal) maxPointerVal = agentPaths[key].Count;
            }

            List<Agent> agents = agentPaths.Keys.ToList();

            while (pointer < maxPointerVal )
            {
                locationAgentDict.Clear();
                passageAgentDict.Clear();
                foreach (var a in agents)
                {
                    if (agentPaths[a].Count <= pointer) continue; //TODO pridat jeho end lokaci a vyresit nejak ten confclit na target miste
                    Location l = agentPaths[a][pointer];
                    if (locationAgentDict.ContainsKey(l.id) == false) locationAgentDict[l.id] = a.id;
                    else
                    {
                        Agent a1 = Algorithm.getAgentById(locationAgentDict[l.id], agents);
                        return new CBS_CONFLICT { time = time, locationId = l.id, agentId1 = a1.id, agentId2 = a.id, passageId = -1 };
                    }
                }
                pointer++;
                time++;
            }
            return new CBS_CONFLICT { empty = true};
        }

        
    }
}
