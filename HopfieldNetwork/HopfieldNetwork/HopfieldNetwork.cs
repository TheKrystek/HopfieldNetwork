using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HopfieldNetwork
{
    class Network
    {
        public static float[,] weightMatrix;
        public static List<Neuron> neurons = new List<Neuron>();
        public static List<int[]> learningVectors = new List<int[]>();
        public static List<Neuron>.Enumerator enumerator;

        static void setWeights(int n)
        {
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



        public static void test()
        {
            int[] t1 = new int[9] { -1, -1, -1, -1, 1, -1, -1, -1, -1 };
            int[] t2 = new int[9] { -1, 1, -1, 1, -1, 1, -1, 1, -1 };
            int[] t3 = new int[9] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            // int[] testVector = new int[9] { 1, -1, 1, -1, -1, 1, 1, -1, 1 };
            int[] testVector = new int[9] { -1, 1, -1, 1, 1, -1, -1, 1, -1 };
            learningVectors.Add(t1);
            learningVectors.Add(t2);
            learningVectors.Add(t3);
            setWeights(9);
            //  printArray(weightMatrix,9);
            for (int i = 0; i < 9; i++)
            {
                neurons.Add(new Neuron(testVector[i], i));
            }

            enumerator = neurons.GetEnumerator();
            Debug.WriteLine("Stan początkowy:");
            foreach (Neuron n in neurons)
            {
                Debug.Write(n.getActivationState() + " ");
            }
            Debug.WriteLine("");
        }


        public static void Iterate()
        {
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                enumerator = neurons.GetEnumerator();
                enumerator.MoveNext();
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
            foreach (Neuron n in neurons)
            {
                if (this.id == n.getId()) continue;
                this.effectiveInput += weightMatrix[this.id, n.getId()] * n.getActivationState();
                Debug.WriteLine(this.id + " " + n.getId() + " " + n.getActivationState() + " " + weightMatrix[this.id, n.getId()] + " " + this.effectiveInput);
            }
            //  this.effectiveInput += prevActivationState;
            //Debug.Write(" " +effectiveInput + " ");
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
}

