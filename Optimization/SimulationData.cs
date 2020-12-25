using System.Collections.Generic;

namespace Optimization
{
    class SimulationData
    {
        public int SimulationBoardSize { get; set; }
        public (int, int) SourceCell { get; set; }
        public (int, int) DestinationCell { get; set; }
        public int PopulationSize { get; set; }
        public int GenerationAmount { get; set; }
        public Grid SimulationGrid { get; set; }
        public List<Path> PopulationPaths { get; set; }
        public Dictionary<int, (double f1, double f2)> Fitnesses { get; set; }
        public Dictionary<int, int> Ranks { get; set; }
        public double MutationProbability { get; set; }
        private static SimulationData instance = new SimulationData();
        public static SimulationData Instance => instance;
        private SimulationData() { }
    }
}
