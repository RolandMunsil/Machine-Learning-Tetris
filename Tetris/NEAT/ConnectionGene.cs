using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    public class ConnectionGene
    {
        //NOTE: node numbers are created assuming the nodes are in the order (input nodes, output nodes, hidden nodes)
        public readonly int inNodeNum;
        public readonly int outNodeNum;
        public readonly double weight;
        public readonly bool enabled;
        public readonly int innovationNumber;

        public ConnectionGene(int inNodeNum, int outNodeNum, double weight, bool enabled, int innovationNumber)
        {
            this.inNodeNum = inNodeNum;
            this.outNodeNum = outNodeNum;
            this.weight = weight;
            this.enabled = enabled;
            this.innovationNumber = innovationNumber;
        }
    }
}
