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
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : Window
    {
        public Simulation()
        {
            //grid 3 sloupce, simulace uprostred, data v levo, updaty (jaky vuz dokoncil jakou objednavku vlevo)
            InitializeComponent();
            generateGrid();
        }

        public void generateGrid()
        {
            int gridWidth = 10;
            int gridHeihght = 10;
            int blankSpace = 40;

            int rectHeight = 20;
            int rectangleWidth = 20;

            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeihght; j++)
                {
                    Rectangle a = new Rectangle
                    {
                        Height = rectHeight,
                        Width = rectangleWidth,
                        Margin = new Thickness(i * blankSpace + blankSpace, j * blankSpace + blankSpace, 0, 0),
                        IsEnabled = true,
                        Fill = Brushes.Red,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,

                    };
                    Simulation_grid.Children.Add(a);
                    Grid.SetColumn(a, 1);
                }
            }
        }


    }
}

