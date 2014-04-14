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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HopfieldNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int iter = 0;
        public MainWindow()
        {
            InitializeComponent();
            textBlock1.Text = "";
            HopfieldNetwork.Network.test();
            textBlock1.Text += "Macierz wag:\n";
            printArray(Network.weightMatrix, 9);

            int i = 0;
            textBlock1.Text += "Wektory uczące:\n";
            foreach (int[] l in Network.learningVectors)
            {
                textBlock1.Text += "Wektor " + i + ":\n";
                printVector(l, 3);
                i++;
            }

            textBlock1.Text += "Stan początkowy sieci:\n";
            printNeurons(Network.neurons.ToArray(), 3);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            iter++;
            HopfieldNetwork.Network.Iterate();
            textBlock1.Text += "Wynik iteracji " + iter + ":\n";
            printNeurons(Network.neurons.ToArray(), 3);
        }

        void printArray(float[,] array, int n)
        {
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    textBlock1.Text += array[x, y].ToString("0.####") + "\t";
                }
                textBlock1.Text += "\n";
            }
        }

        void printNeurons(Neuron[] vector, int n)
        {
            int i = 0;
            foreach (Neuron el in vector)
            {
                textBlock1.Text += el.getActivationState() + "\t";
                i++;
                if (i % n == 0) textBlock1.Text += "\n";
            }
        }

        void printVector(int[] vector, int n)
        {
            int i = 0;
            foreach (int el in vector)
            {
                textBlock1.Text += el + "\t";
                i++;
                if (i % n == 0) textBlock1.Text += "\n";
            }
        }


    }
}
