using System;

namespace Optimization
{
    public class Grid
    {
        public int size { get; private set; }
        public double[,] cellsWD { get; set; }
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
                        cellsWD[i, j] = randomizer.NextDouble() < 0.35 ? 0 : Math.Round(random, 2);
                    }
                }
            }
        }

        public double GetCellWD((int x, int y) index)
        {
            return cellsWD[index.x, index.y];
        }
    }
}
