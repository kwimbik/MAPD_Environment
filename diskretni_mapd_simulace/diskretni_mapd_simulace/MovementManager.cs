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

       
        /// <summary>
        /// For each vehicle, figures new location. If order was on vehicle, it moves as well
        /// </summary>
        public void setResultAndAct(RoutingSolverResults result)
        {
            this.result = result;
            for (int i = 0; i < result.routingSolverManager.VehicleNumber; ++i)
            {
                Vehicle vehicle = result.routingSolverManager.indexToVehicleMap[i];
                //first location is depot, second is first order or depot if vehicle does not move
                var index = result.routingModel.Start(i);
                index = result.solution.Value(result.routingModel.NextVar(index));

                //presun auta do nove lokace
                Location nextLocation = result.routingSolverManager.indexToLocationMap[result.routingIndexManager.IndexToNode(index)];
                vehicle.baseLocation.vehicles.Remove(vehicle);

                foreach ( Order order in vehicle.orders)
                {
                    if (vehicle.baseLocation == order.targetLocation)
                    {
                        Console.WriteLine($"objednavka {order.Id} vylozena na miste {order.targetLocation.id}");
                        vehicle.orders.Remove(order);
                    }
                    else
                    {
                        order.currLocation.orders.Remove(order);
                        order.currLocation = nextLocation;
                        nextLocation.orders.Add(order);
                    }
                }
                vehicle.baseLocation = nextLocation;
                nextLocation.vehicles.Add(vehicle);

                //pridat objednavku na auto, pokud je na nextLocation
                foreach (Order order in nextLocation.orders)
                {
                    vehicle.orders.Add(order);
                }
            }
        }
    }
}
