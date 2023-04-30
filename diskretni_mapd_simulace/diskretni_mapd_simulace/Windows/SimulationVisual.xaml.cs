using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml;
using diskretni_mapd_simulace.Plan_Tools;
using diskretni_mapd_simulace.Windows;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace diskretni_mapd_simulace
{
    /// <summary>
    /// Interaction logic for simulace_visual.xaml
    /// </summary>
    public partial class Simulace_Visual : Window
    {
        List<int[]> orders_coord = new List<int[]>();
        List<int[]> agents_coord = new List<int[]>();
        Database db;
        public PlanReader pr { get; set; }
        public TextBlock info = new TextBlock();
        TextBlock tb_info;

        SImulationSettingsWindow ssw;
        Simulation simulationWindow;

        byte[] targetColor = new byte[] { 0, 255, 0 };
        byte[] blankColor = new byte[] { 255, 240, 245 };

        //rgb(192,192,192)
        byte[] planDefaultColor = new byte[] { 237, 234, 229 };

        Button pause_btn = new Button();

        public bool loaded = false;

        public char[][] map = new char[][] { new char[] { '.', '.', '.' }, new char[] { '.', '.', '.' }, new char[] { '.', '.', '.' } };
        private Rectangle[,] map_tiles;

        public Grid simGrid = new Grid();
        public Grid planGrid = new Grid() { Height = 1500, Width = 1300 };

        //all line between locations for plan validator will be stored here
        Dictionary<(int, int), Line> locationLineDict = new Dictionary<(int, int), Line>();

        int lineThickness = 3;

        public void forcePause()
        {
            this.Dispatcher.Invoke(() =>
            {
                pause_btn.Content = "Resume";
                pr.pausePlan();
            });
        }

        public Simulace_Visual(char[][] map, Database db, PlanReader pr, Simulation simulationWindow)
        {
            this.pr = pr;
            this.WindowState = WindowState.Maximized;
            this.db = db;
            this.map = map;
            map_tiles = new Rectangle[map.GetLength(0), map[1].Length];
            InitializeComponent();
            createVisualization();
            loaded = true;
            this.simulationWindow = simulationWindow;
        }

        public Simulace_Visual()
        {
            //placeholder for empty SV
            loaded = false;
        }

        private void createStackPannel()
        {
            StackPanel sp = new StackPanel
            {

            };

            Border tb_bor = new Border
            {
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = Brushes.Black,
            };

            TextBlock tb = new TextBlock
            {
                Text = @"Time: 0 
Cost: 0 moves
Delivered: 0
Remaining: 0",
                Height = 70,
                TextAlignment = TextAlignment.Left,
                Margin = new(10, 0, 0, 0),
            };
            tb_bor.Child = tb;
            sp.Children.Add(tb_bor);
            info = tb;

            Button pause_btn = new Button
            {
                Content = "Resume",
                Foreground = Brushes.Black,
                Height = 60,
                Style = (Style)FindResource("MenuButonTheme"),
            };

            pause_btn.Click += (sender, e) =>
            {
                if (pause_btn.Content.ToString() == "Pause")
                {
                    pr.pausePlan();
                    pause_btn.Content = "Resume";
                    return;
                }
                if (pause_btn.Content.ToString() == "Resume")
                {
                    pr.resumePlan();
                    pause_btn.Content = "Pause";
                    return;
                }
            };
            sp.Children.Add(pause_btn);
            this.pause_btn = pause_btn;

            Button validatePlan_btn = new Button
            {
                Content = "Validate plan",
                Foreground = Brushes.Black,
                Height = 60,
                Style = (Style)FindResource("MenuButonTheme"),
            };
            validatePlan_btn.Click += (sender, e) =>
            {

                pause_btn.Content = "Resume";
                pr.pausePlan();
                PlanValidator pv = new PlanValidator(this, db.currentPlan);
                if (db.currentPlan.steps.Count > 0)
                {
                    MessageBox.Show("Validation will begin shortly, please do not interrupt this process");
                    pv.Validate();
                }
                else
                {
                    MessageBox.Show("No plan to validate. Please create plan first.");
                }
            };
            sp.Children.Add(validatePlan_btn);

            Button settings_btn = new Button
            {
                Content = "Settings",
                Foreground = Brushes.Black,
                Height = 60,
                Style = (Style)FindResource("MenuButonTheme"),
            };

            settings_btn.Click += (sender, e) =>
            {
                if (ssw == null) ssw = new SImulationSettingsWindow(db, pr); //first time opening window, initialize it
                ssw.Show();
                pause_btn.Content = "Resume";
                pr.pausePlan();
            };
            sp.Children.Add(settings_btn);

            tb_info = new TextBlock
            {
                Text = "",
                FontSize = 15,
                TextAlignment = TextAlignment.Left,
                Background = new SolidColorBrush(Color.FromRgb(133, 205, 253)),
            };
            sp.Children.Add(tb_info);


            simGrid.Children.Add(sp);
            Grid.SetColumn(sp, simGrid.ColumnDefinitions.Count - 1);
            Grid.SetRowSpan(sp, simGrid.RowDefinitions.Count);

        }

        public void colorAssignments()
        {
            foreach (var agent in db.agents)
            {
                changeColor(agent.baseLocation.coordination, agent.color);
            }
            foreach (var order in db.orders)
            {
                changeColor(order.currLocation.coordination, order.color);
            }
        }

        public void createVisualization()
        {
            createMap();
            visualizeAgents();
            visualizeOrders();
            createStackPannel();
            this.Content = simGrid;
        }

        public void visualizeAgents()
        {

            //default color for agents is blue
            byte[] agentColor = new byte[] { 0, 0, 255 };
            foreach (Agent a in db.agents)
            {
                changeColor(a.baseLocation.coordination, agentColor);
            }
        }

        public void visualizeOrders()
        {

            //default color for order is red
            byte[] orderColor = new byte[] { 255, 0, 0 };
            foreach (Order o in db.orders)
            {
                changeColor(o.currLocation.coordination, orderColor);
            }
        }

        public void resetMap()
        {
            if (db.locations?.Any() != true) return;

            foreach (var location in db.locations)
            {
                if (location.type == (int)Location.types.free)
                {
                    changeColor(location.coordination, blankColor);
                }
            }
        }




        public void changeColor(int[] coord, byte[] color)
        {
            this.Dispatcher.Invoke(() =>
            {
                map_tiles[coord[0], coord[1]].Fill = new SolidColorBrush(Color.FromRgb(color[0], color[1], color[2]));
            });
        }

        public void displayOrder(int[] coord, string color)
        {
            this.Dispatcher.Invoke(() =>
            {
                map_tiles[coord[0], coord[1]].Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(@$"resources/icon_order_{color}.png", UriKind.Relative)) };
            });
        }

        public void removeOrderColor()
        {
            foreach (var o in db.orders)
            {
                changeColor(o.currLocation.coordination, blankColor);
            }
        }

        public void changeInfoText(string value)
        {
            this.Dispatcher.Invoke(() =>
            {
                info.Text = value;
            });
        }

        private void createMap()
        {
            Grid visual_grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Style = (Style)FindResource("GridTheme"),
            };

            for (int i = 0; i < map.GetLength(0); i++)
            {
                visual_grid.RowDefinitions.Add(new RowDefinition());

            }

            for (int j = 0; j < map[0].Length; j++)
            {
                visual_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            //this is going to be the control panel with pause button etc
            visual_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10, GridUnitType.Star) });
            simGrid = visual_grid;

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map[0].Length; j++)
                {
                    Rectangle b = new Rectangle
                    {
                        Fill = map[i][j] == '.' ? Brushes.LavenderBlush : Brushes.Black,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Tag = $"{i},{j}",
                        //IsHitTestVisible = false,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    };
                    map_tiles[i, j] = b;
                    simGrid.Children.Add(b);

                    b.MouseEnter += (sender, e) =>
                    {
                        string[] coord = b.Tag.ToString().Split(",");
                        Location l = db.locationMap[int.Parse(coord[0])][int.Parse(coord[1])];
                        if (l.orders.Count >= 1 && l.agents.Count == 0)
                        {
                            Order o;
                            o = l.orders[0];
                            changeColor(o.targetLocation.coordination, targetColor);
                            tb_info.Text = $"Order: {o.id}\nTarget location: {o.targetLocation.id}\nState: {o.state}";
                        }
                        else if (l.agents.Count > 0)
                        {
                            Agent a = l.agents[0];
                            tb_info.Text = $"Agent: {a.id}\n";
                        }
                    };
                    b.MouseLeave += (sender, e) =>
                    {
                        string[] coord = b.Tag.ToString().Split(",");
                        Location l = db.locationMap[int.Parse(coord[0])][int.Parse(coord[1])];
                        Order o;
                        if (l.orders.Count >= 1)
                        {
                            o = l.orders[0];
                            changeColor(o.targetLocation.coordination, blankColor);
                            tb_info.Text = "";
                        }
                        else if (l.agents.Count > 0 && l.agents.Count == 0)
                        {
                            tb_info.Text = "";
                        }
                    };
                    Grid.SetColumn(b, j);
                    Grid.SetRow(b, i);
                }
            }
        }

        public void switchGrid()
        {
            this.Content = planGrid;
        }


        public void clearValidationGrid()
        {
            this.Dispatcher.Invoke(() =>
            {
                planGrid.Children.Clear();
                this.Content = simGrid;
            });
            locationLineDict.Clear();
        }


        private Line DrawLine(double x1, double x2, double y1, double y2, byte[] color, int thickness)
        {
            Color c = Color.FromRgb(color[0], color[1], color[2]);
            Line line = new()
            {
                X1 = x1,
                X2 = x2,
                Y1 = y1,
                Y2 = y2,
                Stroke = new SolidColorBrush(c),
                StrokeThickness = thickness
            };
            planGrid.Children.Add(line);
            return line;
        }

        //draws line between location and all its accessable naighbours
        public void Connect_Neighbours(Location l)
        {
            Point origin = map_tiles[l.coordination[0], l.coordination[1]].TransformToAncestor(simGrid)
                          .Transform(new Point(0, 0));

            foreach (var location in l.accessibleLocations)
            {
                Point target = map_tiles[location.coordination[0], location.coordination[1]].TransformToAncestor(simGrid)
                          .Transform(new Point(0, 0));
                Line line = DrawLine(origin.X, target.X, origin.Y, target.Y, planDefaultColor, lineThickness);

                //adds line to the dictionary, so I can later rewrite its color in the plan validation generating process
                locationLineDict[(l.id, location.id)] = line;
                locationLineDict[(location.id, l.id)] = line;
            }
        }

        //creates planGrid for plan validation
        public void createValidationPlanMap()
        {
            foreach (var l in db.locations)
            {
                this.Dispatcher.Invoke(new Action(() => this.Connect_Neighbours(l)));
            }

            this.Dispatcher.Invoke(() =>
            {
                this.switchGrid();
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                simulationWindow.WindowState = WindowState.Minimized;
            });
        }

        //gets List of location ids, desired color and calls draw line for this path
        public void colorPath(List<int> locationIds, byte[] color)
        {
            Color c = Color.FromRgb(color[0], color[1], color[2]);
            for (int i = 0; i < locationIds.Count - 1; i++)
            {
                //in case agent stays in position for multiple rounds, ignore
                if (locationIds[i] == locationIds[i + 1]) continue;
                if (i == locationIds.Count -2)
                {
                    //head of the path is colored in black, so its better visible
                    c = Color.FromRgb(0, 0, 0);
                }
                 this.Dispatcher.Invoke(() =>
                {
                    locationLineDict[(locationIds[i], locationIds[i + 1])].Stroke = new SolidColorBrush(c);
                });
            }
        }

        //sets color of the entire grid back to default color -> calling between validation iterations
        public void resetPlanGrid(Dictionary<string, List<int>> pathsToReset)
        {
            Color c = Color.FromRgb(planDefaultColor[0], planDefaultColor[1], planDefaultColor[2]);
            foreach (KeyValuePair<string, List<int>> entry in pathsToReset)
            {
                for (int i = 0; i < entry.Value.Count - 1; i++)
                {
                    if (entry.Value[i] == entry.Value[i + 1]) continue;

                    this.Dispatcher.Invoke(() =>
                    {
                        locationLineDict[(entry.Value[i], entry.Value[i + 1])].Stroke = new SolidColorBrush(c);
                    });
                }
            }
        }


        public void SaveAsPng(int index)
        {
            try
            {
                //Creating a new Bitmap object -> this numbers are matched to my screen exactly, please adjust for yours
                System.Drawing.Bitmap captureBitmap = new System.Drawing.Bitmap(1600, 1080, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //Bitmap captureBitmap = new Bitmap(int width, int height, PixelFormat);
                //Creating a Rectangle object which will
                //capture our Current Screen
                System.Drawing.Rectangle captureRectangle = System.Windows.Forms.Screen.AllScreens[0].Bounds;
                //Creating a New Graphics Object
                System.Drawing.Graphics captureGraphics = System.Drawing.Graphics.FromImage(captureBitmap);
                //Copying Image from The Screen 
                // + 170 because I have toolbar on the left side
                captureGraphics.CopyFromScreen(captureRectangle.Left + 170, captureRectangle.Top, 0, 0, captureRectangle.Size);
                //Saving the Image File (I am here Saving it in My E drive).
                captureBitmap.Save(@$"sc/Plan{index}.jpg", ImageFormat.Jpeg);
                //Displaying the Successfull Result
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
