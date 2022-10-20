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
    public partial class simulace_visual : Window
    {
        List<int[]> walls_coord = new List<int[]>();
        List<int[]> orders_coord = new List<int[]>();
        List<int[]> vehicles_coord = new List<int[]>();

        public int[,] map = new int[,] { { 1, 0, 0 }, { 0, 0, 1 }, { 1, 0, 0 } };
        private Rectangle[,] map_tiles;

        Grid simGrid = new Grid();


        public simulace_visual()
        {
            map_tiles = new Rectangle[map.GetLength(0), map.GetLength(1)];
            InitializeComponent();
            createMap();
            this.Content = simGrid;
        }

        
        public void changeColor(int[] coord, SolidColorBrush brush)
        {
            map_tiles[coord[0], coord[1]].Fill = brush;
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

            for (int j = 0; j < map.GetLength(1); j++)
            {
                visual_grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            simGrid = visual_grid;

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Rectangle b = new Rectangle
                    {
                        Fill = map[i, j] == 1 ? Brushes.Black : Brushes.White,
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
