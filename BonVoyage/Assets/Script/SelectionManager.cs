using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectionManager : MonoBehaviour
{

    [SerializeField]
    private Camera mainCamera;

    public LayerMask selectionMask;

    //public HexGrid hexGrid;

    //private List<Vector3Int> neighbours = new List<Vector3Int>();

    private Hex previousHighligthedHex;

    public UnityEvent<GameObject> OnShipSelected;
    public UnityEvent<GameObject> TerrainSelected;

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
            if (ShipSelected(result))
            {
                Debug.Log("Clic on ship detected");
                OnShipSelected?.Invoke(result);
            }
            else
            {
                Debug.Log("clic on terrain detected");
                TerrainSelected?.Invoke(result);
            }
        }

        /*GameObject result;
        if (FindTarget(mousePosition, out result))
        {
            if (previousHighligthedHex != null)
            {
                previousHighligthedHex.DisableHighlightInvalid();
            }            
            Hex selectedHex = result.GetComponent<Hex>();

            selectedHex.DisableHighlight();

            foreach (Vector3Int neighbour in neighbours)
            {
                hexGrid.GetTileAt(neighbour).DisableHighlight();
            }

            //Testing finding neighbours
            //neighbours = hexGrid.GetNeighboursFor(selectedHex.HexCoords);

            if(selectedHex.Ship != null)
            {
                //Display only accessible neighbours in one move:
                neighbours = hexGrid.GetAccessibleNeighboursFor(selectedHex.HexCoords, -selectedHex.Ship.gameObject.transform.right);

                //Display accessible neighbours in a number of move points /!\DOES NOT WORK FOR MOVEPOINTS>1, NEEDS TO HANDLE ROTATION
                BFSResult bfsresult = GraphSearch.BFSGetRange(hexGrid, selectedHex.HexCoords, selectedHex.Ship.MovementPoints);
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
            }
            else //if there is no ship on the cell, just highlight it in different color
            {
                selectedHex.EnableHighlightInvalid();
                previousHighligthedHex = selectedHex;
            }
            

            //TODO: manage the case when the user clicks on a cell without ships => Highmight it or not? Change color?

        }*/
    }

    private bool ShipSelected(GameObject result)
    {
        return result.GetComponent<Ship>() != null;
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
