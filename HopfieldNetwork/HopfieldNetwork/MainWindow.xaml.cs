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
using System.Threading;
using System.ComponentModel;

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
        int delay = 1400;
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public static RoutedCommand ZmniejszPredkoscCom = new RoutedCommand();
        public static RoutedCommand ZwiekszPredkoscCom = new RoutedCommand();

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

            worker.DoWork += worker_DoWork;
           


            // Zbinduj skroty klawiszowe
            CommandBinding cb4 = new CommandBinding(ZwiekszPredkoscCom, ZwiekszPredkosc);
            ZmniejszPredkoscCom.InputGestures.Add(new KeyGesture(Key.OemPlus,ModifierKeys.Control));
            this.CommandBindings.Add(cb4);

            CommandBinding cb5 = new CommandBinding(ZmniejszPredkoscCom, ZmniejszPredkosc);
            ZmniejszPredkoscCom.InputGestures.Add(new KeyGesture(Key.OemMinus, ModifierKeys.Control));
            this.CommandBindings.Add(cb4);
        }

        void ZmniejszPredkosc(object sender, RoutedEventArgs e)
        {
            if (delay < 2000)
                delay += 200;
        }

        
        void ZwiekszPredkosc(object sender, RoutedEventArgs e)
        {
            if (delay > 200)
                delay -= 200;
        }

        void Szybko(object sender, RoutedEventArgs e) {
            delay = 800;
        }
        void Wolno(object sender, RoutedEventArgs e)
        {
            delay = 2000;
        }

        void Srednio(object sender, RoutedEventArgs e)
        {
            delay = 1400;
        }


        // Przycisk Iteruj
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!finished)
            {
                iter++;
                HopfieldNetwork.Network.Iterate();

                Action action = () => {
                    printNeurons(Network.neurons.ToArray(), this.neuronArraySize);
                    this.drawArray(Network.weightMatrix, this.weightMatrixSize);
                    this.drawNeurons(Network.neurons.ToArray(), this.neuronArraySize);

                    this.drawSummer(Network.enumerator.Current.getPartialResults());
                    if (iter < this.weightMatrixSize)
                        this.StatusLabel.Content = String.Format("Iteracja numer: {0}", iter);
                    else
                    {
                        finished = true;
                        this.StatusLabel.Content = "Osiągnięto stan stabilny";
                    }     
                };

                var dispatcher = StatusLabel.Dispatcher;
                if (dispatcher.CheckAccess())
                    action();
                else
                    dispatcher.Invoke(action);       
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
            if (Network.enumerator.Current != null)
            {
                weightsWisualization.Children.Clear();
                if (weightsWisualization.ActualHeight > 0 && weightsWisualization.ActualWidth > 0)
                {
                    int tileSize = getTileSize(n, weightsWisualization);
                    // Ustaw marginesy
                    int startY = (int)(weightsWisualization.ActualHeight - n * tileSize) / 2;
                    int startX = (int)(weightsWisualization.ActualWidth - n * tileSize) / 2;

                    PartialReults pr = Network.enumerator.Current.getPartialResults();
                    for (int x = 0; x < n; x++)
                    {
                        for (int y = 0; y < n; y++)
                        {
                            SolidColorBrush brush = new SolidColorBrush();
                            brush.Color = getColorFromWeight(array[x, y]);

                            Border tile = new Border()
                            {
                                BorderBrush = Brushes.Black,
                                BorderThickness = new Thickness(1),
                                Background = brush,
                                Width = tileSize,
                                Height = tileSize
                            };

                            FontWeight fw = FontWeights.Normal;
                            if (x == pr.id && pr.connections.IndexOf(y) >= 0)
                                fw = FontWeights.Bold;

                            tile.Child = new TextBlock()
                            {
                                Text = String.Format("{0:n2}", array[x, y]),
                                HorizontalAlignment =
                                HorizontalAlignment.Center,
                                VerticalAlignment =
                                VerticalAlignment.Center,
                                FontWeight = fw

                            };
                            weightsWisualization.Children.Add(tile);

                            Canvas.SetTop(tile, startY + tileSize * y - 1);
                            Canvas.SetLeft(tile, startX + tileSize * x - 1);

                        }
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

        void drawSummer(PartialReults pr)
        {
            #region Ustawienia pomocnicze dla rysowania
            Brush b = Brushes.Black;
            int Thickness = 3;
            int marginTop = 20;
            int marginLeftRight = 20;
            int marginBottom = 50;
            int padding = 10;
            int number = pr.weights.Count;
            int space = (int)(summer.ActualWidth - 2*marginLeftRight) / number;
            #endregion
            
            // Wyczysc poprzedni stan plotna 
            summer.Children.Clear();
            if (Network.enumerator.Current != null)
            {
                #region  Rysowanie trojkata sumatora
                // Linia pozioma
                Line line0 = new Line
                {
                    X1 = marginLeftRight,
                    Y1 = marginTop,
                    X2 = summer.ActualWidth - marginLeftRight,
                    Y2 = marginTop,
                    StrokeThickness = Thickness,
                    Stroke = b
                };

                // Linia z lewej na dol do srodka
                Line line1 = new Line
                {
                    X1 = marginLeftRight,
                    Y1 = marginTop,
                    X2 = summer.ActualWidth / 2,
                    Y2 = summer.ActualHeight - marginBottom - 10,
                    StrokeThickness = Thickness,
                    Stroke = b
                };

                // Prawej z lewej na dol do srodka
                Line line2 = new Line
                {
                    X1 = summer.ActualWidth - marginLeftRight,
                    Y1 = marginTop,
                    X2 = summer.ActualWidth / 2,
                    Y2 = summer.ActualHeight - marginBottom - 10,
                    StrokeThickness = Thickness,
                    Stroke = b
                };

                // Linia z wierzcholka na dol
                Line line3 = new Line
                {
                    X1 = summer.ActualWidth / 2,
                    Y1 = summer.ActualHeight - marginBottom - 10,
                    X2 = summer.ActualWidth / 2,
                    Y2 = summer.ActualHeight - marginBottom,
                    StrokeThickness = Thickness,
                    Stroke = b
                };

                // Rysuj trojkat sumatora
                summer.Children.Add(line0);
                summer.Children.Add(line1);
                summer.Children.Add(line2);
                summer.Children.Add(line3);

                // Rysuj Σ w srodku trojkata
                TextBlock sigma = new TextBlock()
                {
                    Text = "Σ",
                    HorizontalAlignment =
                    HorizontalAlignment.Center,
                    VerticalAlignment =
                    VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    FontSize = 100
                };
                summer.Children.Add(sigma);
                Canvas.SetTop(sigma, marginTop - 20);
                Canvas.SetLeft(sigma, summer.ActualWidth / 2  - 30);
                #endregion

                #region rysowanie wartosci posrednich
                for (int i = 1; i <= number; i++)
                {
                    Line line = new Line
                    {
                        X1 = 0.5 * marginLeftRight + i * space,
                        Y1 = marginTop - 5,
                        X2 = 0.5 * marginLeftRight + i * space,
                        Y2 = marginTop,
                        StrokeThickness = Thickness,
                        Stroke = b
                    };
                    summer.Children.Add(line);

                    TextBlock tb = new TextBlock()
                    {
                        Text = String.Format("{0:n0}*{1:n2}", pr.inputs[i-1],pr.weights[i-1]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Background= new SolidColorBrush(getColorFromWeight((double)pr.weights[i - 1]))
                        //FontWeight = (Network.enumerator.Current.getId() == i ? FontWeights.ExtraBold : FontWeights.Normal)
                    };
                    summer.Children.Add(tb);
                    Canvas.SetTop(tb, 0);
                    Canvas.SetLeft(tb, 0.5 * marginLeftRight + i * space - 20); 
                }

                TextBlock output = new TextBlock()
                {
                    Text = String.Format("{0:n0}", pr.output),
                    HorizontalAlignment =
                    HorizontalAlignment.Center,
                    VerticalAlignment =
                    VerticalAlignment.Center,
                    FontWeight = FontWeights.ExtraBold,
                    FontSize = 30,
                    Foreground = new SolidColorBrush(getColorFromWeight(pr.output))

                };
                summer.Children.Add(output);
                Canvas.SetTop(output, summer.ActualHeight - marginBottom);
                Canvas.SetLeft(output, summer.ActualWidth / 2 - 10); 

                #endregion
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

        private void summer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Network.enumerator.Current != null)
            this.drawSummer(Network.enumerator.Current.getPartialResults());
        }

        private void DoKonca_Click(object sender, RoutedEventArgs e)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();

        }


        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!finished)
            {
                Button_Click_1(null, null);
                Thread.Sleep(delay);
            }
        }

  

    }
}
