
using System.Collections.Generic;

namespace Optimization
{
    class SimulationData
    {
        public int SimulationBoardSize { get; set; }
        public (int, int) SourceCell { get; set; }
        public (int, int) DestinationCell { get; set; }
        public int InitialPopulationSize { get; set; }
        public int GenerationAmount { get; set; }
        public Grid SimulationGrid { get; set; }
        public List<Path> PathsPopulation { get; set; }
        public Dictionary<int,(double f1, double f2)> Fitnesses { get; set; }


        private static SimulationData instance = new SimulationData();

        public static SimulationData Instance => instance;
        private SimulationData() { }
    }
}
