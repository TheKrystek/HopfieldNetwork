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

namespace HopfieldNetwork
{
    /// <summary>
    /// Interaction logic for Rename.xaml
    /// </summary>
    public partial class Rename 
    {
        public string tmp = "";
        public string name = "";
        public Rename()
        {
            InitializeComponent();
            this.newName.Text = this.name;
        }

        //anuluj
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.name = this.tmp;
            this.Close();
        }

        // Zapisz
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            name = newName.Text;
            this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.newName.Text = this.name;
            this.tmp = this.name;
        }
    }
}
