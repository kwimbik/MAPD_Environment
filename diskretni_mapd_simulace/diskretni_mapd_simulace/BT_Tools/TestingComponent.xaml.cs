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
        Grid g = new Grid();
        Database db;

        public TestingComponent(Database db)
        {
            InitializeComponent();
            createLayout();
            this.Content = g;
            this.db = db;
        }


        private void createLayout()
        {
            StackPanel sp = new StackPanel();
            g.Children.Add(sp);
            Label orderNum = new Label() { Content = "Num of orders" };
            sp.Children.Add(orderNum);

            TextBox orderNum_tb = new TextBox
            {
                Text = "",
            };
            sp.Children.Add(orderNum_tb);

            Label agentNum = new Label() { Content = "Range of agents" };
            sp.Children.Add(agentNum);

            TextBox agentNum_tb = new TextBox
            {
                Text = "",
            };
            sp.Children.Add(agentNum_tb);

            Label runsNum = new Label() { Content = "Number of runs" };
            sp.Children.Add(runsNum);

            TextBox runsNum_tb = new TextBox
            {
                Text = "",
            };
            sp.Children.Add(runsNum_tb);

            Button start_btn = new Button
            {
                Content = "Start",
            };
            sp.Children.Add(start_btn);

            start_btn.Click += (sender, e) =>
            {
                PlanCreator planCreator = new PlanCreator(db);
                int numOfOrders = int.Parse(orderNum_tb.Text);
               
                
                planCreator.setSettings(3, numOfOrders); //set BT settings + num of orders


                int numOfAgents = int.Parse(agentNum_tb.Text);
                int numOfRuns = int.Parse(runsNum_tb.Text);
                for (int i = 0; i < numOfAgents; i++)
                {
                    for (int j = 0; j < numOfRuns; j++)
                    {
                        planCreator.LoadScenario();
                        Plan plan = planCreator.Solve();
                        SolutionPacket sp = plan.solutionPacket;

                        ExperimentDataWriter.WritedData(sp, "BT_data/data");
                    }
                }
               
            };
        }
    }
}
