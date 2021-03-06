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
        //Debug.Log(movementRange + "called by Hiderange");
        // TODO: Needs to get neighbors of wherever the ship is at (in hex coords)
        foreach (Vector3Int hexPos in movementRange.GetRangePositions())
        {
            hexGrid.GetTileAt(hexPos).DisableHighlight();
            //update cursor
            hexGrid.GetTileAt(hexPos).moveHereCursor = false;
        }
        movementRange = new BFSResult();
    }
    
    public void ShowRange(Ship selectedShip, HexGrid hexGrid)
    {
        CalculateRange(selectedShip, hexGrid);
        Vector3Int shipPos = hexGrid.GetClosestHex(selectedShip.transform.position);
        movementRange = movementRange;
        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            if(shipPos == hexPosition)
            {
                continue;
            }
            hexGrid.GetTileAt(hexPosition).EnableHighLight();
            //update cursor
            hexGrid.GetTileAt(hexPosition).moveHereCursor = true;
        }
    }

    private void CalculateRange(Ship selectedShip, HexGrid hexGrid)
    {
        movementRange = GraphSearch.BFSGetRange(hexGrid, hexGrid.GetClosestHex(selectedShip.transform.position), selectedShip.MovementPoints);
    }

    public void ShowPath(Vector3Int selectedHexPosition, HexGrid hexGrid)
    {
        Debug.Log(movementRange + "called by showpath");
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

    public void SetCurrentPathTo(Vector3Int hexPos)
    {
        _currentPath = movementRange.GetPathTo(hexPos);
    }

    public void MoveShip(Ship selectedShip, HexGrid hexGrid)
    {
        Debug.Log("Moving ship " + selectedShip.name);
        selectedShip.MoveThroughPath(_currentPath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());        
    }
    public void MoveShip(Ship selectedShip, HexGrid hexGrid, List<Vector3> path)
    {
        Debug.Log("Moving ship " + selectedShip.name);
        selectedShip.MoveThroughPath(path);
    }

    

    public bool IsHexInRange(Vector3Int hexPosition)
    {
        return movementRange.IsHexPositionInRange(hexPosition);
    }

}
