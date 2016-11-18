using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    public class Species
    {
        public int speciesNumber;
        public Genome representativeGenome;
        public List<Organism> members;

        public int lastImprovedGeneration;
        public double maxOrigFitnessLastImprovedGeneration;

        public Species(int number, Genome representativeGenome)
        {
            this.speciesNumber = number;
            this.representativeGenome = representativeGenome;
            members = new List<Organism>();

            lastImprovedGeneration = 0;
            maxOrigFitnessLastImprovedGeneration = 0;
        }

        public double AverageFitness
        {
            get
            {
                return members.Average(m => m.fitness);
            }
        }

        public double MaxFitness
        {
            get
            {
                return members.Max(m => m.fitness);
            }
        }

        public override string ToString()
        {
            return $"#{speciesNumber}: {members.Count} members w/ representative {{{representativeGenome}}} and avg fitness {AverageFitness}";
        }
    }
}
