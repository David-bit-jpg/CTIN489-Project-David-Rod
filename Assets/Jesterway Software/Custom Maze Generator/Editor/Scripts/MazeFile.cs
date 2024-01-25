//  An object of this class will have its boolean arrays altered by the Recursive Backtracker function. It will also hold the dimensions of the maze.
//  "False" values will be interpreted in the display function as "Standing wall", while "True" values will represent deleted walls, i.e. gaps.
//  The object will be serialized into an xml file, "Mazefile", once the Recursive Backtracker is finished. The Display function will later deserialize the object from the xml file.


namespace JesterwaySoftware.CustomMazeGenerator
{
    public class MazeFile
    {
        public int gridX;
        public int gridY;
        public bool[] booleanWallsX;
        public bool[] booleanWallsY;


        public MazeFile()  //  The xml deserializer requires an empty constructor.
        {

        }


        public MazeFile(int givenX, int givenY) : this()
        {
            gridX = givenX;
            gridY = givenY;
            booleanWallsX = new bool[gridX * (gridY + 1)];
            booleanWallsY = new bool[(gridX + 1) * gridY];
        }
    }
}