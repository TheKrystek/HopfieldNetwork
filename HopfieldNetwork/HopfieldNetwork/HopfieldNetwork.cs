using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace HopfieldNetwork
{
    class Network
    {
        public static float[,] weightMatrix;
        public static List<Neuron> neurons = new List<Neuron>();
        public static List<int[]> learningVectors = new List<int[]>();
        public static List<Neuron>.Enumerator enumerator;
        public static int size = 3;

        public  static void setWeights()
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

        public static void savePattern(int[] pattern, string filename)
        {
            FileStream fs = new FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            int mask = 7;
            byte b = 0;
            fs.Write(BitConverter.GetBytes(size), 0, 4);
            fs.Write(BitConverter.GetBytes(size), 0, 4);
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < size * size; i++)
            {
                if (mask < 0)
                {
                    Debug.Write("Byte " + Convert.ToString(b, 2) + "\n");
                    bytes.Add(b);
                    mask = 7;
                    b = 0;

                }
                if (pattern[i] == 1)
                {
                    b |= (byte)(1 << mask);
                }
                else if (pattern[i] == -1)
                {

                }
                mask--;
            }

            if (mask > 0)
            {
                bytes.Add(b);
                Debug.Write("Byte " + Convert.ToString(b, 2) + "\n");
            }
            fs.Write(bytes.ToArray(), 0, bytes.Count);
            fs.Close();
        }

        public static int[] loadPattern(string filename)
        {
            FileStream fs = new FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            int x = 0;
            x += fs.ReadByte() + (fs.ReadByte() << 8) + (fs.ReadByte() << 16) + (fs.ReadByte() << 24);
            int y = 0;
            y += fs.ReadByte() + (fs.ReadByte() << 8) + (fs.ReadByte() << 16) + (fs.ReadByte() << 24);

            if ((size == 0 && size == 0))
            {
                size = x;
                size = y;
            }

            if ((size != x || size != y)) return null;


            // else return null;

            List<byte> bytes = new List<byte>();
            List<int> ret = new List<int>();

            int count = 0;
            int counter = size * size;
            if ((size * size) % 8 != 0)
            {
                count = (size * size) / 8 + 1;
            }
            else count = (size * size) / 8;
            Debug.Write("C " + count + "\n");

            for (int i = 0; i < count; i++)
            {
                byte bb = (byte)fs.ReadByte();
                Debug.Write(bb + " ");
                bytes.Add(bb);
            }

            // foreach (byte i in bytes) Debug.Write(i + " ");

            foreach (byte b in bytes)
            {

                for (int mask = 7; mask >= 0; mask--)
                {
                    if (counter == 0) break;
                    if (((b >> mask) & 1) == 1)
                    {
                        ret.Add(1);
                    }
                    else
                    {
                        ret.Add(-1);
                    }
                    counter--;
                }

            }
            foreach (int i in ret) Debug.Write(i + " ");
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
            enumerator = neurons.GetEnumerator();
        }



        public static void Iterate()
        {
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                enumerator = neurons.GetEnumerator();
                enumerator.MoveNext();
                Console.WriteLine("enumerator "+enumerator);
            }
            Debug.WriteLine("Current neuron:" + enumerator.Current.getId());
            enumerator.Current.calculateEffectiveInput(neurons, weightMatrix);
            enumerator.Current.setActivationState();


            foreach (Neuron n in neurons)
            {
                Debug.Write(n.getActivationState() + " ");
            }

            Debug.WriteLine("");
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
            //  this.prevActivationState = this.activationState;
            this.effectiveInput = 0;
            pr = new PartialReults(this.id);
            foreach (Neuron n in neurons)
            {
                if (this.id == n.getId()) continue;
                this.effectiveInput += weightMatrix[this.id, n.getId()] * n.getActivationState();
                this.pr.addPartialResults(n.getId(), n.getActivationState(), weightMatrix[this.id, n.getId()], this.effectiveInput);
                Debug.WriteLine(this.id + " " + n.getId() + " " + n.getActivationState() + " " + weightMatrix[this.id, n.getId()] + " " + this.effectiveInput);
            }
            //  this.effectiveInput += prevActivationState;
            //Debug.Write(" " +effectiveInput + " ");
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

