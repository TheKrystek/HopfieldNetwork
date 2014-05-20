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
using MahApps.Metro.Controls;


namespace HopfieldNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 

    {
        // GUI
        bool showSummer = true;
        bool showWeightsNumbers = true;



        int iter = 0;
        bool finished = false;
     
        int neuronArraySize = 3;
        int weightMatrixSize = 3;
        int delay = 400;

        bool editMode = true;

        int[] learningVector;

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public static RoutedCommand ZmniejszPredkoscCom = new RoutedCommand();
        public static RoutedCommand ZwiekszPredkoscCom = new RoutedCommand();


        bool pokazSumator = false;
        bool pokazWagi = true;


        public MainWindow()
        {
            // Oblicz rozmiar macirzy wag
            weightMatrixSize =  neuronArraySize * neuronArraySize;

            InitializeComponent();
            worker.DoWork += worker_DoWork;

            this.CreateLearningVector();

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
            delay = 600;
        }
        void Wolno(object sender, RoutedEventArgs e)
        {
            delay = 2000;
        }

        void Srednio(object sender, RoutedEventArgs e)
        {
            delay = 1400;
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



        // Przycisk Iteruj
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (HopfieldNetwork.Network.Initialized && !finished)
            {
                iter++;
                HopfieldNetwork.Network.Iterate();

                Action action = () => {
                    this.drawArray(Network.weightMatrix, this.weightMatrixSize);
                    this.drawNeurons(Network.neurons.ToArray(), this.neuronArraySize);

                    this.drawSummer(Network.enumerator.Current.getPartialResults());

                    printNeurons(Network.neurons.ToArray(),neuronArraySize);
                    Console.WriteLine();


                    this.StatusLabel.Content = String.Format("Iteracja numer: {0}", iter);

                    if (iter % weightMatrixSize == 0)
                    {
                        if (HopfieldNetwork.Network.stable)
                        {
                            this.StatusLabel.Content = "Osiągnięto stan stabilny";

                            // Dodaj wynik działania jako nowy wektor na liste
                            this.LearningVectorsList.Items.Add(this.ZapiszStanSieciDoTablicy("Wynik"));
                            finished = true;
                        }
                        else
                            HopfieldNetwork.Network.stable = true;

                        

                    }     
                };
               

                var dispatcher = StatusLabel.Dispatcher;
                if (dispatcher.CheckAccess())
                    action();
                else
                    dispatcher.Invoke(action);    
                
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
    
            if (weight == 2)
                return (Color)ColorConverter.ConvertFromString(colors[0]);
            if (weight == 0)
                return (Color)ColorConverter.ConvertFromString(colors[colors.Length - 1]);
   

           while (true)
               if (weight < (++i +1)*range)
                   break;
           i = i % colors.Length;   

           return (Color)ColorConverter.ConvertFromString(colors[i]);
        }


        void drawArray(float[,] array, int n)
        {
            if (Network.enumerator.Current != null)
            {
                weightsWisualization.Children.Clear();
                if (weightsWisualization.ActualHeight > 0 && weightsWisualization.ActualWidth > 0)
                {
                    int tileSize = getTileSize(n+2, weightsWisualization);
                    // Ustaw marginesy
                    int startY = (int)(weightsWisualization.ActualHeight - n * tileSize) / 4;
                    int startX = (int)(weightsWisualization.ActualWidth - n * tileSize) / 2;

                    PartialReults pr = Network.enumerator.Current.getPartialResults();
                    for (int y = 0; y <= n; y++)
                    {
                        for (int x = -1; x <= y; x++)
                        {
                            FontWeight fw = FontWeights.Normal;
                            int th = 1;
                            SolidColorBrush brush = new SolidColorBrush();
                            brush.Color = Colors.White;
                            String text = (x < 0 ? y+1 : x + 1).ToString();
                         
                            // Jezeli indeks nie jest ujemny
                            if (x >= 0 && y < n) { 
                                brush.Color = getColorFromWeight(array[x, y]);
                                text = String.Format("{0:n2}", array[x, y]); 
                            }

                            if (x == n || (y == n && x == -1))
                                text = "";

                            // Zaznaczaj aktualne komorki jedynie w trybie symulacji
                            if (!this.editMode)
                                if ((x == pr.id && pr.connections.IndexOf(y) >= 0) || (y == pr.id && pr.connections.IndexOf(x) >= 0))
                                {
                                    fw = FontWeights.Bold;
                                    th = 2;
                                }

                            Border tile = new Border()
                            {
                                BorderBrush = (x < 0 || y == n ? Brushes.Transparent : Brushes.Black),
                                BorderThickness =  new Thickness(th),
                                Background = (x < 0 || y > n? Brushes.Transparent : brush),
                                Width = tileSize,
                                Height = tileSize
                                
                            };
                            tile.PreviewMouseDown += klikniecie;

                            // Zaznaczaj aktualne komorki jedynie w trybie symulacji
                            if (!this.editMode)
                                if (x == pr.id && pr.connections.IndexOf(y) >= 0)
                                    fw = FontWeights.Bold;


                            tile.Child = new TextBlock()
                            {
                                // Jeżeli mamy macież większą niż 5 x 5 to nie pokazuj liczb w środku
                                Text = (this.neuronArraySize <= 5 && this.showWeightsNumbers ? text : ""),
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

        private void klikniecie(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("klik");
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
                            BorderThickness = new Thickness((Network.enumerator.Current.getId() == i && editMode == false ? 2 : 1)),
                            Background = brush,
                            Width = tileSize,
                            Height = tileSize,
                            Uid = i.ToString()
                        };
                        tile.PreviewMouseDown += przelaczStan;

                        tile.Child = new TextBlock()
                        {
                            Text = String.Format("{0:n0}", el.getActivationState()),
                            HorizontalAlignment =
                            HorizontalAlignment.Center,
                            VerticalAlignment =
                            VerticalAlignment.Center,
                            FontWeight = (Network.enumerator.Current.getId() == i && editMode == false ? FontWeights.ExtraBold : FontWeights.Normal)
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

        private void przelaczStan(object sender, MouseButtonEventArgs e)
        {
            // Zmiana wartosci pola jedynie w trybie edycji
            if (editMode)
            {
                Border border = sender as Border;
                if (border != null) { 
                    TextBlock tb = border.Child as TextBlock;
                    if (tb != null) { 
                        // Przłącz stan 
                        SolidColorBrush brush = new SolidColorBrush();
                        brush.Color = (tb.Text == "1" ? getColorFromWeight(-1) : getColorFromWeight(1));
                        border.Background = brush;
                        int index = Int32.Parse(border.Uid);
                        this.learningVector[index] = (tb.Text == "1" ? -1 : 1);
                        tb.Text = (tb.Text == "1" ? "-1" : "1");
                        Console.Write("[ ");
                        for (int i = 0; i < learningVector.Length; i++)
                            Console.Write(learningVector[i]+" ");
                        Console.WriteLine("]");
                    }
                }
            }
        }

        private void CreateLearningVector() {
            this.learningVector = new int[weightMatrixSize];
            for (int i = 0; i < learningVector.Length; i++)
                learningVector[i] = -1;
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
            int space = (int)(summer.ActualWidth - 2*marginLeftRight) / (number+1);
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
                for (int i = 0; i < number; i++)
                {
                    TextBlock tb = new TextBlock()
                    {
                        Text = String.Format("{0:n0}*{1:n2}", pr.inputs[i],pr.weights[i]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Background= new SolidColorBrush(getColorFromWeight((double)pr.weights[i]))
                       
                        //FontWeight = (Network.enumerator.Current.getId() == i ? FontWeights.ExtraBold : FontWeights.Normal)
                    };
                    summer.Children.Add(tb);
                    Canvas.SetTop(tb, 0);
                    Canvas.SetLeft(tb, marginLeftRight + (i+1) * space ); 
                }

                TextBlock output = new TextBlock()
                {
                    Text = String.Format("sgn({0:f2}) = {1}", pr.output, (pr.output >= 0 ? 1 : -1)),
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
                Canvas.SetLeft(output, summer.ActualWidth / 2 - 120); 

                #endregion
            }
        }







      

        private void weightsWisualization_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.drawArray(Network.weightMatrix, this.weightMatrixSize);
            this.drawNeurons(Network.neurons.ToArray(), this.neuronArraySize);
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Wczytaj
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Pliki tekstowe (*.txt)|*.txt|Pliki out (*.out)|*.out|Wszystkie pliki (*.*)|*.*";
            Nullable<bool> result = dlg.ShowDialog();
            // Jezeli nie wybrano anuluj
            if (result == true)
            {
                int[] t4 = Network.loadPattern(dlg.FileName);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            // Zapisz
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Pliki tekstowe (*.txt)|*.txt|Pliki out (*.out)|*.out|Wszystkie pliki (*.*)|*.*";
            Nullable<bool> result = dlg.ShowDialog();
            // Jezeli nie wybrano anuluj
            if (result == true)
            {
                int[] t4 = Network.loadPattern(dlg.FileName);
                Network.savePattern(t4,dlg.FileName);
            }
        }

        // Stworz nowa siec o wymiarach N x N
        private void createNewNetwork(int size) {
            if (this.neuronArraySize != size) { 
                this.neuronArraySize = size;
                this.weightMatrixSize = size * size;
                this.LearningVectorsList.Items.Clear();
            }

            HopfieldNetwork.Network.learningVectors.Clear();
            HopfieldNetwork.Network.size = size;
            HopfieldNetwork.Network.neurons.Clear();
            HopfieldNetwork.Network.InitializeNetwork(neuronArraySize);
            // Zresetuj wektor uczący
            this.CreateLearningVector();

                // Ustaw tryb edycji sieci
                this.editMode = true;
                HopfieldNetwork.Network.Iterate();

                Action action = () =>
                {
                    this.drawArray(Network.weightMatrix, this.weightMatrixSize);
                    this.drawNeurons(Network.neurons.ToArray(), this.neuronArraySize);
                    this.drawSummer(Network.enumerator.Current.getPartialResults());

                };
                var dispatcher = StatusLabel.Dispatcher;
                if (dispatcher.CheckAccess())
                    action();
                else
                    dispatcher.Invoke(action);
  
        }

        // Resetuj sieć
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
           // Czyli stwórz nową sieć o takich samym rozmiarze
            if (this.editMode)
            { 
                
            }
            this.createNewNetwork(this.neuronArraySize);
        }
   
        // Zapisz stan sieci jako wektor uczacy
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            this.LearningVectorsList.Items.Add(this.ZapiszStanSieciDoTablicy());
        }

        private string ZapiszStanSieciDoTablicy(String name = "") {
            String vektor = "[";

            for (int i = 0; i < learningVector.Length; i++)
                // Jeżeli tryb edycji to po prostu pobierz dane z tymczasowego wektora uczacego
                if (this.editMode)
                    vektor += (learningVector[i] + " ");
                // jezeli stan symulacji
                else
                    vektor += HopfieldNetwork.Network.neurons[i].getActivationState() + " ";

            if (name == "")
                vektor += "] -  Wektor " + (LearningVectorsList.Items.Count + 1).ToString();
            else
                vektor += "] - " + name;
            return vektor;
        }



        // Wczytaj stan sieci z wektora uczacego na liscie
        private void LearningVectorsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LearningVectorsList.SelectedItem != null)
            {
                this.loadLearningVectorFromString(this.LearningVectorsList.SelectedItem.ToString());
            }
        }


        private void loadLearningVectorFromString(String text, bool addToLerningVectors = false) {
            int[] vector = new int[weightMatrixSize];
            text = text.Substring(1, text.IndexOf(']') - 2);
            string[] numbers = text.Split(' ');
            int i = 0;
            HopfieldNetwork.Network.neurons.Clear();
            foreach (string number in numbers)
            {
                HopfieldNetwork.Network.neurons.Add(new Neuron(int.Parse(number), i));
                vector[i++] = int.Parse(number);
            }
            if (addToLerningVectors) {
                HopfieldNetwork.Network.learningVectors.Add(vector);
                HopfieldNetwork.Network.setWeights();
            }
            this.drawNeurons(Network.neurons.ToArray(), this.neuronArraySize);
        }


        // LearningVectorList ContextMenu Usun
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            if (LearningVectorsList.SelectedIndex == -1) return;
            LearningVectorsList.Items.RemoveAt(LearningVectorsList.SelectedIndex);
        }

        // LearningVectorList ContextMenu Nazwij
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            if (LearningVectorsList.SelectedIndex == -1) return;
            String nazwa = LearningVectorsList.SelectedItem.ToString();
            Console.WriteLine(nazwa.Substring(0, nazwa.IndexOf(']')+1));

            String n = nazwa.Substring(nazwa.IndexOf(']') + 4);
            if (n.IndexOf('✓') >= 0)
                n = n.Substring(0,n.Length - 3);
            Console.WriteLine(n);
        }

        private void DodajWektorDoSieciNeuronowej(object sender, RoutedEventArgs e)
        {
            if (LearningVectorsList.SelectedIndex == -1) return;
            this.loadLearningVectorFromString(LearningVectorsList.SelectedItem.ToString(),true);
            drawArray(HopfieldNetwork.Network.weightMatrix,weightMatrixSize);
            this.LearningVectorsList.Items[LearningVectorsList.SelectedIndex] += " ✓";
        }

        private void RozpocznijSymulacje(object sender, RoutedEventArgs e)
        {
            // zapisz jako stan wejsciowy aktualny stan sieci
            this.LearningVectorsList.Items.Add(this.ZapiszStanSieciDoTablicy("Stan wejściowy"));
            // Wczytaj zawartosc tymczasowego wektora uczacego do sieci neuronowej
            HopfieldNetwork.Network.neurons.Clear();
            for (int i = 0; i < this.learningVector.Length; i++)
                HopfieldNetwork.Network.neurons.Add(new Neuron(this.learningVector[i], i));

            this.editMode = false;
            iter = 0;
        }

        private void new_3x3(object sender, RoutedEventArgs e)
        {
            this.createNewNetwork(3);
        }

        private void new_4x4(object sender, RoutedEventArgs e)
        {
            this.createNewNetwork(4);
        }

        private void new_5x5(object sender, RoutedEventArgs e)
        {
            this.createNewNetwork(5);
        }

        private void new_6x6(object sender, RoutedEventArgs e)
        {
            this.createNewNetwork(6);
        }

        private void new_7x7(object sender, RoutedEventArgs e)
        {
            this.createNewNetwork(7);
        }

        private void new_8x8(object sender, RoutedEventArgs e)
        {
            this.createNewNetwork(8);
        }

        private void new_9x9(object sender, RoutedEventArgs e)
        {
            this.createNewNetwork(9);
        }

        private void PokazSumator(object sender, RoutedEventArgs e)
        {
            this.showSummer = !this.showSummer;
            this.summer.Visibility = (this.showSummer ?  Visibility.Visible : Visibility.Collapsed );
            this.PokazSumatorMenu.Header = (this.showSummer ? "Pokaż sumator ✓" : "Pokaż sumator");
        }

        private void PokazWagi(object sender, RoutedEventArgs e)
        {
            this.showWeightsNumbers = !this.showWeightsNumbers;
            this.PokazWagiMenu.Header = (this.showWeightsNumbers ? "Pokaż wagi ✓" : "Pokaż wagi");
        }

        private void ResetujSymulacje(object sender, RoutedEventArgs e)
        {
            this.editMode = true;
            iter = 0;
            this.finished = false;
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            HopfieldNetwork.Network.hebbs = true;
            this.Hebb_menu.Header = "Reguła Hebb'a ✓";
            this.Storkey_menu.Header = "Reguła Storkey'a";
        }

        private void Storkey(object sender, RoutedEventArgs e)
        {
            HopfieldNetwork.Network.hebbs = false;
            this.Hebb_menu.Header = "Reguła Hebb'a";
            this.Storkey_menu.Header = "Reguła Storkey'a  ✓";
        }
        
    }
}
