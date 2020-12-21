using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    class MooAlgorithm
    {
        private int populationSize { get; set; }
        List<Path> pathsPopulation { get; set; }

        public MooAlgorithm(int initialPopulationSize, (int, int) sIndex, (int, int) dIndex, Grid grid)
        {
            populationSize = initialPopulationSize;
            setInitialPopulation(sIndex, dIndex);
            Printer.PrintPaths(pathsPopulation);
        }

        private void setInitialPopulation((int, int) sIndex, (int, int) dIndex)
        {
            pathsPopulation = PathGenerator.Instance.GetInitialPaths(sIndex, dIndex, populationSize);
        }        
    }
}
