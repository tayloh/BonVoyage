using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note that Pirate ships need the Pirate tag
public class PirateAI : MonoBehaviour
{
    private Ship ship;
    public static HexGrid hexGrid;
    [SerializeField]
    private static GameManager gameManager;

    // For DFS
    private AttackPosDFS _DFS;

    // Needed:
    // DFS search that considers ship direction,
    // stops when depth is reached.
    // Store all visited nodes along with the ships direction at that node.
    // Then, loop through all positions in a path, check if a ship is attackable from that pos (so need to store direction at each tile as well)
    // Choose to move towards (first tile in path to) the tile where it gets the best directional bonus on any ship within the depth

    private void Awake()
    {
        ship = GetComponent<Ship>();
        _DFS = new AttackPosDFS();
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
        //StartCoroutine(EndPirateTurn());
        return new List<Vector3>() { positionGoal};    
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

    private static List<Vector3Int> _GetAttackableTilesFrom(Vector3Int hexcoordinates, Vector3 forwardDir, int side, int range)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        if (hexGrid.GetTileAt(hexcoordinates) != null || range == 0)
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

        float distBetweenHexCenters = HexCoordinates.xOffset * 2;

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

    private class AttackPosDFS
    {
        private Stack<Vector3> _directions;
        private Stack<Vector3Int> _offsetPositions;

        public AttackPosDFS()
        {
            _directions = new Stack<Vector3>();
            _offsetPositions = new Stack<Vector3Int>();
        }

        // TODO: Finish implementing DFS search with our tile system and stuff integrated
        //public List<Vector3> FindPathToAttackPosFrom(Vector3 currentPos, Vector3 currentForwardDir, int depth)
        //{
        //    if (depth == 0) return new List<Vector3>() { currentPos };

        //    Stack<Vector3> path = _DFS(currentPos, currentForwardDir, depth);
            
        //}

        //private Stack<Vector3> _DFS (Vector3 currentPos, Vector3 currentForwardDir, int depth)
        //{
        //    if (depth == 0) 
        //    {
        //        Stack<Vector3> vector3positions = new Stack<Vector3>();

        //        foreach (var item in _offsetPositions)
        //        {
        //            Vector3 pos = hexGrid.GetTileAt(item).transform.position;
        //            vector3positions.Push(pos);
        //        }

        //        return vector3positions;
        //    }
        //    else
        //    {
        //        foreach (var item in hexGrid.GetAccessibleNeighboursFor(_offsetPositions.Pop(), _directions.Pop()))
        //        {
        //            _DFS(hexGrid.GetTileAt(item).transform.position, )
        //        }
        //    }
        //}

    }
    // TODO
    private class DFSHexNode
    {
        // hex
        // direction
        // position
        // etc
    }

}


