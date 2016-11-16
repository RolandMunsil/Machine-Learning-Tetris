﻿using System;
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
            //This takes a lot of time! Mostly because of NeuralNet constructor. Reduce # of neural nets created
            // and/or speed up neural net construcor.

            this.genome = genome;
            this.neuralNet = new NeuralNetwork(genome);
            this.originalFitness = -1;
        }
    }
}