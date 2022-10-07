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
        RoutingSolverResults result;

        public string orderInfo { get; set; }
        /// <summary>
        /// For each vehicle, figures new location. If order was on vehicle, it moves as well
        /// </summary>
        public void setResultAndAct(RoutingSolverResults result)
        {
            this.result = result;

            
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
                        Console.WriteLine($"objednavka {order.Id} nalozena na miste {order.currLocation.id}");
                        orderInfo += $"objednavka {order.Id} nalozena na miste {order.currLocation.id} \n";
                    }
                }


                var index = result.routingModel.Start(i);
                index = result.solution.Value(result.routingModel.NextVar(index));

                //presun auta do nove lokace
                Location nextLocation = result.routingSolverManager.indexToLocationMap[result.routingIndexManager.IndexToNode(index)];

                //in case order is to be picked up, even though solver location differs, actual location is the same
                if (nextLocation == vehicle.baseLocation && result.routingModel.IsEnd(index) == false)
                {
                    index = result.solution.Value(result.routingModel.NextVar(index));
                    nextLocation = result.routingSolverManager.indexToLocationMap[result.routingIndexManager.IndexToNode(index)];
                }
                
                vehicle.baseLocation.vehicles.Remove(vehicle);

                List<Order> delivered = new List<Order>();
                foreach ( Order order in vehicle.orders)
                {
                    if (vehicle.baseLocation == order.targetLocation)
                    {
                        Console.WriteLine($"objednavka {order.Id} vylozena na miste {order.targetLocation.id}");
                        orderInfo += $"objednavka {order.Id} vylozena na miste {order.targetLocation.id} \n";
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

        public string getOrderInfo()
        {
            return orderInfo;
        }
    }
}
