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
            this.ResizeMode = ResizeMode.NoResize;
            g.Style = (Style)FindResource("GridTheme");
            StackPanel sp = new StackPanel();
            g.Children.Add(sp);
            

            TextBlock id_tbl = new TextBlock
            {
                Text = "Select algorithm",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 60,
                TextAlignment = TextAlignment.Center,
            };
            sp.Children.Add(id_tbl);


            ComboBox algo_cb = new ComboBox
            {
                Text = "Select algorithm",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 20,
            };
            foreach (string algo in database.algorithms)
            {
                algo_cb.Items.Add(algo);
            }
            sp.Children.Add(algo_cb);
            algo_cb.SelectedIndex = 0;

            Button bt = new Button
            {
                Content = "Accept",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height= 70,
                Style = (Style)FindResource("MenuButonTheme"),
            };
            sp.Children.Add(bt);

            bt.Click += (sender, e) =>
            {
                database.selectedAlgo = algo_cb.Text;
                this.Close();
            };
        }
    }
}
