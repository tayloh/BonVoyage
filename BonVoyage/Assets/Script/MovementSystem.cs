using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    private BFSResult movementRange = new BFSResult();
    private List<Vector3Int> _currentPath = new List<Vector3Int>();
    
    public void HideRange(HexGrid hexGrid)
    {
        // TODO: Needs to get neighbors of wherever the ship is at (in hex coords)
        foreach (Vector3Int hexPos in movementRange.GetRangePositions())
        {
            hexGrid.GetTileAt(hexPos).DisableHighlight();
        }
        movementRange = new BFSResult();
    }

    //WORK IN PROGRESS
    public void ShowRange(Ship selectedShip, HexGrid hexGrid)
    {
        CalculateRange(selectedShip, hexGrid);
        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            hexGrid.GetTileAt(hexPosition).EnableHighLight();
        }
    }

    private void CalculateRange(Ship selectedShip, HexGrid hexGrid)
    {
        movementRange = GraphSearch.BFSGetRange(hexGrid, hexGrid.GetClosestHex(selectedShip.transform.position), selectedShip.MovementPoints);
    }

    public void ShowPath(Vector3Int selectedHexPosition, HexGrid hexGrid)
    {
        if (movementRange.GetRangePositions().Contains(selectedHexPosition))
        {
            foreach (Vector3Int hexPosition in _currentPath)
            {
                hexGrid.GetTileAt(hexPosition).ResetHighlight();
            }
            _currentPath = movementRange.GetPathTo(selectedHexPosition);
            foreach (Vector3Int hexPosition in _currentPath)
            {
                hexGrid.GetTileAt(hexPosition).HighlightPath();
            }
        }
    }

    public void MoveShip(Ship selectedShip, HexGrid hexGrid)
    {
        Debug.Log("Moving ship " + selectedShip.name);
        selectedShip.MoveThroughPath(_currentPath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());
    }

    public bool IsHexInRange(Vector3Int hexPosition)
    {
        return movementRange.IsHexPositionInRange(hexPosition);
    }
}
