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

        public string[][] map = new string[][] { new string[] { "1", "0", "0" }, new string[] { "0", "0", "1" }, new string[] { "1", "0", "0"} };
        private Rectangle[,] map_tiles;

        public Grid simGrid = new Grid();


        public Simulace_Visual(string[][] map, Database db)
        {
            this.db = db;
            this.map = map;
            map_tiles = new Rectangle[map.GetLength(0), map[1].Length];
            InitializeComponent();
            createVisualization();
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
            this.Content = simGrid;
        }

        private void visualizeAgents()
        {
            //default color for agents is blue
            byte[] agentColor = new byte[] { 0, 0, 255 };
            foreach (Agent a in db.agents)
            {
                changeColor(a.baseLocation.coordination, agentColor);
            }
        }

        private void visualizeOrders()
        {
            //default color for order is red
            byte[] orderColor = new byte[] { 255, 0, 0 };
            foreach (Order o in db.orders)
            {
                changeColor(o.currLocation.coordination, orderColor);
            }
        }


        public void changeColor(int[] coord, byte[] color)
        {
            this.Dispatcher.Invoke(() =>
            {
                map_tiles[coord[0], coord[1]].Fill = new SolidColorBrush(Color.FromRgb(color[0], color[1], color[2])); ;
            });
        }

        private void createMap()
        {
            Grid visual_grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            simGrid = visual_grid;

            for (int i = 0; i < map.GetLength(0); i++)
            {
                visual_grid.RowDefinitions.Add(new RowDefinition());
                
            }

            for (int j = 0; j < map[0].Length; j++)
            {
                visual_grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            simGrid = visual_grid;


            int id_counter = 0;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map[0].Length; j++)
                {
                    Rectangle b = new Rectangle
                    {
                        Fill = map[i][j] == "1" ? Brushes.Black : Brushes.White,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    };
                    map_tiles[i, j] = b;
                    simGrid.Children.Add(b);
                    Grid.SetColumn(b, j);
                    Grid.SetRow(b, i);
                }
            }         
        }
    }
}
