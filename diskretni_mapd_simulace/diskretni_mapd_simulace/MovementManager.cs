using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.ConstraintSolver;


namespace diskretni_mapd_simulace
{
    public class MovementManager
    {
        Database db;

        public string orderInfo { get; set; }
        /// <summary>
        /// For each vehicle, figures new location. If order was on vehicle, it moves as well
        /// </summary>
        /// 

        public MovementManager(Database db)
        {
            this.db = db;
            orderInfo = "";
        }

        /// <summary>
        /// if all vehicles have target location, no need for optimization, set movement instead
        /// </summary>
        public void moveToTargetLocation()
        {
            orderInfo = ""; //reset updates from previous tick
            foreach (Vehicle vehicle in db.vehicles)
            {
                Location nextLocation = getNewLocation(vehicle.baseLocation, vehicle.targetLocation);
                if (vehicle.baseLocation.id == vehicle.targetLocation.id)
                {
                    vehicle.targetLocation = null;

                    //pridat objednavku na auto, pokud je na nextLocation (prozatim vsechny na tom poli, to se upravi) TODO
                    foreach (Order order in vehicle.baseLocation.orders)
                    {
                        if (vehicle.orders.Contains(order) == false)
                        {
                            vehicle.orders.Add(order);
                            Console.WriteLine($"objednavka {order.Id} nalozena na miste {order.currLocation.id}");
                            orderInfo += $"objednavka {order.Id} nalozena na miste {order.currLocation.id} \n";
                        }
                    }
                }
                else
                {
                    vehicle.baseLocation.vehicles.Remove(vehicle);

                    vehicle.baseLocation = nextLocation;
                    nextLocation.vehicles.Add(vehicle);
                }
               

                List<Order> delivered = new List<Order>();
                foreach (Order order in vehicle.orders)
                {
                    if (vehicle.baseLocation == order.targetLocation)
                    {
                        Console.WriteLine($"objednavka {order.Id} vylozena na miste {order.targetLocation.id} by vehicle {vehicle.Id}");
                        orderInfo += $"objednavka {order.Id} vylozena na miste {order.targetLocation.id} by vehicle {vehicle.Id} \n";
                        delivered.Add(order);
                        vehicle.baseLocation.orders.Remove(order);
                        order.state = (int)Order.states.delivered;
                        vehicle.targetLocation = null;
                    }
                    else
                    {
                        order.currLocation.orders.Remove(order);
                        order.currLocation = nextLocation;
                        nextLocation.orders.Add(order);
                    }
                }

                //remove delivered orders
                foreach (Order order in delivered)
                {
                    vehicle.orders.Remove(order);
                }
               
            }
        }


        public void setResultAndAct(RoutingSolverResults result)
        {
            orderInfo = ""; //reset updates from previous tick
            for (int i = 0; i < result.routingSolverManager.VehicleNumber; ++i)
            {
                Vehicle vehicle = result.routingSolverManager.indexToVehicleMap[i];
                //first location is depot, second is first order or depot if vehicle does not move
                
                //pridat objednavku na auto, pokud je na nextLocation (prozatim vsechny na tom poli, to se upravi) TODO
                foreach (Order order in vehicle.baseLocation.orders)
                {
                    if (vehicle.orders.Contains(order) == false)
                    {
                        vehicle.orders.Add(order);
                        Console.WriteLine($"objednavka {order.Id} nalozena na miste {order.currLocation.id} by vehicle {vehicle.Id} ");
                        orderInfo += $"objednavka {order.Id} nalozena na miste {order.currLocation.id} by vehicle {vehicle.Id} \n";
                    }
                }

                var index = result.routingModel.Start(i);
                index = result.solution.Value(result.routingModel.NextVar(index));

                //Problem je, kdyz auto jezdi mezi 2 zakazkama a nemuze si vybrat -> pouziti targetLocation u aut a resit pouze s autama a zakazkama, ktery nejsou commitnuty
                Location nextLocation = getNewLocation(vehicle.baseLocation, result.routingSolverManager.indexToLocationMap[result.routingIndexManager.IndexToNode(index)]);

                //in case order is to be picked up, even though solver location differs, actual location is the same
                if (nextLocation == vehicle.baseLocation && result.routingModel.IsEnd(index) == false)
                {
                    index = result.solution.Value(result.routingModel.NextVar(index));
                    nextLocation = getNewLocation(vehicle.baseLocation, result.routingSolverManager.indexToLocationMap[result.routingIndexManager.IndexToNode(index)]);
                }


                //sets target location for vehicle so that it is not disturbed by another solver step
                vehicle.targetLocation = result.routingSolverManager.indexToLocationMap[result.routingIndexManager.IndexToNode(index)];
                vehicle.baseLocation.vehicles.Remove(vehicle);

                List<Order> delivered = new List<Order>();
                foreach ( Order order in vehicle.orders)
                {
                    if (vehicle.baseLocation == order.targetLocation)
                    {
                        Console.WriteLine($"objednavka {order.Id} vylozena na miste {order.targetLocation.id} by vehicle {vehicle.Id} ");
                        orderInfo += $"objednavka {order.Id} vylozena na miste {order.targetLocation.id} by vehicle {vehicle.Id}\n";
                        delivered.Add(order);
                        vehicle.baseLocation.orders.Remove(order);
                        order.state = (int)Order.states.delivered;
                    }
                    else
                    {
                        order.currLocation.orders.Remove(order);
                        order.currLocation = nextLocation;
                        nextLocation.orders.Add(order);
                    }
                }

                //remove delivered orders
                foreach (Order order in delivered)
                {
                    vehicle.orders.Remove(order);
                }
                vehicle.baseLocation = nextLocation;
                nextLocation.vehicles.Add(vehicle);
            }
        }

        //return new Location for vehicle based on last location
        public Location getNewLocation(Location baseLocation, Location targetLocation)
        {
            if (baseLocation == targetLocation) return baseLocation;
            if (Math.Abs(baseLocation.coordination[0] - targetLocation.coordination[0]) > Math.Abs(baseLocation.coordination[1] - targetLocation.coordination[1]))
            {
                //if true, need to go up
                if (baseLocation.coordination[0] > targetLocation.coordination[0])
                {
                    return db.locationMap[baseLocation.coordination[0]-1][baseLocation.coordination[1]];
                }
                //need to go down
                else
                {
                    return db.locationMap[baseLocation.coordination[0] + 1][baseLocation.coordination[1]];
                }
            }
            else
            {
                //if true, need to go left
                if (baseLocation.coordination[1] > targetLocation.coordination[1])
                {
                    return db.locationMap[baseLocation.coordination[0]][baseLocation.coordination[1]-1];
                }
                //need to go right
                else
                {
                    return db.locationMap[baseLocation.coordination[0]][baseLocation.coordination[1]+1];
                }
            }
        }

        public string getOrderInfo()
        {
            return orderInfo;
        }
    }
}
