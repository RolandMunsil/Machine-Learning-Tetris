using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    [DebuggerDisplay("#{speciesNumber}: {members.Count} members w/ representative {representativeGenome}")]
    class Species
    {
        public int speciesNumber;
        public Genome representativeGenome;
        public List<Organism> members;

        public double AverageFitness
        {
            get
            {
                return members.Average(m => m.fitness);
            }
        }
    }
}
