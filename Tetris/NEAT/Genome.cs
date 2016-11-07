using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    public class Genome
    {
        public int numInputs;
        public int numOutputs;
        public int numHiddenNodes;
        public List<ConnectionGene> connectionGenes;
    }
}
