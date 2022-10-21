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
    /// Interaction logic for NewVehicleWindow.xaml
    /// </summary>
    public partial class NewVehicleWindow : Window
    {
        Database database;
        Location location;
        public NewVehicleWindow(Location location, Database db)
        {
            this.location = location;
            database = db;
            InitializeComponent();
            createControls();
        }

        public void createControls()
        {
            TextBox tb = new TextBox
            {
                Text = "Vehicle ID",
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 20,
                Width = 150,
            };
            NewVehicle_grid.Children.Add(tb);
            

            Button bt = new Button
            {
                Content = "Accept",
                Margin = new Thickness(0, 40, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 20,
                Width = 150,
            };
            NewVehicle_grid.Children.Add(bt);

            bt.Click += (sender, e) =>
            {
                Vehicle vehicle = new Vehicle
                {
                    id = tb.Text,
                    baseLocation = location,
                    targetLocation = location,
                };
                database.vehicles.Add(vehicle);
                location.vehicles.Add(vehicle);
                this.Close();
            };
        }
    }
}
