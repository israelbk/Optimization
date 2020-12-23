
namespace Optimization
{
    class SimulationData
    {
        public int simulationBoardSize { get; set; }
        public int obstaclePrecentageCreation { get; set; }
        public (int, int) sourceCell { get; set; }
        public (int, int) destinationCell { get; set; }
        public int initialPopulationSize { get; set; }
        public int generationAmount { get; set; }
        public Grid simulationGrid { get; set; }

        private static SimulationData instance = new SimulationData();

        public static SimulationData Instance => instance;
        private SimulationData() { }
    }
}
