using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tetris.NEAT;

namespace NEATTests
{
    [TestClass]
    public class NEATTests
    {
        private static Genome MakeGenome(params int[] innovationNums)
        {
            Genome g = new Genome(-1, -1, -1);
            foreach(int inn in innovationNums)
            {
                g.connectionGenes.Add(new ConnectionGene(-1, -1, 1, true, inn));
            }
            return g;
        }

        private static void AssertCompatabilityParts(int[] innovations1, int[] innovations2, 
            int expectedDisjoint, int expectedExcess, int expectedMatching)
        {
            Genome genome1 = MakeGenome(innovations1);
            Genome genome2 = MakeGenome(innovations2);

            int numDisjoint, numExcess, numMatching;
            double weightDiff;
            Genome.CompatabilityParts(genome1, genome2, out numDisjoint, out numExcess, out weightDiff, out numMatching);
            Assert.AreEqual(numDisjoint, expectedDisjoint);
            Assert.AreEqual(expectedExcess, numExcess);
            Assert.AreEqual(expectedMatching, numMatching);
            Assert.AreEqual(weightDiff, 0);

            //Make sure it works both ways.
            Genome.CompatabilityParts(genome2, genome1, out numDisjoint, out numExcess, out weightDiff, out numMatching);
            Assert.AreEqual(numDisjoint, expectedDisjoint);
            Assert.AreEqual(expectedExcess, numExcess);
            Assert.AreEqual(expectedMatching, numMatching);
            Assert.AreEqual(weightDiff, 0);
        }

        [TestMethod]
        public void TestCompatability()
        {
            AssertCompatabilityParts(
                new[] { 1, 2, 3, 4 },
                new[] { 5, 6, 7, 8 },
                4, 4, 0);

            AssertCompatabilityParts(
                new[] { 2, 4, 6, 8, 10 },
                new[] { 1, 3, 5, 7, 9 },
                9, 1, 0);

            AssertCompatabilityParts(
                new[] { 1, 2, 3, 9 },
                new[] { 5, 6, 7, 8 },
                7, 1, 0);

            AssertCompatabilityParts(
                new[] { 1, 2, 3, 4 },
                new[] { 1, 2, 3, 4 },
                0, 0, 4);

            AssertCompatabilityParts(
                new[] { 1 },
                new[] { 2, 3, 4, 5, 6, 7, 8, 9 },
                1, 8, 0);

            AssertCompatabilityParts(
                new int[] { },
                new[] { 2, 3, 4, 5, 6, 7, 8, 9 },
                0, 8, 0);

            AssertCompatabilityParts(
                new[] { 47, 48, 670, 10000 },
                new[] { 47, 48, 670, 10000 },
                0, 0, 4);

            AssertCompatabilityParts(
                new[] {    47, 48, 670, 10000 },
                new[] { 1, 47, 48, 670, 10000 },
                1, 0, 4);

            AssertCompatabilityParts(
                new[] { 1, 2, 3, 4, 5,       8,  },
                new[] { 1, 2, 3, 4, 5, 6, 7,    9  },
                3, 1, 5);

            AssertCompatabilityParts(
                new[] {          4, 5,       8,   },
                new[] { 1, 2, 3, 4, 5, 6, 7,    9 },
                6, 1, 2);

            AssertCompatabilityParts(
                new[] {    2,    4, 5,       8,   },
                new[] { 1,    3, 4, 5, 6, 7,    9 },
                6, 1, 2);

            AssertCompatabilityParts(
                new[] {    2,    4, 5,       8,   },
                new[] { 1, 2, 3, 4, 5, 6, 7,    9 },
                5, 1, 3);
        }
    }
}
