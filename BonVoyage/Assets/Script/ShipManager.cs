using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    [SerializeField]
    private HexGrid hexgrid;

    [SerializeField]
    private MovementSystem movementSystem;
    [SerializeField]
    private GameManager gameManager;

    public bool isNotMoving { get; private set; } = true;
    private bool isPlayerTurn;

    [SerializeField]
    private Ship selectedShip;
    private Hex previouslySelectedHex;

    public void HandleShipSelection(GameObject ship) //not used here sinced the ship is not chosen by the player
    {
        if (isNotMoving == false)
        {
            return;
        }
        Ship shipReference = ship.GetComponent<Ship>();
        if (CheckIfTheSameShipSelected(shipReference))
        {
            return;
        }

        PrepareShipForMovement(shipReference);
    }

    public void StartPlayerTurn(Ship ship)
    {
        PrepareShipForMovement(ship);
    }

    public void HandleTerrainSelected(GameObject hexGO)
    {
        //check if it is not the player's turn or if there is no ship selected, ignore it
        if(gameManager.state != GameState.PlayerMove || selectedShip == null || isNotMoving == false)
        {
            return;
        }

        Hex selectedHex = hexGO.GetComponent<Hex>();
        //check if the selected hex is out of range or under the seleceted ship, ignore it
        if (HandleHexOutOfRange(selectedHex.HexCoords) || HandleSelectedHexIsUnitHex(selectedHex.HexCoords))
        {
            return;
        }

        //else, process the selected hexagon
        HandleTargetHexSelected(selectedHex);
    }
    
    private void HandleTargetHexSelected(Hex selectedHex)
    {
        if (previouslySelectedHex == null || previouslySelectedHex != selectedHex)
        {
            previouslySelectedHex = selectedHex;
            movementSystem.ShowPath(selectedHex.HexCoords, this.hexgrid);
        }
        else
        {
            movementSystem.MoveShip(selectedShip, this.hexgrid);
            isNotMoving = false;                                    //TODO :change here if player's turn can take more than 1 move
            selectedShip.MovementFinished += ResetTurn;
            ClearOldSelection();
        }
    }

    private bool HandleSelectedHexIsUnitHex(Vector3Int hexPosition)
    {
        if (hexPosition == hexgrid.GetClosestHex(selectedShip.transform.position))
        {
            selectedShip.Deselect();
            ClearOldSelection();
            return true;
        }
        return false;
    }

    private bool HandleHexOutOfRange(Vector3Int hexPosition)
    {
        if(movementSystem.IsHexInRange(hexPosition) == false)
        {
            Debug.Log("Hex out of Range!"); //TODO : add a feedback so the player knows he can't reach this tile (sound effect?)
            return true;
        }
        return false;
    }

    private void ResetTurn(Ship selectedShip)
    {
        selectedShip.MovementFinished -= ResetTurn;
        isNotMoving = true;
        gameManager.UpdateGameState(GameState.PlayerFire);
    }

    private void PrepareShipForMovement(Ship shipReference)
    {
        if (this.selectedShip != null)
        {
            ClearOldSelection();
        }
        this.selectedShip = shipReference;
        this.selectedShip.Select();
        movementSystem.ShowRange(this.selectedShip, this.hexgrid);
    }

    private bool CheckIfTheSameShipSelected(Ship shipReference)
    {
        if (this.selectedShip ==  shipReference)
        {
            ClearOldSelection();
            return true;
        }
        return false;
    }

    private void ClearOldSelection()
    {
        previouslySelectedHex = null;
        this.selectedShip.Deselect();
        movementSystem.HideRange(this.hexgrid);
        this.selectedShip = null;
    }

    public void TriggerFiring()
    {
        StartCoroutine("FireActiveShip");
    }

    private IEnumerator FireActiveShip()
    {
        //TODO
        //fire canons from selectedShip
        //...
        throw new NotImplementedException();

        //wait for the end of firing animation
        //...
        gameManager.UpdateGameState(GameState.PirateTurn);
        yield return null;
    }
}
