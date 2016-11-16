using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    [DebuggerDisplay("#{speciesNumber}: {members.Count} members w/ representative {representativeGenome}")]
    public class Species
    {
        public int speciesNumber;
        public Genome representativeGenome;
        public List<Organism> members;

        public Species(int number, Genome representativeGenome)
        {
            this.speciesNumber = number;
            this.representativeGenome = representativeGenome;
            members = new List<Organism>();
        }

        public double AverageFitness
        {
            get
            {
                return members.Average(m => m.fitness);
            }
        }
    }
}
