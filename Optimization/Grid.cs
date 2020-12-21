using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    class Grid
    {
        public int size { get; private set; }
        private double[,] cellsWD { get; set; }
        private Random randomizer;

        public Grid(int size, int obstaclesAnount)
        {
            this.size = size;
            this.cellsWD = new double[size, size];
            randomizer = new Random();
            initializeGrid(obstaclesAnount);
        }

        private void initializeGrid(int amountOfObstacles)
        {
            int obstaclesToCreate = amountOfObstacles;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int boardTotalSize = size * size;
                    int subSize = (i * size + j);
                    double propability = (double)obstaclesToCreate / (double)(boardTotalSize - subSize);
                    double random = randomizer.NextDouble();
                    if (random < propability)
                    {
                        cellsWD[i, j] = 1;
                        obstaclesToCreate--;
                    }
                    else
                    {
                        cellsWD[i, j] = Math.Round(random, 2);
                    }
                }
            }
        }
        
        public double GetCellWD((int x,int y) index)
        {
            return cellsWD[index.x, index.y];
        }

        public void setGrid(double[,] grid)
        {
            this.cellsWD = grid;
        }
    }
}
