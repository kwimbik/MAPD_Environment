using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.ConstraintSolver;


namespace diskretni_mapd_simulace
{
    //in Routing_solverManager svm, in RoutingModel routing, in RoutingIndexManager manager,
    //in Assignment solution
    public class RoutingSolverResults
    {
        public Routing_solverManager routingSolverManager;
        public RoutingModel routingModel;
        public RoutingIndexManager routingIndexManager;
        public Assignment solution;

        public  RoutingSolverResults(Routing_solverManager routingSolverManager, RoutingModel routingModel, RoutingIndexManager routingIndexManager,
                              Assignment solution)
        {
            this.routingSolverManager = routingSolverManager;
            this.routingModel = routingModel;
            this.routingIndexManager = routingIndexManager;
            this.solution = solution;
        }

    }
}
