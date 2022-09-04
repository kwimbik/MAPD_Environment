using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.ConstraintSolver;


namespace diskretni_mapd_simulace
{
    internal class Routing_solver
    {
        Routing_solverManager svm;
        public Routing_solver(Routing_solverManager sm)
        {
            svm = sm;
        }

        public void solveProblemAndPrintResults()
        {
            // Create Routing Index Manager
            RoutingIndexManager manager = new RoutingIndexManager(
                svm.TimeMatrix.GetLength(0),
                svm.VehicleNumber,
                svm.Depot);

            // Create Routing Model.
            RoutingModel routing = new RoutingModel(manager);

            // Create and register a transit callback.
            int transitCallbackIndex = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
            {
                // Convert from routing variable Index to time
                // matrix NodeIndex.
                var fromNode = manager.IndexToNode(fromIndex);
                var toNode = manager.IndexToNode(toIndex);
                return svm.TimeMatrix[fromNode][toNode];
            });

            // Define cost of each arc.
            routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

            // Add Time constraint.
            routing.AddDimension(transitCallbackIndex, // transit callback
                                 30,                   // allow waiting time
                                 1000,                 // capacities are solved in different dimension
                                 false,                // start cumul to zero
                                 "Time");
            RoutingDimension timeDimension = routing.GetMutableDimension("Time");

            int demandCallbackIndex = routing.RegisterUnaryTransitCallback((long fromIndex) =>
            {
                // Convert from routing variable Index to
                // demand NodeIndex.
                var fromNode =
                    manager.IndexToNode(fromIndex);
                return svm.Demands[fromNode];
            });
            routing.AddDimensionWithVehicleCapacity(demandCallbackIndex, 0, // null capacity slack
                                                    svm.VehicleCapacities, // vehicle maximum capacities
                                                    true,                   // start cumul to zero
                                                    "Capacity");

            // Add time window constraints for each location except depot.
            for (int i = 1; i < svm.TimeWindows.GetLength(0); ++i)
            {
                long index = manager.NodeToIndex(i);
                timeDimension.CumulVar(index).SetRange(svm.TimeWindows[i][0], svm.TimeWindows[i][1]);
            }
            // Add time window constraints for each vehicle start node.
            for (int i = 0; i < svm.VehicleNumber; ++i)
            {
                long index = routing.Start(i);
                timeDimension.CumulVar(index).SetRange(svm.TimeWindows[0][0], svm.TimeWindows[0][1]);
            }

            // Instantiate route start and end times to produce feasible times.
            for (int i = 0; i < svm.VehicleNumber; ++i)
            {
                routing.AddVariableMinimizedByFinalizer(timeDimension.CumulVar(routing.Start(i)));
                routing.AddVariableMinimizedByFinalizer(timeDimension.CumulVar(routing.End(i)));
            }

            // Add Distance constraint.
            routing.AddDimension(
                transitCallbackIndex,
                0,
                3000, // capacity is set manualy from input
                true, // start cumul to zero
                "Distance");
            RoutingDimension distanceDimension = routing.GetMutableDimension("Distance");
            distanceDimension.SetGlobalSpanCostCoefficient(100);

            // Define Transportation Requests.
            Solver solver = routing.solver();
            for (int i = 0; i < svm.PickupsDeliveries.GetLength(0); i++)
            {
                long pickupIndex = manager.NodeToIndex(svm.PickupsDeliveries[i][0]);
                long deliveryIndex = manager.NodeToIndex(svm.PickupsDeliveries[i][1]);
                routing.AddPickupAndDelivery(pickupIndex, deliveryIndex);
                solver.Add(solver.MakeEquality(
                    routing.VehicleVar(pickupIndex),
                    routing.VehicleVar(deliveryIndex)));
                solver.Add(solver.MakeLessOrEqual(
                    distanceDimension.CumulVar(pickupIndex),
                    distanceDimension.CumulVar(deliveryIndex)));
            }

            // Setting first solution heuristic.
            RoutingSearchParameters searchParameters =
                operations_research_constraint_solver.DefaultRoutingSearchParameters();


            // Solve the problem.
            solver.MakeSolutionsLimit(1);
            Assignment solution = routing.SolveWithParameters(searchParameters);

            }
        }
}
