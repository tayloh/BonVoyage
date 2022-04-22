using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    private Dictionary<Vector3Int, Hex> hexTileDict = new Dictionary<Vector3Int, Hex>();
    private Dictionary<Vector3Int, List<Vector3Int>> hexTileNeighboursDict = new Dictionary<Vector3Int, List<Vector3Int>>();
    public GameObject hexParent;

    private static float xOffset = 2;
    private static float yOffset = 1;
    private static float zOffset = 1.73f;

    private int side = -1;

    private Vector3 originGrid = new Vector3(0, 0, 0);

    public int gridSideSize = 10;
    [SerializeField]
    private GameObject tile;
    [SerializeField]
    private Camera mainCamera;

    private void Awake()
    {
        xOffset = HexCoordinates.xOffset;
        yOffset = HexCoordinates.yOffset;
        zOffset = HexCoordinates.zOffset;

        GenerateGrid();
    }

    private void Start()
    {
        var allHex = hexParent.GetComponentsInChildren<Hex>();
        foreach (Hex hexElement in allHex)
        {
            hexTileDict.Add(hexElement.HexCoords, hexElement);
        }
    }

    //TESTING
    public int horShift = 1;
    public int verShift = 1;
    private void Update()
    {
        
            AdaptToPlayersView(Camera.main.transform.position);
        

        /*if (Input.GetKeyDown(KeyCode.B))
        {
            List<Vector3Int> example;
            List<Vector3Int> example_otherside;
            example = GetAttackableTilesFor(new Vector3Int(4, 0, 2), 0, 2);
            example_otherside = GetAttackableTilesFor(new Vector3Int(4, 0, 2), 1, 2);
            example.AddRange(example_otherside);
            foreach (var tile in example)   
            {
                Hex hex = GetTileAt(tile);
                if (hex != null)
                {
                    GetTileAt(tile).EnableHighLight();
                } 
            }
        }*/
    }

    private void AdaptToPlayersView(Vector3 cameraPos)
    {
        Vector3Int originOffset = HexCoordinates.ConvertPositionToOffset(cameraPos);
        Vector3Int originShift = originOffset - HexCoordinates.ConvertPositionToOffset(originGrid);
        HorizontalShifting(originShift.x);
        VerticalShifting(originShift.z);
    }

    private void HorizontalShifting(int direction) //direction>0 is right
    {
        while (direction != 0)
        {
            Vector3 destroyHexWorldCoord;
            Vector3 createHexWorldCoord;
            if (direction > 0)
            {
                //Deleting farthest columns
                //delete the extremity 
                destroyHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - 1), 0, originGrid.z);
                DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                for (int row = 1; row < gridSideSize; row++)
                {
                    //above central line
                    destroyHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - 1 - (row / 2 + 1 * PositiveModulo(row, 2)/2f)), 0, originGrid.z + row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                    //under central line
                    destroyHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - 1 - (row / 2 + 1 * PositiveModulo(row, 2)/2f)), 0, originGrid.z - row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }

                //Creating the new columns
                createHexWorldCoord = new Vector3(originGrid.x + xOffset * gridSideSize, 0, originGrid.z);
                CreateTileAt(createHexWorldCoord);
                for (int row = 1; row < gridSideSize; row++)
                {
                    //above central line
                    createHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z + row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                    //under central line
                    createHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z - row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }

                originGrid.x = originGrid.x + xOffset;
                direction -= 1;
            }
            else
            {
                //Deleting farthest columns
                //delete the extremity 
                destroyHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - 1), 0, originGrid.z);
                DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                for (int row = 1; row < gridSideSize; row++)
                {
                    //above central line
                    destroyHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - 1 - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z + row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                    //under central line
                    destroyHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - 1 - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z - row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }

                //Creating the new columns
                createHexWorldCoord = new Vector3(originGrid.x - xOffset * gridSideSize, 0, originGrid.z);
                CreateTileAt(createHexWorldCoord);
                for (int row = 1; row < gridSideSize; row++)
                {
                    //above central line
                    createHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z + row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                    //under central line
                    createHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z - row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }

                originGrid.x = originGrid.x - xOffset;
                direction += 1;
            }
        }
    }

    private void VerticalShifting(int direction) //direction>0 is up
    {
        while (direction != 0)
        {
            Vector3 destroyHexWorldCoord;
            Vector3 createHexWorldCoord;
            if (direction > 0) //moving up
            {
                //deleting bottom line
                for (int i = 0; i < gridSideSize; i++)
                {
                    destroyHexWorldCoord = new Vector3(originGrid.x - (gridSideSize / 2f - 0.5f - i) * xOffset, 0, originGrid.z - (gridSideSize - 1) * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }

                //creating top line
                for (int i = 0; i < gridSideSize - 1; i++)
                {
                    createHexWorldCoord = new Vector3(originGrid.x - (gridSideSize / 2f - 1 - i) * xOffset, 0, originGrid.z + (gridSideSize) * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }
                //determining which side to move : left, then right...
                side = side == 1 ? -1 : 1;

                //deleting bottom side line 
                for (int row = 0; row < gridSideSize - 1; row++)
                {
                    //under central line
                    destroyHexWorldCoord = new Vector3(originGrid.x + side * xOffset * (gridSideSize - 1 - (row / 2)), 0, originGrid.z - row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }
                //creating top side line 
                for (int row = 1; row < gridSideSize + 1; row++)
                {
                    //above central line
                    createHexWorldCoord = new Vector3(originGrid.x - side * xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z + row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }
                direction -= 1;
                originGrid.z += zOffset;
                originGrid.x -= side * xOffset / 2;
            }
            else //moving down
            {
                //deleting top line
                for (int i = 0; i < gridSideSize; i++)
                {
                    destroyHexWorldCoord = new Vector3(originGrid.x - (gridSideSize / 2f - 0.5f - i) * xOffset, 0, originGrid.z + (gridSideSize - 1) * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }

                //creating bottom line
                for (int i = 0; i < gridSideSize - 1; i++)
                {
                    createHexWorldCoord = new Vector3(originGrid.x - (gridSideSize / 2f - 1 - i) * xOffset, 0, originGrid.z - (gridSideSize) * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }
                //determining which side to move : left, then right...
                side = side == 1 ? -1 : 1;

                //deleting top side line 
                for (int row = 0; row < gridSideSize - 1; row++)
                {
                    //under central line
                    destroyHexWorldCoord = new Vector3(originGrid.x + side * xOffset * (gridSideSize - 1 - (row / 2)), 0, originGrid.z + row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }
                //creating bottom side line 
                for (int row = 1; row < gridSideSize + 1; row++)
                {
                    //above central line
                    createHexWorldCoord = new Vector3(originGrid.x - side * xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z - row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }
                direction += 1;
                //TODO update origin x and z
                originGrid.z -= zOffset;
                originGrid.x -= side * xOffset / 2;
            }


        }
    }

    private void DestroyTileAt(Vector3Int hexCoord)
    {
        Destroy(GetTileAt(hexCoord).gameObject);
        hexTileDict.Remove(hexCoord);
    }

    private void CreateTileAt(Vector3 worldCoord)
    {
        GameObject newHex = Instantiate(tile, worldCoord, Quaternion.identity, hexParent.transform);
        hexTileDict.Add(HexCoordinates.ConvertPositionToOffset(worldCoord), newHex.GetComponent<Hex>());
    }

    private void GenerateGrid()
    {
        int originShift = Mathf.RoundToInt((gridSideSize * 2 - 1) / 2);
        //Instantiating the grid
        //middle row is drawn first to not draw it twice when drawing up and down rows:
        for (int i = 0; i < 2 * gridSideSize - 1; i++)
        {
            Instantiate(tile, new Vector3((i - originShift) * xOffset, 0, 0), Quaternion.identity, hexParent.transform);
        }
        //upper and down rows:
        int rowSize = 2 * gridSideSize - 2;
        int rowIndex = 1;
        while (rowSize >= gridSideSize)
        {
            for (int i = 0; i < rowSize; i++)
            {
                Instantiate(tile, new Vector3((rowIndex / 2f + i - originShift) * xOffset, 0, rowIndex * zOffset), Quaternion.identity, hexParent.transform);
                Instantiate(tile, new Vector3((rowIndex / 2f + i - originShift) * xOffset, 0, -rowIndex * zOffset), Quaternion.identity, hexParent.transform);
            }
            rowSize -= 1;
            rowIndex += 1;
        }
    }

    public void DisableHighlightOfAllHexes()
    {
        foreach (var hex in hexTileDict.Values)
        {
            hex.DisableHighlight();
        }
    }

    public Hex GetTileAt(Vector3Int hexCoordinates)
    //Return the hex situated at given coordinates, null if there is no corresponding hex
    {
        Hex result = null;
        hexTileDict.TryGetValue(hexCoordinates, out result);
        return result;
    }

    public List<Vector3Int> GetNeighboursFor(Vector3Int hexCoordinates)
    {
        if (hexTileDict.ContainsKey(hexCoordinates) == false) //if the tile does not exist, no neighbour
        {
            return new List<Vector3Int>();
        }
        if (hexTileNeighboursDict.ContainsKey(hexCoordinates)) //if we ve already calculated the neighbours of the tile, return them
        {
            return hexTileNeighboursDict[hexCoordinates];
        }
        //if not, find neighbours and enter them in the dictionary
        hexTileNeighboursDict.Add(hexCoordinates, new List<Vector3Int>());
        foreach (Vector3Int direction in Direction.GetDirectionList(hexCoordinates.z))
        {
            if (hexTileDict.ContainsKey(hexCoordinates + direction))
            {
                hexTileNeighboursDict[hexCoordinates].Add(hexCoordinates + direction);
            }
        }
        return hexTileNeighboursDict[hexCoordinates];
    }

    public Vector3Int GetClosestHex(Vector3 worldPosition)
    {
        worldPosition.y = 0;
        return HexCoordinates.ConvertPositionToOffset(worldPosition);
    }

    /// <summary>
    /// Given offset coordinates, broadside (0 = right, 1 = left), and firing range
    /// Returns the attackable tiles from the provided offset coordinate position
    /// </summary>
    public List<Vector3Int> GetAttackableTilesFor(Vector3Int hexcoordinates, int side, int range)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        Ship ship = GetTileAt(hexcoordinates).Ship;

        if (hexTileDict.ContainsKey(hexcoordinates) == false || ship == null || range == 0)
        {
            return result;
        }

        // Forward: >
        // Hex:     X
        //
        //   L2 L1
        //   X  X
        // X SHIP> X
        //   X  X
        //   R2 R1

        int distBetweenHexCenters = 2;
        Transform shipTransform = ship.transform;
        Vector3 currHexWorldPosition = GetTileAt(hexcoordinates).gameObject.transform.position;

        // R1, R2 or L1, L2
        Vector3 dir1 = new Vector3();
        Vector3 dir2 = new Vector3();

        // Get directions for the arc from the ship broadside
        switch (side)
        {
            case 0:
                dir1 = (Quaternion.AngleAxis(60, Vector3.up) * shipTransform.forward).normalized;
                dir2 = (Quaternion.AngleAxis(120, Vector3.up) * shipTransform.forward).normalized;
                break;

            case 1:
                dir1 = (Quaternion.AngleAxis(-60, Vector3.up) * shipTransform.forward).normalized;
                dir2 = (Quaternion.AngleAxis(-120, Vector3.up) * shipTransform.forward).normalized;
                break;

            default:
                throw new Exception("Invalid broadside number (0=right, 1=left");
        }

        Debug.DrawLine(currHexWorldPosition, currHexWorldPosition + distBetweenHexCenters * range * dir1, Color.red, 100f, false);
        Debug.DrawLine(currHexWorldPosition, currHexWorldPosition + distBetweenHexCenters * range * dir2, Color.red, 100f, false);

        // Get upper and lower bounds for the firing cone
        Vector3[] hexLineUpper = new Vector3[range];
        Vector3[] hexLineLower = new Vector3[range];

        for (int i = 0; i < range; i++)
        {
            hexLineUpper[i] = currHexWorldPosition + (i + 1) * distBetweenHexCenters * dir1;
        }

        for (int i = 0; i < range; i++)
        {
            hexLineLower[i] = currHexWorldPosition + (i + 1) * distBetweenHexCenters * dir2;
        }

        //Debug.Log("Interpolating...");
        // Interpolate between LineUpper and LineLower
        for (int i = 0; i < range; i++)
        {
            Vector3 interpolateDir = hexLineLower[i] - hexLineUpper[i];

            float distance = interpolateDir.magnitude;
            int numHexesInLine = Mathf.RoundToInt(distance / distBetweenHexCenters) + 1;

            interpolateDir.Normalize();

            Debug.DrawLine(hexLineUpper[i], hexLineUpper[i] + distBetweenHexCenters * (numHexesInLine - 1) * interpolateDir, Color.red, 100f, false);

            for (int j = 0; j < numHexesInLine; j++)
            {
                Vector3 pos = hexLineUpper[i] + distBetweenHexCenters * j * interpolateDir;

                pos.x = Mathf.Round(pos.x); // x-positions are always integers (just remove the floating point error)
                pos.z = Mathf.Round(pos.z / 1.73f) * 1.73f; // z-positions are always a multiple of 1.73 (make sure they are)

                Vector3Int hexPos = GetClosestHex(pos);

                //Debug.Log(pos + "->" + hexPos);

                result.Add(hexPos);
            }
        }

        return result;
    }

    public List<Vector3Int> GetAccessibleNeighboursFor(Vector3Int hexcoordinates, Vector3 forward)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        //TODO : return the list of neighbours accessible for a ship pointing in the towards direction (only 3 possible tiles)
        if (hexTileDict.ContainsKey(hexcoordinates) == false) //if the tile does not exist, no neighbour
        {
            return result;
        }
        switch (Direction.IsOffsetEven(hexcoordinates.z))
        {
            case true:
                forward = HexCoordinates.ConvertVectorToOffset(forward);
                for (int i = 0; i < Direction.directionsOffsetEven.Count; i++)
                {
                    if (Direction.directionsOffsetEven[i] == forward)
                    {
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i - 1, 6)]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i - 1, 6)]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[i]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[i]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i + 1, 6)]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i + 1, 6)]);
                        }
                    }
                }
                break;
            case false:
                forward = HexCoordinates.ConvertVectorToOffset(forward);
                for (int i = 0; i < Direction.directionsOffsetOdd.Count; i++)
                {
                    if (Direction.directionsOffsetEven[i] == forward) //direction is always convert as an even configuration
                    {
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i - 1, 6)]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i - 1, 6)]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[i]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[i]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i + 1, 6)]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i + 1, 6)]);
                        }
                    }
                }
                break;
        }
        return result;
    }

    public static int PositiveModulo(int a, int b)
    {
        int c = a % b;
        if ((c < 0 && b > 0) || (c > 0 && b < 0))
        {
            c += b;
        }
        return c;
    }

    public void PlaceShip(Vector3Int hexCoord, Ship ship)
    {
        if (hexTileDict.ContainsKey(hexCoord))
        {
            hexTileDict[hexCoord].Ship = ship;
            Debug.Log("Ship placed at " + hexCoord);
        }
        else
        {
            Debug.Log("The ship is not placed on an existing tile.");
            throw new KeyNotFoundException();
        }
    }
}


public static class Direction
{
    public static List<Vector3Int> directionsOffsetOdd = new List<Vector3Int> //make it private ?
    {
        new Vector3Int(-1,0,1), //NW
        new Vector3Int(0,0,1),  //NE
        new Vector3Int(1,0,0),  //E
        new Vector3Int(0,0,-1),  //SE
        new Vector3Int(-1,0,-1),  //SW
        new Vector3Int(-1,0,0),  //W
    };

    public static List<Vector3Int> directionsOffsetEven = new List<Vector3Int>
    {
        new Vector3Int(0,0,1), //NW
        new Vector3Int(1,0,1),  //NE
        new Vector3Int(1,0,0),  //E
        new Vector3Int(1,0,-1),  //SE
        new Vector3Int(0,0,-1),  //SW
        new Vector3Int(-1,0,0),  //W
    };

    public static bool IsOffsetEven(int z)
    {
        return (z % 2 == 0 ? true : false);
    }

    public static List<Vector3Int> GetDirectionList(int z)
    //return the correct directions vectors depending on the z of a tile (even or odd line)
    {
        return (z % 2 == 0 ? directionsOffsetEven : directionsOffsetOdd);
    }
}
