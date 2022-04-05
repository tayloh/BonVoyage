using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{

    [SerializeField]
    private Camera mainCamera;

    public LayerMask selectionMask;

    public HexGrid hexGrid;

    private List<Vector3Int> neighbours = new List<Vector3Int>();

    [SerializeField]
    private int movementPoints = 1;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    public void HandleClick(Vector3 mousePosition)
    {
        GameObject result;
        if (FindTarget(mousePosition, out result))
        {
            Hex selectedHex = result.GetComponent<Hex>();

            selectedHex.DisableHighlight();

            foreach (Vector3Int neighbour in neighbours)
            {
                hexGrid.GetTileAt(neighbour).DisableHighlight();
            }

            //Testing finding neighbours
            //neighbours = hexGrid.GetNeighboursFor(selectedHex.HexCoords);

            //Display only accessible neighbours in one move:
            neighbours = hexGrid.GetAccessibleNeighboursFor(selectedHex.HexCoords,-selectedHex.Ship.gameObject.transform.right);

            //Display accessible neighbours in a number of move points /!\DOES NOT WORK FOR MOVEPOINTS>1, NEEDS TO HANDLE ROTATION
            BFSResult bfsresult = GraphSearch.BFSGetRange(hexGrid, selectedHex.HexCoords, movementPoints);
            neighbours = new List<Vector3Int>(bfsresult.GetRangePositions());

            foreach (Vector3Int neighbour in neighbours)
            {
                hexGrid.GetTileAt(neighbour).EnableHighLight();
            }

            Debug.Log($"Neighbours of {selectedHex.HexCoords} are: ");
            foreach (Vector3Int pos in neighbours)
            {
                Debug.Log(pos);
            }

            //TODO: manage the case when the user clicks on a cell without ships => Highmight it or not? Change color?

        }
    }

    private bool FindTarget(Vector3 mousePosition, out GameObject result)
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out hit, selectionMask))
        {
            result = hit.collider.gameObject;
            return true;
        }
        result = null;
        return false;
    }
}
