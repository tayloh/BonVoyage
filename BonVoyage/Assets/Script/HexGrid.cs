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
                forward = HexCoordinates.ConvertPositionToOffset(forward);
                for(int i =0; i<Direction.directionsOffsetEven.Count; i++)
                {
                    if(Direction.directionsOffsetEven[i]==forward)
                    {
                        if(hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[(i - 1) % 6]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[(i - 1) % 6]); 
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[i]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[i]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[(i + 1) % 6]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[(i + 1) % 6]);
                        }
                    }
                }
                break;
            case false:
                forward = HexCoordinates.ConvertPositionToOffset(forward);
                for (int i = 0; i < Direction.directionsOffsetOdd.Count; i++)
                {
                    if (Direction.directionsOffsetEven[i] == forward) //direction is always convert as an even configuration
                    {
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[(i - 1) % 6]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[(i - 1) % 6]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[i]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[i]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[(i + 1) % 6]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[(i + 1) % 6]);
                        }
                    }
                }
                break;
        }
        //Debug.Log("a neighbour of the ship is "+result[0]);
        return result;
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
