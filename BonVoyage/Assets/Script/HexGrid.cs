using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    private Dictionary<Vector3Int, Hex> hexTileDict = new Dictionary<Vector3Int, Hex>();
    private Dictionary<Vector3Int, List<Vector3Int>> hexTileNeighboursDict = new Dictionary<Vector3Int, List<Vector3Int>>();
    public GameObject hexParent;

    private static float xOffset = 2;
    private static float yOffset = 1;
    private static float zOffset = 1.73f;

    private int side = -1;

    private Vector3 originGrid = new Vector3(0, 0, 0);

    public int gridSideSize = 10;
    [SerializeField]
    private GameObject tile;
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private GameManager gameManager;

    private void Awake()
    {
        xOffset = HexCoordinates.xOffset;
        yOffset = HexCoordinates.yOffset;
        zOffset = HexCoordinates.zOffset;

        GenerateGrid();
    }

    private void Start()
    {
        var allHex = hexParent.GetComponentsInChildren<Hex>();
        foreach (Hex hexElement in allHex)
        {
            hexTileDict.Add(hexElement.HexCoords, hexElement);
        }
    }

    //TESTING
    public int horShift = 1;
    public int verShift = 1;
    private void Update()
    {
        
            //AdaptToPlayersView(Camera.main.transform.position);
        

        /*if (Input.GetKeyDown(KeyCode.B))
        {
            List<Vector3Int> example;
            List<Vector3Int> example_otherside;
            example = GetAttackableTilesFor(new Vector3Int(4, 0, 2), 0, 2);
            example_otherside = GetAttackableTilesFor(new Vector3Int(4, 0, 2), 1, 2);
            example.AddRange(example_otherside);
            foreach (var tile in example)   
            {
                Hex hex = GetTileAt(tile);
                if (hex != null)
                {
                    GetTileAt(tile).EnableHighLight();
                } 
            }
        }*/
    }

    private void LateUpdate()
    {
        AdaptToPlayersView(Camera.main.transform.position);
    }

    private void AdaptToPlayersView(Vector3 cameraPos)
    {
        Vector3Int originOffset = HexCoordinates.ConvertPositionToOffset(cameraPos);
        Vector3Int originShift = originOffset - HexCoordinates.ConvertPositionToOffset(originGrid);
        HorizontalShifting(originShift.x);
        VerticalShifting(originShift.z);
    }

    private void HorizontalShifting(int direction) //direction>0 is right
    {
        while (direction != 0)
        {
            Vector3 destroyHexWorldCoord;
            Vector3 createHexWorldCoord;
            if (direction > 0)
            {
                //Deleting farthest columns
                //delete the extremity 
                destroyHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - 1), 0, originGrid.z);
                DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                for (int row = 1; row < gridSideSize; row++)
                {
                    //above central line
                    destroyHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - 1 - (row / 2 + 1 * PositiveModulo(row, 2)/2f)), 0, originGrid.z + row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                    //under central line
                    destroyHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - 1 - (row / 2 + 1 * PositiveModulo(row, 2)/2f)), 0, originGrid.z - row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }

                //Creating the new columns
                createHexWorldCoord = new Vector3(originGrid.x + xOffset * gridSideSize, 0, originGrid.z);
                CreateTileAt(createHexWorldCoord);
                for (int row = 1; row < gridSideSize; row++)
                {
                    //above central line
                    createHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z + row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                    //under central line
                    createHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z - row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }

                originGrid.x = originGrid.x + xOffset;
                direction -= 1;
            }
            else
            {
                //Deleting farthest columns
                //delete the extremity 
                destroyHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - 1), 0, originGrid.z);
                DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                for (int row = 1; row < gridSideSize; row++)
                {
                    //above central line
                    destroyHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - 1 - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z + row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                    //under central line
                    destroyHexWorldCoord = new Vector3(originGrid.x + xOffset * (gridSideSize - 1 - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z - row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }

                //Creating the new columns
                createHexWorldCoord = new Vector3(originGrid.x - xOffset * gridSideSize, 0, originGrid.z);
                CreateTileAt(createHexWorldCoord);
                for (int row = 1; row < gridSideSize; row++)
                {
                    //above central line
                    createHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z + row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                    //under central line
                    createHexWorldCoord = new Vector3(originGrid.x - xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z - row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }

                originGrid.x = originGrid.x - xOffset;
                direction += 1;
            }
        }
    }

    private void VerticalShifting(int direction) //direction>0 is up
    {
        while (direction != 0)
        {
            Vector3 destroyHexWorldCoord;
            Vector3 createHexWorldCoord;
            if (direction > 0) //moving up
            {
                //deleting bottom line
                for (int i = 0; i < gridSideSize; i++)
                {
                    destroyHexWorldCoord = new Vector3(originGrid.x - (gridSideSize / 2f - 0.5f - i) * xOffset, 0, originGrid.z - (gridSideSize - 1) * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }

                //creating top line
                for (int i = 0; i < gridSideSize - 1; i++)
                {
                    createHexWorldCoord = new Vector3(originGrid.x - (gridSideSize / 2f - 1 - i) * xOffset, 0, originGrid.z + (gridSideSize) * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }
                //determining which side to move : left, then right...
                side = side == 1 ? -1 : 1;

                //deleting bottom side line 
                for (int row = 0; row < gridSideSize - 1; row++)
                {
                    //under central line
                    destroyHexWorldCoord = new Vector3(originGrid.x + side * xOffset * (gridSideSize - 1 - (row / 2)), 0, originGrid.z - row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }
                //creating top side line 
                for (int row = 1; row < gridSideSize + 1; row++)
                {
                    //above central line
                    createHexWorldCoord = new Vector3(originGrid.x - side * xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z + row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }
                direction -= 1;
                originGrid.z += zOffset;
                originGrid.x -= side * xOffset / 2;
            }
            else //moving down
            {
                //deleting top line
                for (int i = 0; i < gridSideSize; i++)
                {
                    destroyHexWorldCoord = new Vector3(originGrid.x - (gridSideSize / 2f - 0.5f - i) * xOffset, 0, originGrid.z + (gridSideSize - 1) * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }

                //creating bottom line
                for (int i = 0; i < gridSideSize - 1; i++)
                {
                    createHexWorldCoord = new Vector3(originGrid.x - (gridSideSize / 2f - 1 - i) * xOffset, 0, originGrid.z - (gridSideSize) * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }
                //determining which side to move : left, then right...
                side = side == 1 ? -1 : 1;

                //deleting top side line 
                for (int row = 0; row < gridSideSize - 1; row++)
                {
                    //under central line
                    destroyHexWorldCoord = new Vector3(originGrid.x + side * xOffset * (gridSideSize - 1 - (row / 2)), 0, originGrid.z + row * zOffset);
                    DestroyTileAt(HexCoordinates.ConvertPositionToOffset(destroyHexWorldCoord));
                }
                //creating bottom side line 
                for (int row = 1; row < gridSideSize + 1; row++)
                {
                    //above central line
                    createHexWorldCoord = new Vector3(originGrid.x - side * xOffset * (gridSideSize - (row / 2 + 1 * PositiveModulo(row, 2) / 2f)), 0, originGrid.z - row * zOffset);
                    CreateTileAt(createHexWorldCoord);
                }
                direction += 1;
                //TODO update origin x and z
                originGrid.z -= zOffset;
                originGrid.x -= side * xOffset / 2;
            }


        }
    }

    private void DestroyTileAt(Vector3Int hexCoord)
    {
        Destroy(GetTileAt(hexCoord).gameObject);
        hexTileDict.Remove(hexCoord);
    }

    private void CreateTileAt(Vector3 worldCoord)
    {
        GameObject newHex = Instantiate(tile, worldCoord, Quaternion.identity, hexParent.transform);
        hexTileDict.Add(HexCoordinates.ConvertPositionToOffset(worldCoord), newHex.GetComponent<Hex>());
    }

    private int ComputeMinimalNumberOfHexToFillView(float viewDistance) //viewDistance is the max diagonal of the image rendered by the camera projected on the sea level
    {
        return Mathf.CeilToInt(viewDistance / 2 * zOffset + 1); //value of gridSideSize for viewDistance equals the inner radius of the hexGrid
    }

    private void GenerateGrid()
    {
        int originShift = Mathf.RoundToInt((gridSideSize * 2 - 1) / 2);
        //Instantiating the grid
        //middle row is drawn first to not draw it twice when drawing up and down rows:
        for (int i = 0; i < 2 * gridSideSize - 1; i++)
        {
            Instantiate(tile, new Vector3((i - originShift) * xOffset, 0, 0), Quaternion.identity, hexParent.transform);
        }
        //upper and down rows:
        int rowSize = 2 * gridSideSize - 2;
        int rowIndex = 1;
        while (rowSize >= gridSideSize)
        {
            for (int i = 0; i < rowSize; i++)
            {
                Instantiate(tile, new Vector3((rowIndex / 2f + i - originShift) * xOffset, 0, rowIndex * zOffset), Quaternion.identity, hexParent.transform);
                Instantiate(tile, new Vector3((rowIndex / 2f + i - originShift) * xOffset, 0, -rowIndex * zOffset), Quaternion.identity, hexParent.transform);
            }
            rowSize -= 1;
            rowIndex += 1;
        }
    }

    public void DisableHighlightOfAllHexes()
    {
        foreach (var hex in hexTileDict.Values)
        {
            hex.DisableHighlight();
        }
    }

    public Hex GetTileAt(Vector3Int hexCoordinates)
    //Return the hex situated at given coordinates, null if there is no corresponding hex
    {
        Hex result = null;
        hexTileDict.TryGetValue(hexCoordinates, out result);
        return result;
    }

    public List<Vector3Int> GetNeighboursFor(Vector3Int hexCoordinates)
    {
        if (hexTileDict.ContainsKey(hexCoordinates) == false) //if the tile does not exist, no neighbour
        {
            return new List<Vector3Int>();
        }
        if (hexTileNeighboursDict.ContainsKey(hexCoordinates)) //if we ve already calculated the neighbours of the tile, return them
        {
            return hexTileNeighboursDict[hexCoordinates];
        }
        //if not, find neighbours and enter them in the dictionary
        hexTileNeighboursDict.Add(hexCoordinates, new List<Vector3Int>());
        foreach (Vector3Int direction in Direction.GetDirectionList(hexCoordinates.z))
        {
            if (hexTileDict.ContainsKey(hexCoordinates + direction))
            {
                hexTileNeighboursDict[hexCoordinates].Add(hexCoordinates + direction);
            }
        }
        return hexTileNeighboursDict[hexCoordinates];
    }

    public Vector3Int GetClosestHex(Vector3 worldPosition)
    {
        worldPosition.y = 0;
        return HexCoordinates.ConvertPositionToOffset(worldPosition);
    }

    public bool LineCircleIntersect(Vector2 p1, Vector2 p2, Vector2 center, float radius, float maxDist)
    {
        Vector2 r0 = p1;
        Vector2 rd = (p2 - p1).normalized;

        float lambda = 0;
        float a = Vector2.Dot(rd, rd);
        float b = 2 * Vector2.Dot(rd, r0 - center);
        float c = Vector2.Dot(r0 - center, r0 - center) - (radius * radius);

        float x0 = 0;
        float x1 = 0;

        float discriminator = b * b - 4 * a * c;

        if (discriminator < 0) return false;

        else if (discriminator == 0)
        {
            x0 = -b / (2 * a);
            x1 = x0;
        }
        else
        {
            x0 = (-b + Mathf.Sqrt(discriminator)) / (2 * a);
            x1 = (-b - Mathf.Sqrt(discriminator)) / (2 * a);
        }

        if (x0 > x1)
        {
            float temp = x0;
            x0 = x1;
            x1 = temp;
        }

        lambda = x0;

        if (lambda < 0 || lambda + 0.001f > maxDist)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Given offset coordinates, broadside (0 = right, 1 = left), and firing range
    /// Returns the attackable tiles from the provided offset coordinate position
    /// </summary>
    public List<Vector3Int> GetAttackableTilesFor(Vector3Int hexcoordinates, int side, int range)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        Ship ship = GetTileAt(hexcoordinates).Ship;

        if (hexTileDict.ContainsKey(hexcoordinates) == false || ship == null || range == 0)
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

        int distBetweenHexCenters = 2;
        Transform shipTransform = ship.transform;
        Vector3 currHexWorldPosition = GetTileAt(hexcoordinates).gameObject.transform.position;

        // R1, R2 or L1, L2
        Vector3 dir1 = new Vector3();
        Vector3 dir2 = new Vector3();

        // Get directions for the arc from the ship broadside
        switch (side)
        {
            case 0:
                dir1 = (Quaternion.AngleAxis(60, Vector3.up) * shipTransform.forward).normalized;
                dir2 = (Quaternion.AngleAxis(120, Vector3.up) * shipTransform.forward).normalized;
                break;

            case 1:
                dir1 = (Quaternion.AngleAxis(-60, Vector3.up) * shipTransform.forward).normalized;
                dir2 = (Quaternion.AngleAxis(-120, Vector3.up) * shipTransform.forward).normalized;
                break;

            default:
                throw new Exception("Invalid broadside number (0=right, 1=left");
        }

        Debug.DrawLine(currHexWorldPosition, currHexWorldPosition + distBetweenHexCenters * range * dir1, Color.red, 100f, false);
        Debug.DrawLine(currHexWorldPosition, currHexWorldPosition + distBetweenHexCenters * range * dir2, Color.red, 100f, false);

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

            Debug.DrawLine(hexLineUpper[i], hexLineUpper[i] + distBetweenHexCenters * (numHexesInLine - 1) * interpolateDir, Color.red, 100f, false);

            for (int j = 0; j < numHexesInLine; j++)
            {
                Vector3 pos = hexLineUpper[i] + distBetweenHexCenters * j * interpolateDir;

                pos.x = Mathf.Round(pos.x); // x-positions are always integers (just remove the floating point error)
                pos.z = Mathf.Round(pos.z / 1.73f) * 1.73f; // z-positions are always a multiple of 1.73 (make sure they are)

                Vector3Int hexPos = GetClosestHex(pos);

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
                var hasIntersection = LineCircleIntersect(lineStart, lineEnd, origin, 0.9f, maxDistance);

                // Check if there was an intersection or not
                if (hasIntersection)
                {
                    foundIntersection = true;
                    break;
                }
            }

            if (foundIntersection)
            {
                var tileConsideredForRemoval = GetClosestHex(vec3Positions[i]);
                result.Remove(tileConsideredForRemoval);
            }

        }



        // Raytrace from the ship position to all attackable tiles, if a ship is in the path
        // remove the tile
        //for (int i = 0; i < vec3Positions.Count; i++)
        //{
        //    var rayDir = vec3Positions[i] - currHexWorldPosition;
        //    var distance = rayDir.magnitude;
        //    rayDir.Normalize();

        //    RaycastHit[] hits = Physics.RaycastAll(currHexWorldPosition + rayDir * 1.0f, rayDir, distance);
        //    var isInLineOfSight = true;
        //    Vector3Int tileToBeRemoved = Vector3Int.zero;

        //    foreach (var hit in hits)
        //    {
        //        var hitGO = hit.transform.gameObject;

        //        if (hitGO.CompareTag("Pirate") || hitGO.CompareTag("PlayerShip"))
        //        {
        //            // Check if there is a ship on the tile
        //            tileToBeRemoved = GetClosestHex(vec3Positions[i]);
        //            var shipTile = hitGO.GetComponent<Ship>().hexCoord;

        //            // If there is, it should not be removed, since the ship is attackable.
        //            if (shipTile == tileToBeRemoved) continue;

        //            isInLineOfSight = false;
        //            break;
        //        }
        //    }
        //    if (!isInLineOfSight)
        //    {
        //        //result.Remove(tileToBeRemoved);
        //    }
        //}

        // Remove tiles that are obstructed by a ship.
        // Essentially, trace a ray to each of the max range tiles
        // if the ray hits a ship, the following tiles in the same direction
        // are obstructed.
        //for (int i = 0; i < maxRangeTilePositions.Count; i++)
        //{
        //    // Offset the y coordinate so we don't collide with hexes.
        //    var yOffset = 0.25f;
        //    var origin = currHexWorldPosition;
        //    origin.y += yOffset;

        //    var destination = maxRangeTilePositions[i];
        //    destination.y += yOffset;

        //    //Debug.Log(origin + "->" + destination);

        //    var rayDir = (destination - origin).normalized;

        //    //Debug.DrawLine(origin, origin + rayDir * distBetweenHexCenters * range, Color.green, 100f, false);

        //    RaycastHit[] hits = Physics.RaycastAll(origin + rayDir*0.1f, rayDir, range * distBetweenHexCenters);
        //    if (hits.Length != 0)
        //    {
        //        foreach (var hit in hits)
        //        {
        //            //Debug.Log("Obstruction detected!" + hit.transform.name);
        //            var hitGO = hit.transform.gameObject;
        //            if (hitGO.CompareTag("PlayerShip") || hitGO.CompareTag("Pirate"))
        //            {
        //                Debug.Log(hitGO.name);
        //                var localOrigin = hitGO.transform.position;
        //                localOrigin.y += yOffset;

        //                var localRayDir = (destination - localOrigin);
        //                if (localRayDir.magnitude < 0.1) continue; // skip ships if they are on last tile (won't block anyway)

        //                localRayDir.Normalize();

        //                //Debug.DrawLine(localOrigin, localOrigin + localRayDir * distBetweenHexCenters, Color.green, 100f, false);

        //                var pos = localOrigin + distBetweenHexCenters * localRayDir;
        //                pos.x = Mathf.Round(pos.x); // x-positions are always integers (just remove the floating point error)
        //                pos.z = Mathf.Round(pos.z / 1.73f) * 1.73f; // z-positions are always a multiple of 1.73 (make sure they are)

        //                //result.Remove(GetClosestHex(pos));

        //                //for (int j = 2; j < 4; j++)
        //                //{

        //                //    //Debug.Log(localOrigin + localRayDir * j);

        //                //    // Bug here
        //                //    var hexPos = GetClosestHex(localOrigin + localRayDir * j);
        //                //    //Debug.Log(hexPos);
                            
                            
        //                //    result.Remove(hexPos);
        //                //}
        //            }
        //        }
 
        //    }

        //}

        return result;
    }

    public List<Vector3Int> GetAccessibleNeighboursFor(Vector3Int hexcoordinates, Vector3 forward)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        //TODO : return the list of neighbours accessible for a ship pointing in the towards direction (only 3 possible tiles)
        if (hexTileDict.ContainsKey(hexcoordinates) == false) //if the tile does not exist, no neighbour
        {
            return result;
        }
        switch (Direction.IsOffsetEven(hexcoordinates.z))
        {
            case true:
                forward = HexCoordinates.ConvertVectorToOffset(forward);
                for (int i = 0; i < Direction.directionsOffsetEven.Count; i++)
                {
                    if (Direction.directionsOffsetEven[i] == forward)
                    {
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i - 1, 6)]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i - 1, 6)]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[i]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[i]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i + 1, 6)]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetEven[PositiveModulo(i + 1, 6)]);
                        }
                    }
                }
                break;
            case false:
                forward = HexCoordinates.ConvertVectorToOffset(forward);
                for (int i = 0; i < Direction.directionsOffsetOdd.Count; i++)
                {
                    if (Direction.directionsOffsetEven[i] == forward) //direction is always convert as an even configuration
                    {
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i - 1, 6)]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i - 1, 6)]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[i]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[i]);
                        }
                        if (hexTileDict.ContainsKey(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i + 1, 6)]))
                        {
                            result.Add(hexcoordinates + Direction.directionsOffsetOdd[PositiveModulo(i + 1, 6)]);
                        }
                    }
                }
                break;
        }
        return result;
    }

    public static int PositiveModulo(int a, int b)
    {
        int c = a % b;
        if ((c < 0 && b > 0) || (c > 0 && b < 0))
        {
            c += b;
        }
        return c;
    }

    public void PlaceShip(Vector3Int hexCoord, Ship ship)
    {
        if (hexTileDict.ContainsKey(hexCoord))
        {
            hexTileDict[hexCoord].Ship = ship;
            Debug.Log("Ship placed at " + hexCoord);
        }
        else
        {
            Debug.Log("The ship is not placed on an existing tile.");
            throw new KeyNotFoundException();
        }
    }
}


public static class Direction
{
    public static List<Vector3Int> directionsOffsetOdd = new List<Vector3Int> //make it private ?
    {
        new Vector3Int(-1,0,1), //NW
        new Vector3Int(0,0,1),  //NE
        new Vector3Int(1,0,0),  //E
        new Vector3Int(0,0,-1),  //SE
        new Vector3Int(-1,0,-1),  //SW
        new Vector3Int(-1,0,0),  //W
    };

    public static List<Vector3Int> directionsOffsetEven = new List<Vector3Int>
    {
        new Vector3Int(0,0,1), //NW
        new Vector3Int(1,0,1),  //NE
        new Vector3Int(1,0,0),  //E
        new Vector3Int(1,0,-1),  //SE
        new Vector3Int(0,0,-1),  //SW
        new Vector3Int(-1,0,0),  //W
    };

    public static bool IsOffsetEven(int z)
    {
        return (z % 2 == 0 ? true : false);
    }

    public static List<Vector3Int> GetDirectionList(int z)
    //return the correct directions vectors depending on the z of a tile (even or odd line)
    {
        return (z % 2 == 0 ? directionsOffsetEven : directionsOffsetOdd);
    }
}
