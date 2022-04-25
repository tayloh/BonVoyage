using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{

    public bool PirateMovement = true;

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
    [SerializeField] private Ship _oldSelection;
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

    public void StartPirateTurn(Ship ship)
    {
        Debug.Log("START OF PIRATE TURN:" + ship.gameObject.name);
        activeShip = ship;
        activeShip.MovementFinished += PirateAIAttack;

        if (PirateMovement)
        {
            MovePirateShip(ship);
        }
        else
        {
            PirateAIAttack(ship);
        }
    }

    private void PirateAIAttack(Ship ship)
    {
        // Either we loop through all attackble tiles and attack the first
        // available player ship (this effectivley requires a rewrite of the 
        // PerformAttackOn function
        // Or
        // We can loop through all player ships, and do PerformAttackOn
        // on each of them, which checks internally if the ship is attackable or not
        // So, add to gamemanager a function (or field) that gets all player ships.

        //var attackableLeftBroadSide = ship.GetAttackableTilesFor(1);
        //var attackableRightBroadSide = ship.GetAttackableTilesFor(0);

        //var allAttackable = attackableLeftBroadSide;
        //allAttackable.AddRange(attackableRightBroadSide);

        //Ship attackableShip = null;

        //for (int i = 0; i < allAttackable.Count; i++)
        //{
        //    var currentHex = hexgrid.GetTileAt(allAttackable[i]);
        //    if (currentHex != null)
        //    {
        //        if (currentHex.Ship != null)
        //        {
        //            attackableShip = currentHex.Ship;
        //        }
        //    }
        //}

        //if (attackableShip != null)
        //{
        //    attackableShip.TakeDamage(ship.AttackDamage);
        //}

        Debug.Log("PIRATE SHOOTS");

        var playerShips = gameManager.GetPlayerShips();
        var foundAttackable = false;

        foreach (var playerShip in playerShips)
        {
            if(PerformAttackOn(playerShip))
            {
                foundAttackable = true;
                break;
            }
        }

        activeShip.MovementFinished -= PirateAIAttack;

        if (!foundAttackable) gameManager.NextTurn();

    }

    public void HandleShipSelected(GameObject shipGO)
    {

        Ship ship = shipGO.GetComponent<Ship>();

        GameState state = gameManager.state;

        switch (state)
        {
            case GameState.PlayerFire:
                PerformAttackOn(ship);
                break;

            // Add other states here (for instance upgrading a ship)
            //case GameState.UpgradeStage:
            //     PerformUpgradeOn(ship);
            //     break;
        }

    }

    private bool PerformAttackOn(Ship ship)
    {
        // To prevent being able to fire multiple times before the game state has changed
        // The game state doesn't change until the fire animation has played
        if (activeShip.HasFiredLeft || activeShip.HasFiredRight)
        {
            return false;
        }

        // If it's the players turn, make sure you can't friendly fire.
        if (gameManager.state == GameState.PlayerFire && ship.gameObject.tag != "Pirate") return false;

        // Get attackable tiles
        List<Vector3Int> attackableRightSide = activeShip.GetAttackableTilesFor(0);
        List<Vector3Int> attackableLeftSide = activeShip.GetAttackableTilesFor(1);

        List<Vector3Int> attackableTiles = attackableRightSide;
        attackableTiles.AddRange(attackableLeftSide);


        // Find out if ship is in an attackable tile
        bool isAttackable = false;
        Vector3Int attackedPosition = Vector3Int.zero;
        foreach (var tilePos in attackableTiles)
        {
            Hex currentHex = hexgrid.GetTileAt(tilePos); // currentHex can be null since tilePos might be outside of the grid!

            if (currentHex != null && currentHex.Ship != null && currentHex.Ship.gameObject.GetInstanceID() == ship.gameObject.GetInstanceID())
            {
                isAttackable = true;
                attackedPosition = tilePos;
                break;
            }
           
        }

        if (isAttackable)
        {
            if (attackableLeftSide.Contains(attackedPosition))
            {
                activeShip.HasFiredLeft = true;
            }
            else
            {
                activeShip.HasFiredRight = true;
            }

            ship.TakeDamage(activeShip.AttackDamage);
            //after an attack it will incerease the last player ship selected.
            if (_oldSelection != null) _oldSelection.Repair();
            TriggerFiring();

            Debug.Log(ship + " took " + activeShip.AttackDamage + " damage, and has health " + ship.Health);

            return true;
        }

        //Debug.Log("Attempted attack " + isAttackable);
        return false;
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
        
        PrepareShipForFiring(activeShip);
    }

    public void SkipPhase()
    {
        switch (gameManager.state)
        {
            case GameState.PlayerMove:
                //Prepare terrain for attack phase by highlighting the attackable hexagons only
                movementSystem.HideRange(this.hexgrid); //clean accessible hexagons 
                activeShip.HighLightAttackableTiles(0); //highlight attackable hex
                activeShip.HighLightAttackableTiles(1); //
                ResetTurn(selectedShip);
                gameManager.UpdateGameState(GameState.PlayerFire);
                break;
            case GameState.PlayerFire:
                hexgrid.DisableHighlightOfAllHexes();
                gameManager.NextTurn();
                break;
        }
    }

    private void PrepareShipForFiring(Ship ship)
    {
        if (ship.gameObject.tag != "Pirate")
        {
            ship.HighLightAttackableTiles(0);
            ship.HighLightAttackableTiles(1);
        }
    }

    private void PrepareShipForMovement(Ship shipReference)
    {
        _oldSelection = shipReference;
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

        var fireAnimation = activeShip.gameObject.GetComponent<FireAnimation>();
        
        if (activeShip.HasFiredRight)
        {
            fireAnimation.PlayFireAnimation(0);
            yield return new WaitForSeconds(fireAnimation.AnimationDuration);
        }
        else
        {
            fireAnimation.PlayFireAnimation(1);
            yield return new WaitForSeconds(fireAnimation.AnimationDuration);
        }


        //switch (gameManager.state)
        //{
        //    case GameState.PlayerFire:
        //        gameManager.UpdateGameState(GameState.PirateTurn);
        //        break;
        //    case GameState.PirateTurn:
        //        gameManager.UpdateGameState(GameState.PlayerMove);
        //        break;
        //}

        activeShip.HasFiredLeft = false;
        activeShip.HasFiredRight = false;
        gameManager.NextTurn();

    }
}
