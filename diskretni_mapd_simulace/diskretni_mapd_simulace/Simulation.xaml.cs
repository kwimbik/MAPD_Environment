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
        Simulace_Visual sv;



        //controls
        List<Button> location_buttons = new List<Button>();
        Dictionary<Button, Location> butt_loc_dict = new Dictionary<Button, Location>();

        public Simulation()
        {
            //grid 3 sloupce, simulace uprostred, data v levo, updaty (jaky vuz dokoncil jakou objednavku vlevo)
            InitializeComponent();
            mapParserIO mp = new mapParserIO("map.txt", db);
            sv = new Simulace_Visual(mp.readInputFile(), db);
            sv.Show();
            db.setLocationMap(sv.map.GetLength(0), sv.map[0].Length);
            db.setTestData(); //TODO: ruzne moznosti tesstovani, pro realny beh smazat
            generateGrid();
            //testt plan exe
            PlanReader pr = new PlanReader(sv, db);
            //pr.readPlan();
        }

        public void generateGrid()
        {
            Simulation_grid.RowDefinitions.Add(new RowDefinition());
            Simulation_grid.RowDefinitions.Add(new RowDefinition());
            Simulation_grid.RowDefinitions.Add(new RowDefinition());
            Simulation_grid.RowDefinitions.Add(new RowDefinition());
            Simulation_grid.RowDefinitions.Add(new RowDefinition());

            generateSetupPanel();
            generateMapPanel();

        }
        private void generateMapPanel()
        {
            Button createPlanBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Create plan",
            };
            createPlanBtn.Click += (sender, e) =>
            {
                //TODO: forms explorer with selection of file
            };
            Simulation_grid.Children.Add(createPlanBtn);
            Grid.SetColumn(createPlanBtn, 1);
            Grid.SetRow(createPlanBtn, 4);


        }


        private void generateSetupPanel()
        {
            Button fileButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Upload file",
            };
            fileButton.Click += (sender, e) =>
            {
                SimulationController.run = true;
                simulationThread = new Thread(new ThreadStart(runTSP));
                simulationThread.Start();
            };
            Simulation_grid.Children.Add(fileButton);
            Grid.SetColumn(fileButton, 0);
            Grid.SetRow(fileButton, 0);

           

            Button genMap = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Generate map",
            };
            genMap.Click += (sender, e) =>
            {
                //TODO: generate map with specific parameters
            };
            Simulation_grid.Children.Add(genMap);
            Grid.SetColumn(genMap, 0);
            Grid.SetRow(genMap, 1);

            Button chooseAlgo = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Choose algorhitm",
            };
            chooseAlgo.Click += (sender, e) =>
            {
                //TODO: selects algorithm for plan
            };
            Simulation_grid.Children.Add(chooseAlgo);
            Grid.SetColumn(chooseAlgo, 0);
            Grid.SetRow(chooseAlgo, 2);

            TextBlock tb = new TextBlock
            {
                Text = "Current output file: plan.txt",
                TextAlignment = TextAlignment.Center,
                
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            Simulation_grid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 3);

            Button outputFileBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Choose different outputFile",
            };
            outputFileBtn.Click += (sender, e) =>
            {
                //TODO: selects output file in explorer -> forms component
            };
            Simulation_grid.Children.Add(outputFileBtn);
            Grid.SetColumn(outputFileBtn, 0);
            Grid.SetRow(outputFileBtn, 4);

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
                    MessageBox.Show("All orders have been delivered");
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

