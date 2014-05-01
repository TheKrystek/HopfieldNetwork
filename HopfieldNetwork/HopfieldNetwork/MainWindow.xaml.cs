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
        bool finished = false;

        int neuronArraySize = 3;
        int weightMatrixSize = 3;



        public MainWindow()
        {
            // Oblicz rozmiar macirzy wag
            weightMatrixSize =  neuronArraySize * neuronArraySize;

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


        // Przycisk Iteruj
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!finished)
            {
                iter++;
                HopfieldNetwork.Network.Iterate();
                textBlock1.Text += "Wynik iteracji " + iter + ":\n";
                printNeurons(Network.neurons.ToArray(), this.neuronArraySize);
                this.drawArray(Network.weightMatrix, this.weightMatrixSize);
                this.drawNeurons(Network.neurons.ToArray(), this.neuronArraySize);
                //this.drawSummer(Network.neurons);
                if (iter < this.weightMatrixSize)
                    this.StatusLabel.Content = String.Format("Iteracja numer: {0}", iter);
                else
                {
                    finished = true;
                    this.StatusLabel.Content = "Osiągnięto stan stabilny";
                }
            }
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


        
        int getTileSize(int n, Canvas c)
        {
            double s = c.ActualHeight;
            if (c.ActualWidth < c.ActualHeight)
                s = c.ActualWidth;
            return (int)(s / n); // rozmiar jednego kafelka
        }
       

        Color getColorFromWeight(double weight) {
          // string []colors = {"#ff4400", "#ff8800", "#ffcc00", "#ccff00", "#00ff00"};
           string[] colors = {"#F7977A","#F9AD81","#FDC68A","#FFF79A","#C4DF9B","#A2D39C" /*,"#82CA9D"*/ };
           double max = 1;
           double min = -1;
           double range = (max - min) / colors.Length;
           weight = (weight + 1);
           int i = 0;
           while (true)
               if (weight < (++i +1)*range)
                   break;
           return (Color)ColorConverter.ConvertFromString(colors[colors.Length - i]);
        }


        void drawArray(float[,] array, int n)
        {
            weightsWisualization.Children.Clear();
            if (weightsWisualization.ActualHeight > 0 && weightsWisualization.ActualWidth > 0)
            {
                int tileSize = getTileSize(n,weightsWisualization);
                // Ustaw marginesy
                int startY = (int)(weightsWisualization.ActualHeight - n * tileSize) / 2;
                int startX = (int)(weightsWisualization.ActualWidth - n * tileSize) / 2;
              
                for (int x = 0; x < n; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        SolidColorBrush brush = new SolidColorBrush();
                        brush.Color = getColorFromWeight(array[x,y]);

                        Border tile = new Border()
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            Background = brush,
                            Width = tileSize,
                            Height = tileSize
                        };

                        tile.Child = new TextBlock()
                        {
                            Text = String.Format("{0:n2}", array[x,y]),
                            HorizontalAlignment =
                            HorizontalAlignment.Center,
                            VerticalAlignment =
                            VerticalAlignment.Center
                        };
                        weightsWisualization.Children.Add(tile);

                        Canvas.SetTop(tile, startY + tileSize * y - 1);
                        Canvas.SetLeft(tile, startX + tileSize * x - 1);

                    }
                }

            }
        }


        void drawNeurons(Neuron[] vector, int n)
        {
            neuronsWisualization.Children.Clear();
            if (Network.enumerator.Current != null)
            {
                if (neuronsWisualization.ActualHeight > 0 && neuronsWisualization.ActualWidth > 0)
                {
                    int tileSize = getTileSize(n, neuronsWisualization);
                    // Ustaw marginesy
                    int startY = (int)(neuronsWisualization.ActualHeight - n * tileSize) / 2;
                    int startX = (int)(neuronsWisualization.ActualWidth - n * tileSize) / 2;
                    int x = 0;
                    int y = 0;
                    int i = 0;
                    foreach (var el in vector)
                    {
                        SolidColorBrush brush = new SolidColorBrush();
                        brush.Color = getColorFromWeight(el.getActivationState());

                        Border tile = new Border()
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            Background = brush,
                            Width = tileSize,
                            Height = tileSize
                        };

                        tile.Child = new TextBlock()
                        {
                            Text = String.Format("{0:n0}", el.getActivationState()),
                            HorizontalAlignment =
                            HorizontalAlignment.Center,
                            VerticalAlignment =
                            VerticalAlignment.Center,
                            FontWeight = (Network.enumerator.Current.getId() == i ? FontWeights.ExtraBold : FontWeights.Normal)
                        };
                        ;

                        neuronsWisualization.Children.Add(tile);

                        Canvas.SetTop(tile, startY + tileSize * y - 1);
                        Canvas.SetLeft(tile, startX + tileSize * x - 1);

                        // Oblicz wsp dla rysowanych komórek
                        i++;
                        x++;
                        if (i % n == 0)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        void drawSummer(Neuron[] vector, int n)
        {
            neuronsWisualization.Children.Clear();
            if (Network.enumerator.Current != null)
            {
                if (neuronsWisualization.ActualHeight > 0 && neuronsWisualization.ActualWidth > 0)
                {
                    int tileSize = getTileSize(n, neuronsWisualization);
                    // Ustaw marginesy
                    int startY = (int)(neuronsWisualization.ActualHeight - n * tileSize) / 2;
                    int startX = (int)(neuronsWisualization.ActualWidth - n * tileSize) / 2;
                    int x = 0;
                    int y = 0;
                    int i = 0;
                    foreach (var el in vector)
                    {
                        SolidColorBrush brush = new SolidColorBrush();
                        brush.Color = getColorFromWeight(el.getActivationState());

                        Border tile = new Border()
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            Background = brush,
                            Width = tileSize,
                            Height = tileSize
                        };

                        tile.Child = new TextBlock()
                        {
                            Text = String.Format("{0:n0}", el.getActivationState()),
                            HorizontalAlignment =
                            HorizontalAlignment.Center,
                            VerticalAlignment =
                            VerticalAlignment.Center,
                            FontWeight = (Network.enumerator.Current.getId() == i ? FontWeights.ExtraBold : FontWeights.Normal)
                        };
                        ;

                        neuronsWisualization.Children.Add(tile);

                        Canvas.SetTop(tile, startY + tileSize * y - 1);
                        Canvas.SetLeft(tile, startX + tileSize * x - 1);

                        // Oblicz wsp dla rysowanych komórek
                        i++;
                        x++;
                        if (i % n == 0)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
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

      

        private void weightsWisualization_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.drawArray(Network.weightMatrix, 9);
            this.drawNeurons(Network.neurons.ToArray(), 3);
        }


    }
}
