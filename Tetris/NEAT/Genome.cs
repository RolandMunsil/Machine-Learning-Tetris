using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    [DebuggerDisplay("{numInputs} inputs, {numHiddenNodes} hidden, {numOutputs} outputs. {connectionGenes.Count} connections")]
    public class Genome
    {
        public int numInputs;
        public int numOutputs;
        public int numHiddenNodes;
        public List<ConnectionGene> connectionGenes;

        public Genome(int numInputs, int numOutputs, int numHiddenNodes)
        {
            this.numInputs = numInputs;
            this.numOutputs = numOutputs;
            this.numHiddenNodes = numHiddenNodes;
            connectionGenes = new List<ConnectionGene>();
        }

        public Genome Clone()
        {
            Genome copy = new Genome(numInputs, numOutputs, numHiddenNodes);
            copy.connectionGenes.AddRange(connectionGenes);
            return copy;
        }

        public bool IsInput(int node)
        {
            return node < numInputs;
        }
        public bool IsOutput(int node)
        {
            return !IsInput(node) && !IsHidden(node);
        }
        public bool IsHidden(int node)
        {
            return node >= (numInputs + numOutputs);
        }

        public IEnumerable<int> GetInputsTo(int node)
        {
            return from gene in connectionGenes
                   where gene.outNodeNum == node
                   select gene.inNodeNum;
        }
        public IEnumerable<int> GetOutputsOf(int node)
        {
            return from gene in connectionGenes
                   where gene.inNodeNum == node
                   select gene.inNodeNum;
        }

        public bool NodeDependsOn(int node, int possibleDependNode)
        {
            foreach (int inputTo in GetInputsTo(node))
            {
                if (inputTo == possibleDependNode || NodeDependsOn(inputTo, possibleDependNode))
                    return true;
            }
            return true;
        }
    }
}
