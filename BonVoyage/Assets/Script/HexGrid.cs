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

        ////Testing finding neighbours
        //List<Vector3Int> neighbours = GetNeighboursFor(new Vector3Int(0, 0, 0));
        //Debug.Log("Neighbours of 0,0,0 are: ");
        //foreach (Vector3Int pos in neighbours)
        //{
        //    Debug.Log(pos);
        //}
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
        if (hexTileNeighboursDict.ContainsKey(hexCoordinates)) //if we already calculated the neighbours of the tile, return them
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
                Debug.Log("found " + (hexCoordinates + direction).ToString());
            }
        }
        return hexTileNeighboursDict[hexCoordinates];
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

    public static List<Vector3Int> GetDirectionList(int z)
        //return the correct directions vectors depending on the z of a tile (even or odd line)
    {
        return (z % 2 == 0 ? directionsOffsetEven : directionsOffsetOdd);
    }
}
