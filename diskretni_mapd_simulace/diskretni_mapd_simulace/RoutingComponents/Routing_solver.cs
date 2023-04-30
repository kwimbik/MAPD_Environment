using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.ConstraintSolver;
using System.Diagnostics;


namespace diskretni_mapd_simulace
{
    internal class Routing_solver
    {
        Routing_solverManager svm;
        public Routing_solver(Routing_solverManager sm)
        {
            svm = sm;
        }

        static void PrintSolution(in RoutingSolverResults rsr)
        {
            Console.WriteLine($"Objective {rsr.solution.ObjectiveValue()}:");

            // Inspect solution.
            RoutingDimension timeDimension = rsr.routingModel.GetMutableDimension("Time");
            long totalTime = 0;
            for (int i = 0; i < rsr.routingSolverManager.AgentNumber; ++i)
            {
                Console.WriteLine("Route for Agent {0}:", i);
                var index = rsr.routingModel.Start(i);
                while (rsr.routingModel.IsEnd(index) == false)
                {
                    var timeVar = timeDimension.CumulVar(index);
                    Console.Write("{0} Time({1},{2}) -> ", rsr.routingIndexManager.IndexToNode(index), rsr.solution.Min(timeVar),
                                  rsr.solution.Max(timeVar));
                    index = rsr.solution.Value(rsr.routingModel.NextVar(index));
                }
                var endTimeVar = timeDimension.CumulVar(index);
                Console.WriteLine("{0} Time({1},{2})", rsr.routingIndexManager.IndexToNode(index), rsr.solution.Min(endTimeVar),
                                  rsr.solution.Max(endTimeVar));
                Console.WriteLine("Time of the route: {0}min", rsr.solution.Min(endTimeVar));
                totalTime += rsr.solution.Min(endTimeVar);
            }
            Console.WriteLine("Total time of all routes: {0}min", totalTime);
        }

        //misto void vratit objekt vysledku, co pak poslu adekvatni komponente na zpracovani
        public RoutingSolverResults solveProblemAndPrintResults()
        {
            // Create Routing Index Manager
            RoutingIndexManager manager = new RoutingIndexManager(
                svm.TimeMatrix.GetLength(0),
                svm.AgentNumber,
                svm.Depot,
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

            // Add Time constraint, for #orders > 100 set to maybe 3000-4000 time per agent
            routing.AddDimension(transitCallbackIndex, // transit callback
                                 100,                   // allow waiting time
                                 2000,                 // capacities are solved in different dimension
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
                                                    svm.AgentCapacities, // vehicle maximum capacities
                                                    true,                   // start cumul to zero
                                                    "Capacity");

            // Add time window constraints for each location except depot.
            for (int i = 1; i < svm.TimeWindows.GetLength(0); ++i)
            {
                long index = manager.NodeToIndex(i);
                timeDimension.CumulVar(index).SetRange(svm.TimeWindows[i][0], svm.TimeWindows[i][1]);
            }
            // Add time window constraints for each vehicle start node.
            for (int i = 0; i < svm.AgentNumber; ++i)
            {
                long index = routing.Start(i);
                timeDimension.CumulVar(index).SetRange(svm.TimeWindows[0][0], svm.TimeWindows[0][1]);
            }

            // Instantiate route start and end times to produce feasible times.
            for (int i = 0; i < svm.AgentNumber; ++i)
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
            distanceDimension.SetGlobalSpanCostCoefficient(200);

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
            RoutingSolverResults srs = new RoutingSolverResults(svm, routing, manager, solution);

            PrintSolution(srs);
            return srs;
        }
    }
}
