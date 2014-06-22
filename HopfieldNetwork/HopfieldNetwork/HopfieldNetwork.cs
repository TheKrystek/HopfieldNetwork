using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using ListExtension;

namespace HopfieldNetwork
{
    class Network
    {
        public static float[,] weightMatrix;
        public static List<Neuron> neurons = new List<Neuron>();
        public static List<int[]> learningVectors = new List<int[]>();
        public static List<int> neuronEnum;
        //public static List<Neuron>.Enumerator enumerator;
        public static int size = 3;
        public static int current_neuron = 0;
        public static bool Initialized = false;
        public static bool hebbs = false;
        public static bool stable = true;
        public static bool randomSequence = false;

        public static void setWeights()
        {
            if (hebbs)
                setWeightsHebb();
            else
                setWeightsStorkey();
        }


        public static void setWeightsHebb()
        {
            int n = size * size;
            weightMatrix = new float[n, n];
            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                {
                    weightMatrix[x, y] = 0;
                    if (x == y) continue;
                    foreach (int[] learningVector in learningVectors)
                    {
                        weightMatrix[x, y] += learningVector[x] * learningVector[y];
                    }
                    weightMatrix[x, y] = weightMatrix[x, y] / n;
                }
        }


        public static void setWeightsStorkey()
        {
             int n = size * size;
            weightMatrix = new float[n, n];

            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                {
                    weightMatrix[x, y] = 0;
                }

            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                {
                    if (x == y) continue;
                    foreach (int[] learningVector in learningVectors)
                    {

                        float hij = 0;
                        for (int k = 0; k < n; k++)
                        {

                            if (k == x || k == y) continue;
                            hij += weightMatrix[x, k] * (float)learningVector[k];
                        }
                        weightMatrix[x, y] += (float)((learningVector[x] * learningVector[y]) / (float)n - (hij * learningVector[x]) / (float)n - (hij * learningVector[y]) / (float)n);

                    }
                }

        }


        public static void savePattern(int[] pattern, string filename)
        {
            FileStream fs = new FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            foreach (int i in pattern)
            {
                if(i==1)
                {
                    fs.WriteByte((byte)'1');
                }
                else
                {
                    fs.WriteByte((byte)'0');
                }
            }
            fs.Close();
        }

        public static int[] loadPattern(string filename)
        {
            FileStream fs = new FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            List<int> ret = new List<int>();

            int read;
            while((read = fs.ReadByte()) > 0)
            {
                Debug.WriteLine(read);
                if((char) read =='1')
                {
                    ret.Add(1);
                }else
                {
                    ret.Add(-1);
                }
            }

            if (ret.Count != size * size) throw new Exception("Wczytano wektor o rozmiarze innym niż aktualnie używana sieć");
            return ret.ToArray();
        }


        // Tworzy pustą sieć o rozmiarze N x N
        public static void InitializeNetwork(int N){
            // Ustaw wagi na 0
            size = N;
            setWeights();
            neurons.Clear();
            for (int i = 0 ; i < N*N; i++)
                neurons.Add(new Neuron(-1, i));
            current_neuron = 0;
            neuronEnum = new List<int>();
            Initialized = true;
            stable = true;
        }



        public static void Iterate()
        {
            
            if (neuronEnum.Count == 0) for (int i = 0; i < size * size; i++) neuronEnum.Add(i);
            if (randomSequence == true) neuronEnum.Shuffle();
            current_neuron =  neuronEnum.First();
            neuronEnum.Remove(neuronEnum.First());
            neurons[current_neuron].calculateEffectiveInput(neurons, weightMatrix);
            int tmpActivationState = neurons[current_neuron].getActivationState();
           neurons[current_neuron].setActivationState();
            
            if (tmpActivationState != neurons[current_neuron].getActivationState())
                stable = false;
            
        }

        public static float calculateEnergy()
        {
            float result=0;

            int i = 0, j = 0;

         /*   for (i=0; i < size;i++ )
                for (j = 0; j < size; j++)
                {
                    if (i == j) continue;
                    result += (float)weightMatrix[i, j] * (float)neurons.ElementAt(i).getActivationState() * (float)neurons.ElementAt(j).getActivationState();
                }

                return -(result / 2);*/

            for (i = 0; i < size; i++)
                for (j = 0; j <=i; j++)
                {
                    if (i == j) continue;
                    result += (float)weightMatrix[i, j] * (float)neurons.ElementAt(i).getActivationState() * (float)neurons.ElementAt(j).getActivationState();
                }

            return -result;
        }

     
    }



    class Neuron
    {
        int id;
        int activationState;
        float effectiveInput;
        float prevActivationState;
        PartialReults pr;


        public int getActivationState()
        {
            return this.activationState;
        }

        public int getId()
        {
            return this.id;
        }

        public void calculateEffectiveInput(List<Neuron> neurons, float[,] weightMatrix)
        {
            this.effectiveInput = 0;
            pr = new PartialReults(this.id);
            foreach (Neuron n in neurons)
            {
                if (this.id == n.getId()) continue;
                this.effectiveInput += weightMatrix[this.id, n.getId()] * n.getActivationState();
                this.pr.addPartialResults(n.getId(), n.getActivationState(), weightMatrix[this.id, n.getId()], this.effectiveInput);
                Debug.WriteLine(this.id + " " + n.getId() + " " + n.getActivationState() + " " + weightMatrix[this.id, n.getId()] + " " + this.effectiveInput);
            }
        }

        public PartialReults getPartialResults()
        {
            return pr;
        }

        public void setActivationState()
        {
            Debug.Write("eff " + effectiveInput + " \n");
            if (this.effectiveInput > 0) this.activationState = 1;
            else if (this.effectiveInput < 0) this.activationState = -1;

        }
        public Neuron(int input, int id)
        {
            this.id = id;
            this.activationState = input;
            this.effectiveInput = 0;
            // this.prevActivationState = 0;
        }
    }



    class PartialReults {
        public PartialReults(int id) {
            this.id = id;
            this.connections = new List<int>();
            this.weights = new List<float>();
            this.inputs = new List<int>();
           
        }

        public void addPartialResults(int connection, int input, float weight, float output) {
            this.connections.Add(connection);
            this.inputs.Add(input);
            this.weights.Add(weight);
            this.output = output;
        }

        public int id {get;set;}
        public List<int> connections { get; set; }
        public List<int> inputs { get; set; }
        public List<float> weights { get; set; }
        public float output { get; set; }


    }
}

