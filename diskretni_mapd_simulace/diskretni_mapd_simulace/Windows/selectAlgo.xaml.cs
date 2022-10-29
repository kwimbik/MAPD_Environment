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

namespace diskretni_mapd_simulace.Windows
{
    /// <summary>
    /// Interaction logic for selectAlgo.xaml
    /// </summary>
    public partial class SelectAlgo : Window
    {
        Database database;
        Simulation sim;
        Grid g = new Grid();

        public SelectAlgo(Database db, Simulation simulationUI)
        {
            database = db;
            sim = simulationUI;
            InitializeComponent();
            createControls();
            this.Content = g;
        }

        public void createControls()
        {
            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions.Add(new RowDefinition());
            TextBlock id_tbl = new TextBlock
            {
                Text = "Select algorithm",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            g.Children.Add(id_tbl);
            Grid.SetRow(id_tbl, 0);


            ComboBox algo_cb = new ComboBox
            {
                Text = "Initial location",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            foreach (string algo in database.algorithms)
            {
                algo_cb.Items.Add(algo);
            }
            g.Children.Add(algo_cb);
            Grid.SetRow(algo_cb, 1);

            Button bt = new Button
            {
                Content = "Accept",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            g.Children.Add(bt);
            Grid.SetRow(bt, 3);


            //TODO: check na validitu lokace
            bt.Click += (sender, e) =>
            {
                database.selectedAlgo = algo_cb.Text;
                this.Close();
            };
        }
    }
}
