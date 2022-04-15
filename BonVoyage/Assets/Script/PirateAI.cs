using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note that Pirate ships need the Pirate tag
public class PirateAI : MonoBehaviour
{
    private Ship ship;
    public HexGrid hexGrid;
    [SerializeField]
    private GameManager gameManager;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }
    public List<Vector3> ChoosePath()
    {
        //TODO : choose how to move the pirate ship 
        //
        //
        //default for debugging : chooses the first available hex
        Vector3Int offsetPosOfShip = HexCoordinates.ConvertPositionToOffset(transform.position - new Vector3(0,1,0));
        List<Vector3Int> neighbours = hexGrid.GetAccessibleNeighboursFor(offsetPosOfShip, transform.forward);
        Vector3Int neighbourCoord = new Vector3Int();
        if (!hexGrid.GetTileAt(neighbours[0]).IsObstacle())
        {
            neighbourCoord = neighbours[0];
        }
        else
        {
            neighbourCoord = neighbours[1]; //just to minize the possibility to choose an occupied tile, the AI will have to check it properly
        }
        Vector3 positionGoal = hexGrid.GetTileAt(neighbourCoord).transform.position;
        StartCoroutine(EndPirateTurn());
        return new List<Vector3>() { positionGoal};    
    }

    private IEnumerator EndPirateTurn()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(ship.MovementDuration + 0.1f);
        gameManager.NextTurn(); 
        yield return null;
    }
}
