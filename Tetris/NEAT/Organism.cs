using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    public class Organism
    {
        public NeuralNetwork neuralNet;
        public Genome genome;

        public long originalFitness;
        public double fitness;

        public Organism(Genome genome)
        {
            this.genome = genome;
            this.neuralNet = new NeuralNetwork(genome);
            this.originalFitness = -1;
        }
    }
}
