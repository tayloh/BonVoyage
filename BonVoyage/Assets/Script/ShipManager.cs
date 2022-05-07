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
        activeShip.IsPlaying = true;

        // Moved repair to start of players turn
        ship.Repair();
        PrepareShipForMovement(ship);
    }

    public void StartPirateTurn(Ship ship)
    {
        Debug.Log("START OF PIRATE TURN:" + ship.gameObject.name);
        activeShip = ship;
        activeShip.MovementFinished += PirateAIAttack;

        // Moved repair to start of players turn
        ship.Repair();

        var ai = ship.GetComponent<PirateAI>();

        if (PirateMovement)
        {
            List<Vector3> path = ai.ChoosePath(); // Only one element (don't support multi tile movement yet)
            Vector3 nextPos = path[0];

            // Don't move if the AI determined the next pos is the same pos as before
            if (!ai.IsNewPosSamePos(nextPos))
            {
                MovePirateShip(ship, path);
            }
            else
            {
                PirateAIAttack(ship);
            }

        }
        else
        {
            PirateAIAttack(ship);
        }
        
    }

    private void PirateAIAttack(Ship ship)
    {

        activeShip.MovementFinished -= PirateAIAttack;

        var ai = ship.GetComponent<PirateAI>();
        var bestShipToFireOn = ai.GetMostFavorableShipToAttackVariant();

        var didAttack = false;

        if (bestShipToFireOn != null)
        {
            // 
            didAttack = AIPerformAttackOn(ship, bestShipToFireOn);

            // This should never happen, but it does, problem in PerformAttackOn()?
            // Edit: haven't encountered it in a while now, but it's bound to happen
            // Edit: made another function that only the AI uses when attacking
            if (!didAttack)
            {
                ai.AIDebug("PerformAttackOn() not agreeing with pirateAI");
            }
        }
        else
        {
            //gameManager.NextTurn();
        }

        if (!didAttack)
        {
            gameManager.NextTurn();
        }
    }

    public void HandleShipSelected(GameObject shipGO)
    {

        Ship ship = shipGO.GetComponent<Ship>();

        // Be able to skip phase by clicking the active ship
        if (ship.gameObject.GetInstanceID() == activeShip.gameObject.GetInstanceID())
        {
            // SkipPhase function handles the logic for checking that it's the players turn.
            // So you can't spam click the active ship to skip everything.
            SkipPhase();
        }

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

    private bool AIPerformAttackOn(Ship aiShip, Ship targetedShip)
    {
        // Get attackable tiles
        List<Vector3Int> attackableRightSide = aiShip.GetAttackableTilesFor(0);
        List<Vector3Int> attackableLeftSide = aiShip.GetAttackableTilesFor(1);

        var didAttack = false;

        foreach (var tilePos in attackableLeftSide)
        {
            Hex hex = hexgrid.GetTileAt(tilePos); // currentHex can be null since tilePos might be outside of the grid!

            if (hex != null && hex.Ship != null && hex.Ship.GetInstanceID() == targetedShip.GetInstanceID())
            {
                aiShip.HasFiredLeft = true;
                activeShip.HasFiredLeft = true;

                didAttack = true;
            }
        }

        foreach (var tilePos in attackableRightSide)
        {
            Hex hex = hexgrid.GetTileAt(tilePos);

            if (hex != null && hex.Ship != null && hex.Ship.GetInstanceID() == targetedShip.GetInstanceID())
            {
                aiShip.HasFiredRight = true;
                activeShip.HasFiredRight = true; // just to be safe...

                didAttack = true;
            }
        }

        if (didAttack)
        {
            targetedShip.TakeDamage(DamageModel.GetCannonWiseDamageFor(aiShip, targetedShip));
            TriggerFiring();
        }

        return didAttack;
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
        Vector3Int attackedPosition = new Vector3Int(-9999, -9999, -9999);
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

            ship.TakeDamage(DamageModel.GetCannonWiseDamageFor(activeShip, ship));
            //after an attack it will incerease the last player ship selected.
            //if (_oldSelection != null) _oldSelection.Repair();
            TriggerFiring();

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
        if (HandleHexOutOfRange(selectedHex.HexCoords)|| HandleSelectedHexIsUnitHex(selectedHex.HexCoords))
        {
            return;
        }

        //else, process the selected hexagon
        HandleTargetHexSelected(selectedHex);
        
    }
    
    // Is called on ship turn
    private void HandleTargetHexSelected(Hex selectedHex)
    {
        // This was commented out to enable the player to just click once for moving their ship.
        //if (previouslySelectedHex == null || previouslySelectedHex != selectedHex)
        //{
        //    previouslySelectedHex = selectedHex;

        //    // When longer movements are possible, this shows the path that will be taken
        //    // before the player commits to it by clicking again.
        //    movementSystem.ShowPath(selectedHex.HexCoords, this.hexgrid);
        //}
        //else
        //{
        //    movementSystem.MoveShip(selectedShip, this.hexgrid);
        //    isNotMoving = false;                                    //TODO :change here if player's turn can take more than 1 move
        //    selectedShip.MovementFinished += ResetTurn;

        //    ClearOldSelection();
        //}

        movementSystem.SetCurrentPathTo(selectedHex.HexCoords);
        movementSystem.MoveShip(selectedShip, this.hexgrid);
        isNotMoving = false;
        selectedShip.MovementFinished += ResetTurn;

        ClearOldSelection();
    }

    private bool HandleSelectedHexIsUnitHex(Vector3Int hexPosition)
    {
        if (hexPosition == hexgrid.GetClosestHex(selectedShip.transform.position))
        {
            // Skip to next phase if we click on hex tile of "selected" ship
            //SkipPhase();

            //selectedShip.Deselect();

            // Can't clear the old selection here:
            // if the player clicks the hex that has the selected ship on it
            // the movment system HideRange() gets called which resets the available
            // movement options. => Just do nothing when the selected ships hex is clicked.
            //ClearOldSelection();
            
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

    internal void MovePirateShip(Ship ship, List<Vector3> path)
    {
        movementSystem.MoveShip(ship, hexgrid, path);
    }

    private void ResetTurn(Ship selectedShip)
    {
        selectedShip.MovementFinished -= ResetTurn;
        isNotMoving = true;
        gameManager.UpdateGameState(GameState.PlayerFire);
        
        PrepareShipForFiring(activeShip);

        if (!activeShip.HasAttackableInRange("Pirate")) { SkipPhase(); }
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
                //update cursor
                activeShip.ResetAttackableShips();
                activeShip.IsPlaying = false;
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
        activeShip.RemoveHighLightAttackableTiles(0);
        activeShip.RemoveHighLightAttackableTiles(1);

        var fireAnimation = activeShip.gameObject.GetComponent<FireAnimation>();
        
        if (activeShip.HasFiredRight)
        {
            fireAnimation.PlayFireAnimation(0);
            Debug.Log("FIREANIME " + fireAnimation.GetFireAnimationTime());
            yield return new WaitForSeconds(fireAnimation.GetFireAnimationTime());
        }
        else
        {
            fireAnimation.PlayFireAnimation(1);
            yield return new WaitForSeconds(fireAnimation.GetFireAnimationTime());
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
        activeShip.ResetAttackableShips();
        activeShip.IsPlaying = false;
        gameManager.NextTurn();

    }
}
