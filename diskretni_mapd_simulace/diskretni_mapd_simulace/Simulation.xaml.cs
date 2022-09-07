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
        Database db = new Database();
        Routing_solverManager rsm;
        Routing_solver rs;
        List<Button> location_buttons = new List<Button>();
        Dictionary<Button, Location> butt_loc_dict = new Dictionary<Button, Location>();


        public Simulation()
        {
            rsm = new Routing_solverManager(db);
            //grid 3 sloupce, simulace uprostred, data v levo, updaty (jaky vuz dokoncil jakou objednavku vlevo)
            InitializeComponent();
            generateGrid();
            rs = new Routing_solver(rsm);
        }

        public void generateGrid()
        {
            int gridWidth = 10;
            int gridHeihght = 10;
            int blankSpace = 40;
            int locationCounter = 0;

            int rectHeight = 20;
            int rectangleWidth = 20;

            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeihght; j++)
                {
                    
                    Button button = new Button
                    { 
                        Height = rectHeight,
                        Width = rectangleWidth,
                        Margin = new Thickness(i * blankSpace + blankSpace, j * blankSpace + blankSpace, 0, 0),
                        IsEnabled = true,
                        Background = Brushes.Black,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Content = ' ',
                    };
                    Simulation_grid.Children.Add(button);
                    Grid.SetColumn(button, 1);
                    Location location = new Location { id = locationCounter++, coordination = new int[] { i, j } };
                    db.locations.Add(location);
                    location_buttons.Add(button);
                    butt_loc_dict.Add(button, location);

                    button.Click += (sender, e) =>
                    {
                        //TODO: proc locationOpetionWindow with button to add vehicle or Order
                        // fill with function to proc correct window
                        LocationOptionWindow locoptwindow = new LocationOptionWindow(butt_loc_dict[button], db);
                        locoptwindow.Show();
                    };
                }
            }
        }
       

        private void solve_btn_Click(object sender, RoutedEventArgs e)
        {
            rsm.getSolutionData();
            rs.solveProblemAndPrintResults();
        }
    }
}

