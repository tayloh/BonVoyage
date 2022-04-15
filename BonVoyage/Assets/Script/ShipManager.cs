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

    private Ship activeShip;

    public Ship GetActiveShip()
    {
        return activeShip;
    }

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
        activeShip = ship;
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

    internal void MovePirateShip(Ship ship)
    {
        PirateAI pirateAI = ship.GetComponent<PirateAI>();
        movementSystem.MoveShip(ship, hexgrid, pirateAI.ChoosePath());
    }

    private void ResetTurn(Ship selectedShip)
    {
        selectedShip.MovementFinished -= ResetTurn;
        isNotMoving = true;
        gameManager.UpdateGameState(GameState.PlayerFire);
        //PrepareShipForFiring(selectedShip);
    }

    public void SkipPhase()
    {
        /*switch (gameManager.state)
        {
            case GameState.PlayerMove:
                //Prepare terrain for attack phase by highlighting the attackable hexagons only
                movementSystem.HideRange(this.hexgrid); //clean accessible hexagons 
                activeShip.HighLightAttackableTiles(0); //highlight attackable hex
                activeShip.HighLightAttackableTiles(1); //
                ResetTurn(activeShip);
                gameManager.UpdateGameState(GameState.PlayerFire);
                break;
            case GameState.PlayerFire:
                hexgrid.DisableHighlightOfAllHexes();
                gameManager.NextTurn();
                break;
        }*/
    }

    private void PrepareShipForFiring(Ship ship)
    {
        ship.HighLightAttackableTiles(0);
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
        hexgrid.DisableHighlightOfAllHexes();
        //TODO
        //fire canons from selectedShip (selectedShip gets set to null when selection highlight
        // dissappears so made a private variable to keep track of the current active ship

        //Debug.Log("Attempting fire from " + activeShip.gameObject.GetComponent<FireAnimation>());
        var fireAnimation = activeShip.gameObject.GetComponent<FireAnimation>();

        fireAnimation.PlayFireAnimation(0);
        yield return new WaitForSeconds(1); // it takes 1 second for one side to shoot (TODO shouldn't be hardcoded like this...)
        fireAnimation.PlayFireAnimation(1);


        //wait for the end of firing animation
        //...
        yield return new WaitForSeconds(1);

        /*switch (gameManager.state)
        {
            case GameState.PlayerFire:
                gameManager.UpdateGameState(GameState.PirateTurn);
                break;
            case GameState.PirateTurn:
                gameManager.UpdateGameState(GameState.PlayerMove);
                break;
        }*/
        gameManager.NextTurn();
        //throw new NotImplementedException();
        yield return null;
    }
}
