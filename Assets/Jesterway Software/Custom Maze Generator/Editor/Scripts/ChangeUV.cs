//  This class adjusts UV values of a primitive Unity Cube, to rotate and scale.

using UnityEngine;


namespace JesterwaySoftware.CustomMazeGenerator
{
    public static class ChangeUV
    {
        //  Will rotate certain faces of the cube, to remediate the upside-down texturing of some faces which occurs with the default UV mapping of a cube.
        public static void RotateUVs(GameObject givenCube)
        {
            Mesh mesh = Mesh.Instantiate(givenCube.GetComponent<MeshFilter>().sharedMesh) as Mesh;  //  Creating a new sharedMesh

            Vector2[] uvs = mesh.uv;

            //  changing the primitive cubes default UV rotations here
            //  top rotation
            uvs[4] = new Vector2(1.00f, 0.00f);
            uvs[5] = new Vector2(0.00f, 0.00f);
            uvs[8] = new Vector2(1.00f, 1.00f);
            uvs[9] = new Vector2(0.00f, 1.00f);

            //  large front side rotation
            uvs[6] = new Vector2(1.00f, 0.00f);
            uvs[7] = new Vector2(0.00f, 0.00f);
            uvs[10] = new Vector2(1.00f, 1.00f);
            uvs[11] = new Vector2(0.00f, 1.00f);

            //  bottom rotation
            uvs[12] = new Vector2(1.00f, 1.00f);
            uvs[13] = new Vector2(1.00f, 0.00f);
            uvs[14] = new Vector2(0.00f, 0.00f);
            uvs[15] = new Vector2(0.00f, 1.00f);

            mesh.uv = uvs;
            mesh.name = "Cube_UVrotated";
            givenCube.GetComponent<MeshFilter>().sharedMesh = mesh;  //  Giving the new and edited sharedMesh to the GameObject
        }


        //  This function will scale the UVs of any face to match its geometry accordingly. This allows texture tiling to be uniform even on differently sized faces.
        //  individual wall length is "wall.transform.localScale.x" which varies from one wall to the other, when using the "unifyAdjacentWalls" option.
        public static void ScaleUVs(GameObject givenCube, float individualWallLength, float individualWallHeight, float individualWallThickness)
        {
            Mesh mesh = Mesh.Instantiate(givenCube.GetComponent<MeshFilter>().sharedMesh) as Mesh;  //  Creating a new sharedMesh

            Vector2[] uvs = mesh.uv;

            float uvTileSize = 1.00f;  //  The dimension of a UV tile. With Unitys primitive cube it is 1.00f
            float newScale;
            float adjustmentValue;

            //  The following 3 faces are already by default rotated as they should be.
            //  0, 1, 2, 3
            //  16, 17, 18, 19
            //  20, 21, 22, 23

            //  Therefore, the UV scaling done to them is obviously independent from the "rotateUVs" option.
            //  The faces that actually need custom scaling are handled by an if/else.


            //  large back side face, adjusting for wallLength
            newScale = uvTileSize * individualWallLength; //  This gives either a value above or below uvTileSize
            adjustmentValue = newScale - uvTileSize;  //  float by which the distance between two opposing UVs should be scaled. Either positive or negative number.

            uvs[1] = new Vector2(uvs[1].x + adjustmentValue, uvs[1].y);
            uvs[3] = new Vector2(uvs[3].x + adjustmentValue, uvs[3].y);


            //  large back side face, adjusting for wallHeight
            newScale = uvTileSize * individualWallHeight;
            adjustmentValue = newScale - uvTileSize;

            uvs[2] = new Vector2(uvs[2].x, uvs[2].y + adjustmentValue);
            uvs[3] = new Vector2(uvs[3].x, uvs[3].y + adjustmentValue);


            //  thin left side face, adjusting for wallHeight
            newScale = uvTileSize * individualWallHeight;
            adjustmentValue = newScale - uvTileSize;

            uvs[17] = new Vector2(uvs[17].x, uvs[17].y + adjustmentValue);
            uvs[18] = new Vector2(uvs[18].x, uvs[18].y + adjustmentValue);


            //  thin left side face, adjusting for wallThickness
            newScale = uvTileSize * individualWallThickness;
            adjustmentValue = newScale - uvTileSize;

            uvs[18] = new Vector2(uvs[18].x + adjustmentValue, uvs[18].y);
            uvs[19] = new Vector2(uvs[19].x + adjustmentValue, uvs[19].y);


            //  thin right side face, adjusting for wallThickness
            newScale = uvTileSize * individualWallThickness;
            adjustmentValue = newScale - uvTileSize;

            uvs[22] = new Vector2(uvs[22].x + adjustmentValue, uvs[22].y);
            uvs[23] = new Vector2(uvs[23].x + adjustmentValue, uvs[23].y);


            //  thin right side face, adjusting for wallHeight
            newScale = uvTileSize * individualWallHeight;
            adjustmentValue = newScale - uvTileSize;

            uvs[21] = new Vector2(uvs[21].x, uvs[21].y + adjustmentValue);
            uvs[22] = new Vector2(uvs[22].x, uvs[22].y + adjustmentValue);



            //  From hereon follow faces that are actually different depending on whether the rotateUVs option is checked, and therefore need different UV scaling.
            //  This is the option if the user wants to work with the default rotation of a primitive cubes UV faces, but nonetheless adjust the UV faces scalings. It's like saying: !rotateUVs && scaleUVs
            if (!CMG_GUI.rotateUVs_toggle.value)  
            {
                //  top face, adjusting for wallThickness
                newScale = uvTileSize * individualWallThickness;
                adjustmentValue = newScale - uvTileSize;

                uvs[4] = new Vector2(uvs[4].x, uvs[4].y + adjustmentValue);
                uvs[5] = new Vector2(uvs[5].x, uvs[5].y + adjustmentValue);


                //  top face, adjusting for wallLength
                newScale = uvTileSize * individualWallLength;
                adjustmentValue = newScale - uvTileSize;

                uvs[5] = new Vector2(uvs[5].x + adjustmentValue, uvs[5].y);
                uvs[9] = new Vector2(uvs[9].x + adjustmentValue, uvs[9].y);


                //  large front side face, adjusting for wallHeight
                newScale = uvTileSize * individualWallHeight;
                adjustmentValue = newScale - uvTileSize;

                uvs[6] = new Vector2(uvs[6].x, uvs[6].y + adjustmentValue);
                uvs[7] = new Vector2(uvs[7].x, uvs[7].y + adjustmentValue);


                //  large front side face, adjusting for wallLength
                newScale = uvTileSize * individualWallLength;
                adjustmentValue = newScale - uvTileSize;

                uvs[7] = new Vector2(uvs[7].x + adjustmentValue, uvs[7].y);
                uvs[11] = new Vector2(uvs[11].x + adjustmentValue, uvs[11].y);


                //  bottom face, adjusting for wallLength
                newScale = uvTileSize * individualWallLength;
                adjustmentValue = newScale - uvTileSize;

                uvs[14] = new Vector2(uvs[14].x + adjustmentValue, uvs[14].y);
                uvs[15] = new Vector2(uvs[15].x + adjustmentValue, uvs[15].y);


                //  bottom face, adjusting for wallThickness
                newScale = uvTileSize * individualWallThickness;
                adjustmentValue = newScale - uvTileSize;

                uvs[13] = new Vector2(uvs[13].x, uvs[13].y + adjustmentValue);
                uvs[14] = new Vector2(uvs[14].x, uvs[14].y + adjustmentValue);


                mesh.name = "Cube_UVscaled";
            }
            else if (CMG_GUI.rotateUVs_toggle.value)
            {
                //  top face, adjusting for wallThickness
                newScale = uvTileSize * individualWallThickness;
                adjustmentValue = newScale - uvTileSize;

                uvs[8] = new Vector2(uvs[8].x, uvs[8].y + adjustmentValue);
                uvs[9] = new Vector2(uvs[9].x, uvs[9].y + adjustmentValue);


                //  top face, adjusting for wallLength
                newScale = uvTileSize * individualWallLength;
                adjustmentValue = newScale - uvTileSize;

                uvs[4] = new Vector2(uvs[4].x + adjustmentValue, uvs[4].y);
                uvs[8] = new Vector2(uvs[8].x + adjustmentValue, uvs[8].y);


                //  large front side face, adjusting for wallLength
                newScale = uvTileSize * individualWallLength;
                adjustmentValue = newScale - uvTileSize;

                uvs[6] = new Vector2(uvs[6].x + adjustmentValue, uvs[6].y);
                uvs[10] = new Vector2(uvs[10].x + adjustmentValue, uvs[10].y);


                //  large front side face, adjusting for wallHeight
                newScale = uvTileSize * individualWallHeight;
                adjustmentValue = newScale - uvTileSize;

                uvs[10] = new Vector2(uvs[10].x, uvs[10].y + adjustmentValue);
                uvs[11] = new Vector2(uvs[11].x, uvs[11].y + adjustmentValue);


                //  bottom face, adjusting for wallThickness
                newScale = uvTileSize * individualWallThickness;
                adjustmentValue = newScale - uvTileSize;

                uvs[12] = new Vector2(uvs[12].x, uvs[12].y + adjustmentValue);
                uvs[15] = new Vector2(uvs[15].x, uvs[15].y + adjustmentValue);


                //  bottom face, adjusting for wallLength
                newScale = uvTileSize * individualWallLength;
                adjustmentValue = newScale - uvTileSize;

                uvs[12] = new Vector2(uvs[12].x + adjustmentValue, uvs[12].y);
                uvs[13] = new Vector2(uvs[13].x + adjustmentValue, uvs[13].y);


                mesh.name = "Cube_UVrotatedscaled";
            }
            mesh.uv = uvs;
            givenCube.GetComponent<MeshFilter>().sharedMesh = mesh;  //  Giving the new and edited sharedMesh to the GameObject
        }
    }
}