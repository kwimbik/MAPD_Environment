using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using diskretni_mapd_simulace.IO_Tools;
using diskretni_mapd_simulace.Plan_Tools;
using diskretni_mapd_simulace.Windows;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using diskretni_mapd_simulace.Entities;
using OperationsResearch.Pdlp;
using System.Collections.Generic;
using System.Numerics;

namespace diskretni_mapd_simulace

{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : Window
    {
        Database db = new Database();
        Thread assignThread;
        Simulace_Visual sv;
        
        PlanCreator planCreator;
        PlanReader planReader;

        //grid settings for window generation
        private int grid_height = 5;
        private int buttonHeight = 65;


        //upload file names -> might delete if doesnt work properly and just create them for every load instead
        private string uploadMapFile = "";
        private string uploadScenarioFile = "";
        private string uploadPlanFile = "";

        private Button loadMapButton;
        private Button loadScenarioButton;
        private Button loadPlanFileButton;

        bool mapLoaded = false;
        bool scenarioLoaded = false;
        bool planLoaded = false;

        //folder
        string plans_folder = "plans/";


        //name of the output file -> "name of the map".plan
        private string outputFile = "map.plan";

        private bool timedOut = false;


        TextBlock info;
        //Label outputFileString;

        bool testMode = true;

        public Simulation()
        {
            //Sets windows initial settings
            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.NoResize; 
            InitializeComponent();
            planCreator = new PlanCreator(db);
            planReader = new PlanReader(db);

            generateGrid();
            restrictButtons();



            ///TESTING MODE
            if (testMode)
            {
                mapParserIO mp = new mapParserIO("maps/room-64-64-16.map", db);
                sv = new Simulace_Visual(mp.readInputFile(), db, planReader, this);
                planReader.sv = sv;
                db.mapName = "room-64-64-16-random";
                mapLoaded = true;
                db.outputFile = outputFile;
                //db.setTestData();
                db.loadMap(sv.map.GetLength(0), sv.map[0].Length);
                sv.createVisualization();
                //Added so I dont have to switch tabs every time for debug
                //sv.WindowState = WindowState.Minimized;
                sv.Show();

                ScenarioParserIO sp = new ScenarioParserIO("mapd-scenarios/room-64-64-16-random.scen", db);
                sp.loadScenario();
                scenarioLoaded = true;
                //sv.createPlanMap();
                updateUI();
            }
        }

        //grid generating function: Set rows, and set 3 columns in their fction respectively
        public void generateGrid()
        {
            for (int i = 0; i < grid_height; i++)
            {
                Simulation_grid.RowDefinitions.Add(new RowDefinition());
            }

            Simulation_grid.Style = (Style)FindResource("GridTheme");
            generateSetupPanel();
            generateMapPanel();
            generatePlanPanel();
        }

        private void generatePlanPanel()
        {
            StackPanel sp = new StackPanel();
            Simulation_grid.Children.Add(sp);
            Grid.SetColumn(sp, 2);
            Grid.SetRowSpan(sp, grid_height);

            Button addAgentBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Add new agent",
                Style  = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            addAgentBtn.Click += (sender, e) =>
            {
                NewAgentWindow naw = new NewAgentWindow(db, this, sv);
                naw.Show();

            };
            sp.Children.Add(addAgentBtn);
            
            Button addOrderBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Add new order",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };

            addOrderBtn.Click += (sender, e) =>
            {
                NewOrderWindow now = new NewOrderWindow(db, this, sv);
                now.Show();

            };
            sp.Children.Add(addOrderBtn);

            Button clearOrders_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Clear orders",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };

            //select stress test/number of orders -> scenario and map stays the same
            clearOrders_btn.Click += (sender, e) =>
            {
                if (planReader.readerStatus != (int)PlanReader.status.reading)
                {
                    db.clearOrders();
                    updateUI();
                }
                else
                {
                    MessageBox.Show("Unable to clear orders, wait for plan to finish");
                }

            };
            sp.Children.Add(clearOrders_btn);

            Button clearScenario_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Clear scenario",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };

            //New scenario or plan needs to be uploaded
            clearScenario_btn.Click += (sender, e) =>
            {
                if (planReader.readerStatus != (int)PlanReader.status.reading)
                {
                    db.clearScenario();
                    scenarioLoaded = false;
                    planLoaded = false;
                    updateUI();
                }
                else
                {
                    MessageBox.Show("Unable to clear scenario, wait for plan to finish");
                }
            };
            sp.Children.Add(clearScenario_btn);

            Button clearMap_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Clear map",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            sp.Children.Add(clearMap_btn);

            Button help_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Content = "Help",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
                Width = buttonHeight,
            };
            Simulation_grid.Children.Add(help_btn);
            Grid.SetColumn(help_btn, 2);
            Grid.SetRowSpan(help_btn, grid_height);

            help_btn.Click += (sender, e) =>
            {
                //Process.Start();
                Process p = new Process();
                p.StartInfo.FileName = (@"Documentation\mapd_user_documentation.pdf");
                p.StartInfo.UseShellExecute = true;
                p.Start();
            };

                //new map + scenario/plan needsto be uploaded
                clearMap_btn.Click += (sender, e) =>
            {
                if (planReader.readerStatus != (int)PlanReader.status.reading)
                {
                    sv.Close();
                    db.clearMap();

                    //creates empty sv placeholder
                    sv = new Simulace_Visual();

                    mapLoaded = false;
                    scenarioLoaded = false;
                    planLoaded = false;
                    updateUI();
                }
                else
                {
                    MessageBox.Show("Unable to clear map, wait for plan to finish");
                }
            };
        }

        private void generateMapPanel()
        {
            Border sp_bod = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1,0,1,0),
            };
            StackPanel sp = new StackPanel();

            sp_bod.Child = sp;
            Simulation_grid.Children.Add(sp_bod);
            Grid.SetColumn(sp_bod, 1);
            Grid.SetRowSpan(sp_bod, grid_height);

            TextBlock size_tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Text = "Info",
                TextAlignment = TextAlignment.Center,  
            };
            sp.Children.Add(size_tb);
            info = size_tb;

            CheckBox stressTest_cb = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "stress test",
            };

           
            sp.Children.Add(stressTest_cb);

             CheckBox showPlan_cb = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "show plan",
                IsChecked = true,
            };
            sp.Children.Add(showPlan_cb);
            
            CheckBox outputIndividualPlans_cb = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Output all planns",
                IsChecked = false,
                IsEnabled = false,
            };
            sp.Children.Add(outputIndividualPlans_cb);

            WrapPanel wp2 = new WrapPanel();

            TextBox limit_tb = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 15,
                Width = 50,
                Text = "",
                IsEnabled = false,
            };

            TextBlock limit_tblock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Text = "Time limit in seconds",
                FontSize = 15,
            };

            wp2.Children.Add(limit_tb);
            wp2.Children.Add(limit_tblock);
            sp.Children.Add(wp2);

            WrapPanel wp = new WrapPanel();


            TextBox numOfOrders_tb = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 15,
                Width = 50,
                Text = "",
            };

            TextBlock numberOfOrders_tblock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Text = "Number of Orders",
                FontSize = 15,
            };

            wp.Children.Add(numOfOrders_tb);
            wp.Children.Add(numberOfOrders_tblock);
            sp.Children.Add(wp);

          
            Button save_Btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Save",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
                IsEnabled = true,
            };
            sp.Children.Add(save_Btn);

            Button createPlanBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Create plan",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
                IsEnabled = false,
            };
            sp.Children.Add(createPlanBtn);

           
            showPlan_cb.Checked += (sender, e) =>
            {
                planCreator.setShowPlan(true);
            };

            showPlan_cb.Unchecked += (sender, e) =>
            {
                planCreator.setShowPlan(false);
            };

            //sets the current settings for plan creating
            save_Btn.Click += (sender, e) =>
            {
                if (scenarioLoaded == false)
                {
                    MessageBox.Show("Please load scenario first");
                    return;
                }
                if (planReader.readerStatus == (int)PlanReader.status.reading)
                {
                    //need to free the file for new plan
                    planReader.stopPlanExecution();
                    sv.forcePause();
                }

                if (stressTest_cb.IsChecked == true)
                {
                    if (outputIndividualPlans_cb.IsChecked == true)
                    {
                        //creates all planns
                        planCreator.setSettings(0, 1);
                    }
                    else
                    {
                        //does not create individual plans
                        planCreator.setSettings(2, 1);
                    }
                    int numOfSeconds;
                    bool successfullyParsed = int.TryParse(limit_tb.Text, out numOfSeconds);
                    if (successfullyParsed && numOfSeconds > 0)
                    {
                        planCreator.secondsForStressTestTimeOut= numOfSeconds;
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid number of seconds");
                        return;
                    }
                }
                else
                {
                    int numOfOrders;
                    bool successfullyParsed = int.TryParse(numOfOrders_tb.Text, out numOfOrders);
                    if (successfullyParsed && numOfOrders > 0)
                    {
                        if (numOfOrders > db.scenario.orders.Count) numOfOrders = db.scenario.orders.Count;
                        numOfOrders_tb.Text = numOfOrders.ToString();
                        planCreator.setSettings(1, numOfOrders);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid number of orders");
                        return;
                    }
                }
                sv.removeOrderColor();
                planCreator.LoadScenario();
                //if (!testMode) MessageBox.Show("Orders have been assigned to agents");
                createPlanBtn.IsEnabled = true;
                updateUI();
            };

            stressTest_cb.Checked += (sender, e) =>
            {
                numOfOrders_tb.IsEnabled = false;
                showPlan_cb.Content = "show plan for best solution";
                createPlanBtn.Content = "Create plans";
                limit_tb.IsEnabled = true;

                outputIndividualPlans_cb.IsEnabled = true; //for single scenario, plan is always output
            };

            stressTest_cb.Unchecked += (sender, e) =>
            {
                showPlan_cb.Content = "show plan";
                createPlanBtn.Content = "Create plan";
                numOfOrders_tb.IsEnabled = true;
                limit_tb.IsEnabled = false;


                outputIndividualPlans_cb.IsEnabled = false; //for single scenario, plan is always output
            };

            
            createPlanBtn.Click += async (sender, e) =>
            {
                bool createPlan = showPlan_cb.IsChecked == true;

                //simple create plan for given problem + export it to .plan
                if (stressTest_cb.IsChecked == false)
                {
                    Plan plan = planCreator.Solve();
                    db.currentPlan = plan;
                    Task readPlanTask = new Task(() => planReader.readPlan(db.outputFile));

                    if (createPlan == true)
                    {
                        MessageBox.Show($"Plan is ready");
                        readPlanTask.Start();
                    }
                    createPlanBtn.IsEnabled = false;
                }

                //stress test mode
                else
                {
                    bool createAllPlans = outputIndividualPlans_cb.IsChecked == true;

                    StressTestWindow stw = new StressTestWindow(this, db);
                    stw.Show();
                    

                    Plan bestPlan = new Plan();
                    
                    planCreator.currentRun = 1; //resets the current run variable

                    //run stress test in background
                    BackgroundWorker bw = new BackgroundWorker();

                    bw.WorkerReportsProgress = true;

                    bw.DoWork += new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        BackgroundWorker b = o as BackgroundWorker;

                        timedOut = false;
                        int bestRun = 0;
                        Plan bestPlan = new Plan();

                        while (timedOut == false)
                        {
                            Plan newPlan = new Plan();
                            SolutionPacket sp = SolutionPacket.defaultSolutionPacket();
                            bool contin = planCreator.LoadScenario();

                            if (contin == false)
                            {
                                this.Dispatcher.Invoke(new Action(() => stw.stressTestFinished()));
                                timedOut = true;
                            }

                            var task = Task.Run(() =>
                            {
                                newPlan = planCreator.Solve();
                                planCreator.currentRun++;
                            });

                            //current run was withing time range
                            if (task.Wait(TimeSpan.FromSeconds(planCreator.secondsForStressTestTimeOut)) && contin)
                            {
                                bestRun++;
                                bestPlan = newPlan;
                                stw.updateSolutionSheet(bestPlan.solutionPacket);

                                //timeout created manually
                                if (timedOut) this.Dispatcher.Invoke(new Action(() => stw.stressTestFinished()));
                            }

                            //time range exceeded
                            else
                            {
                                this.Dispatcher.Invoke(new Action(() => stw.stressTestFinished()));
                                timedOut = true;
                            }

                            //timeout can be procced outside, if createAllPlans was true, plan was already created
                            if (timedOut == true && createAllPlans == false)
                            {
                                PlanWriterIO.writePlan(db.outputFile.Split(".")[0] + $" - {bestRun}.plan", bestPlan);
                            }
                        }

                       //if option for plan reading is selected, load and read the plan (or best plan for stress)
                        if (createPlan)
                        {
                            MessageBox.Show($"plan for {bestRun} orders has been created");
                            //ceate performance graph for plan
                            
                            this.Dispatcher.Invoke(new Action(() => planReader.readPlan(db.outputFile.Split(".")[0] + $" - {bestRun}.plan")));
                        }
                        db.currentPlan = bestPlan;
                    });

                    bw.RunWorkerAsync();
                }
            };
        }

        

        private void generateSetupPanel()
        {
            Border sp_bod = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 0, 1, 0),
            };
            StackPanel sp = new StackPanel();

            sp_bod.Child = sp;
            Simulation_grid.Children.Add(sp_bod);
            Grid.SetColumn(sp_bod, 0);
            Grid.SetRowSpan(sp_bod, grid_height);

            Button uploadMap_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Upload map",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            uploadMap_btn.Click += (sender, e) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    uploadMapFile = openFileDialog.FileName;

                if (uploadMapFile != "" && uploadMapFile.Contains(".map"))
                {
                    mapParserIO mp = new mapParserIO(uploadMapFile, db);
                    char[][] map = mp.readInputFile();

                    if (map.Length == 0)
                    {
                        MessageBox.Show("Map file is not in correct format. Please select correct map format");
                        db.clearMap(); //if file is partly loaded and then found corrup, need to clean up
                        return;
                    }

                    sv = new Simulace_Visual(map, db, planReader, this);
                    planReader.sv = sv;
                    db.loadMap(sv.map.GetLength(0), sv.map[0].Length);

                    db.mapName = Path.GetFileName(uploadMapFile);
                    sv.createVisualization();
                    sv.Show();
                    mapLoaded = true;
                    updateUI();
                }
                else
                {
                    MessageBox.Show("Map file is not in correct format. Please select correct map format");
                    return;
                }

            };
            sp.Children.Add(uploadMap_btn);
            loadMapButton = uploadMap_btn;

            Button loadScenario_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Upload scenario",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };

            Button loadPlan_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Load Plan",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };

            loadScenario_btn.Click += (sender, e) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    uploadScenarioFile = openFileDialog.FileName;
                if (uploadScenarioFile != "" && uploadScenarioFile.Contains(".scen"))
                {
                    ScenarioParserIO sp = new ScenarioParserIO(uploadScenarioFile, db);
                    db.clearScenario();
                    bool validityCheck = sp.loadScenario();
                    if (validityCheck == false) 
                    {
                        //not need to do further checks, load scenario loads a Scneario object into db. If failed, the object is not loaded
                        MessageBox.Show("Invalid scenario. Please load correct scneario file");
                        return;
                    }

                    string mapName = Path.GetFileName(uploadScenarioFile);
                    db.outputFile = plans_folder + mapName + ".plan";
                    scenarioLoaded = true;
                    updateUI();
                }
                else
                {
                    MessageBox.Show("Invalid scenario. Please load correct scneario file");
                    return;
                }
            };
            sp.Children.Add(loadScenario_btn);
            loadScenarioButton = loadScenario_btn;
            
            loadPlan_btn.Click += (sender, e) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    uploadPlanFile = openFileDialog.FileName;

                if (uploadPlanFile != "" && uploadPlanFile.Contains(".plan"))
                {
                    //restart database -> clear agents, clear orders
                    db.clearScenario();
                    planReader.readPlan(uploadPlanFile);
                    planLoaded = true;
                    updateUI();
                }
                else
                {
                    MessageBox.Show("Please select valid .plan file");
                    return;
                }
            };
            sp.Children.Add(loadPlan_btn);
            loadPlanFileButton = loadPlan_btn;

            Button chooseAlgo = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Choose algorhitm",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            chooseAlgo.Click += (sender, e) =>
            {
                SelectAlgo sa = new SelectAlgo(db, this);
                sa.Show();
            };
            sp.Children.Add(chooseAlgo);

            //Button outputFileBtn = new Button
            //{
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    VerticalAlignment = VerticalAlignment.Stretch,
            //    Content = "Choose different outputFile",
            //    Style = (Style)FindResource("MenuButonTheme"),
            //    Height = buttonHeight,
            //};
            //outputFileBtn.Click += (sender, e) =>
            //{
            //    VistaFolderBrowserDialog openFileDialog = new VistaFolderBrowserDialog();
            //    if (openFileDialog.ShowDialog() == true)
            //        outputFile = openFileDialog.SelectedPath;

            //};
            //sp.Children.Add(outputFileBtn);

            //Label tb = new Label
            //{
            //    Content = $"Current output file: {db.outputFile}",
            //    HorizontalContentAlignment = HorizontalAlignment.Center,
            //    Background = Brushes.White,
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    VerticalAlignment = VerticalAlignment.Stretch,
            //};
            //outputFileString = tb;
            //sp.Children.Add(tb);
        }

        //assigns orders to agents with tsp heurisic (ignores walls, might not output best solution)
        

        public void timeOut() => timedOut = true;

        //based on loaded map/scenario, some options are limited. Upon clearing scenario/map, they are enabled
        private void restrictButtons()
        {
            if (mapLoaded)
            {
                loadMapButton.IsEnabled = false;
            }
            else
            {
                loadMapButton.IsEnabled = true;
                loadScenarioButton.IsEnabled = false;
                loadPlanFileButton.IsEnabled = false;
                return;
            }
            if (scenarioLoaded)
            {
                loadScenarioButton.IsEnabled = false;
                loadPlanFileButton.IsEnabled = false;
            }
            else if (planLoaded)
            {
                loadScenarioButton.IsEnabled = false;
                loadPlanFileButton.IsEnabled = false;
            }
            else
            {
                loadScenarioButton.IsEnabled = true;
                loadPlanFileButton.IsEnabled = true;
            }
        }

        //Reads the new state of the UI (#agents, #orders, #map name, #map size) and update UI info block + visualisation of agents
        public void updateUI()
        {
            restrictButtons();
            //outputFileString.Content = $"Current output file: {db.outputFile}";

            if (sv.loaded == false) return;
            sv.resetMap();
            sv.visualizeAgents();
            sv.visualizeOrders();
            string Text = $@"Size of map: {db.locationMap.GetLength(0)}x{db.locationMap[0].Length}
                Agents: {db.agents.Count}
                Orders: {db.orders.Count}";
            info.Text = Text;
        }
    }
}

