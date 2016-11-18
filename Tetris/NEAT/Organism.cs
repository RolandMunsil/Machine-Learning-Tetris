using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    public class Organism
    {
        private static int nextUID = 1;

        public NeuralNetwork neuralNet;
        public Genome genome;

        public double fitness;

        public readonly int uid;

        public Organism(Genome genome)
        {
            //This takes a lot of time! Mostly because of NeuralNet constructor. Reduce # of neural nets created
            // and/or speed up neural net construcor.

            this.genome = genome;
            this.neuralNet = new NeuralNetwork(genome);
            this.fitness = -1;

            this.uid = nextUID++;
        }

        public override string ToString()
        {
            return $"{{{genome}}} w/ fitness {fitness}";
        }
    }
}
