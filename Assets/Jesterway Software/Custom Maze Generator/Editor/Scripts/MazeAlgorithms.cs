using System;
using System.Collections.Generic;


public static class MazeAlgorithms
{
    static void ShuffleDirections(int[] givenDirections, Random random)
    {
        int n = 4;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (givenDirections[n], givenDirections[k]) = (givenDirections[k], givenDirections[n]);
        }
    }


    public static void RecursiveBacktracker(bool[] wallsX, bool[] wallsY, int gridX, int gridY, int givenLoopChance)
    {
        Random rnd = new Random();
        int[] directions = new int[] { 0, 1, 2, 3 };  //  4 directions, 0 is up, 1 is right, 2 is down, 3 is left.
        bool wallChangePossible = true;  //  This boolean gets set to true whenever the algorithm makes a normal step. Thus, when backtracking, it is only true at the first step back, i. e. at the very end of the dead-end.
        int stepsTaken = 0;  //  The number of steps that have been taken. Once Steps == number of tiles, every tile has been visited and the algorithm is done.
        int rowCounter = 0;  //  Incremented when going one step north, decremented when going one step south. Used for accurate wall deletion when going left or right.
        int gridSize = gridX * gridY;
        
        bool[] booleanTiles = new bool[gridSize];  //  Is used to keep track of where the algorithm has stepped already. False means we haven't been on that tile before.
        List<int> position = new List<int>();  //  Stack of positions, will be used for backtracking whenever a dead-end is reached.

        int currentPosition = gridX / 2;  //  Where the algorithm starts from, the middle of the lowest row. Any other position would be fine too.
        booleanTiles[currentPosition] = true;
        stepsTaken++;
        position.Add(currentPosition);

        int deadEnd;  //  This gets incremented whenever a step in any one direction has failed, either because it would have led off the maze altogether or because the tile the algorithm wanted to step on had been visited before.
                      //  If this number becomes 4, meaning all 4 directions from where the algorithm currently stands are unfit to be stepped on, the current position will be considered a dead-end.

        while (stepsTaken < gridSize)
        {
            deadEnd = 0;

            ShuffleDirections(directions, rnd);
            
            foreach (int i in directions)
            {
                if (i == 0)  //  If the first step in the shuffled list was 0 which means "Up", then try to take a step to the tile above, or "North" of the current position.
                {
                    try
                    {
                        if (booleanTiles[currentPosition + gridX] != true)  //  If we haven't been on that tile before.
                        {
                            booleanTiles[currentPosition + gridX] = true;
                            wallsX[currentPosition + gridX] = true;
                            currentPosition += gridX;
                            stepsTaken++;
                            position.Add(currentPosition);
                            wallChangePossible = true;
                            rowCounter++;
                            break;
                        }
                        deadEnd += 1;
                    }
                    catch (IndexOutOfRangeException)  //  If the tile we wanted to step on is not in the array of existing tiles. That would be like stepping off of the maze at the edge.
                    {
                        deadEnd += 1;
                    }
                }
                else if (i == 1)  //  Right/East
                {
                    if (((currentPosition + 1) % gridX) != 0)  //  If the current position is not the very right of any row.
                    {
                        if (booleanTiles[currentPosition + 1] != true)  //  If we haven't been there before.
                        {
                            booleanTiles[currentPosition + 1] = true;
                            wallsY[currentPosition + 1 + rowCounter] = true;
                            currentPosition += 1;
                            stepsTaken++;
                            position.Add(currentPosition);
                            wallChangePossible = true;
                            break;
                        }
                        deadEnd += 1;
                    }
                    else
                    {
                        deadEnd += 1;
                    }
                }
                else if (i == 2)  //  Down/South
                {
                    try
                    {
                        if (booleanTiles[currentPosition - gridX] != true)
                        {
                            booleanTiles[currentPosition - gridX] = true;
                            wallsX[currentPosition] = true;
                            currentPosition -= gridX;
                            stepsTaken++;
                            position.Add(currentPosition);
                            wallChangePossible = true;
                            rowCounter--;
                            break;
                        }
                        deadEnd += 1;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        deadEnd += 1;
                    }
                }
                else if (i == 3)  //  Left/West
                {
                    if ((currentPosition % gridX) != 0)  //  If the current position is not the very left of any row.
                    {
                        if (booleanTiles[currentPosition - 1] != true)
                        {
                            booleanTiles[currentPosition - 1] = true;
                            wallsY[currentPosition + rowCounter] = true;
                            currentPosition -= 1;
                            stepsTaken++;
                            position.Add(currentPosition);
                            wallChangePossible = true;
                            break;
                        }
                        deadEnd += 1;
                    }
                    else
                    {
                        deadEnd += 1;
                    }
                }
            }
            if (deadEnd == 4)
            {
                if (wallChangePossible)
                {
                    int wallChange = rnd.Next(1, 101);
                    if (wallChange <= givenLoopChance)
                    {
                        ShuffleDirections(directions, rnd);

                        foreach (int i in directions)
                        {
                            if (i == 0)
                            {
                                if ((position[position.Count - 2] != currentPosition + gridX) && (position[position.Count - 4] != currentPosition + gridX) && (currentPosition + gridX) < (gridSize))
                                {
                                    wallsX[currentPosition + gridX] = true;
                                    break;
                                }
                            }
                            else if (i == 1)
                            {
                                if ((position[position.Count - 2] != currentPosition + 1) && (position[position.Count - 4] != currentPosition + 1) && ((currentPosition + 1) % gridX) != 0)
                                {
                                    wallsY[currentPosition + 1 + rowCounter] = true;
                                    break;
                                }
                            }
                            else if (i == 2)
                            {
                                if ((position[position.Count - 2] != currentPosition - gridX) && (position[position.Count - 4] != currentPosition - gridX) && currentPosition >= gridX)
                                {
                                    wallsX[currentPosition] = true;
                                    break;
                                }
                            }
                            else if (i == 3)
                            {
                                if ((position[position.Count - 2] != currentPosition - 1) && (position[position.Count - 4] != currentPosition - 1) && (currentPosition % gridX) != 0)
                                {
                                    wallsY[currentPosition + rowCounter] = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (position[position.Count - 2] > (position[position.Count - 1] + 1))  //  If the last position was north of the current position.
                {
                    rowCounter++;
                }
                else if (position[position.Count - 2] < (position[position.Count - 1] - 1))  //  Or south.
                {
                    rowCounter--;
                }
                position.RemoveAt(position.Count - 1);
                currentPosition = position[position.Count - 1];  //  Backtracking to the former position.
                wallChangePossible = false;
            }
        }
    }
}