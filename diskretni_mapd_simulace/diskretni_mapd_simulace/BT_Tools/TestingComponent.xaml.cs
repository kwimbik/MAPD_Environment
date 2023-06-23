using diskretni_mapd_simulace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace diskretni_mapd_simulace.BT_Tools
{
    /// <summary>
    /// Interaction logic for TestingComponent.xaml
    /// </summary>
    public partial class TestingComponent : Window
    {
        PlanCreator pr;
        Grid g = new Grid();
        Database db;
        Simulation s;
        Simulace_Visual sv;
        int[] agents = {30};
        double[] frequencies = {0.5};
        int numberOfOrders = 200;
        int seed = 43;
        


        public TestingComponent(Database db, Simulation s, Simulace_Visual sv, PlanCreator pr)
        {
            InitializeComponent();
            this.db = db;
            this.sv = sv;
            this.s = s;
            this.pr = pr;
            createLayout();
            this.Content = g;
        }


        private void createLayout()
        {
            StackPanel sp = new StackPanel();
            g.Children.Add(sp);
            Label orderNum = new Label() { Content = "Number of Orders" };
            sp.Children.Add(orderNum);

            TextBox orderNum_tb = new TextBox
            {
                Text = numberOfOrders.ToString(),
            };
            sp.Children.Add(orderNum_tb);

            Label orderFreq = new Label() { Content = "Order frequencies"  };
            sp.Children.Add(orderFreq);

            TextBox orderFreq_tb = new TextBox
            {
                Text = string.Join(",", frequencies),
            };
            sp.Children.Add(orderFreq_tb);

            Label agentNum = new Label() { Content = "Number of agents"  };
            sp.Children.Add(agentNum);

            TextBox agentNum_tb = new TextBox
            {
                Text = string.Join(",", agents),
            };
            sp.Children.Add(agentNum_tb);

            Label seedNum = new Label() { Content = "seed"  };
            sp.Children.Add(seedNum);

            TextBox seedNumTxb = new TextBox
            {
                Text = seed.ToString(),
            };
            sp.Children.Add(seedNumTxb);

            
            Button start_btn = new Button
            {
                Content = "Start",
            };
            sp.Children.Add(start_btn);

            start_btn.Click += (sender, e) =>
            {
                pr.setSettings(3, numberOfOrders); //set BT settings + num of orders
                for (int i = 0; i < agents.Length; i++)
                {
                    db.generateAgents(agents[i], seed);

                    for (int j = 0; j < frequencies.Length; j++)
                    {
                        db.setFrequencies(frequencies[j]);
                        var validScenario = pr.LoadScenario();
                        if (validScenario)
                        {
                            Plan plan = pr.Solve();
                            SolutionPacket sp = plan.solutionPacket;
                            int movements = plan.steps.Where(x => x.type == (int)PlanStep.types.movement).Count();
                            int waiting = plan.steps.Where(x => x.type == (int)PlanStep.types.waiting).Count();
                            double cost = movements + 0.1*waiting;

                            ExperimentDataWriter.WritedData(sp, frequencies[j],seed, "BT_data/Experiment2_WAREHOUSE.csv", movements, waiting, cost);
                        }
                        else
                        {
                            //throw new ArgumentException("Wrong scenario");
                            Console.WriteLine(  );
                        }
                       
                    }
                }
               
            };
        }
    }
}
