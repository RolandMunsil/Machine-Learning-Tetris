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

        public bool GenesAreInOrder()
        {
            return connectionGenes.SequenceEqual(connectionGenes.OrderBy(g => g.innovationNumber));
        }

        public void InsertGene(ConnectionGene gene)
        {
            int insertAfterIndex = connectionGenes.FindLastIndex(g => g.innovationNumber < gene.innovationNumber);
            connectionGenes.Insert(insertAfterIndex + 1, gene);
            if (!GenesAreInOrder()) Debugger.Break();
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

        public static void CompatabilityParts(Genome genome1, Genome genome2, out int numDisjoint, out int numExcess, out double weightDiff, out int numMatching)
        {
            numDisjoint = 0;
            numExcess = 0;
            weightDiff = 0;
            numMatching = 0;
            int i1 = 0;
            int i2 = 0;
            while (true)
            {
                if (i1 == genome1.connectionGenes.Count)
                {
                    numExcess = genome2.connectionGenes.Count - i2;
                    break;
                }
                if (i2 == genome2.connectionGenes.Count)
                {
                    numExcess = genome1.connectionGenes.Count - i1;
                    break;
                }

                ConnectionGene gene1 = genome1.connectionGenes[i1];
                ConnectionGene gene2 = genome2.connectionGenes[i2];
                if (gene1.innovationNumber < gene2.innovationNumber)
                {
                    numDisjoint++;
                    i1++;
                }
                else if (gene2.innovationNumber < gene1.innovationNumber)
                {
                    numDisjoint++;
                    i2++;
                }
                else //equal
                {
                    numMatching++;
                    weightDiff += Math.Abs(gene1.weight - gene2.weight);
                    i1++;
                    i2++;
                }
            }
        }
    }
}
