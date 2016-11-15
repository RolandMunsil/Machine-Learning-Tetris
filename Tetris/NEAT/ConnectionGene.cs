using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    [DebuggerDisplay("{inNodeNum}->{outNodeNum} with weight {weight} (inn {innovationNumber}) [enabled={enabled}]")]
    public struct ConnectionGene
    {
        //NOTE: node numbers are created assuming the nodes are in the order (input nodes, output nodes, hidden nodes)
        public int inNodeNum;
        public int outNodeNum;
        public double weight;
        public bool enabled;
        public int innovationNumber;

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
