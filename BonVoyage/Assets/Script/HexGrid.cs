using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    private Dictionary<Vector3Int, Hex> hexTileDict = new Dictionary<Vector3Int, Hex>();
    private Dictionary<Vector3Int, List<Vector3Int>> hexTileNeighboursDict = new Dictionary<Vector3Int, List<Vector3Int>>();
    public GameObject hexParent;

    private void Start()
    {
        var allHex = hexParent.GetComponentsInChildren<Hex>();
        foreach (Hex hexElement in allHex) 
        {
            hexTileDict.Add(hexElement.HexCoords, hexElement);
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
            if (hexTileDict.ContainsKey(hexCoordinates+direction))
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

    //TESTING
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            List<Vector3Int> example;
            example = GetAccessibleNeighboursFor(new Vector3Int(5, 0, 7), new Vector3(1f, 0f, 0f));
        }

        if (Input.GetKeyDown(KeyCode.B))
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
        }
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

        var maxRangeTilePositions = new List<Vector3>();

        //Debug.Log("Interpolating...");
        // Interpolate between LineUpper and LineLower
        for (int i = 0; i < range; i++)
        {
            Vector3 interpolateDir = hexLineLower[i] - hexLineUpper[i];
            
            float distance = interpolateDir.magnitude;
            int numHexesInLine = Mathf.RoundToInt(distance / distBetweenHexCenters) + 1;

            interpolateDir.Normalize();

            Debug.DrawLine(hexLineUpper[i], hexLineUpper[i] + distBetweenHexCenters * (numHexesInLine-1) * interpolateDir, Color.red, 100f, false);

            for (int j = 0; j < numHexesInLine; j++)
            {
                Vector3 pos = hexLineUpper[i] + distBetweenHexCenters * j * interpolateDir;
                
                pos.x = Mathf.Round(pos.x); // x-positions are always integers (just remove the floating point error)
                pos.z = Mathf.Round(pos.z / 1.73f) * 1.73f; // z-positions are always a multiple of 1.73 (make sure they are)

                Vector3Int hexPos = GetClosestHex(pos);
                
                //Debug.Log(pos + "->" + hexPos);

                result.Add(hexPos);

                // Store the tilepositions of the max range row
                if (i == range - 1)
                {
                    maxRangeTilePositions.Add(pos);
                }
            }
        }

        // Remove tiles that are obstructed by a ship.
        // Essentially, trace a ray to each of the max range tiles
        // if the ray hits a ship, the following tiles in the same direction
        // are obstructed.
        for (int i = 0; i < maxRangeTilePositions.Count; i++)
        {
            // Offset the y coordinate so we don't collide with hexes.
            var yOffset = 0.25f;
            var origin = currHexWorldPosition;
            origin.y += yOffset;

            var destination = maxRangeTilePositions[i];
            destination.y += yOffset;

            //Debug.Log(origin + "->" + destination);

            var rayDir = (destination - origin).normalized;

            Debug.DrawLine(origin, origin + rayDir * distBetweenHexCenters * range, Color.green, 100f, false);

            RaycastHit hit;
            if (Physics.Raycast(origin, rayDir, out hit))
            {
                //Debug.Log("Obstruction detected!" + hit.transform.name);
                var hitGO = hit.transform.gameObject;
                if (hitGO.CompareTag("PlayerShip") || hitGO.CompareTag("Pirate"))
                {
                    for (int j = 1; j < (hit.transform.position - destination).magnitude; j+=2)
                    {
                        var hexPos = GetClosestHex(hit.transform.position + rayDir*j);
                        result.Remove(hexPos);
                    }
                } 
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
                for(int i =0; i<Direction.directionsOffsetEven.Count; i++)
                {
                    if(Direction.directionsOffsetEven[i]==forward)
                    {
                        if(hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i - 1, 6)]))
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
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i-1, 6)]))
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
        Debug.Log("a neighbour of the ship is "+result[0] +" ; "+ (result.Count > 1 ? result[1] : new Vector3Int(-1, -1, -1)) + " ; "+ (result.Count > 2 ? result[2] : new Vector3Int(-1, -1, -1)));
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
        if(hexTileDict.ContainsKey(hexCoord))
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
