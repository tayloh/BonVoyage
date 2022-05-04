using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note that Pirate ships need the Pirate tag
public class PirateAI : MonoBehaviour
{
    // High search depth doesn't work well for DFS since then
    // it will probably find a path to every node in the grid
    // but they will be very long paths. And, since we don't 
    // allow loops, these nodes will not be considered later by shorter paths.
    // TODO: Fix this by checking if the new path to an already visited node is shorter.
    // Status: Fixed
    
    // Improvement: Collect all hex positions in nearby vicinity and do Djisktras alg
    // to get shortest path to all of them, then check from which of them the AI can reach player ships,
    // then evaluate those positions. (Basically, swap extended DFS to Dijkstras, kind of requires a reimplementation)
    
    public static int SearchDepth = 16;

    private Ship ship;
    public HexGrid hexGrid;
    [SerializeField]
    private GameManager gameManager;

    // Needed:
    // DFS search that considers ship direction <- this made the search space way too large
    // stops when depth is reached.
    // Store all visited nodes along with the ships direction at that node.
    // Then, loop through all positions in a path, check if a ship is attackable from that pos (so need to store direction at each tile as well)
    // Choose to move towards (first tile in path to) the tile where it gets the best directional bonus on any ship within the depth

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    public List<Vector3> ChoosePath()
    {
        var nextPos = new AttackPosDFS(this, ship.hexCoord, ship.gameObject.transform.forward, ship.FireRange).FindNextPos();

        // AI code is a little bit unstable right now, returns null if something went wrong, and we use previous method instead
        if (nextPos != null)
        {
            Debug.Log("DFS New AI");
            return nextPos;
        }
        Debug.Log("DFS Old AI");
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

        public List<Vector3> FindNextPos()
        {
            List<DFSPathNode> result = _DFSFindPathToAttackPos(PirateAI.SearchDepth);
            DFSPathNode desiredNode = _FindBestNode(result);

            if (desiredNode == null)
            {
                return null;
            }

            Vector3Int nextPos = _GetFirstPosInPath(desiredNode);

            if (nextPos != Vector3Int.zero && nextPos != _pirateAI.ship.hexCoord)
            {
                return new List<Vector3> { _pirateAI.hexGrid.GetTileAt(nextPos).transform.position };
            }

            return null;
        }

        private DFSPathNode _FindBestNode(List<DFSPathNode> attackNodes)
        {
            if (attackNodes.Count == 0) return null;

            int minMoves = int.MaxValue;
            DFSPathNode bestNode = attackNodes[0];

            foreach (var node in attackNodes)
            {
                if (node.Path.Count < minMoves)
                {
                    minMoves = node.Path.Count;
                    bestNode = node;
                }
            }

            return bestNode;
        }

        private Vector3Int _GetFirstPosInPath(DFSPathNode node)
        {
            if (node.Path.Count > 1) return node.Path[1]; // [0] is current position

            return Vector3Int.zero;
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

                var neighbours = _pirateAI.hexGrid.GetAccessibleNeighboursFor(currNode.OffsetCoordinate, currNode.ShipDirection);
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
                    Debug.Log("DFS reached limit of " + DFSLoopLimit);
                    return availableAttackNodes;
                }

            }

            Debug.Log("DFS SEARCH: " + loopCount);
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


