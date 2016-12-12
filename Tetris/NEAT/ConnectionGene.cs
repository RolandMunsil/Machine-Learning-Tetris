using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    public struct ConnectionGene
    {
        public int inNode;
        public int outNode;
        public double weight;
        public bool enabled;
        public int innovationNumber;

        public ConnectionGene(int inNodeNum, int outNodeNum, double weight, bool enabled, int innovationNumber)
        {
            this.inNode = inNodeNum;
            this.outNode = outNodeNum;
            this.weight = weight;
            this.enabled = enabled;
            this.innovationNumber = innovationNumber;
        }

        public override string ToString()
        {
            return (enabled ? "" : "[DISABLED] ") + $"{inNode}->{outNode} with weight {weight} (inn {innovationNumber})";
        }
    }
}
