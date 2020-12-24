namespace Optimization
{
    class Program
    {
        static void Main(string[] args)
        {
            SimulationInitializer.InitSimulation();
            Printer.PrintGrid();
            MooAlgorithm alg = new MooAlgorithm();
            alg.RunSimulation();
        }
    }
}
