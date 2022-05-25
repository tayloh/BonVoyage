using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note that Pirate ships need the Pirate tag
public class PirateAI : MonoBehaviour
{
    
    public static int SearchDepth = 6; // AI maximum planned path is 12 hexes long
    public static float DFSOutOfRangeDistance = 9; // Euclidian distance. Set this value slightly less than double SearchDepth. NOT USED ATM.
    public static float AIConsideredLowHpThreshold = 16; // Assume ships will deal at least 16 damage

    public static Vector3 AIFailed = new Vector3(-9999, -9999, -9999); // use instead of Vector3.zero (not nullable)
    public static Vector3Int AIFailedInt = new Vector3Int(-9999, -9999, -9999);

    private Ship ship;
    public HexGrid hexGrid;
    [SerializeField]
    private GameManager gameManager;

    private Vector3 _NextPos;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    public List<Vector3> ChoosePath()
    {
        AIDebug("----NEW SHIP TURN (" + ship.name + ")----");

        var AIMove = _GetAIMoveAction();
        var nextPos = _AIMoveHandler(AIMove);

        if (nextPos != PirateAI.AIFailed)
        {
            AIDebug("New AI chose path");
            return new List<Vector3> { nextPos };
        }
        
        
        AIDebug("Old AI chose path");

        //default for debugging : chooses the first available hex
        Vector3Int offsetPosOfShip = HexCoordinates.ConvertPositionToOffset(transform.position - new Vector3(0,1,0));
        List<Vector3Int> neighbours = _GetNonObstacleNeighbours(offsetPosOfShip, transform.forward);

        // All AI move towards same goal position when game is over (DFS move will fail when there are no player ships)
        var destination = new Vector3(100000, 0, 0);
        var closestTileToDestination = neighbours[0];
        var closestDist = float.MaxValue;
        foreach (var tilePos in neighbours)
        {
            var currTile = hexGrid.GetTileAt(tilePos);
            if (currTile == null) continue;

            var dist = (destination - currTile.transform.position).magnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                closestTileToDestination = tilePos;
            }
        }

        Vector3 positionGoal = hexGrid.GetTileAt(closestTileToDestination).transform.position;
        //StartCoroutine(EndPirateTurn());
        return new List<Vector3>() { positionGoal };    
    }

    public Ship GetMostFavorableShipToAttack()
    {
        var attackableShips = _GetAttackablePlayerShips();

        AIDebug("Found " + attackableShips.Count + " available targets");

        if (attackableShips.Count == 0) return null;

        var atkTypeOfBestShip = AttackType.Bow;
        var atkNameOfBestShip = "";
        if (attackableShips.Count == 1)
        {
            var ship = attackableShips[0];

            atkTypeOfBestShip = DamageModel.GetDirectionalAttackType(this.ship, ship);
            atkNameOfBestShip = DamageModel.GetAttackTypeString(atkTypeOfBestShip);

            AIDebug("Attacking " + ship.name + " (Type: " + atkNameOfBestShip + ", Health: " + ship.Health + ")" );
            return attackableShips[0];
        }

        // Prio: attacktype -> distance vs. health

        // Build attacktype dict
        var attackTypeDict = new Dictionary<AttackType, List<Ship>>();
        attackTypeDict.Add(AttackType.Stern, new List<Ship>());
        attackTypeDict.Add(AttackType.Side, new List<Ship>());
        attackTypeDict.Add(AttackType.Bow, new List<Ship>());

        foreach (var ship in attackableShips)
        {
            var attackType = DamageModel.GetDirectionalAttackType(this.ship, ship);
            attackTypeDict[attackType].Add(ship);
        }

        // Filter on best directional attack
        var shipsFilteredOnBestDirectional = new List<Ship>();
        if (attackTypeDict[AttackType.Stern].Count > 0)
        {
            // Consider these ships (highest damage)
            shipsFilteredOnBestDirectional = attackTypeDict[AttackType.Stern];
        }
        else if (attackTypeDict[AttackType.Side].Count > 0)
        {
            // Consider these ships (most accurate attack)
            shipsFilteredOnBestDirectional = attackTypeDict[AttackType.Side];
        }
        else
        {
            // Consider all ships
            shipsFilteredOnBestDirectional = attackableShips;
        }

        // Filter on shortest distance
        var shortestDist = float.MaxValue;
        var shortestDistShip = shipsFilteredOnBestDirectional[0];
        foreach (var ship in shipsFilteredOnBestDirectional)
        {
            var distanceToShip = (this.transform.position - ship.transform.position).magnitude;
            if (distanceToShip < shortestDist)
            {
                shortestDistShip = ship;
                shortestDist = distanceToShip;
            }
        }

        // Filter on health
        var lowestHealth = float.MaxValue;
        var lowestHealthShip = shipsFilteredOnBestDirectional[0];
        foreach (var ship in shipsFilteredOnBestDirectional)
        {
            var shipHealth = ship.Health;
            if (shipHealth < lowestHealth)
            {
                lowestHealthShip = ship;
                lowestHealth = shipHealth;
            }
        }

        // Weigth distance vs health
        var bestShip = lowestHealthShip;
        if (lowestHealthShip.Health > AIConsideredLowHpThreshold)
        {
            bestShip = shortestDistShip;
        }

        //var index = UnityEngine.Random.Range(0, attackableShips.Count);
        //return attackableShips[index];

        atkTypeOfBestShip = DamageModel.GetDirectionalAttackType(this.ship, bestShip);
        atkNameOfBestShip = DamageModel.GetAttackTypeString(atkTypeOfBestShip);

        AIDebug("Attacking " + bestShip.name + " (Type: " + atkNameOfBestShip + ", Health: " + bestShip.Health + ")");

        return bestShip;
    }

    public Ship GetMostFavorableShipToAttackVariant()
    {
        var attackableShips = _GetAttackablePlayerShips();

        AIDebug("Found " + attackableShips.Count + " available targets");

        if (attackableShips.Count == 0) return null;

        var lowPercentageHealthShips = _FilterShipsOnLowestPercentageHealth(attackableShips);
        var bestDirectionalShips = _FilterShipsOnBestDirectional(lowPercentageHealthShips);
        var closestShips = _FilterShipsOnShortestDistance(bestDirectionalShips);
        var lowestMaxHealthShips = _FilterShipsOnLowestMaxHealth(closestShips);

        return lowestMaxHealthShips[0];
        
    }

    private List<Ship> _FilterShipsOnLowestMaxHealth(List<Ship> ships)
    {
        if (ships.Count == 1)
        {
            return ships;
        }

        var lowestMaxHealth = float.MaxValue;
        var shipWithLowestMaxHealth = ships[0];
        foreach (var ship in ships)
        {
            if (ship.MaxHealth < lowestMaxHealth)
            {
                lowestMaxHealth = ship.MaxHealth;
                shipWithLowestMaxHealth = ship;
            }
        }

        var result = new List<Ship>();
        foreach (var ship in ships)
        {
            if (Mathf.Approximately(ship.MaxHealth, lowestMaxHealth))
            {
                result.Add(ship);
            }
        }
        return result;
    }

    private List<Ship> _FilterShipsOnBestDirectional(List<Ship> ships)
    {
        if (ships.Count == 1)
        {
            return ships;
        }

        // Build attacktype dict
        var attackTypeDict = new Dictionary<AttackType, List<Ship>>();
        attackTypeDict.Add(AttackType.Stern, new List<Ship>());
        attackTypeDict.Add(AttackType.Side, new List<Ship>());
        attackTypeDict.Add(AttackType.Bow, new List<Ship>());

        foreach (var ship in ships)
        {
            var attackType = DamageModel.GetDirectionalAttackType(this.ship, ship);
            attackTypeDict[attackType].Add(ship);
        }

        // Filter on best directional attack
        var shipsFilteredOnBestDirectional = new List<Ship>();
        if (attackTypeDict[AttackType.Stern].Count > 0)
        {
            // Consider these ships (highest damage)
            shipsFilteredOnBestDirectional = attackTypeDict[AttackType.Stern];
        }
        else if (attackTypeDict[AttackType.Side].Count > 0)
        {
            // Consider these ships (most accurate attack)
            shipsFilteredOnBestDirectional = attackTypeDict[AttackType.Side];
        }
        else
        {
            // Consider all ships
            shipsFilteredOnBestDirectional = ships;
        }

        return shipsFilteredOnBestDirectional;
    }

    private List<Ship> _FilterShipsOnShortestDistance(List<Ship> ships)
    {

        if (ships.Count == 1)
        {
            return ships;
        }

        // Filter on shortest distance
        var shortestDist = float.MaxValue;
        foreach (var ship in ships)
        {
            var distanceToShip = (this.transform.position - ship.transform.position).magnitude;
            if (distanceToShip < shortestDist)
            {
                shortestDist = distanceToShip;
            }
        }

        var result = new List<Ship>();
        foreach (var ship in ships)
        {
            var distanceToShip = (this.transform.position - ship.transform.position).magnitude;
            if (Mathf.Approximately(distanceToShip, shortestDist))
            {
                result.Add(ship);
            }
        }
        return result;
    }

    private List<Ship> _FilterShipsOnLowestHealth(List<Ship> ships)
    {

        if (ships.Count == 1)
        {
            return ships;
        }

        // Filter on health
        var lowestHealth = float.MaxValue;
        var lowestHealthShip = ships[0];
        foreach (var ship in ships)
        {
            var shipHealth = ship.Health;
            if (shipHealth < lowestHealth)
            {
                lowestHealthShip = ship;
                lowestHealth = shipHealth;
            }
        }

        var result = new List<Ship>();
        foreach (var ship in ships)
        {
            var health = ship.Health;
            if (health == lowestHealth)
            {
                result.Add(ship);
            }
        }
        return result;
    }

    private List<Ship> _FilterShipsOnLowestPercentageHealth(List<Ship> ships)
    {
        if (ships.Count == 1)
        {
            return ships;
        }

        var lowestHealthPercentage = float.MaxValue;
        foreach (var ship in ships)
        {
            var shipHealthPercentage = ship.Health / ship.MaxHealth;
            if (shipHealthPercentage < lowestHealthPercentage)
            {
                lowestHealthPercentage = shipHealthPercentage;
            }
        }

        var result = new List<Ship>();
        foreach (var ship in ships)
        {
            var healthPercentage = ship.Health / ship.MaxHealth;
            if (Mathf.Approximately(healthPercentage, lowestHealthPercentage))
            {
                result.Add(ship);
            }
        }
        return result;
    }

    private IEnumerator EndPirateTurn()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(ship.MovementDuration + 0.1f);

        // Updating the turn after pirate in shipmanager instead, see PirateAIAttack() and FireActiveShip()
        gameManager.NextTurn(); 
        yield return null;
    }

    public bool HasAttackableInRange()
    {
        // Get attackable tiles
        List<Vector3Int> attackableRightSide = gameObject.GetComponent<Ship>().GetAttackableTilesFor(0);
        List<Vector3Int> attackableLeftSide = gameObject.GetComponent<Ship>().GetAttackableTilesFor(1);

        //List<Vector3Int> attackableRightSide = _GetAttackableTilesFrom(this.ship.hexCoord, this.ship.transform.forward, 0, this.ship.FireRange);
        //List<Vector3Int> attackableLeftSide = _GetAttackableTilesFrom(this.ship.hexCoord, this.ship.transform.forward, 1, this.ship.FireRange);

        List<Vector3Int> attackableTiles = attackableRightSide;
        attackableTiles.AddRange(attackableLeftSide);

        foreach (var tile in attackableTiles)
        {
            Hex currentHex = hexGrid.GetTileAt(tile);
            if (currentHex != null && currentHex.Ship != null && currentHex.Ship.gameObject.CompareTag("PlayerShip"))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsNewPosSamePos(Vector3 newPos)
    {
        if (newPos == hexGrid.GetTileAt(ship.hexCoord).transform.position)
        {
            return true;
        }

        return false;
    }

    private List<Ship> _GetAttackablePlayerShips()
    {
        var attackableTiles = this.ship.GetAttackableTilesFor(0);
        attackableTiles.AddRange(this.ship.GetAttackableTilesFor(1));

        List<Ship> attackableShips = new List<Ship>();

        foreach (var tilePos in attackableTiles)
        {
            var tile = hexGrid.GetTileAt(tilePos);
            
            if (tile == null) continue;
            if (tile.Ship == null) continue;
            if (tile.Ship.CompareTag(this.gameObject.tag)) continue;
            if (tile.Ship.IsDead) continue;

            attackableShips.Add(tile.Ship);
        }
        return attackableShips;
    }

    private List<Vector3Int> _GetAttackableTilesFrom(Vector3Int hexcoordinates, Vector3 forwardDir, int side, int range)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        if (hexGrid.GetTileAt(hexcoordinates) == null || range == 0)
        {
            return result;
        }

        // Forward: >
        // Hex:     X
        //
        //   L2 L1
        //   X  X
        // X SHIP> X
        //   X  X
        //   R2 R1

        float distBetweenHexCenters = HexCoordinates.xOffset;

        Vector3 currHexWorldPosition = hexGrid.GetTileAt(hexcoordinates).gameObject.transform.position;

        // R1, R2 or L1, L2
        Vector3 dir1 = new Vector3();
        Vector3 dir2 = new Vector3();

        // Get directions for the arc from the ship broadside
        switch (side)
        {
            case 0:
                dir1 = (Quaternion.AngleAxis(60, Vector3.up) * forwardDir).normalized;
                dir2 = (Quaternion.AngleAxis(120, Vector3.up) * forwardDir).normalized;
                break;

            case 1:
                dir1 = (Quaternion.AngleAxis(-60, Vector3.up) * forwardDir).normalized;
                dir2 = (Quaternion.AngleAxis(-120, Vector3.up) * forwardDir).normalized;
                break;

            default:
                throw new Exception("Invalid broadside number (0=right, 1=left");
        }

        //Debug.DrawLine(currHexWorldPosition, currHexWorldPosition + distBetweenHexCenters * range * dir1, Color.red, 100f, false);
        //Debug.DrawLine(currHexWorldPosition, currHexWorldPosition + distBetweenHexCenters * range * dir2, Color.red, 100f, false);

        // Get upper and lower bounds for the firing cone
        Vector3[] hexLineUpper = new Vector3[range];
        Vector3[] hexLineLower = new Vector3[range];

        for (int i = 0; i < range; i++)
        {
            hexLineUpper[i] = currHexWorldPosition + (i + 1) * distBetweenHexCenters * dir1;
        }

        for (int i = 0; i < range; i++)
        {
            hexLineLower[i] = currHexWorldPosition + (i + 1) * distBetweenHexCenters * dir2;
        }

        var maxRangeTilePositions = new List<Vector3>();
        var vec3Positions = new List<Vector3>();

        //Debug.Log("Interpolating...");
        // Interpolate between LineUpper and LineLower
        for (int i = 0; i < range; i++)
        {
            Vector3 interpolateDir = hexLineLower[i] - hexLineUpper[i];

            float distance = interpolateDir.magnitude;
            int numHexesInLine = Mathf.RoundToInt(distance / distBetweenHexCenters) + 1;

            interpolateDir.Normalize();

            //Debug.DrawLine(hexLineUpper[i], hexLineUpper[i] + distBetweenHexCenters * (numHexesInLine - 1) * interpolateDir, Color.red, 100f, false);

            for (int j = 0; j < numHexesInLine; j++)
            {
                Vector3 pos = hexLineUpper[i] + distBetweenHexCenters * j * interpolateDir;

                pos.x = Mathf.Round(pos.x); // x-positions are always integers (just remove the floating point error)
                pos.z = Mathf.Round(pos.z / 1.73f) * 1.73f; // z-positions are always a multiple of 1.73 (make sure they are)

                Vector3Int hexPos = hexGrid.GetClosestHex(pos);

                //Debug.Log(pos + "->" + hexPos);

                result.Add(hexPos);

                // Need this for checking line of sight
                vec3Positions.Add(pos);

                // Store the tilepositions of the max range row (might remove this)
                //if (i == range - 1)
                //{
                //    maxRangeTilePositions.Add(pos);
                //}
            }
        }

        // Direct 2D raytrace calculations
        var shipWorldPositions = gameManager.GetShipWorldPositions();


        // Trace a ray to each vec3position on the grid of attackable tiles
        for (int i = 0; i < vec3Positions.Count; i++)
        {
            var lineStart = new Vector2(currHexWorldPosition.x, currHexWorldPosition.z);
            var lineEnd = new Vector2(vec3Positions[i].x, vec3Positions[i].z);
            var maxDistance = (lineEnd - lineStart).magnitude - distBetweenHexCenters / 2;

            var foundIntersection = false;

            // For each ray, check if it intersects with any ship (circle collider)
            for (int j = 0; j < shipWorldPositions.Count; j++)
            {

                var origin = new Vector2(shipWorldPositions[j].x, shipWorldPositions[j].z);
                var hasIntersection = hexGrid.LineCircleIntersect(lineStart, lineEnd, origin, 0.9f, maxDistance);

                // Check if there was an intersection or not
                if (hasIntersection)
                {
                    foundIntersection = true;
                    break;
                }
            }

            if (foundIntersection)
            {
                var tileConsideredForRemoval = hexGrid.GetClosestHex(vec3Positions[i]);
                result.Remove(tileConsideredForRemoval);
            }

        }
        return result;
    }

    private bool _HasAttackableFrom(Vector3Int hexcoordinates, Vector3 forwardDir, int range)
    {
        // Get attackable tiles
        List<Vector3Int> attackableRightSide = _GetAttackableTilesFrom(hexcoordinates, forwardDir, 0, range);
        List<Vector3Int> attackableLeftSide = _GetAttackableTilesFrom(hexcoordinates, forwardDir, 1, range);

        List<Vector3Int> attackableTiles = attackableRightSide;
        attackableTiles.AddRange(attackableLeftSide);

        foreach (var tile in attackableTiles)
        {
            Hex currentHex = hexGrid.GetTileAt(tile);
            if (currentHex != null && currentHex.Ship != null && currentHex.Ship.gameObject.CompareTag("PlayerShip"))
            {
                //var from = hexGrid.GetTileAt(hexcoordinates);
                //from.EnableHighLight();

                //currentHex.EnableHighLight();
                //Debug.Log("DFS DEBUG: " + hexcoordinates + "->" + currentHex.HexCoords);
                //Debug.DrawLine(from.transform.position, from.transform.position + forwardDir * 2, Color.red, 100f, false);

                //foreach (var item in attackableTiles)
                //{
                //    if (hexGrid.GetTileAt(item) != null) hexGrid.GetTileAt(item).EnableHighLight();
                //}
                return true;
            }
        }

        return false;
    }

    private List<Vector3Int> _GetNonObstacleNeighbours(Vector3Int hexCoords, Vector3 forwardDir)
    {
        var accessibleNeighbours = hexGrid.GetAccessibleNeighboursFor(hexCoords, forwardDir);
        var result = new List<Vector3Int>();
        foreach (var neighbour in accessibleNeighbours)
        {
            var tile = hexGrid.GetTileAt(neighbour);
            if (tile == null) continue;

            if (!tile.IsObstacle())
            {
                result.Add(neighbour);
            }
        }
        return result;
    }

    private float _GetDistanceToClosestPlayerShip(Vector3 from)
    {
        var closestDistance = float.MaxValue;
        var playerShips = gameManager.GetPlayerShipWorldPositions();
        foreach (var shipPos in playerShips)
        {
            var distToShip = (shipPos - from).magnitude;
            if (distToShip < closestDistance)
            {
                closestDistance = distToShip;
            }
        }
        return closestDistance;
    }

    private float _GetDistanceToClosestPlayerShip()
    {
        return _GetDistanceToClosestPlayerShip(this.gameObject.transform.position);
    }

    private Ship _GetClosestPlayerShip(Vector3 from)
    {
        Vector3Int closestShipPos = PirateAI.AIFailedInt;
        var closestDistance = float.MaxValue;
        var playerShips = gameManager.GetPlayerShipWorldPositions();
        foreach (var shipPos in playerShips)
        {
            var distToShip = (shipPos - from).magnitude;
            if (distToShip < closestDistance)
            {
                closestDistance = distToShip;
                closestShipPos = hexGrid.GetClosestHex(shipPos); // please work
            }
        }

        var tile = hexGrid.GetTileAt(closestShipPos);
        if (tile == null) return null;
        if (tile.Ship == null) return null;

        return tile.Ship;
    }

    private List<Ship> _GetShipsThatCanFireOnMe()
    {
        var result = new List<Ship>();
        foreach (var shipPos in gameManager.GetPlayerShipOffsetPositions())
        {
            var tile = hexGrid.GetTileAt(shipPos);
            if (tile == null) continue;

            var playerShip = tile.Ship;
            if (playerShip == null) continue;

            var firingArc = playerShip.GetAttackableTilesFor(0);
            firingArc.AddRange(playerShip.GetAttackableTilesFor(1));

            foreach (var item in firingArc)
            {
                var tileInArc = hexGrid.GetTileAt(item);
                if (tileInArc == null) continue;

                if (tileInArc.Ship == null) continue;

                if (tileInArc.Ship.gameObject.GetInstanceID() == this.ship.gameObject.GetInstanceID())
                {
                    result.Add(playerShip);
                }
            }
        }

        return result;

    }

    private int _GetNumberOfPlayerShipsAttackingPos(Vector3Int position)
    {
        var result = 0;
        foreach (var shipPos in gameManager.GetPlayerShipOffsetPositions())
        {
            var tile = hexGrid.GetTileAt(shipPos);
            if (tile == null) continue;

            var playerShip = tile.Ship;
            if (playerShip == null) continue;

            var firingArc = playerShip.GetAttackableTilesFor(0);
            firingArc.AddRange(playerShip.GetAttackableTilesFor(1));

            foreach (var vec3int in firingArc)
            {
                if (vec3int == position)
                {
                    result++;
                }
            }
        }

        return result;
    }

    private bool _HasShipInFiringArc(Ship ship)
    {
        var AIFiringArc = this.ship.GetAttackableTilesFor(0);
        AIFiringArc.AddRange(this.ship.GetAttackableTilesFor(1));

        foreach (var item in AIFiringArc)
        {
            var tile = hexGrid.GetTileAt(item);
            if (tile == null) continue;
            if (tile.Ship == null) continue;

            if (tile.Ship.gameObject.GetInstanceID() == ship.gameObject.GetInstanceID())
            {
                return true;
            }
        }

        return false;
    }

    public void AIDebug(string msg)
    {
        Debug.Log("AI - " + msg);
    }

    private Vector3 _AIMoveHandler(AIMoveAction moveAction)
    {
        var resultPos = Vector3.zero;
        switch (moveAction)
        {
            // Handle flee
            case AIMoveAction.FleeMove:
                resultPos = _HandleFlee();
                AIDebug("Flee Move");
                break;
            
                // Handle stay
            case AIMoveAction.StayMove:
                resultPos = _HandleStay();
                AIDebug("Stay Move");
                break;
            
                // Handle outofrange
            case AIMoveAction.OutOfRangeMove:
                resultPos = _HandleOOR();
                AIDebug("Out of range Move");
                break;
            
                // Handle dfs
            case AIMoveAction.DFSMove:
                resultPos = _HandleDFS();
                AIDebug("DFS Move");
                break;
        }
        return resultPos;
    }

    private Vector3 _HandleFlee()
    {
        // Get the accessible neighbours
        var accessibleNeighbours = _GetNonObstacleNeighbours(ship.hexCoord, ship.transform.forward);

        // Choose the one that results in fewest player ships firing on it
        var shipsAttackingCurrentPos = _GetNumberOfPlayerShipsAttackingPos(ship.hexCoord);
        var lowestNumAttacking = shipsAttackingCurrentPos;
        var neighbourPosToChoose = accessibleNeighbours[0]; // move here as default

        foreach (var neighbour in accessibleNeighbours)
        {
            var tile = hexGrid.GetTileAt(neighbour);
            if (tile == null) continue;

            var numAttackingNeighbourPos = _GetNumberOfPlayerShipsAttackingPos(neighbour);
            if (numAttackingNeighbourPos <= lowestNumAttacking) // Move even if it might not be beneficial
            {
                lowestNumAttacking = numAttackingNeighbourPos;
                neighbourPosToChoose = neighbour;
            }
        }

        var tileWorldPos = hexGrid.GetTileAt(neighbourPosToChoose).transform.position;
        return tileWorldPos;
    }

    private Vector3 _HandleStay()
    {
        return hexGrid.GetTileAt(ship.hexCoord).transform.position;
    }

    private Vector3 _HandleDFS()
    {
        return _NextPos;
    }

    private Vector3 _HandleOOR()
    {
        // Choose the accessible hex closest to the closest player ship
        var accessibleNeighbours = _GetNonObstacleNeighbours(ship.hexCoord, ship.transform.forward);

        var neighbourToChoose = PirateAI.AIFailed; // dont use zero vector, could be close to a player ship
        var closestDistance = float.MaxValue;
        foreach (var neighbour in accessibleNeighbours)
        {
            // Use euclidian distance on world coords
            var neighbourTile = hexGrid.GetTileAt(neighbour);
            if (neighbourTile == null) continue;

            var distanceToClosestPlayerShip = _GetDistanceToClosestPlayerShip(neighbourTile.transform.position);
            if (distanceToClosestPlayerShip < closestDistance)
            {
                closestDistance = distanceToClosestPlayerShip;
                neighbourToChoose = neighbourTile.transform.position;
            }
        }
        return neighbourToChoose;
    }

    private AIMoveAction _GetAIMoveAction()
    {
        // First, check if it can even move at all
        var moveableTo = _GetNonObstacleNeighbours(ship.hexCoord, ship.transform.forward);
        if (moveableTo.Count == 0) return AIMoveAction.StayMove;

        // ON LOW HEALTH
        // Flee if low hp AND in enemy fire arc AND enemy shoots before me - IF NOT, then Stay
        if (ship.Health <= PirateAI.AIConsideredLowHpThreshold)
        {
            var shipsFiringOnMe = _GetShipsThatCanFireOnMe();
            if (shipsFiringOnMe.Count > 1)
            {
                return AIMoveAction.FleeMove;
            }
            // One ship fires on me but it has low hp (need turn order here as well ideally)
            // and I can shoot it as well
            else if (shipsFiringOnMe.Count > 0 && shipsFiringOnMe[0].Health < PirateAI.AIConsideredLowHpThreshold) 
            {
                if (_HasShipInFiringArc(shipsFiringOnMe[0]))
                {
                    return AIMoveAction.StayMove;
                }

                return AIMoveAction.FleeMove;
            }
            
        }

        // NOT ON LOW HEALTH
        // If dfs doesn't find an attackable pos, then use out of range move
        // Store dfs position so we don't have to recalculate it
        var dfs = new AttackPosDFS(this, ship.hexCoord, ship.gameObject.transform.forward, ship.FireRange);
        _NextPos = dfs.FindNextPos();

        if (_NextPos == PirateAI.AIFailed)
        {
            return AIMoveAction.OutOfRangeMove;
        }
        else
        {
            // This should be the most common move 
            return AIMoveAction.DFSMove;
        }

    }

    private enum AIMoveAction
    {
        DFSMove,
        OutOfRangeMove,
        FleeMove,
        StayMove
    }

    private class AttackPosDFS
    {
        private static int DFSLoopLimit = 5000;

        private Vector3Int _startPos;
        private Vector3 _startDir;

        private int _shipRange;

        PirateAI _pirateAI;

        public AttackPosDFS(PirateAI aiRef, Vector3Int startPos, Vector3 startDir, int shipRange)
        {
            _startPos = startPos;
            _startDir = startDir;
            _shipRange = shipRange;
            _pirateAI = aiRef;
        }

        public Vector3 FindNextPos()
        {
            List<DFSPathNode> result = _DFSFindPathToAttackPos(PirateAI.SearchDepth);
            DFSPathNode desiredNode = _FindBestNode(result);

            if (desiredNode == null)
            {
                return PirateAI.AIFailed;
            }

            Vector3Int nextPos = _GetFirstPosInPath(desiredNode);

            if (nextPos != PirateAI.AIFailedInt) //&& nextPos != _pirateAI.ship.hexCoord)
            {
                return _pirateAI.hexGrid.GetTileAt(nextPos).transform.position;
            }

            return PirateAI.AIFailed;
        }

        private DFSPathNode _FindBestNode(List<DFSPathNode> attackNodes)
        {
            if (attackNodes.Count == 0) return null;

            // Apply heuristic for finding the most favorable node here
            // Consider num ships that attack the position, and how far away it is

            int minMoves = int.MaxValue;
            DFSPathNode bestNode = attackNodes[0];

            // Find shortest pathnode
            foreach (var node in attackNodes)
            {
                if (node.Path.Count < minMoves)
                {
                    minMoves = node.Path.Count;

                    // Be aware that when AI already stands in a spot where it can attack
                    // the path length will be 1, which is the best it can be.
                    // Thus, it will only move when there are no attackable ships in range anymore
                    // So, need to fix that. (1)
                    bestNode = node; 
                }
            }

            // Check if there are other nodes with the same path length
            var nodesWithShortestPath = new List<DFSPathNode>();
            foreach (var node in attackNodes)
            {
                if (bestNode.Path.Count == 1)
                {
                    // Fix to (1): if best node is current node (count==1)
                    // also include nodes where path is 2 (moving once)
                    if (node.Path.Count < 3)
                    {
                        nodesWithShortestPath.Add(node);
                    }
                }
                else if (node.Path.Count == bestNode.Path.Count)
                {
                    // Otherwise, just add if path is same length
                    nodesWithShortestPath.Add(node);
                }
            }

            // Check which node has the least player ships attacking its position
            var nodeNumAttackDict = new Dictionary<DFSPathNode, int>();
            var minAttackingShips = int.MaxValue;
            foreach (var node in nodesWithShortestPath)
            {
                var attackingShips = _pirateAI._GetNumberOfPlayerShipsAttackingPos(node.OffsetCoordinate);
                nodeNumAttackDict.Add(node, attackingShips);

                if (attackingShips < minAttackingShips)
                {
                    minAttackingShips = attackingShips;
                    bestNode = node;
                }
            }

            // Check if there are other nodes with the same nr of ships attacking it
            var nodesWithLowNumAttackingShips = new List<DFSPathNode>();
            foreach (var node in nodesWithShortestPath)
            {
                var nodeAtking = nodeNumAttackDict[node];
                var bestAtking = nodeNumAttackDict[bestNode];
                if (nodeAtking == bestAtking)
                {
                    nodesWithLowNumAttackingShips.Add(node);
                }
            }

            // For those who meets both above conditions, check which node is closest to the
            // closest player ship (sometimes the pirate ship will move away otherwise, which looks weird)
            // This will also result in better accuracy for the AI
            var shortestDistToPlayerShip = float.MaxValue;
            foreach (var node in nodesWithLowNumAttackingShips)
            {
                var tile = _pirateAI.hexGrid.GetTileAt(node.OffsetCoordinate);
                if (tile == null) continue;

                var fromPos = tile.transform.position;
                var distToClosestPlayerShip = _pirateAI._GetDistanceToClosestPlayerShip(fromPos);

                if (distToClosestPlayerShip < shortestDistToPlayerShip)
                {
                    shortestDistToPlayerShip = distToClosestPlayerShip;
                    bestNode = node;
                }
            }

            // Checking for directional modifiers are a little bit unstable. Might remove.
            var bestModifierNode = bestNode;

            var bestModNodeWorldPos = _pirateAI.hexGrid.GetTileAt(bestModifierNode.OffsetCoordinate).transform.position;
            bestModNodeWorldPos.y = HexCoordinates.yOffset; //ship y offset
            var shipToCompareWith = _pirateAI._GetClosestPlayerShip(bestModNodeWorldPos);
            var shipToCompareWithPos = shipToCompareWith.transform.position;

            var bestDirectionalModifier = DamageModel.GetDirectionalAttackType((bestModNodeWorldPos - shipToCompareWithPos).normalized, shipToCompareWith.transform.forward);
            var currentMod = DamageModel.GetAttackTypeString(bestDirectionalModifier);

            foreach (var node in nodesWithLowNumAttackingShips)
            {
                
                var tile = _pirateAI.hexGrid.GetTileAt(node.OffsetCoordinate);
                if (tile == null) continue;
                var worldPos = tile.transform.position;
                worldPos.y = HexCoordinates.yOffset;

                // Get the closest ship from the considered nodes world position
                var closestShip = _pirateAI._GetClosestPlayerShip(worldPos);
                var closestShipPos = closestShip.transform.position;

                var attackDir = (worldPos - closestShipPos).normalized;
                var forwardDir = closestShip.gameObject.transform.forward;
                var directionalMod = DamageModel.GetDirectionalAttackType(attackDir, forwardDir);
                
                if (directionalMod > bestDirectionalModifier)
                {
                    _pirateAI.AIDebug(DamageModel.GetAttackTypeString(directionalMod));
                    bestDirectionalModifier = directionalMod;
                    bestModifierNode = node;
                }
            }

            // Weigh modifier vs distance
            // If ship is already close in terms of accuracy, then choose the best directional node
            var badDistanceThreshold = HexCoordinates.xOffset * 2.5;
            if (shortestDistToPlayerShip < badDistanceThreshold)
            {
                _pirateAI.AIDebug("Adjusted best node based on modifier " + DamageModel.GetAttackTypeString(bestDirectionalModifier) + " from " + currentMod);
                bestNode = bestModifierNode;
            }

            return bestNode;
        }

        private Vector3Int _GetFirstPosInPath(DFSPathNode node)
        {

            // If the current position is the desired position, then
            // take the first pos in the path since it is the current position
            if (node.OffsetCoordinate == _pirateAI.ship.hexCoord)
            {
                return node.Path[0];
            }

            // If the path length is longer than 1, then 
            // return the first position in the path
            if (node.Path.Count > 1)
            {
                return node.Path[1]; 
            }

            // If none of the above are true, something went wrong
            return PirateAI.AIFailedInt;
        }

        private List<DFSPathNode> _DFSFindPathToAttackPos(int maxDepth)
        {
            var availableAttackNodes = new List<DFSPathNode>();

            var startHex = _pirateAI.hexGrid.GetTileAt(_startPos);
            if (startHex == null) return availableAttackNodes;

            var visited = new List<DFSPathNode>();
            var stack = new Stack<DFSPathNode>();

            stack.Push(new DFSPathNode(_startPos, new List<Vector3Int> { _startPos }, _startDir.normalized));

            var loopCount = 0;

            while (stack.Count > 0)
            {
                var currNode = stack.Pop();

                // Highlight search area for debugging
                //foreach (var item in currNode.Path)
                //{
                //    _pirateAI.hexGrid.GetTileAt(item).EnableHighLight();
                //}

                // Pop the node first, and don't continue if it the path is longer than the max depth
                if (currNode.Path.Count - 1 > maxDepth) // - 1 to remove starting tile
                {
                    continue;
                }

                // Check if we already visited this node, then skip it (don't want loops)
                // But check if the new path is shorter, then replace it.
                if (visited.Contains(currNode))
                {
                    continue;
                }

                visited.Add(currNode);

                // Check if attackable in range => return path
                var hasAttackable = _pirateAI._HasAttackableFrom(currNode.OffsetCoordinate, currNode.ShipDirection, _shipRange);
                //Debug.Log(currNode.ShipDirection);
                if (hasAttackable)
                {
                    availableAttackNodes.Add(currNode);
                    //_pirateAI.hexGrid.GetTileAt(currNode.OffsetCoordinate).EnableHighLight();
                    // For now, just return as soon as we find an attack pos
                    //return availableAttackNodes;
                }

                var neighbours = _pirateAI._GetNonObstacleNeighbours(currNode.OffsetCoordinate, currNode.ShipDirection);
                foreach (var neighbour in neighbours)
                {
                    // Don't consider positions where the hexes are not initialized
                    var neighbourHex = _pirateAI.hexGrid.GetTileAt(neighbour);
                    if (neighbourHex == null) continue;

                    var path = new List<Vector3Int>();
                    path.AddRange(currNode.Path);
                    path.Add(neighbour);

                    var dir = neighbourHex.transform.position - _pirateAI.hexGrid.GetTileAt(currNode.OffsetCoordinate).transform.position;
                    dir.Normalize();
                    
                    stack.Push(new DFSPathNode(neighbour, path, dir));
                }
                loopCount++;

                if (loopCount > DFSLoopLimit)
                {
                    _pirateAI.AIDebug("DFS reached limit of " + DFSLoopLimit);
                    return availableAttackNodes;
                }

            }

            _pirateAI.AIDebug("DFS iteration count: " + loopCount);
            return availableAttackNodes;
        }
    }

    private class DFSPathNode
    {
        public Vector3Int OffsetCoordinate;
        public List<Vector3Int> Path;
        public Vector3 ShipDirection;

        public DFSPathNode(Vector3Int offsetCoordinate, List<Vector3Int> path, Vector3 shipDirection)
        {
            OffsetCoordinate = offsetCoordinate;
            Path = path;
            ShipDirection = shipDirection;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            DFSPathNode other = (DFSPathNode) obj;

            // Must have same offset coordinate
            if (this.OffsetCoordinate != other.OffsetCoordinate)
            {
                return false;
            }

            // Must have same length of path (replaces directions in equals)
            // If they don't have the same path, then one of the paths might be better,
            // so the path nodes are not equal.
            if (this.Path.Count != other.Path.Count)
            {
                return false;
            }

            // Must have same ship direction: this was way too heavy
            // since the search space increases dramatically, meaning we also
            // need larger depth to find useful positions.

            //var cosAngle = Vector3.Dot(this.ShipDirection.normalized, other.ShipDirection.normalized);

            //if (!Mathf.Approximately(cosAngle, 1.0f))
            //{
            //    return false;
            //}

            return true;
        }

        // Auto generated
        public override int GetHashCode()
        {
            int hashCode = -2141650902;
            hashCode = hashCode * -1521134295 + OffsetCoordinate.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Vector3Int>>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + ShipDirection.GetHashCode();
            return hashCode;
        }
    }

}


