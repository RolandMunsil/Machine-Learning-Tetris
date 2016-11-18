using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    public class Genome
    {
        public int numInputs;
        public int numOutputs;
        //public int numHiddenNodes;
        private HashSet<int> hiddenNodes;
        public List<ConnectionGene> connectionGenes;

        public Genome(int numInputs, int numOutputs, IEnumerable<int> hiddenNodes)
        {
            this.numInputs = numInputs;
            this.numOutputs = numOutputs;
            this.hiddenNodes = new HashSet<int>(hiddenNodes);
            this.connectionGenes = new List<ConnectionGene>();
        }

        //A Genome with no hidden nodes
        public Genome(int numInputs, int numOutputs)
        {
            this.numInputs = numInputs;
            this.numOutputs = numOutputs;
            this.hiddenNodes = new HashSet<int>();
            this.connectionGenes = new List<ConnectionGene>();
        }

        public Genome Clone()
        {
            Genome copy = new Genome(numInputs, numOutputs, hiddenNodes);
            copy.connectionGenes.AddRange(connectionGenes);
            return copy;
        }

        public bool GenesAreInOrder()
        {
            return connectionGenes.SequenceEqual(connectionGenes.OrderBy(g => g.innovationNumber));
        }

        public void CheckConnectionsMatchHiddenNodes()
        {
            List<int> allReferencedNodes = connectionGenes.SelectMany(g => new[] { g.inNodeNum, g.outNodeNum }).Distinct().ToList();
            foreach(int node in allReferencedNodes)
            {
                if(node >= numInputs + numOutputs && !hiddenNodes.Contains(node))
                {
                    throw new Exception("A hidden node is referenced that is not in hiddenNodes!");
                }
            }
            foreach(int node in hiddenNodes)
            {
                if(!allReferencedNodes.Contains(node))
                {
                    throw new Exception("There is a node that has no references!");
                }
            }
        }

        public void AddGene(ConnectionGene gene)
        {
            if(IsHidden(gene.inNodeNum))
                hiddenNodes.Add(gene.inNodeNum);
            if (IsHidden(gene.outNodeNum))
                hiddenNodes.Add(gene.outNodeNum);

            if (NodeDependsOn(gene.inNodeNum, gene.outNodeNum))
                Debugger.Break();

            if (connectionGenes.Count == 0 ||
                gene.innovationNumber > connectionGenes[connectionGenes.Count - 1].innovationNumber)
            {
                connectionGenes.Add(gene);
            }
            else
            {
                int insertAfterIndex = connectionGenes.FindLastIndex(g => g.innovationNumber < gene.innovationNumber);
                connectionGenes.Insert(insertAfterIndex + 1, gene);
            }
            if (!GenesAreInOrder()) throw new Exception("Genes are not in order");
            CheckConnectionsMatchHiddenNodes();
            if (connectionGenes.GroupBy(g => g.innovationNumber).Count() != connectionGenes.Count)
                throw new Exception("There are duplicate gene innovation numbers");
            if (connectionGenes.GroupBy(g => new Tuple<int, int>(g.inNodeNum, g.outNodeNum)).Count() != connectionGenes.Count)
                throw new Exception("There are duplicate gene connections");
        }

        public IEnumerable<int> HiddenNodes()
        {
            return hiddenNodes;
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
            return false;
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

        public override string ToString()
        {
            return $"{numInputs} in, {numOutputs} out, {hiddenNodes.Count} hidden nodes, {connectionGenes.Count(g => g.enabled)}/{connectionGenes.Count} enabled connections";
        }
    }
}
