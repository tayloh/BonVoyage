using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{

    private List<Vector3Int> _currentPath = new List<Vector3Int>();
    
    public void HideRange(HexGrid hexGrid)
    {
        // TODO: Needs to get neighbors of wherever the ship is at (in hex coords)
        foreach (Vector3Int hexPos in hexGrid.GetNeighboursFor(new Vector3Int(0, 0, 0)))
        {
            hexGrid.GetTileAt(hexPos).DisableHighlight();
        }
    }

    public void ShowRange(Ship selectedShip, HexGrid hexGrid)
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
