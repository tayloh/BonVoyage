using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    // missing BFS
    private List<Vector3Int> _currentPath = new List<Vector3Int>();
    
    public void HideRange(HexGrid hexGrid)
    {
        // TODO: Needs to get neighbors of wherever the ship is at (in hex coords)
        foreach (Vector3Int hexPos in hexGrid.GetNeighboursFor(new Vector3Int(0, 0, 0)))
        {
            hexGrid.GetTileAt(hexPos).DisableHighlight();
        }
    }

    //WORK IN PROGRESS
    /*public void ShowRange(Ship selectedShip, HexGrid hexGrid)
    {
        CalculateRange(selectedShip, hexGrid);
        foreach (Vector3Int hexPosition in *//*movementRange.GetRangePositions())*//*)
        {

        }
    }*/

    private void CalculateRange(Ship selectedShip, HexGrid hexGrid)
    {
        throw new NotImplementedException();
    }
}
