using System;
using UnityEditor;


namespace JesterwaySoftware.CustomMazeGenerator
{
    public static class Calculate
    {
        public static void CalculateMazeFile(int givenMazeX, int givenMazeY, int loopChance)
        {
            string serializeNameString = String.Format("{0}x{1}_Loop_{2}%", givenMazeX.ToString(), givenMazeY.ToString(), loopChance.ToString());
            string chosenPathWriting = EditorUtility.SaveFilePanel("Save MazeFile", "", serializeNameString + ".xml", "xml");

            if (chosenPathWriting.Length > 0)
            {
                if (givenMazeX < 3 || givenMazeY < 3)
                {
                    UnityEngine.Debug.LogWarning("Both X and Y need to be at least 3 or higher!");
                }
                else
                {
                    MazeFile mazeFile = new MazeFile(givenMazeX, givenMazeY);  //  New object which will have its arrays altered by the algorithm.

                    MazeAlgorithms.RecursiveBacktracker(mazeFile.booleanWallsX, mazeFile.booleanWallsY, mazeFile.gridX, mazeFile.gridY, loopChance);

                    Serialization.SerializeFile(mazeFile, chosenPathWriting);

                    Array.Clear(mazeFile.booleanWallsX, 0, mazeFile.booleanWallsX.Length);
                    Array.Clear(mazeFile.booleanWallsY, 0, mazeFile.booleanWallsY.Length);
                }
            }
        }
    }
}