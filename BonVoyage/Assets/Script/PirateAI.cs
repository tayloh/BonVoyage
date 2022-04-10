using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        //default for debugging : chooses the first available hex
        Vector3Int offsetPosOfShip = HexCoordinates.ConvertPositionToOffset(transform.position - new Vector3(0,1,0));
        List<Vector3Int> neighbours = hexGrid.GetAccessibleNeighboursFor(offsetPosOfShip, transform.forward);
        Vector3Int neighbourCoord = neighbours[0];
        Vector3 positionGoal = hexGrid.GetTileAt(neighbourCoord).transform.position;
        StartCoroutine("EndPirateTurn");
        return new List<Vector3>() { positionGoal};    
    }

    private IEnumerator EndPirateTurn()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(ship.MovementDuration);
        gameManager.UpdateGameState(GameState.PlayerMove);
        yield return null;
    }
}
