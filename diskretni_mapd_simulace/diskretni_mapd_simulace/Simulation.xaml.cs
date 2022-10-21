using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace diskretni_mapd_simulace
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : Window
    {
        Database db = new Database();
        Routing_solver rs;
        Routing_solverManager rsm;
        Thread simulationThread;



        //controls
        List<Button> location_buttons = new List<Button>();
        Dictionary<Button, Location> butt_loc_dict = new Dictionary<Button, Location>();
        TextBox update_textbox;
        TextBox vehicleOrderPosition_textbox;

        public Simulation()
        {
            //grid 3 sloupce, simulace uprostred, data v levo, updaty (jaky vuz dokoncil jakou objednavku vlevo)
            InitializeComponent();
            generateGrid();
            mapParserIO mp = new mapParserIO("map.txt", db);
            simulace_visual sv = new simulace_visual(mp.readInputFile(), db);
            sv.Show();
            db.setLocationMap(sv.map.GetLength(0), sv.map[0].Length);
            db.setTestData(); //TODO: ruzne moznosti tesstovani, pro realny beh smazat

            //testt plan exe
            PlanReader pr = new PlanReader(sv, db);
            pr.readPlan();
        }



        public void generateGrid()
        {
            //add textbox for updates on simulation
            TextBox tb = new TextBox
            {
                Name = "updates_textbox",
                Text = "Updates",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Simulation_grid.Children.Add(tb);
            Grid.SetColumn(tb, 2);
            update_textbox = tb;

            //add textbox for vehicle positions
            TextBox tbv = new TextBox
            {
                Name = "vehiclePosition_textbox",
                Text = "Vehicles",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Simulation_grid.Children.Add(tbv);
            Grid.SetColumn(tbv, 0);
            vehicleOrderPosition_textbox = tbv;
        }

        private void solve_btn_Click(object sender, RoutedEventArgs e)
        {
            SimulationController.run = true;
            simulationThread = new Thread(new ThreadStart(runTSP));
            simulationThread.Start();
        }

        private void runTSP()
        {
            Routing_solverManager rsm = new Routing_solverManager(db);
            rsm.getSolutionData();
            if (rsm.ordersToProcess.Count == 0)
            {
                //TODO: add some steps or meters -> store in resultManager
                this.Dispatcher.Invoke(() =>
                {
                    update_textbox.Text = "All Orders have been delivered";
                });
            }
            else
            {
                Routing_solver rs = new Routing_solver(rsm);
                RoutingSolverResults result = rs.solveProblemAndPrintResults();
            }
        }

        private void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            SimulationController.run = false;
        }
    }
}

