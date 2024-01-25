using System.IO;
using UnityEditor;
using UnityEngine;


namespace JesterwaySoftware.CustomMazeGenerator
{
    public class Display : MonoBehaviour
    {
        private static readonly float zFightingDisplacement = 0.0002f;
        private static readonly float decreaseWallLength = zFightingDisplacement * 4;
        private static readonly float innerWallScaleReduction = 0.3f;

        //  Opening entrances
        private static void Entrances(MazeFile givenDeserializedMazeFile)
        {
            if (CMG_GUI.entranceNorth_toggle.value)
            {
                givenDeserializedMazeFile.booleanWallsX[(givenDeserializedMazeFile.gridX / 2) + (givenDeserializedMazeFile.gridX * givenDeserializedMazeFile.gridY)] = true;
            }
            if (CMG_GUI.entranceEast_toggle.value)
            {
                givenDeserializedMazeFile.booleanWallsY[(givenDeserializedMazeFile.gridX * (givenDeserializedMazeFile.gridY / 2)) + (givenDeserializedMazeFile.gridY / 2) + givenDeserializedMazeFile.gridX] = true;
            }
            if (CMG_GUI.entranceSouth_toggle.value)
            {
                givenDeserializedMazeFile.booleanWallsX[givenDeserializedMazeFile.gridX / 2] = true;
            }
            if (CMG_GUI.entranceWest_toggle.value)
            {
                givenDeserializedMazeFile.booleanWallsY[(givenDeserializedMazeFile.gridX * (givenDeserializedMazeFile.gridY / 2)) + (givenDeserializedMazeFile.gridY / 2)] = true;
            }
        }

        
        //  To avoid texture flickering when one end of a wall intersects through a wall of the other orientation.
        private static void ZFightingScaleDecrease(GameObject originalWall)
        {
            originalWall.transform.localScale -= new Vector3(decreaseWallLength, 0f, 0f);
        }


        //  Used only when CMG_GUI.unify_toggle.value is false. An overload is defined below.
        //  The orientation number is used to define whether it is a lengthwise x oriented or lengthwise y oriented wall. 0 = x, 1 = y

        //  To remediate all texture flickering of walls in a straight line, like two x walls standing next to each other,
        //  or two y walls standing next to each other, every second wall in the line has its position slightly changed. x walls on the Z axis,
        //  and y walls on the X axis.

        //  Texture flickering from intersecting walls of another orientation is avoided by scaling them all lengthwise by a slight amount. This is happening elsewhere, in the ZFightingScaleDecrease function.

        //  To solve the texture flickering ontop, the difference between two walls of the same orientation is twice as big, and displaced from the walls of the other orientation.
        //  That means some x walls are heightened and their direct neighbors slightly lowered. For y walls, some are left as they are, and their direct neighbors
        //  lowered by the displacement value * 2.
        //  Thus, regardless of which wall intersects with another wall, there is always a height difference of at least the displacementValue. Which only needs to be barely enough to avoid flickering.
        //  At least 0.00015f.
        private static void ZFightingAdjustments(GameObject currentWall, int itemCounterVariable, int rowCounterVariable, byte orientation)
        {
            if (orientation == 0)
            {
                if ((itemCounterVariable % 2) == 0)
                {
                    currentWall.transform.localPosition += new Vector3(0f, -zFightingDisplacement, zFightingDisplacement);
                }
                else
                {
                    currentWall.transform.localPosition += new Vector3(0f, zFightingDisplacement, 0f);
                }
            }
            else if (orientation == 1)
            {
                if ((rowCounterVariable % 2) == 0)
                {
                    currentWall.transform.localPosition += new Vector3(zFightingDisplacement, -zFightingDisplacement * 2, 0f);
                }
            }
        }


        //  This overload gets called when the CMG_GUI.unify_toggle.value option is set to true. Since all adjacent walls are unified already,
        //  the only neccessary displacement to take care of is the height of the walls, for places where y and x walls intersect.
        //  Horizontal normal walls remain as they are, their gap walls are lowered by the usual value * 2 or * 3.
        //  Vertical normal walls are lowered by the normal value, their gap walls heightened by the normal value.
        //  Alternation is done because of flickering of two adjacent gapWalls.
        static bool alternate = true;
        private static void ZFightingAdjustments(GameObject currentWall, bool gap, int orientation)
        {
            if (orientation == 0)
            {
                if (gap)
                {
                    if (alternate)
                    {
                        currentWall.transform.localPosition += new Vector3(0, -zFightingDisplacement * 2, zFightingDisplacement);
                    }
                    else
                    {
                        currentWall.transform.localPosition += new Vector3(0, -zFightingDisplacement * 3, -zFightingDisplacement);
                    }
                }
            }
            else if (orientation == 1)
            {
                if (gap)
                {
                    if (alternate)
                    {
                        currentWall.transform.localPosition += new Vector3(zFightingDisplacement, zFightingDisplacement, 0);
                    }
                    else
                    {
                        currentWall.transform.localPosition += new Vector3(-zFightingDisplacement, zFightingDisplacement * 2, 0);
                    }
                }
                else
                {
                    currentWall.transform.localPosition += new Vector3(0, -zFightingDisplacement, 0);
                }
            }
            alternate = !alternate;
        }


        //  Inner walls are useful when you use a partially transparent outer wall material to simulate a hedge.
        private static void InstantiateInnerWall(float scaleReduction, GameObject parentWall, GameObject innerWallsFolder, bool gap)
        {
            GameObject innerWall = Instantiate(parentWall, parentWall.transform, true);
            innerWall.name = parentWall.name + " Inner";
            innerWall.transform.parent = innerWallsFolder.transform;
            innerWall.transform.localScale -= new Vector3(scaleReduction, scaleReduction, scaleReduction);
            if (gap)
            {
                innerWall.SetActive(false);
            }
        }


        //  Orientation 0 = x, 1 = y
        private static void PlaceSingularWalls(byte givenOrientation, bool[] booleanWallsCarved, GameObject givenWall, GameObject givenFolder, GameObject givenInnerFolder, int givenGridX)
        {
            int itemCounter = 0;
            int rowCounter = 0;
            string orientationLetter = "";

            if (givenOrientation == 0)
            {
                orientationLetter = "X";
            }
            else if (givenOrientation == 1)
            { 
                orientationLetter = "Y";
            }

            foreach (bool item in booleanWallsCarved)  //  Creating all of the walls as copies of one original wall
            {
                if (item && !CMG_GUI.inactiveWalls_toggle.value)  //  If there is a gap here and we skip gap walls
                {
                    //
                }
                else
                {
                    string nameString = string.Format("{0}{1}c{2}", orientationLetter, rowCounter, itemCounter);
                    GameObject wall = Instantiate(givenWall, givenFolder.transform, false);
                    wall.name = nameString;

                    wall.transform.localPosition += new Vector3(itemCounter * CMG_GUI.tileSize_floatField.value, 0, rowCounter * CMG_GUI.tileSize_floatField.value);
                    
                    if (CMG_GUI.zFighting_toggle.value)
                    {
                        ZFightingAdjustments(wall, itemCounter, rowCounter, givenOrientation);
                    }
                                        
                    if (CMG_GUI.innerWalls_toggle.value)
                    {
                        InstantiateInnerWall(innerWallScaleReduction, wall, givenInnerFolder, item);
                    }

                    if (item)
                    {
                        wall.SetActive(false);
                    }
                }

                itemCounter++;

                if (itemCounter == givenGridX)
                {
                    rowCounter += 1;
                    itemCounter = 0;
                }
            }
            DestroyImmediate(givenWall);  //  Destroying the original x or y wall
        }


        private static void PlaceUnifiedWalls(string orientation, bool[] booleanWallsCarved, GameObject givenWall, GameObject givenFolder, GameObject givenInnerFolder, int givenGridX, int givenGridY)
        {
            int columnCounter = 0;
            int rowCounter = 0;
            int startCounter = 0;
            bool wallSeen = false;  //  To avoid writing walls when there are gaps at the start of a row, or multiple gaps next to each other.

            if (orientation == "x")
            {
                foreach (bool item in booleanWallsCarved)
                {
                    columnCounter++;

                    if (item == false)  //  If there is a wall supposed to stand here.
                    {
                        if (columnCounter == givenGridX)  //  If it's standing at the rightmost position of that row.
                        {
                            string nameString;

                            if (startCounter == (givenGridX - 1))
                            {
                                nameString = string.Format("X{0}c{1}", rowCounter, startCounter);
                            }
                            else
                            {
                                nameString = string.Format("X{0}c{1}-{2}", rowCounter, startCounter, givenGridX - 1);
                            }

                            GameObject wall = Instantiate(givenWall, givenFolder.transform, false);
                            wall.name = nameString;
                            wall.transform.localPosition += new Vector3(((givenGridX - 1 - startCounter) * CMG_GUI.tileSize_floatField.value / 2) + startCounter * CMG_GUI.tileSize_floatField.value, 0, rowCounter * CMG_GUI.tileSize_floatField.value);

                            if (CMG_GUI.zFighting_toggle.value)
                            {
                                ZFightingAdjustments(wall, false, 0);
                            }

                            wall.transform.localScale += new Vector3((givenGridX - 1 - startCounter) * CMG_GUI.tileSize_floatField.value, 0, 0);

                            if (CMG_GUI.scaleUVs_toggle.value)
                            {
                                ChangeUV.ScaleUVs(wall, wall.transform.localScale.x, wall.transform.localScale.y, wall.transform.localScale.z);
                            }

                            if (CMG_GUI.innerWalls_toggle.value)
                            {
                                InstantiateInnerWall(innerWallScaleReduction, wall, givenInnerFolder, false);
                            }

                            wallSeen = false;
                        }
                        else
                        {
                            wallSeen = true;
                        }
                    }
                    else  //  If there is a gap at this position.
                    {
                        if (wallSeen)
                        {
                            string nameString;

                            if (startCounter == (columnCounter - 2))
                            {
                                nameString = string.Format("X{0}c{1}", rowCounter, startCounter);
                            }
                            else
                            {
                                nameString = string.Format("X{0}c{1}-{2}", rowCounter, startCounter, columnCounter - 2);
                            }

                            GameObject wall = Instantiate(givenWall, givenFolder.transform, false);
                            wall.name = nameString;
                            wall.transform.localPosition += new Vector3(((columnCounter - 2 - startCounter) * CMG_GUI.tileSize_floatField.value / 2) + startCounter * CMG_GUI.tileSize_floatField.value, 0, rowCounter * CMG_GUI.tileSize_floatField.value);

                            if (CMG_GUI.zFighting_toggle.value)
                            {
                                ZFightingAdjustments(wall, false, 0);
                            }

                            wall.transform.localScale += new Vector3((columnCounter - 2 - startCounter) * CMG_GUI.tileSize_floatField.value, 0, 0);

                            if (CMG_GUI.scaleUVs_toggle.value)
                            {
                                ChangeUV.ScaleUVs(wall, wall.transform.localScale.x, wall.transform.localScale.y, wall.transform.localScale.z);
                            }

                            if (CMG_GUI.innerWalls_toggle.value)
                            {
                                InstantiateInnerWall(innerWallScaleReduction, wall, givenInnerFolder, false);
                            }

                            wallSeen = false;
                        }

                        if (CMG_GUI.inactiveWalls_toggle.value)
                        {
                            string nameStringGap = string.Format("X{0}c{1}", rowCounter, columnCounter - 1);
                            GameObject wallGap = Instantiate(givenWall, givenFolder.transform, false);
                            wallGap.name = nameStringGap;
                            wallGap.transform.localPosition += new Vector3((columnCounter - 1) * CMG_GUI.tileSize_floatField.value, 0, rowCounter * CMG_GUI.tileSize_floatField.value);

                            if (CMG_GUI.zFighting_toggle.value)
                            {
                                ZFightingAdjustments(wallGap, true, 0);
                            }

                            if (CMG_GUI.scaleUVs_toggle.value)
                            {
                                ChangeUV.ScaleUVs(wallGap, wallGap.transform.localScale.x, wallGap.transform.localScale.y, wallGap.transform.localScale.z);
                            }

                            if (CMG_GUI.innerWalls_toggle.value)
                            {
                                InstantiateInnerWall(innerWallScaleReduction, wallGap, givenInnerFolder, true);
                            }

                            wallGap.SetActive(false);
                        }
                    }

                    if (columnCounter == givenGridX)  //  If the gap is at the very end of this row.
                    {
                        rowCounter ++;
                        columnCounter = 0;
                        startCounter = 0;
                    }
                    else
                    {
                        if (!wallSeen)
                        {
                            startCounter = columnCounter;
                        }
                    }
                }
            }
            else if (orientation == "y")
            {
                for (int i = 0; i < booleanWallsCarved.Length; i += givenGridX + 1)
                {
                    rowCounter++;

                    if (booleanWallsCarved[i] == false)  //  If there is a wall supposed to stand here.
                    {
                        if (rowCounter == givenGridY)  //  If it's standing at the topmost position of that column.
                        {
                            string nameString;
                            
                            if (startCounter == i)  //  If the wall is only 1 wall segment long
                            {
                                nameString = string.Format("Y{0}c{1}", rowCounter - 1, columnCounter);
                            }
                            else
                            {
                                nameString = string.Format("Y{0}-{1}c{2}", startCounter / (givenGridX + 1), givenGridY - 1, columnCounter);
                            }
                            
                            GameObject wall = Instantiate(givenWall, givenFolder.transform, false);
                            wall.name = nameString;
                            wall.transform.localPosition += new Vector3(columnCounter * CMG_GUI.tileSize_floatField.value, 0, ((givenGridY - 1 - (startCounter / (givenGridX + 1))) * CMG_GUI.tileSize_floatField.value / 2) + (startCounter / (givenGridX + 1) * CMG_GUI.tileSize_floatField.value));
                            
                            if (CMG_GUI.zFighting_toggle.value)
                            {
                                ZFightingAdjustments(wall, false, 1);
                            }

                            wall.transform.localScale += new Vector3((givenGridY - 1 - (startCounter / (givenGridX + 1))) * CMG_GUI.tileSize_floatField.value, 0, 0);

                            if (CMG_GUI.scaleUVs_toggle.value)
                            {
                                ChangeUV.ScaleUVs(wall, wall.transform.localScale.x, wall.transform.localScale.y, wall.transform.localScale.z);
                            }

                            if (CMG_GUI.innerWalls_toggle.value)
                            {
                                InstantiateInnerWall(innerWallScaleReduction, wall, givenInnerFolder, false);
                            }

                            wallSeen = false;
                        }
                        else
                        {
                            wallSeen = true;
                        }
                    }
                    else  //  If there is a gap at this position, make a wall from StartCounter up to just before the gap
                    {
                        if (wallSeen)
                        {
                            string nameString;

                            if (startCounter == i - (givenGridX + 1))
                            {
                                nameString = string.Format("Y{0}c{1}", rowCounter - 2, columnCounter);
                            }
                            else
                            {
                                nameString = string.Format("Y{0}-{1}c{2}", startCounter / (givenGridX + 1), rowCounter - 2, columnCounter);
                            }

                            GameObject wall = Instantiate(givenWall, givenFolder.transform, false);
                            wall.name = nameString;
                            wall.transform.localPosition += new Vector3(columnCounter * CMG_GUI.tileSize_floatField.value, 0, ((rowCounter - 2 - (startCounter / (givenGridX + 1))) * CMG_GUI.tileSize_floatField.value / 2) + (startCounter / (givenGridX + 1) * CMG_GUI.tileSize_floatField.value));

                            if (CMG_GUI.zFighting_toggle.value)
                            {
                                ZFightingAdjustments(wall, false, 1);
                            }

                            wall.transform.localScale += new Vector3((rowCounter - 2 - (startCounter / (givenGridX + 1))) * CMG_GUI.tileSize_floatField.value, 0, 0);

                            if (CMG_GUI.scaleUVs_toggle.value)
                            {
                                ChangeUV.ScaleUVs(wall, wall.transform.localScale.x, wall.transform.localScale.y, wall.transform.localScale.z);
                            }

                            if (CMG_GUI.innerWalls_toggle.value)
                            {
                                InstantiateInnerWall(innerWallScaleReduction, wall, givenInnerFolder, false);
                            }

                            wallSeen = false;
                        }

                        if (CMG_GUI.inactiveWalls_toggle.value)
                        {
                            string nameStringGap = string.Format("Y{0}c{1}", rowCounter - 1, columnCounter);
                            GameObject wallGap = Instantiate(givenWall, givenFolder.transform, false);
                            wallGap.name = nameStringGap;
                            wallGap.transform.localPosition += new Vector3(columnCounter * CMG_GUI.tileSize_floatField.value, 0, (rowCounter - 1) * CMG_GUI.tileSize_floatField.value);

                            if (CMG_GUI.zFighting_toggle.value)
                            {
                                ZFightingAdjustments(wallGap, true, 1);
                            }

                            if (CMG_GUI.scaleUVs_toggle.value)
                            {
                                ChangeUV.ScaleUVs(wallGap, wallGap.transform.localScale.x, wallGap.transform.localScale.y, wallGap.transform.localScale.z);
                            }

                            if (CMG_GUI.innerWalls_toggle.value)
                            {
                                InstantiateInnerWall(innerWallScaleReduction, wallGap, givenInnerFolder, true);
                            }

                            wallGap.SetActive(false);
                        }
                    }

                    if (rowCounter == givenGridY)  //  If this is at the very top of this column.
                    {
                        if (columnCounter == givenGridX)  //  If this is the very last position, upper right corner, of the y wall grid, end the iteration, else go to next column 
                        {
                            break;
                        }
                        else
                        {
                            columnCounter++;
                            rowCounter = 0;
                            startCounter = columnCounter;
                            i = -(givenGridX + 1);
                            i += columnCounter;
                        }
                    }
                    else
                    {
                        if (!wallSeen)
                        {
                            startCounter = i + givenGridX + 1;
                        }
                    } 
                }
            }
            DestroyImmediate(givenWall);
        }


        public static void DisplayMazeFile()
        {
            string chosenPathReading = EditorUtility.OpenFilePanel("Select a MazeFile", "", "xml");

            if (chosenPathReading.Length > 0)
            {   
                MazeFile deserializedMazeFile = Serialization.DeSerializeFile(chosenPathReading);

                Entrances(deserializedMazeFile);
                
                float wallLength = CMG_GUI.tileSize_floatField.value + CMG_GUI.wallThickness_floatField.value;
                float fieldWidth = (deserializedMazeFile.gridX * CMG_GUI.tileSize_floatField.value) + CMG_GUI.wallThickness_floatField.value;
                float fieldLength = (deserializedMazeFile.gridY * CMG_GUI.tileSize_floatField.value) + CMG_GUI.wallThickness_floatField.value;
                float[] fieldSize = new float[3];
                fieldSize[0] = fieldWidth;
                fieldSize[1] = CMG_GUI.wallHeight_floatField.value / 30f;
                fieldSize[2] = fieldLength;
                Vector3 fieldSizeVector3 = new Vector3(fieldSize[0], fieldSize[1], fieldSize[2]);

                GameObject mazeParent = new GameObject(Path.GetFileNameWithoutExtension(chosenPathReading));
                mazeParent.transform.position = CMG_GUI.mazePosition_vector3.value;
                mazeParent.transform.Rotate(CMG_GUI.mazeRotation_vector3.value);

                GameObject mazeFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mazeFloor.transform.localScale = fieldSizeVector3;
                mazeFloor.transform.parent = mazeParent.transform;
                mazeFloor.name = "mazeFloor";
                mazeFloor.transform.SetPositionAndRotation(mazeParent.transform.position, mazeParent.transform.rotation);
                mazeFloor.isStatic = true;

                if (CMG_GUI.rotateUVs_toggle.value)
                {
                    ChangeUV.RotateUVs(mazeFloor);
                }

                //  The floor will have its UV scale adjusted aswell.
                if (CMG_GUI.scaleUVs_toggle.value)
                {
                    ChangeUV.ScaleUVs(mazeFloor, mazeFloor.transform.localScale.x, mazeFloor.transform.localScale.y, mazeFloor.transform.localScale.z);
                }

                if (CMG_GUI.floorMaterial_objectField.value != null)
                {
                    var floorRenderer = mazeFloor.GetComponent<Renderer>();
                    floorRenderer.material = (Material)CMG_GUI.floorMaterial_objectField.value;
                }

                GameObject xWallsFolder = new GameObject("X Walls");
                xWallsFolder.transform.SetParent(mazeParent.transform, false);
                GameObject yWallsFolder = new GameObject("Y Walls");
                yWallsFolder.transform.SetParent(mazeParent.transform, false);

                GameObject innerXWallsFolder = new GameObject("inner X Walls");
                innerXWallsFolder.transform.SetParent(xWallsFolder.transform, false);
                GameObject innerYWallsFolder = new GameObject("inner Y Walls");
                innerYWallsFolder.transform.SetParent(yWallsFolder.transform, false);

                GameObject xWall = GameObject.CreatePrimitive(PrimitiveType.Cube);  //  creating one lengthwise x oriented wall

                //  Placing one original x wall at the 0:0 location
                xWall.transform.SetParent(mazeParent.transform, false);
                xWall.transform.localScale = new Vector3(wallLength, CMG_GUI.wallHeight_floatField.value, CMG_GUI.wallThickness_floatField.value);
                xWall.transform.localPosition = new Vector3((wallLength / 2) - (mazeFloor.transform.localScale.x / 2), (xWall.transform.localScale.y / 2f) + (mazeFloor.transform.localScale.y / 2), (CMG_GUI.wallThickness_floatField.value / 2) - (mazeFloor.transform.localScale.z / 2));
                xWall.isStatic = true;

                //  Placing one original y wall at the 0:0 location
                GameObject yWall = Instantiate(xWall);
                yWall.transform.Rotate(0f, -90f, 0f);
                yWall.transform.localPosition = new Vector3((CMG_GUI.wallThickness_floatField.value / 2) - (mazeFloor.transform.localScale.x / 2), (yWall.transform.localScale.y / 2f) + (mazeFloor.transform.localScale.y / 2), (wallLength / 2) - (mazeFloor.transform.localScale.z / 2));

                if (CMG_GUI.zFighting_toggle.value)
                {
                    ZFightingScaleDecrease(xWall);
                    ZFightingScaleDecrease(yWall);
                }

                if (CMG_GUI.rotateUVs_toggle.value)
                {
                    ChangeUV.RotateUVs(xWall);
                    ChangeUV.RotateUVs(yWall);
                }

                if (CMG_GUI.wallMaterial_objectField.value != null)
                {
                    var wallRendererX = xWall.GetComponent<Renderer>();
                    wallRendererX.material = (Material)CMG_GUI.wallMaterial_objectField.value;

                    var wallRendererY = yWall.GetComponent<Renderer>();
                    wallRendererY.material = (Material)CMG_GUI.wallMaterial_objectField.value;
                }

                if (!CMG_GUI.unify_toggle.value) //  Here the regular method of creating all of the walls
                {
                    if (CMG_GUI.scaleUVs_toggle.value)  //  When CMG_GUI.unify_toggle.value is unchecked, the ScaleUVs function can be called here, once, instead of calling it for every single wall during positioning
                    {
                        ChangeUV.ScaleUVs(xWall, xWall.transform.localScale.x, xWall.transform.localScale.y, xWall.transform.localScale.z);
                        ChangeUV.ScaleUVs(yWall, yWall.transform.localScale.x, yWall.transform.localScale.y, yWall.transform.localScale.z);
                    }

                    PlaceSingularWalls(0, deserializedMazeFile.booleanWallsX, xWall, xWallsFolder, innerXWallsFolder, deserializedMazeFile.gridX);
                    PlaceSingularWalls(1, deserializedMazeFile.booleanWallsY, yWall, yWallsFolder, innerYWallsFolder, deserializedMazeFile.gridX + 1);
                }
                else if (CMG_GUI.unify_toggle.value)
                {
                    PlaceUnifiedWalls("x", deserializedMazeFile.booleanWallsX, xWall, xWallsFolder, innerXWallsFolder, deserializedMazeFile.gridX, deserializedMazeFile.gridY);
                    PlaceUnifiedWalls("y", deserializedMazeFile.booleanWallsY, yWall, yWallsFolder, innerYWallsFolder, deserializedMazeFile.gridX, deserializedMazeFile.gridY);
                }

                if (!CMG_GUI.innerWalls_toggle.value)
                {
                    DestroyImmediate(innerXWallsFolder);
                    DestroyImmediate(innerYWallsFolder);
                }
            }
        }
    }
}